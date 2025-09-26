namespace DevCapacityApi.DTOs;

public class EngineerCalendarDto
{
    public int EngineerCalendarId { get; set; }
    public int EngineerId { get; set; }
    public IEnumerable<DateTime> Vacations { get; set; } = Array.Empty<DateTime>();
}