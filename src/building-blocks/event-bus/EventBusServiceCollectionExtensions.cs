using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus;

/// <summary>
/// Extension methods for registering event bus services.
/// </summary>
public static class EventBusServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus event bus implementation to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Azure Service Bus connection string.</param>
    /// <param name="subscriptionName">Name of the subscription for this service.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureServiceBusEventBus(
        this IServiceCollection services,
        string connectionString,
        string subscriptionName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        if (string.IsNullOrWhiteSpace(subscriptionName))
            throw new ArgumentNullException(nameof(subscriptionName));

        // Register Service Bus client
        var client = new ServiceBusClient(connectionString);
        services.AddSingleton(client);

        // Register event publisher and subscriber
        services.AddSingleton<IEventPublisher, AzureServiceBusEventPublisher>();
        services.AddSingleton(sp =>
            new AzureServiceBusEventSubscriber(
                sp.GetRequiredService<ServiceBusClient>(),
                subscriptionName,
                sp.GetRequiredService<ILogger<AzureServiceBusEventSubscriber>>()));

        services.AddSingleton<IEventSubscriber>(sp =>
            sp.GetRequiredService<AzureServiceBusEventSubscriber>());

        return services;
    }

    /// <summary>
    /// Adds a hosted service that automatically starts the event subscriber.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEventBusHostedService(this IServiceCollection services)
    {
        services.AddHostedService<EventBusHostedService>();
        return services;
    }
}

/// <summary>
/// Hosted service that manages the lifecycle of the event subscriber.
/// </summary>
public class EventBusHostedService : BackgroundService
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ILogger<EventBusHostedService> _logger;

    public EventBusHostedService(
        IEventSubscriber eventSubscriber,
        ILogger<EventBusHostedService> logger)
    {
        _eventSubscriber = eventSubscriber ?? throw new ArgumentNullException(nameof(eventSubscriber));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Bus hosted service is starting");

        try
        {
            await _eventSubscriber.StartListeningAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event Bus hosted service is shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Event Bus hosted service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Event Bus hosted service");
        await _eventSubscriber.StopListeningAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
