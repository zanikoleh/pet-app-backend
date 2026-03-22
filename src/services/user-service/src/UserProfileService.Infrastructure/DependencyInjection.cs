using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserProfileService.Application.Interfaces;
using UserProfileService.Infrastructure.Persistence;
using SharedKernel.Infrastructure.EventBus;

namespace UserProfileService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add database context
        var connectionString = configuration.GetConnectionString("UserProfileServiceDb") ?? 
            throw new InvalidOperationException("Connection string 'UserProfileServiceDb' not found.");

        services.AddDbContext<UserProfileServiceDbContext>((provider, options) =>
        {
            options.UseNpgsql(connectionString, postgresOptions =>
            {
                postgresOptions.MigrationsAssembly("UserProfileService.Infrastructure");
                postgresOptions.CommandTimeout(30);
            });

            var loggingSection = configuration.GetSection("Logging");
            if (loggingSection.GetValue<bool>("EnableSqlLogging"))
            {
                options.LogTo(Console.WriteLine);
            }
        });

        // Add repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        // Add event bus
        services.AddEventBus(configuration);

        return services;
    }
}
