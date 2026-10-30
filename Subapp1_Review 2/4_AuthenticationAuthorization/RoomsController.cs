using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;
using StudyRoomBooking.Services;

namespace StudyRoomBooking.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly IRoomService _rooms;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IRoomService rooms, ILogger<RoomsController> logger)
        {
            _rooms = rooms;
            _logger = logger;
        }

        public async Task<IActionResult> Index() => View(await _rooms.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Json(new { room.Name, room.Floor, room.Capacity, room.HasProjector });
        }

        [HttpGet]
        public IActionResult Create() => View(new Room { Floor = 1, Capacity = 4 });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            room.Id = 0;
            if (!ModelState.IsValid) return View(room);
            try
            {
                await _rooms.AddAsync(room);
                TempData["Success"] = $"Room {room.Name} was created.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not create room {Name}", room.Name);
                ModelState.AddModelError("", "The room could not be saved. Please try again.");
                return View(room);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room == null ? NotFound() : View(room);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return BadRequest();
            if (!ModelState.IsValid) return View(room);
            try
            {
                await _rooms.UpdateAsync(room);
                TempData["Success"] = $"Room {room.Name} was updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not update room {RoomId}", id);
                ModelState.AddModelError("", "The room could not be updated. It may have been deleted.");
                return View(room);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room == null ? NotFound() : View(room);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _rooms.DeleteAsync(id);
                TempData["Success"] = "Room deleted (and its bookings).";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not delete room {RoomId}", id);
                TempData["Error"] = "The room could not be deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
