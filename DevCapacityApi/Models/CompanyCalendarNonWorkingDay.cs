namespace DevCapacityApi.Models;

public class CompanyCalendarNonWorkingDay
{
    public int Id { get; set; }

    public int CompanyCalendarId { get; set; }
    public CompanyCalendar? CompanyCalendar { get; set; }

    // stored as enum DayOfWeek (int in DB)
    public DayOfWeek Day { get; set; }
}