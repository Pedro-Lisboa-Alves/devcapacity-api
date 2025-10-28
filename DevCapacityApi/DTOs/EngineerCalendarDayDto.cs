namespace DevCapacityApi.DTOs;

public class EngineerCalendarDayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    // "Available" | "Vacations" | "Absence" | "Assigned"
    public string? Type { get; set; }

    // new: assignment id if the day is assigned (nullable)
    public int? AssignmentId { get; set; }
}