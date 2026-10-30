using StudyRoomBooking.DAL;
using StudyRoomBooking.Domain;
using StudyRoomBooking.Models;
using Microsoft.Extensions.Logging;

namespace StudyRoomBooking.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookings, ILogger<BookingService> logger)
    {
        _bookings = bookings;
        _logger = logger;
    }

    public Task<List<Booking>> GetAllAsync() => _bookings.GetAllAsync();
    public Task<Booking?> GetByIdAsync(int id) => _bookings.GetByIdAsync(id);
    public Task<List<Booking>> SearchAsync(string? subject, int? courseId, DateTime? from, DateTime? until) =>
        _bookings.SearchAsync(subject, courseId, from, until);

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime start, DateTime end, int? excludeBookingId = null) =>
        !await _bookings.HasConflictAsync(roomId, start, end, excludeBookingId);

    public Task<List<Room>> GetAvailableRoomsAsync(DateTime start, DateTime end, int? floor, bool needsProjector) =>
        _bookings.FindAvailableRoomsAsync(start, end, floor, needsProjector);

    public async Task<bool> CreateAsync(Booking booking, IEnumerable<int>? courseIds = null)
    {
        if (!BookingRules.HasValidWindow(booking.StartTime, booking.EndTime))
        {
            _logger.LogWarning("Rejected booking with invalid time window for room {RoomId}", booking.RoomId);
            return false;
        }
        if (!await IsRoomAvailableAsync(booking.RoomId, booking.StartTime, booking.EndTime))
        {
            _logger.LogWarning("Booking conflict for room {RoomId} from {StartTime} to {EndTime}",
                booking.RoomId, booking.StartTime, booking.EndTime);
            return false;
        }
        booking.Courses = await _bookings.GetCoursesByIdsAsync(courseIds ?? []);
        await _bookings.AddAsync(booking);
        await _bookings.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} created for room {RoomId}", booking.Id, booking.RoomId);
        return true;
    }

    public async Task<bool> UpdateAsync(Booking booking, IEnumerable<int>? courseIds = null)
    {
        if (!BookingRules.HasValidWindow(booking.StartTime, booking.EndTime))
        {
            _logger.LogWarning("Rejected update for booking {BookingId} with an invalid time window", booking.Id);
            return false;
        }
        if (!await IsRoomAvailableAsync(booking.RoomId, booking.StartTime, booking.EndTime, booking.Id))
        {
            _logger.LogWarning("Booking {BookingId} update conflicts in room {RoomId}", booking.Id, booking.RoomId);
            return false;
        }
        var existing = await _bookings.GetByIdAsync(booking.Id);
        if (existing is null)
        {
            _logger.LogWarning("Could not update missing booking {BookingId}", booking.Id);
            return false;
        }

        existing.RoomId = booking.RoomId;
        existing.CreatorStudentId = booking.CreatorStudentId;
        existing.Subject = booking.Subject;
        existing.Description = booking.Description;
        existing.StartTime = booking.StartTime;
        existing.EndTime = booking.EndTime;
        existing.OwnerUserId = booking.OwnerUserId;
        existing.Courses = await _bookings.GetCoursesByIdsAsync(courseIds ?? []);
        await _bookings.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} updated for room {RoomId}", booking.Id, booking.RoomId);
        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await _bookings.GetByIdAsync(id);
        if (booking == null) return;
        _bookings.Remove(booking);
        await _bookings.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} deleted", id);
    }
}
