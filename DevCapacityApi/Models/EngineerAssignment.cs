namespace DevCapacityApi.Models;

public class EngineerAssignment
{
    public int AssignmentId { get; set; }

    // FK to Engineer
    public int EngineerId { get; set; }
    public Engineer? Engineer { get; set; }

    // external task id (no navigation provided)
    public int TaskId { get; set; }

    // percentage/share (or units) of capacity assigned
    public int CapacityShare { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}