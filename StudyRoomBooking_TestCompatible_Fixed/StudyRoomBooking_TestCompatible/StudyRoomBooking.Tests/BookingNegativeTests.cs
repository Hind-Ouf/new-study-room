using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StudyRoomBooking.DAL;
using StudyRoomBooking.Models;
using StudyRoomBooking.Services;

namespace StudyRoomBooking.Tests;

public sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public ApplicationDbContext Context { get; }
    public BookingRepository Repository { get; }
    public BookingService Service { get; }

    private TestDb(SqliteConnection connection, ApplicationDbContext context)
    {
        _connection = connection;
        Context = context;
        Repository = new BookingRepository(context);
        Service = new BookingService(Repository, NullLogger<BookingService>.Instance);
    }

    public static async Task<TestDb> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return new TestDb(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

public class BookingNegativeTests
{
    private static readonly DateTime Day = new(2026, 11, 10);

    private static Booking MakeBooking(int roomId, int startHour, int endHour, string subject = "Study") => new()
    {
        RoomId = roomId,
        CreatorStudentId = "s123456",
        Subject = subject,
        OwnerUserId = "user-1",
        StartTime = Day.AddHours(startHour),
        EndTime = Day.AddHours(endHour)
    };

    [Fact]
    public async Task Create_EndTimeBeforeStartTime_ReturnsFalseAndSavesNothing()
    {
        await using var db = await TestDb.CreateAsync();

        var ok = await db.Service.CreateAsync(MakeBooking(1, 14, 13));

        Assert.False(ok);
        Assert.Empty(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_EndTimeEqualsStartTime_ReturnsFalseAndSavesNothing()
    {
        await using var db = await TestDb.CreateAsync();

        var ok = await db.Service.CreateAsync(MakeBooking(1, 14, 14));

        Assert.False(ok);
        Assert.Empty(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_FullyOverlappingBooking_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        var ok = await db.Service.CreateAsync(MakeBooking(1, 10, 12, "Second"));

        Assert.False(ok);
        Assert.Single(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_PartiallyOverlappingBooking_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        var ok = await db.Service.CreateAsync(MakeBooking(1, 11, 13, "Second"));

        Assert.False(ok);
        Assert.Single(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_BookingInsideExistingBooking_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 14)));

        Assert.False(await db.Service.CreateAsync(MakeBooking(1, 11, 12, "Inside")));
        Assert.Single(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_BookingEnclosingExistingBooking_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 11, 12)));

        Assert.False(await db.Service.CreateAsync(MakeBooking(1, 10, 14, "Enclosing")));
        Assert.Single(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Create_BackToBackBookings_AreAllowed()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 12, 13, "After")));
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 9, 10, "Before")));
        Assert.Equal(3, await db.Context.Bookings.CountAsync());
    }

    [Fact]
    public async Task Create_SameTimeInDifferentRoom_IsAllowed()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        Assert.True(await db.Service.CreateAsync(MakeBooking(2, 10, 12)));
        Assert.Equal(2, await db.Context.Bookings.CountAsync());
    }

    [Fact]
    public async Task Update_ConflictWithAnotherBooking_ReturnsFalseAndKeepsOriginalTimes()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 13, 14, "Other")));
        var other = await db.Context.Bookings.SingleAsync(b => b.Subject == "Other");

        var update = MakeBooking(1, 11, 13, "Other");
        update.Id = other.Id;
        var ok = await db.Service.UpdateAsync(update);

        Assert.False(ok);
        db.Context.ChangeTracker.Clear();
        var reloaded = await db.Context.Bookings.SingleAsync(b => b.Id == other.Id);
        Assert.Equal(Day.AddHours(13), reloaded.StartTime);
        Assert.Equal(Day.AddHours(14), reloaded.EndTime);
    }

    [Fact]
    public async Task Update_OverlapWithItself_IsAllowed()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));
        var existing = await db.Context.Bookings.SingleAsync();

        var update = MakeBooking(1, 11, 13);
        update.Id = existing.Id;

        Assert.True(await db.Service.UpdateAsync(update));
    }

    [Fact]
    public async Task Update_InvalidTimeWindow_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));
        var existing = await db.Context.Bookings.SingleAsync();

        var update = MakeBooking(1, 15, 14);
        update.Id = existing.Id;

        Assert.False(await db.Service.UpdateAsync(update));
    }

    [Fact]
    public async Task Update_MissingBooking_ReturnsFalse()
    {
        await using var db = await TestDb.CreateAsync();

        var update = MakeBooking(1, 10, 12);
        update.Id = 99999;

        Assert.False(await db.Service.UpdateAsync(update));
        Assert.Empty(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNull()
    {
        await using var db = await TestDb.CreateAsync();

        Assert.Null(await db.Service.GetByIdAsync(99999));
    }

    [Fact]
    public async Task Delete_UnknownId_DoesNotThrow()
    {
        await using var db = await TestDb.CreateAsync();

        await db.Service.DeleteAsync(99999);

        Assert.Empty(await db.Context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task Delete_ExistingBooking_RemovesItAndFreesTheSlot()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));
        var existing = await db.Context.Bookings.SingleAsync();

        await db.Service.DeleteAsync(existing.Id);

        Assert.Empty(await db.Context.Bookings.ToListAsync());
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12, "Replacement")));
    }

    [Fact]
    public async Task Create_WithCourseIds_AttachesOnlyExistingCourses()
    {
        await using var db = await TestDb.CreateAsync();

        var ok = await db.Service.CreateAsync(MakeBooking(1, 10, 12), new[] { 1, 2, 99999 });

        Assert.True(ok);
        var saved = await db.Repository.GetByIdAsync((await db.Context.Bookings.SingleAsync()).Id);
        Assert.NotNull(saved);
        Assert.Equal(new[] { 1, 2 }, saved!.Courses.Select(c => c.Id).OrderBy(i => i).ToArray());
    }

    [Fact]
    public async Task IsRoomAvailable_ReturnsFalseDuringBookingAndTrueAfter()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        Assert.False(await db.Service.IsRoomAvailableAsync(1, Day.AddHours(11), Day.AddHours(13)));
        Assert.True(await db.Service.IsRoomAvailableAsync(1, Day.AddHours(12), Day.AddHours(13)));
    }

    [Fact]
    public async Task GetAvailableRooms_ExcludesBookedRoom_AndHonoursProjectorFilter()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(2, 10, 12)));

        var rooms = await db.Service.GetAvailableRoomsAsync(Day.AddHours(10), Day.AddHours(12), floor: null, needsProjector: false);
        Assert.DoesNotContain(rooms, r => r.Id == 2);
        Assert.Equal(3, rooms.Count);

        var projectorRooms = await db.Service.GetAvailableRoomsAsync(Day.AddHours(10), Day.AddHours(12), floor: null, needsProjector: true);
        Assert.Equal(new[] { 4 }, projectorRooms.Select(r => r.Id).ToArray());
    }

    [Fact]
    public async Task Search_UnknownSubject_ReturnsEmpty()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12, "Algorithms")));

        Assert.Empty(await db.Service.SearchAsync("no-such-subject", null, null, null));
        Assert.Single(await db.Service.SearchAsync("Algo", null, null, null));
    }

    [Fact]
    public async Task Search_DateRangeOutsideBooking_ReturnsEmpty()
    {
        await using var db = await TestDb.CreateAsync();
        Assert.True(await db.Service.CreateAsync(MakeBooking(1, 10, 12)));

        Assert.Empty(await db.Service.SearchAsync(null, null, Day.AddHours(12), Day.AddHours(15)));
        Assert.Single(await db.Service.SearchAsync(null, null, Day.AddHours(11), Day.AddHours(15)));
    }
}
