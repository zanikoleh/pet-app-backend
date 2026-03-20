using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add database context
        var connectionString = configuration.GetConnectionString("IdentityServiceDb") ?? 
            throw new InvalidOperationException("Connection string 'IdentityServiceDb' not found.");

        services.AddDbContext<IdentityServiceDbContext>((provider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("IdentityService.Infrastructure");
                sqlOptions.CommandTimeout(30);
            });

            // Enable query logging in development
            if (configuration.GetValue<bool>("Logging:EnableSqlLogging"))
            {
                options.LogTo(Console.WriteLine);
            }
        });

        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Add JWT token service
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("SecretKey") ?? 
            throw new InvalidOperationException("JwtSettings:SecretKey not configured.");
        var issuer = jwtSettings.GetValue<string>("Issuer") ?? 
            throw new InvalidOperationException("JwtSettings:Issuer not configured.");
        var audience = jwtSettings.GetValue<string>("Audience") ?? 
            throw new InvalidOperationException("JwtSettings:Audience not configured.");
        var accessTokenExpirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes", 15);
        var refreshTokenExpirationMinutes = jwtSettings.GetValue<int>("RefreshTokenExpirationMinutes", 10080); // 7 days

        var jwtTokenService = new JwtTokenService(
            secretKey,
            issuer,
            audience,
            accessTokenExpirationMinutes,
            refreshTokenExpirationMinutes);

        services.AddSingleton<IJwtTokenService>(jwtTokenService);

        // Add OAuth provider service
        var oauthSettings = configuration.GetSection("OAuthSettings");
        var clientIds = oauthSettings.GetSection("ClientIds").Get<Dictionary<string, string>>() ?? new();
        var clientSecrets = oauthSettings.GetSection("ClientSecrets").Get<Dictionary<string, string>>() ?? new();

        services.AddScoped<IOAuthProviderService>(provider =>
            new OAuthProviderService(clientIds, clientSecrets));

        // Add event publishing
        services.AddEventBus(configuration);

        return services;
    }
}
