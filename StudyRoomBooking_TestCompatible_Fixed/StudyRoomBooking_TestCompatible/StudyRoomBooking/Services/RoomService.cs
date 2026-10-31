using StudyRoomBooking.DAL;
using StudyRoomBooking.Models;
using Microsoft.Extensions.Logging;

namespace StudyRoomBooking.Services;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _rooms;
    private readonly ILogger<RoomService> _logger;

    public RoomService(IRoomRepository rooms, ILogger<RoomService> logger)
    {
        _rooms = rooms;
        _logger = logger;
    }
    public Task<List<Room>> GetAllAsync() => _rooms.GetAllAsync();
    public Task<Room?> GetByIdAsync(int id) => _rooms.GetByIdAsync(id);
    public async Task AddAsync(Room room)
    {
        await _rooms.AddAsync(room);
        await _rooms.SaveChangesAsync();
        _logger.LogInformation("Room {RoomId} '{RoomName}' created", room.Id, room.Name);
    }

    public async Task UpdateAsync(Room room)
    {
        _rooms.Update(room);
        await _rooms.SaveChangesAsync();
        _logger.LogInformation("Room {RoomId} updated", room.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _rooms.GetByIdAsync(id);
        if (room == null) return;
        _rooms.Remove(room);
        await _rooms.SaveChangesAsync();
        _logger.LogInformation("Room {RoomId} deleted", id);
    }
}
