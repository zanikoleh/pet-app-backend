using System.Text.Json;
using MediatR;

namespace SharedKernel.Infrastructure.EventBus;

/// <summary>
/// Wrapper for integration events from external services.
/// Used with MediatR INotificationHandler for pub/sub event handling.
/// </summary>
public class IntegrationEventNotification : INotification
{
    /// <summary>
    /// The type of integration event (e.g., "user.registered", "pet.created")
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// The event data as a dictionary for flexible property access
    /// </summary>
    public Dictionary<string, JsonElement> Data { get; set; }

    public IntegrationEventNotification(string eventType, Dictionary<string, JsonElement> data)
    {
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}
