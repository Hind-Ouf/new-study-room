using System.ComponentModel.DataAnnotations;

namespace StudyRoomBooking.Models;

public sealed class Course
{
    public int Id { get; set; }

    [Required, StringLength(20, MinimumLength = 2)]
    [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "Use letters, numbers, or hyphens.")]
    [Display(Name = "Course code")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 3)]
    [Display(Name = "Course name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
