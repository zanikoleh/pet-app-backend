namespace SharedKernel;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Aggregate roots are the entry points to aggregates and enforce invariants.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Version for optimistic concurrency control.
    /// Increment on each modification to prevent lost updates.
    /// </summary>
    public int Version { get; protected set; }

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id)
    {
        Version = 0;
    }

    /// <summary>
    /// Increments the version for optimistic concurrency.
    /// Call this in event handlers after state changes.
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }

    /// <summary>
    /// Raises a domain event and adds it to the aggregate's event collection.
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
        IncrementVersion();
    }
}
