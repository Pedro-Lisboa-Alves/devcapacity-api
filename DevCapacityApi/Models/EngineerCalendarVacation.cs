namespace DevCapacityApi.Models;

public class EngineerCalendarVacation
{
    public int Id { get; set; }

    public int EngineerCalendarId { get; set; }
    public EngineerCalendar? EngineerCalendar { get; set; }

    // store date only (use DateTime.Date for comparisons)
    public DateTime Date { get; set; }
}