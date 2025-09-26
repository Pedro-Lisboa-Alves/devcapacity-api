namespace DevCapacityApi.DTOs;

public class CompanyCalendarDto
{
    public int CompanyCalendarId { get; set; }
    public IEnumerable<System.DayOfWeek> NonWorkingDays { get; set; } = Array.Empty<System.DayOfWeek>();
}