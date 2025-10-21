namespace DevCapacityApi.DTOs;

public class EngineerDto
{
    public int EngineerId { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public int DailyCapacity { get; set; }

    // added TeamId to transport team association
    public int? TeamId { get; set; }

    // calendar (returned on GET). Nullable when engineer has no calendar.
    public EngineerCalendarDto? EngineerCalendar { get; set; }
}