using System.ComponentModel.DataAnnotations;

namespace StudyRoomBooking.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a room name.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Room name must be 2-50 characters.")]
        [Display(Name = "Room name")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 10, ErrorMessage = "Floor must be between 0 and 10.")]
        public int Floor { get; set; }

        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        [Display(Name = "Has projector")]
        public bool HasProjector { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
