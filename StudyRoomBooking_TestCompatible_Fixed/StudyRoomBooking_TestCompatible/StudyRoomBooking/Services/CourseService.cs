using StudyRoomBooking.DAL;
using StudyRoomBooking.Models;
using Microsoft.Extensions.Logging;

namespace StudyRoomBooking.Services;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courses, ILogger<CourseService> logger)
    {
        _courses = courses;
        _logger = logger;
    }

    public Task<List<Course>> GetAllAsync() => _courses.GetAllAsync();
    public Task<Course?> GetByIdAsync(int id) => _courses.GetByIdAsync(id);

    public async Task AddAsync(Course course)
    {
        Normalize(course);
        await _courses.AddAsync(course);
        await _courses.SaveChangesAsync();
        _logger.LogInformation("Course {CourseCode} created", course.Code);
    }

    public async Task UpdateAsync(Course course)
    {
        Normalize(course);
        _courses.Update(course);
        await _courses.SaveChangesAsync();
        _logger.LogInformation("Course {CourseId} updated", course.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null) return;
        _courses.Remove(course);
        await _courses.SaveChangesAsync();
        _logger.LogInformation("Course {CourseId} deleted", id);
    }

    private static void Normalize(Course course)
    {
        course.Code = course.Code.Trim().ToUpperInvariant();
        course.Name = course.Name.Trim();
    }
}
