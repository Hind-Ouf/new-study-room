using System.ComponentModel.DataAnnotations;
using StudyRoomBooking.Models;
using StudyRoomBooking.Domain;

namespace StudyRoomBooking.ViewModels
{
    public class BookingFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a study room.")]
        [Display(Name = "Study room")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        [StringLength(50)]
        [Display(Name = "Student ID")]
        public string CreatorStudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a subject.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Subject must be 3-100 characters.")]
        public string Subject { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "Start time")]
        public DateTime StartTime { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "End time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Courses")]
        public List<int> CourseIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (EndTime <= StartTime)
                yield return new ValidationResult("End time must be after start time.", new[] { nameof(EndTime) });
            else if (!BookingRules.HasValidWindow(StartTime, EndTime))
                yield return new ValidationResult("A session cannot exceed 4 hours.", new[] { nameof(EndTime) });
        }

        public Booking ToBooking() => new()
        {
            Id = Id, RoomId = RoomId, CreatorStudentId = CreatorStudentId.Trim(),
            Subject = Subject.Trim(), Description = Description?.Trim(),
            StartTime = StartTime, EndTime = EndTime
        };

        public static BookingFormViewModel FromBooking(Booking b) => new()
        {
            Id = b.Id, RoomId = b.RoomId, CreatorStudentId = b.CreatorStudentId,
            Subject = b.Subject, Description = b.Description, StartTime = b.StartTime, EndTime = b.EndTime,
            CourseIds = b.Courses.Select(c => c.Id).ToList()
        };
    }
}
