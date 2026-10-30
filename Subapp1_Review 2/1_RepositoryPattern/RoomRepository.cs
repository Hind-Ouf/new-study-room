using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;

namespace StudyRoomBooking.DAL;

public interface IRoomRepository : IRepository<Room> { }

public sealed class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _db;
    public RoomRepository(ApplicationDbContext db) => _db = db;
    public Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    public Task<List<Room>> GetAllAsync(CancellationToken cancellationToken = default) => _db.Rooms.OrderBy(r => r.Name).ToListAsync(cancellationToken);
    public async Task AddAsync(Room entity, CancellationToken cancellationToken = default) => await _db.Rooms.AddAsync(entity, cancellationToken);
    public void Update(Room entity) => _db.Rooms.Update(entity);
    public void Remove(Room entity) => _db.Rooms.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _db.SaveChangesAsync(cancellationToken);
}
