namespace DevCapacityApi.Models;

public enum EngineerCalendarDayType
{
    Available,
    Vacations,
    Weekends,
    Absence,
    Assigned
}

public class EngineerCalendarDay
{
    public int Id { get; set; }

    public int EngineerCalendarId { get; set; }
    public EngineerCalendar? EngineerCalendar { get; set; }

    public DateTime Date { get; set; }

    public EngineerCalendarDayType Type { get; set; }

    // novo: referencia opcional para a assignment (persistida)
    // só deve ser preenchida quando Type == Assigned
    public int? AssignmentId { get; set; }
}
