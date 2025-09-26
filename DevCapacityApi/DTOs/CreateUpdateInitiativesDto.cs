namespace DevCapacityApi.DTOs;

public class CreateUpdateInitiativesDto
{
    public string Name { get; set; } = null!;
    public int? ParentInitiative { get; set; }
    public int Status { get; set; }
    public int PDs { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}