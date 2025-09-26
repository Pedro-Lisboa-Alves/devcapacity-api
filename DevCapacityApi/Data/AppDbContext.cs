using DevCapacityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DevCapacityApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;
    public DbSet<EngineerAssignment> EngineerAssignments { get; set; } = null!;
    public DbSet<Tasks> Tasks { get; set; } = null!;
    public DbSet<Initiatives> Initiatives { get; set; } = null!;
    public DbSet<CompanyCalendar> CompanyCalendars { get; set; } = null!;
    public DbSet<CompanyCalendarNonWorkingDay> CompanyCalendarNonWorkingDays { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // índice único para Name (evita duplicados a nível de BD)
        modelBuilder.Entity<Engineer>()
            .HasIndex(e => e.Name)
            .IsUnique();

        // Team self reference (parent/children)
        modelBuilder.Entity<Team>()
            .HasKey(t => t.TeamId);

        modelBuilder.Entity<Team>()
            .HasMany(t => t.Children)
            .WithOne(t => t.Parent)
            .HasForeignKey(t => t.ParentTeam)
            .OnDelete(DeleteBehavior.Restrict);

        // Team -> Engineers
        modelBuilder.Entity<Team>()
            .HasMany(t => t.Engineers)
            .WithOne(e => e.Team)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Status>().HasKey(s => s.StatusId);

        modelBuilder.Entity<EngineerAssignment>().HasKey(a => a.AssignmentId);

        modelBuilder.Entity<EngineerAssignment>()
            .HasOne(a => a.Engineer)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EngineerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tasks>().HasKey(t => t.TaskId);

        modelBuilder.Entity<Tasks>()
            .HasMany(t => t.Assignments)
            .WithOne(a => a.Task)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Initiatives self reference (parent/children)
        modelBuilder.Entity<Initiatives>().HasKey(i => i.InitiativeId);

        modelBuilder.Entity<Initiatives>()
            .HasMany(i => i.Children)
            .WithOne(i => i.Parent)
            .HasForeignKey(i => i.ParentInitiative)
            .OnDelete(DeleteBehavior.Restrict);

        // Initiatives -> Tasks
        modelBuilder.Entity<Initiatives>()
            .HasMany(i => i.Tasks)
            .WithOne(t => t.InitiativeNav)
            .HasForeignKey(t => t.Initiative)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CompanyCalendar>().HasKey(c => c.CompanyCalendarId);

        modelBuilder.Entity<CompanyCalendarNonWorkingDay>().HasKey(d => d.Id);

        modelBuilder.Entity<CompanyCalendar>()
            .HasMany(c => c.NonWorkingDays)
            .WithOne(d => d.CompanyCalendar)
            .HasForeignKey(d => d.CompanyCalendarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}