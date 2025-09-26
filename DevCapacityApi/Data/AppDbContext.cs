using Microsoft.EntityFrameworkCore;
using DevCapacityApi.Models;

namespace DevCapacityApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;

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
    }
}