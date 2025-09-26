namespace DevCapacityApi.DTOs;

public class InitiativesDto
{
    public int InitiativeId { get; set; }
    public string? Name { get; set; }
    public int? ParentInitiative { get; set; }
    public int Status { get; set; }
    public int PDs { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<int> TaskIds { get; set; } = Array.Empty<int>();
}