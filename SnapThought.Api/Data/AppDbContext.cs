using Microsoft.EntityFrameworkCore;
using SnapThought.Api.Models;

namespace SnapThought.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Thought> Thoughts => Set<Thought>();

    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Many-to-many via an EF-managed implicit join table.
        modelBuilder.Entity<Thought>()
            .HasMany(t => t.Tags)
            .WithMany(t => t.Thoughts);
    }
}
