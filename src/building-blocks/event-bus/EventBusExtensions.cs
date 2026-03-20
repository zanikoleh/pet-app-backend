using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Infrastructure.EventBus;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register the in-memory event bus implementation
        services.AddSingleton<IEventPublisher, InMemoryEventBus>();
        services.AddSingleton<IEventSubscriber, InMemoryEventBus>();
        
        return services;
    }
}