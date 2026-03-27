using PetService.Application;
using PetService.Infrastructure;
using InfrastructureWeb.Middleware;
using InfrastructureWeb;
using SharedKernel.Infrastructure.EventBus;
using Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("pet-service");

// Add structured logging
builder.AddStructuredLogging("pet-service");

// Add services
builder.Services.AddControllers();
builder.Services.AddModelValidationErrorHandling();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    { 
        Title = "Pet Service API", 
        Version = "v1",
        Description = "Pet management service with CRUD operations, search, and file attachment capabilities"
    });
    
    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Get configuration
var connectionString = builder.Configuration.GetConnectionString("PetServiceDb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'PetServiceDb' or 'DefaultConnection' not found.");

// Add application layer
builder.Services.AddApplication();

// Add infrastructure layer with database context
builder.Services.AddInfrastructure(connectionString);

// Add event bus using in-memory implementation
builder.Services.AddEventBus(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var serviceScopeFactory = scope.ServiceProvider;
    var context = serviceScopeFactory.GetRequiredService<PetService.Infrastructure.Persistence.PetServiceDbContext>();
    var logger = serviceScopeFactory.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Ensuring database exists and applying migrations...");
        // Ensure database and schema are created
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database setup completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while setting up the database");
        throw;
    }
}

// Configure the HTTP request pipeline
app.UseCorrelationIdMiddleware();
app.UseRequestResponseLogging();
app.UseExceptionHandling();
app.UseTraceContextPropagation();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirect in Docker - internal communication uses HTTP
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

namespace PetService.Api { public class PetApiMarker { } }