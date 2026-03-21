using System.Collections.Concurrent;

namespace SharedKernel.Infrastructure.EventBus;

/// <summary>
/// In-memory event bus implementation for local event publishing and subscription.
/// For production, consider using RabbitMQ, Azure Service Bus, or similar.
/// </summary>
public class InMemoryEventBus : IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<string, List<Delegate>> _subscribers = new();

    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) 
        where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            var tasks = handlers.OfType<Func<TEvent, CancellationToken, Task>>()
                .Select(handler => handler(domainEvent, cancellationToken));
            
            return Task.WhenAll(tasks);
        }

        return Task.CompletedTask;
    }

    public Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
        
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            var method = typeof(InMemoryEventBus)
                .GetMethod(nameof(PublishAsync), new[] { domainEvent.GetType(), typeof(CancellationToken) });
            
            if (method != null)
            {
                return method.Invoke(this, new object[] { domainEvent, cancellationToken }) as Task ?? Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }

    public string Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> eventHandler) 
        where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        var subscriptionId = Guid.NewGuid().ToString();
        
        _subscribers.AddOrUpdate(eventType,
            new List<Delegate> { eventHandler },
            (_, handlers) =>
            {
                handlers.Add(eventHandler);
                return handlers;
            });

        return subscriptionId;
    }

    public void Unsubscribe<TEvent>(string subscriptionId) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            // Note: This implementation doesn't track subscription IDs per handler
            // For production, implement a more sophisticated tracking system
        }
    }
}