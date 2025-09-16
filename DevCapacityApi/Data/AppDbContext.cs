using Microsoft.EntityFrameworkCore;
using DevCapacityApi.Models;

namespace DevCapacityApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Engineer> Engineers => Set<Engineer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // índice único para Name (evita duplicados a nível de BD)
        modelBuilder.Entity<Engineer>()
            .HasIndex(e => e.Name)
            .IsUnique();
    }
}