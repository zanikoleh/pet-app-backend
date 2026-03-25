using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NotificationService.Infrastructure;
using InfrastructureWeb.Middleware;
using InfrastructureWeb;
using Observability;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("notification-service");

// Add structured logging
builder.AddStructuredLogging("notification-service");

// Add services
builder.Services.AddControllers();

// Add API documentation  
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Email and SMS notification service"
    });
});

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCorrelationIdMiddleware();
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
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

namespace NotificationService.Api { public class NotificationApiMarker { } }
