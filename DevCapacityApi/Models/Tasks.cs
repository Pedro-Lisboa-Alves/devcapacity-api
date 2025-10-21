using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DevCapacityApi.Models;

public class Tasks
{
    public int TaskId { get; set; }
    public string Name { get; set; } = null!;

    // reference to Initiative (stores InitiativeId)
    public int Initiative { get; set; }

    // navigation to Initiatives entity
    public Initiatives? InitiativeNav { get; set; }

    // status FK (references Status.StatusId)
    public int Status { get; set; }

    public int PDs { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // assignments for this task
    public ICollection<EngineerAssignment> Assignments { get; set; } = new List<EngineerAssignment>();

    // calculated property: PDs not assigned to engineers.
    // Not mapped to the database; computed from PDs minus sum of assignment shares.
    [NotMapped]
    public int UnassignedPDs => Math.Max(0, PDs - (Assignments?.Sum(a => a.CapacityShare) ?? 0));
}