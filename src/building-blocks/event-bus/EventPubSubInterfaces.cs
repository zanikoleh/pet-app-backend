namespace SharedKernel.Infrastructure.EventBus;

/// <summary>
/// Interface for publishing domain events.
/// Implementations handle routing events to appropriate subscribers and external systems.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event to publish.</typeparam>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) 
        where TEvent : DomainEvent;

    /// <summary>
    /// Publishes a domain event asynchronously (non-generic overload).
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for subscribing to domain events.
/// Implementations register event handlers for specific event types.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes a handler to a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="eventHandler">The async handler function to execute when the event is published.</param>
    /// <returns>A subscription ID that can be used to unsubscribe later.</returns>
    string Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> eventHandler) 
        where TEvent : DomainEvent;

    /// <summary>
    /// Unsubscribes a previously subscribed handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="subscriptionId">The subscription ID returned from Subscribe.</param>
    void Unsubscribe<TEvent>(string subscriptionId) where TEvent : DomainEvent;
}
