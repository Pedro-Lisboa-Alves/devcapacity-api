namespace DevCapacityApi.Models;

public class Initiatives
{
    public int InitiativeId { get; set; }
    public string Name { get; set; } = null!;

    // nullable FK to parent initiative
    public int? ParentInitiative { get; set; }
    public Initiatives? Parent { get; set; }

    // children initiatives
    public ICollection<Initiatives> Children { get; set; } = new List<Initiatives>();

    // tasks under this initiative
    public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

    // status FK (StatusId)
    public int Status { get; set; }

    // PDs metric
    public int PDs { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}