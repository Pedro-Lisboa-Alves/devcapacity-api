namespace DevCapacityApi.DTOs;

public class CreateUpdateTeamDto
{
    public string Name { get; set; } = null!;
    public int? ParentTeam { get; set; }
}