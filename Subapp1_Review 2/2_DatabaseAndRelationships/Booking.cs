namespace StudyRoomBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public string CreatorStudentId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? OwnerUserId { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
