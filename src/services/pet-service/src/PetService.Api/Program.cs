using PetService.Application;
using PetService.Infrastructure;
using InfrastructureWeb.Middleware;
using SharedKernel.Infrastructure.EventBus;
using Observability;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("pet-service");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

// Add logging
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
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