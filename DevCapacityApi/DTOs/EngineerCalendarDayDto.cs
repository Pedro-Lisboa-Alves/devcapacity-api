namespace DevCapacityApi.DTOs;

public class EngineerCalendarDayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    // "Available" | "Vacations" | "Absence"
    public string? Type { get; set; }
}