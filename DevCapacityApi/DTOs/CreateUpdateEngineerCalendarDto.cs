namespace DevCapacityApi.DTOs;

public class CreateUpdateEngineerCalendarDayDto
{
    public DateTime Date { get; set; }
    public string? Type { get; set; } // "Available" | "Vacations" | "Absence" | "Assigned"
    // note: clients should not set AssignmentId; server manages it
}

public class CreateUpdateEngineerCalendarDto
{
    public int EngineerId { get; set; }
    public IEnumerable<CreateUpdateEngineerCalendarDayDto> Days { get; set; } = Array.Empty<CreateUpdateEngineerCalendarDayDto>();
}