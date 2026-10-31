using Microsoft.AspNetCore.Mvc;
using StudyRoomBooking.Services;
using StudyRoomBooking.ViewModels;

namespace StudyRoomBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBookingService _bookings;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IBookingService bookings, ILogger<HomeController> logger)
        {
            _bookings = bookings;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(RoomSearchViewModel search)
        {
            if (search.Searched)
            {
                if (search.EndTime <= search.StartTime)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }
                else
                {
                    search.Results = await _bookings.GetAvailableRoomsAsync(
                        search.StartTime, search.EndTime, search.Floor, search.NeedsProjector);
                    _logger.LogInformation("Room search {Start}-{End} returned {Count} rooms",
                        search.StartTime, search.EndTime, search.Results.Count);
                }
            }
            return View(search);
        }

        public IActionResult Error(int? code)
        {
            ViewBag.Code = code;
            return View();
        }
    }
}
