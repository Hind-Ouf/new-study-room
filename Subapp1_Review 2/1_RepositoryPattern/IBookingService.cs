using StudyRoomBooking.Models;

namespace StudyRoomBooking.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<List<Booking>> SearchAsync(string? subject, int? courseId, DateTime? from, DateTime? until);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime start, DateTime end, int? excludeBookingId = null);
        Task<List<Room>> GetAvailableRoomsAsync(DateTime start, DateTime end, int? floor, bool needsProjector);
        Task<bool> CreateAsync(Booking booking, IEnumerable<int>? courseIds = null);
        Task<bool> UpdateAsync(Booking booking, IEnumerable<int>? courseIds = null);
        Task DeleteAsync(int id);
    }
}
