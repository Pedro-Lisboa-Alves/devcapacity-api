using System.Collections.Generic;

namespace DevCapacityApi.Models;

public class Team
{
    public int TeamId { get; set; }
    public string Name { get; set; } = null!;

    // nullable FK to parent team
    public int? ParentTeam { get; set; }
    public Team? Parent { get; set; }

    // children teams
    public ICollection<Team> Children { get; set; } = new List<Team>();

    // engineers in this team
    public ICollection<Engineer> Engineers { get; set; } = new List<Engineer>();
}