namespace DevCapacityApi.Models;

public class EngineerCalendar
{
    public int EngineerCalendarId { get; set; }

    // FK to Engineer
    public int EngineerId { get; set; }
    public Engineer? Engineer { get; set; }

    // vacation dates
    public ICollection<EngineerCalendarDay> Vacations { get; set; } = new List<EngineerCalendarDay>();

    // método pedido
    public bool IsVacation(DateTime date)
    {
        var d = date.Date;
        return Vacations.Any(v => v.Date.Date == d);
    }
}