namespace SharedKernel;

/// <summary>
/// Base class for all domain events in the system.
/// Domain events represent something significant that happened in the domain.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for the event instance.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing related events across services.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional metadata for the event (e.g., user context, request ID).
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Marker interface for MediatR notification integration.
/// Domain events are published as notifications.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string? CorrelationId { get; set; }
}

/// <summary>
/// Base class for domain events that also implement IDomainEvent for MediatR.
/// </summary>
public abstract class DomainEventNotification : DomainEvent, IDomainEvent
{
}
