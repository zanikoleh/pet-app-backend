using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserProfileService.Application.Interfaces;
using UserProfileService.Infrastructure.Persistence;

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
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("UserProfileService.Infrastructure");
                sqlOptions.CommandTimeout(30);
            });

            if (configuration.GetValue<bool>("Logging:EnableSqlLogging"))
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
