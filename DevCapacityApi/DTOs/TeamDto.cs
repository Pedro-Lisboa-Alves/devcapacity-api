using System.Collections.Generic;

namespace DevCapacityApi.DTOs;

public class TeamDto
{
    public int TeamId { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentTeam { get; set; }
    public IEnumerable<int> EngineerIds { get; set; } = new List<int>();
}