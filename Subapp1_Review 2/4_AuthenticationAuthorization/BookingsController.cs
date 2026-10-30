using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;
using StudyRoomBooking.Domain;
using StudyRoomBooking.Services;
using StudyRoomBooking.ViewModels;

namespace StudyRoomBooking.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private const string ConflictMessage =
            "This room is already booked in that time slot. Please choose another time or room.";

        private readonly IBookingService _bookings;
        private readonly IRoomService _rooms;
        private readonly ICourseService _courses;
        private readonly UserManager<ApplicationUser> _users;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookings, IRoomService rooms, ICourseService courses,
            UserManager<ApplicationUser> users, ILogger<BookingsController> logger)
        {
            _bookings = bookings;
            _rooms = rooms;
            _courses = courses;
            _users = users;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? subject, int? courseId, DateTime? from, DateTime? until)
        {
            var courses = await _courses.GetAllAsync();
            var results = await _bookings.SearchAsync(subject, courseId, from, until);
            return View(new BookingSearchViewModel
            {
                Subject = subject, CourseId = courseId, From = from, Until = until,
                Courses = courses, Results = results
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? roomId, DateTime? start, DateTime? end)
        {
            await LoadRoomsAsync(roomId);
            await LoadCoursesAsync();
            var t = DateTime.Now.AddHours(1);
            var defaultStart = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
            return View(new BookingFormViewModel
            {
                RoomId = roomId ?? 0,
                CreatorStudentId = (await _users.GetUserAsync(User))?.StudentNumber ?? string.Empty,
                StartTime = start ?? defaultStart,
                EndTime = end ?? defaultStart.AddHours(2)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingFormViewModel vm)
        {
            vm.Id = 0;
            if (!BookingRules.StartsNearOrAfterNow(vm.StartTime, DateTime.Now))
                ModelState.AddModelError(nameof(vm.StartTime), "Start time cannot be in the past.");

            if (!ModelState.IsValid) return await ReturnFormAsync(vm);

            try
            {
                var booking = vm.ToBooking();
                booking.OwnerUserId = _users.GetUserId(User);
                booking.CreatorStudentId = (await _users.GetUserAsync(User))?.StudentNumber ?? string.Empty;
                if (!await _bookings.CreateAsync(booking, vm.CourseIds))
                {
                    ModelState.AddModelError("", ConflictMessage);
                    return await ReturnFormAsync(vm);
                }
                TempData["Success"] = "Your booking was created.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not save booking for room {RoomId}", vm.RoomId);
                ModelState.AddModelError("", "The booking could not be saved. Please check the room and try again.");
                return await ReturnFormAsync(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookings.GetByIdAsync(id);
            if (booking == null) return NotFound();
            await LoadRoomsAsync(booking.RoomId);
            await LoadCoursesAsync(booking.Courses.Select(c => c.Id));
            return View(BookingFormViewModel.FromBooking(booking));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            var existing = await _bookings.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.OwnerUserId != _users.GetUserId(User)) return Forbid();
            if (!ModelState.IsValid) return await ReturnFormAsync(vm);

            try
            {
                var booking = vm.ToBooking();
                booking.OwnerUserId = existing.OwnerUserId;
                booking.CreatorStudentId = existing.CreatorStudentId;
                if (!await _bookings.UpdateAsync(booking, vm.CourseIds))
                {
                    _logger.LogWarning("Booking {Id} update conflict in room {RoomId}", id, vm.RoomId);
                    ModelState.AddModelError("", ConflictMessage);
                    return await ReturnFormAsync(vm);
                }
                TempData["Success"] = "Booking updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not update booking {Id}", id);
                ModelState.AddModelError("", "The booking could not be updated. It may have been deleted.");
                return await ReturnFormAsync(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookings.GetByIdAsync(id);
            if (booking == null) return NotFound();
            if (booking.OwnerUserId != _users.GetUserId(User)) return Forbid();
            return View(booking);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var booking = await _bookings.GetByIdAsync(id);
                if (booking == null) return NotFound();
                if (booking.OwnerUserId != _users.GetUserId(User)) return Forbid();
                await _bookings.DeleteAsync(id);
                TempData["Success"] = "Booking deleted.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Could not delete booking {Id}", id);
                TempData["Error"] = "The booking could not be deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ReturnFormAsync(BookingFormViewModel vm)
        {
            await LoadRoomsAsync(vm.RoomId);
            await LoadCoursesAsync(vm.CourseIds);
            return View(vm.Id == 0 ? "Create" : "Edit", vm);
        }

        private async Task LoadCoursesAsync(IEnumerable<int>? selectedIds = null)
        {
            var courses = await _courses.GetAllAsync();
            ViewBag.Courses = new MultiSelectList(courses.Select(c => new { c.Id, Label = $"{c.Code} — {c.Name}" }),
                "Id", "Label", selectedIds);
        }

        private async Task LoadRoomsAsync(int? selectedId)
        {
            var rooms = await _rooms.GetAllAsync();
            ViewBag.Rooms = new SelectList(
                rooms.Select(r => new { r.Id, Label = $"{r.Name} (floor {r.Floor}, {r.Capacity} seats)" }),
                "Id", "Label", selectedId);
        }
    }
}
