using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventBus;

/// <summary>
/// Azure Service Bus implementation of the event publisher.
/// Publishes events as JSON messages to Service Bus topics.
/// </summary>
public class AzureServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;

    public AzureServiceBusEventPublisher(
        ServiceBusClient client,
        ILogger<AzureServiceBusEventPublisher> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                var topicName = GetTopicName(domainEvent.GetType());
                var sender = _client.CreateSender(topicName);

                var message = new ServiceBusMessage(
                    JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions { WriteIndented = false }))
                {
                    Subject = domainEvent.GetType().Name,
                    MessageId = domainEvent.EventId.ToString(),
                    CorrelationId = domainEvent.CorrelationId ?? Guid.NewGuid().ToString(),
                    ContentType = "application/json"
                };

                // Add custom properties for routing
                message.ApplicationProperties["EventType"] = domainEvent.GetType().FullName ?? "";
                message.ApplicationProperties["OccurredAt"] = domainEvent.OccurredAt;

                await sender.SendMessageAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Domain event published. EventType: {EventType}, EventId: {EventId}, Topic: {Topic}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId,
                    topicName);

                await sender.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing domain event. EventType: {EventType}, EventId: {EventId}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);
                throw;
            }
        }
    }

    private static string GetTopicName(Type eventType)
    {
        // Topic naming convention: "EventNameTopic"
        // E.g., UserRegisteredEvent -> UserRegisteredEventTopic
        return $"{eventType.Name}Topic".ToLower();
    }
}

/// <summary>
/// Azure Service Bus implementation of the event subscriber.
/// Listens to topics and invokes handlers when messages arrive.
/// </summary>
public class AzureServiceBusEventSubscriber : IEventSubscriber
{
    private readonly ServiceBusClient _client;
    private readonly string _subscriptionName;
    private readonly ILogger<AzureServiceBusEventSubscriber> _logger;
    private readonly Dictionary<string, ServiceBusProcessor> _processors = new();
    private bool _isListening;

    public AzureServiceBusEventSubscriber(
        ServiceBusClient client,
        string subscriptionName,
        ILogger<AzureServiceBusEventSubscriber> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _subscriptionName = subscriptionName ?? throw new ArgumentNullException(nameof(subscriptionName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        try
        {
            var topicName = GetTopicName(typeof(TEvent));
            var processorKey = $"{topicName}-{_subscriptionName}";

            if (_processors.ContainsKey(processorKey))
            {
                _logger.LogWarning("Subscription already exists for topic: {TopicName}", topicName);
                return;
            }

            var processor = _client.CreateProcessor(
                topicName,
                _subscriptionName,
                new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var body = args.Message.Body.ToString();
                    var eventData = JsonSerializer.Deserialize<TEvent>(body);

                    if (eventData != null)
                    {
                        await handler(eventData);
                        await args.CompleteMessageAsync();

                        _logger.LogInformation(
                            "Event processed successfully. EventType: {EventType}, Topic: {Topic}",
                            typeof(TEvent).Name,
                            topicName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing message. Topic: {Topic}, MessageId: {MessageId}",
                        topicName,
                        args.Message.MessageId);

                    // Abandon message to retry later
                    await args.AbandonMessageAsync();
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(
                    args.Exception,
                    "Error in Service Bus processor. Topic: {Topic}",
                    topicName);
                return Task.CompletedTask;
            };

            _processors[processorKey] = processor;

            if (_isListening)
            {
                await processor.StartProcessingAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Subscribed to topic: {TopicName}, Subscription: {SubscriptionName}",
                topicName,
                _subscriptionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error subscribing to event. EventType: {EventType}",
                typeof(TEvent).Name);
            throw;
        }
    }

    public async Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        try
        {
            var topicName = GetTopicName(typeof(TEvent));
            var processorKey = $"{topicName}-{_subscriptionName}";

            if (_processors.TryGetValue(processorKey, out var processor))
            {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
                _processors.Remove(processorKey);

                _logger.LogInformation(
                    "Unsubscribed from topic: {TopicName}",
                    topicName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error unsubscribing from event. EventType: {EventType}",
                typeof(TEvent).Name);
            throw;
        }
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _isListening = true;

            foreach (var processor in _processors.Values)
            {
                await processor.StartProcessingAsync(cancellationToken);
            }

            _logger.LogInformation("Event subscriber started listening");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting event listener");
            _isListening = false;
            throw;
        }
    }

    public async Task StopListeningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _isListening = false;

            foreach (var processor in _processors.Values)
            {
                await processor.StopProcessingAsync(cancellationToken);
            }

            _logger.LogInformation("Event subscriber stopped listening");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping event listener");
            throw;
        }
    }

    private static string GetTopicName(Type eventType)
    {
        return $"{eventType.Name}Topic".ToLower();
    }
}
