namespace DevCapacityApi.DTOs;

public class TasksDto
{
    public int TaskId { get; set; }
    public string? Name { get; set; }
    public int Initiative { get; set; }
    public int Status { get; set; }
    public int PDs { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // assignment ids
    public IEnumerable<int> AssignmentIds { get; set; } = Array.Empty<int>();

    // calculated field returned on GETs only
    public int UnassignedPDs { get; set; }
}