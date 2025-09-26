namespace DevCapacityApi.DTOs;

public class EngineerAssignmentDto
{
    public int AssignmentId { get; set; }
    public int EngineerId { get; set; }
    public int TaskId { get; set; }
    public int CapacityShare { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}