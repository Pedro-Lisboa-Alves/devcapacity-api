namespace DevCapacityApi.Models;

public enum EngineerCalendarDayType
{
    Available,
    Vacations,
    Absence
}

public class EngineerCalendarDay
{
    public int Id { get; set; }

    public int EngineerCalendarId { get; set; }
    public EngineerCalendar? EngineerCalendar { get; set; }

    // store date only (use DateTime.Date for comparisons)
    public DateTime Date { get; set; }

    public EngineerCalendarDayType Type { get; set; }
}
