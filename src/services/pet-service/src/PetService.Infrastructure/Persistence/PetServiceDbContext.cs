using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetService.Domain.Aggregates;
using SharedKernel.Infrastructure;
using SharedKernel.Infrastructure.EventBus;

namespace PetService.Infrastructure.Persistence;

/// <summary>
/// DbContext for Pet Service.
/// </summary>
public sealed class PetServiceDbContext : ApplicationDbContextBase
{
    public DbSet<Pet> Pets => Set<Pet>();

    public PetServiceDbContext(
        DbContextOptions<PetServiceDbContext> options,
        IEventPublisher eventPublisher,
        ILogger<PetServiceDbContext> logger)
        : base(options, eventPublisher, logger)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all entities
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetServiceDbContext).Assembly);
    }
}
