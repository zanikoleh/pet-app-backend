using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Infrastructure.EventBus;

namespace SharedKernel.Infrastructure;

/// <summary>
/// Base implementation of DbContext that automatically publishes domain events after saving changes.
/// Services should extend this class for their DbContext implementations.
/// </summary>
public abstract class ApplicationDbContextBase : DbContext
{
    private readonly IEventPublisher? _eventPublisher;
    private readonly ILogger<ApplicationDbContextBase>? _logger;

    protected ApplicationDbContextBase(
        DbContextOptions options,
        IEventPublisher? eventPublisher = null,
        ILogger<ApplicationDbContextBase>? logger = null) 
        : base(options)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Saves changes and automatically publishes any domain events raised by aggregates.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Publish domain events if event publisher is available
        if (_eventPublisher != null)
        {
            await PublishDomainEventsAsync(cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Synchronous version of SaveChanges that also publishes domain events.
    /// </summary>
    public override int SaveChanges()
    {
        var result = base.SaveChanges();
        
        // Note: Publishing events synchronously is not ideal, but kept for compatibility
        if (_eventPublisher != null)
        {
            PublishDomainEventsAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    /// <summary>
    /// Publishes all domain events from tracked entities that inherit from Entity.
    /// </summary>
    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before publishing to avoid republishing
        entities.ForEach(a => a.ClearDomainEvents());

        // Publish all events
        foreach (var domainEvent in domainEvents)
        {
            _logger?.LogInformation(
                "Publishing domain event: {EventType}",
                domainEvent.GetType().Name);
            
            await _eventPublisher!.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
