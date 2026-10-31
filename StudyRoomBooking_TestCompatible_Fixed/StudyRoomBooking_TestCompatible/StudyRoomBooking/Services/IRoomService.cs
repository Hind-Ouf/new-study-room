using StudyRoomBooking.Models;

namespace StudyRoomBooking.Services
{
    public interface IRoomService
    {
        Task<List<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(int id);
    }
}
