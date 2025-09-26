namespace DevCapacityApi.DTOs;

public class CreateUpdateEngineerCalendarDto
{
    public int EngineerId { get; set; }
    public IEnumerable<DateTime> Vacations { get; set; } = Array.Empty<DateTime>();
}