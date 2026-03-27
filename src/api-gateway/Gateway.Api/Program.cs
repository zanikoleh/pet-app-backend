using Yarp.ReverseProxy.Configuration;
using Observability;
using InfrastructureWeb;
using InfrastructureWeb.Middleware;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("api-gateway");

// Add structured logging
builder.AddStructuredLogging("api-gateway");

// Add secure CORS policies
builder.Services.AddSecureCors(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    { 
        Title = "Pet App API Gateway", 
        Version = "v1",
        Description = "Central API Gateway for the Pet App microservices architecture. Routes requests to Pet Service, Identity Service, File Service, Notification Service, and User Profile Service.",
        Contact = new OpenApiContact
        {
            Name = "Pet App Development Team",
            Email = "support@petapp.dev"
        },
        License = new OpenApiLicense
        {
            Name = "MIT"
        }
    });
});

// Load configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("yarpconfig.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCorrelationIdMiddleware();
app.UseTraceContextPropagation();

// Apply security middleware in order
app.UseSecurityHeaders();
app.UseRateLimiting();
app.UseInputValidation();
app.UseCorsPolicyValidation();

// Use CORS
app.UseSecureCors();

// Add response headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Powered-By", "Pet-App-Gateway/1.0");
    context.Response.Headers.Append("X-Gateway-Version", "1.0");
    
    // Preserve custom request headers
    if (context.Request.Headers.TryGetValue("X-Gateway-Request-Id", out var requestId))
    {
        context.Response.Headers.Append("X-Gateway-Request-Id", requestId);
    }
    
    await next();
});

// HTTP Method Override middleware
app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.Headers.TryGetValue("X-HTTP-Method-Override", out var methodOverride))
    {
        context.Request.Method = methodOverride.ToString();
    }
    
    await next();
});

// Health check endpoint - accepts GET and POST for flex method override support
app.MapMethods("/health", new[] { "GET", "POST" }, () => new { status = "healthy", timestamp = DateTime.UtcNow })
    .WithName("HealthCheck")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status503ServiceUnavailable);

// Root endpoint - accepts GET and POST
app.MapMethods("/", new[] { "GET", "POST" }, () => Results.Ok(new { message = "API Gateway - Pet App Microservices", version = "1.0" }))
    .WithName("Root");

// Enable Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pet App API Gateway v1");
        options.RoutePrefix = "swagger";
    });
}

// Map YARP reverse proxy - must be last
app.MapReverseProxy();

app.Run();

public partial class Program { }
