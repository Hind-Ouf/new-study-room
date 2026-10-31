using StudyRoomBooking.Models;

namespace StudyRoomBooking.Services;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(int id);
}
