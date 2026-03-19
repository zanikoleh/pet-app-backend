namespace EventBus;

/// <summary>
/// Interface for publishing domain events to the event bus.
/// Events are persisted and delivered to all subscribers asynchronously.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event to the event bus.
    /// The event will be delivered to all subscribers.
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple domain events to the event bus.
    /// </summary>
    /// <param name="domainEvents">The domain events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for subscribing to domain events from the event bus.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes a handler to a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The handler function to invoke when the event is published.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Unsubscribes a handler from a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Starts listening for events from the event bus.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartListeningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening for events from the event bus.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StopListeningAsync(CancellationToken cancellationToken = default);
}
