using StudyRoomBooking.Models;

namespace StudyRoomBooking.ViewModels
{
    public class RoomSearchViewModel
    {
        public DateTime StartTime { get; set; } = NextHour();
        public DateTime EndTime { get; set; } = NextHour().AddHours(2);
        public int? Floor { get; set; }
        public bool NeedsProjector { get; set; }
        public bool Searched { get; set; }
        public List<Room> Results { get; set; } = new();

        private static DateTime NextHour()
        {
            var t = DateTime.Now.AddHours(1);
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
        }
    }
}
