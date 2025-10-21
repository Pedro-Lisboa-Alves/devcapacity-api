namespace DevCapacityApi.DTOs;

public class EngineerCalendarDto
{
    public int EngineerCalendarId { get; set; }
    public int EngineerId { get; set; }
    public IEnumerable<EngineerCalendarDayDto> Days { get; set; } = Array.Empty<EngineerCalendarDayDto>();
}