using System;
using System.Linq;
using System.Threading.Tasks;
using PetService.Api;
using PetService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Factories;

/// <summary>
/// WebApplicationFactory for PetService integration tests.
/// </summary>
public class PetServiceWebApplicationFactory : TestWebApplicationFactory<PetApiMarker>
{
    public PetServiceWebApplicationFactory()
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
                var dbContext = scope.ServiceProvider.GetRequiredService<PetServiceDbContext>();
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

        // Use test database connection string - PetService looks for "DefaultConnection"
        SetConfiguration("ConnectionStrings:DefaultConnection",
            "Host=localhost;Database=PetApp_PetService_IntegrationTests;Username=postgres;Password=TestPassword123!@#;Port=5433;");

        // JWT Settings
        SetConfiguration("JwtSettings:SecretKey", "your-super-secret-key-that-is-at-least-32-characters-long-for-256bit");
        SetConfiguration("JwtSettings:Issuer", "pet-app-identity-service");
        SetConfiguration("JwtSettings:Audience", "pet-app-api");

        // Observability
        SetConfiguration("Observability:EnableConsoleExporter", "false");

        // Logging
        SetConfiguration("Logging:LogLevel:Default", "Information");
        SetConfiguration("Logging:LogLevel:Microsoft.AspNetCore", "Warning");
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Remove the DbContext and re-register it for testing
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<PetServiceDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register DbContext with test configuration
        services.AddDbContext<PetServiceDbContext>((serviceProvider, options) =>
        {
            var connectionString = "Host=localhost;Database=PetApp_PetService_IntegrationTests;Username=postgres;Password=TestPassword123!@#;Port=5433;";
            options.UseNpgsql(connectionString, postgresOptions =>
            {
                postgresOptions.MigrationsAssembly("PetService.Infrastructure");
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
                var dbContext = scope.ServiceProvider.GetRequiredService<PetServiceDbContext>();
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
