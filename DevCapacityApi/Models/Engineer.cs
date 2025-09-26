namespace DevCapacityApi.Models;

public class Engineer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Role { get; set; }
    public int DailyCapacity { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<EngineerAssignment> Assignments { get; set; } = new List<EngineerAssignment>();

    // optional navigation: an engineer can have a calendar
    public EngineerCalendar? Calendar { get; set; }
}