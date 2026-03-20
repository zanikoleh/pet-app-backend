using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NotificationService.Application;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Services;
using SharedKernel.Infrastructure.EventBus;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add application layer
        services.AddApplication();

        // Add notification services
        var useRealEmail = configuration.GetValue<bool>("Email:UseReal", false);
        
        if (useRealEmail)
        {
            // In production, use SendGrid or similar
            // services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IEmailService, MockEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, MockEmailService>();
        }

        services.AddScoped<ISmsService, MockSmsService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Add event bus
        services.AddEventBus(configuration);

        return services;
    }
}
