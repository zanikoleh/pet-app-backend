using Microsoft.EntityFrameworkCore;
using UserProfileService.Domain.Aggregates;
using SharedKernel.Infrastructure;

namespace UserProfileService.Infrastructure.Persistence;

/// <summary>
/// Database context for User Profile Service.
/// </summary>
public class UserProfileServiceDbContext : ApplicationDbContextBase
{
    public UserProfileServiceDbContext(DbContextOptions<UserProfileServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
    }
}
