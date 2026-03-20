namespace SharedKernel;

/// <summary>
/// Non-generic base class for all entities.
/// Provides access to domain events.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Collection of domain events that occurred on this entity.
    /// These should be published after the entity is persisted.
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets all domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// The event will be published after persistence.
    /// </summary>
    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from the collection.
    /// </summary>
    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Call this after publishing events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Base class for all entities in the domain.
/// Entities have identity and are mutable.
/// </summary>
public abstract class Entity<TId> : Entity, IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// The unique identifier for this entity.
    /// </summary>
    public TId Id { get; protected set; }

    protected Entity()
    {
        Id = default!;
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Determines equality based on entity ID.
    /// Two entities are equal if they have the same ID.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
