namespace DevCapacityApi.DTOs;

public class CreateUpdateTasksDto
{
    public string Name { get; set; } = null!;
    public int Initiative { get; set; }
    public int Status { get; set; }
    public int PDs { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}