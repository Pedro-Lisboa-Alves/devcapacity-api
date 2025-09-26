namespace DevCapacityApi.Models;

public class Tasks
{
    public int TaskId { get; set; }
    public string Name { get; set; } = null!;

    // reference to Initiative (external entity id)
    public int Initiative { get; set; }

    // status FK (references Status.StatusId)
    public int Status { get; set; }

    // PDs (whatever metric you use)
    public int PDs { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // assignments for this task
    public ICollection<EngineerAssignment> Assignments { get; set; } = new List<EngineerAssignment>();
}