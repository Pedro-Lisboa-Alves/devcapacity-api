namespace DevCapacityApi.Models;

public class Engineer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Role { get; set; }
    public int DailyCapacity { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // added assignments navigation
    public ICollection<EngineerAssignment> Assignments { get; set; } = new List<EngineerAssignment>();
}