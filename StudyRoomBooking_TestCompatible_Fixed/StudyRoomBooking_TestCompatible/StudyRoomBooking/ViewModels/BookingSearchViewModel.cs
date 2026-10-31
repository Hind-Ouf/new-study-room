using StudyRoomBooking.Models;

namespace StudyRoomBooking.ViewModels;

public sealed class BookingSearchViewModel
{
    public string? Subject { get; set; }
    public int? CourseId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? Until { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<Booking> Results { get; set; } = new();
}
