namespace StudyRoomBooking.Domain;

public static class BookingRules
{
    public static bool HasValidWindow(DateTime start, DateTime end) =>
        end > start && end - start <= TimeSpan.FromHours(4);

    public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB) =>
        startA < endB && endA > startB;

    public static bool StartsNearOrAfterNow(DateTime start, DateTime now) =>
        start >= now.AddMinutes(-5);
}
