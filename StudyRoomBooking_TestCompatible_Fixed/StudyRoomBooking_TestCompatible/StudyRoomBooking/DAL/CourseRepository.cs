using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;

namespace StudyRoomBooking.DAL;

public interface ICourseRepository : IRepository<Course> { }

public sealed class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _db;
    public CourseRepository(ApplicationDbContext db) => _db = db;
    public Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    public Task<List<Course>> GetAllAsync(CancellationToken cancellationToken = default) => _db.Courses.OrderBy(c => c.Code).ToListAsync(cancellationToken);
    public async Task AddAsync(Course entity, CancellationToken cancellationToken = default) => await _db.Courses.AddAsync(entity, cancellationToken);
    public void Update(Course entity) => _db.Courses.Update(entity);
    public void Remove(Course entity) => _db.Courses.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _db.SaveChangesAsync(cancellationToken);
}
