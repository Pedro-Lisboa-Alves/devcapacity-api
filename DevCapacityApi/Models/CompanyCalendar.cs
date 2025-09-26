namespace DevCapacityApi.Models;

public class CompanyCalendar
{
    public int CompanyCalendarId { get; set; }

    // weekly non-working days (stored via CompanyCalendarNonWorkingDay)
    public ICollection<CompanyCalendarNonWorkingDay> NonWorkingDays { get; set; } = new List<CompanyCalendarNonWorkingDay>();

    // Método conforme pedido
    public bool IsCompanyWorkingDay(DateTime date)
    {
        var dow = date.DayOfWeek;
        return !NonWorkingDays.Any(n => n.Day == dow);
    }
}