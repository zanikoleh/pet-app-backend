using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Api;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Factories;

  /// <summary>
/// WebApplicationFactory for IdentityService integration tests.
/// </summary>
public class IdentityServiceWebApplicationFactory : TestWebApplicationFactory<IdentityApiMarker>
{
    public IdentityServiceWebApplicationFactory()
    {
        // Initialize database after factory is created
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using (var scope = Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
                // Ensure database exists and apply migrations
                dbContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization: {ex.Message}");
        }
    }

    protected override void SetupTestConfiguration()
    {
        base.SetupTestConfiguration();

        // Use test database connection string
        SetConfiguration("ConnectionStrings:IdentityServiceDb",
            "Host=localhost;Database=PetApp_IdentityService_IntegrationTests;Username=postgres;Password=TestPassword123!@#;Port=5433;");

        // JWT Settings
        SetConfiguration("JwtSettings:SecretKey", "your-super-secret-key-that-is-at-least-32-characters-long-for-256bit");
        SetConfiguration("JwtSettings:Issuer", "pet-app-identity-service");
        SetConfiguration("JwtSettings:Audience", "pet-app-api");
        SetConfiguration("JwtSettings:AccessTokenExpirationMinutes", "15");
        SetConfiguration("JwtSettings:RefreshTokenExpirationMinutes", "10080");

        // Observability
        SetConfiguration("Observability:EnableConsoleExporter", "false");

        // Logging
        SetConfiguration("Logging:LogLevel:Default", "Information");
        SetConfiguration("Logging:LogLevel:Microsoft.AspNetCore", "Warning");
        SetConfiguration("Logging:EnableSqlLogging", "false");
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Remove the DbContext and re-register it for testing
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityServiceDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register DbContext with test configuration
        services.AddDbContext<IdentityServiceDbContext>((serviceProvider, options) =>
        {
            var connectionString = "Host=localhost;Database=PetApp_IdentityService_IntegrationTests;Username=postgres;Password=TestPassword123!@#;Port=5433;";
            options.UseNpgsql(connectionString, postgresOptions =>
            {
                postgresOptions.MigrationsAssembly("IdentityService.Infrastructure");
                postgresOptions.CommandTimeout(30);
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        // Clean up test database if needed
        using (var scope = Services.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        await base.DisposeAsync();
    }
}
