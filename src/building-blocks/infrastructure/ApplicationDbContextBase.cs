using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

/// <summary>
/// Base class for application DbContexts implementing DDD patterns.
/// Handles domain event publishing and custom audit trails.
/// </summary>
public abstract class ApplicationDbContextBase : DbContext
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ApplicationDbContextBase> _logger;

    protected ApplicationDbContextBase(
        DbContextOptions options,
        IEventPublisher eventPublisher,
        ILogger<ApplicationDbContextBase> logger)
        : base(options)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Override OnModelCreating to configure entity mappings.
    /// Implement in derived classes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Configure soft delete for entities
        ApplySoftDeleteFilter(modelBuilder);
    }

    /// <summary>
    /// Applies global soft delete filters to entities.
    /// </summary>
    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // For soft delete support, implement ISoftDeletable interface
        // and configure query filters here
    }

    /// <summary>
    /// Saves changes and publishes any domain events raised by aggregates.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Collect domain events from all aggregate roots
            var domainEvents = new List<DomainEvent>();

            foreach (var entry in ChangeTracker.Entries<AggregateRoot<int>>().Union(
                ChangeTracker.Entries<AggregateRoot<Guid>>()))
            {
                var aggregateRoot = entry.Entity;
                domainEvents.AddRange(aggregateRoot.DomainEvents);
            }

            // Save changes to database
            var result = await base.SaveChangesAsync(cancellationToken);

            // Publish domain events
            if (domainEvents.Any())
            {
                await _eventPublisher.PublishAsync(domainEvents, cancellationToken);

                // Clear events after publishing
                foreach (var entry in ChangeTracker.Entries<AggregateRoot<int>>().Union(
                    ChangeTracker.Entries<AggregateRoot<Guid>>()))
                {
                    entry.Entity.ClearDomainEvents();
                }

                _logger.LogInformation(
                    "Published {EventCount} domain events",
                    domainEvents.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Saves changes synchronously and publishes domain events.
    /// </summary>
    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }
}
