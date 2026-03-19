namespace SharedKernel;

/// <summary>
/// Base class for all domain events in the system.
/// Domain events represent something that happened in the domain and are used for cross-aggregate communication.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// The aggregate root ID that raised this event.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing related events across services.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Causation ID to track what command or event caused this event.
    /// </summary>
    public Guid? CausationId { get; set; }

    /// <summary>
    /// Version of the aggregate when this event was raised.
    /// Used for optimistic concurrency control.
    /// </summary>
    public int AggregateVersion { get; set; }
}