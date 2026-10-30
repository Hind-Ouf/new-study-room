using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;

namespace StudyRoomBooking.DAL;

public interface IBookingRepository : IRepository<Booking>
{
    Task<bool> HasConflictAsync(int roomId, DateTime start, DateTime end, int? excludingId = null, CancellationToken cancellationToken = default);
    Task<List<Room>> FindAvailableRoomsAsync(DateTime start, DateTime end, int? floor, bool needsProjector, CancellationToken cancellationToken = default);
    Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<List<Booking>> SearchAsync(string? subject, int? courseId, DateTime? from, DateTime? until, CancellationToken cancellationToken = default);
}

public sealed class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _db;
    public BookingRepository(ApplicationDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Bookings.Include(b => b.Room).Include(b => b.Courses).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<Booking>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Bookings.Include(b => b.Room).Include(b => b.Courses).OrderBy(b => b.StartTime).ToListAsync(cancellationToken);

    public Task<bool> HasConflictAsync(int roomId, DateTime start, DateTime end, int? excludingId = null, CancellationToken cancellationToken = default) =>
        _db.Bookings.AnyAsync(b => b.RoomId == roomId && (!excludingId.HasValue || b.Id != excludingId.Value)
            && start < b.EndTime && end > b.StartTime, cancellationToken);

    public Task<List<Room>> FindAvailableRoomsAsync(DateTime start, DateTime end, int? floor, bool needsProjector, CancellationToken cancellationToken = default)
    {
        var rooms = _db.Rooms.AsQueryable();
        if (floor.HasValue) rooms = rooms.Where(r => r.Floor == floor.Value);
        if (needsProjector) rooms = rooms.Where(r => r.HasProjector);
        return rooms.Where(r => !r.Bookings.Any(b => start < b.EndTime && end > b.StartTime))
            .OrderBy(r => r.Name).ToListAsync(cancellationToken);
    }

    public Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var selected = ids.Distinct().ToArray();
        return _db.Courses.Where(c => selected.Contains(c.Id)).OrderBy(c => c.Code).ToListAsync(cancellationToken);
    }

    public async Task<List<Booking>> SearchAsync(string? subject, int? courseId, DateTime? from, DateTime? until, CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings.Include(b => b.Room).Include(b => b.Courses).AsQueryable();
        if (!string.IsNullOrWhiteSpace(subject)) query = query.Where(b => b.Subject.Contains(subject.Trim()));
        if (courseId.HasValue) query = query.Where(b => b.Courses.Any(c => c.Id == courseId.Value));
        if (from.HasValue) query = query.Where(b => b.EndTime > from.Value);
        if (until.HasValue) query = query.Where(b => b.StartTime < until.Value);
        return await query.OrderBy(b => b.StartTime).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Booking entity, CancellationToken cancellationToken = default) =>
        await _db.Bookings.AddAsync(entity, cancellationToken);
    public void Update(Booking entity) => _db.Bookings.Update(entity);
    public void Remove(Booking entity) => _db.Bookings.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _db.SaveChangesAsync(cancellationToken);
}
