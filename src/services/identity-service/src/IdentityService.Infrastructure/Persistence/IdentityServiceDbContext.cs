using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Aggregates;
using SharedKernel.Infrastructure;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Database context for Identity Service.
/// </summary>
public class IdentityServiceDbContext : ApplicationDbContextBase
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
