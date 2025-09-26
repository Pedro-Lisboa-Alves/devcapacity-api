namespace DevCapacityApi.DTOs;

public class CreateUpdateCompanyCalendarDto
{
    public IEnumerable<System.DayOfWeek> NonWorkingDays { get; set; } = Array.Empty<System.DayOfWeek>();
}