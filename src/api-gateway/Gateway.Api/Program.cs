using Yarp.ReverseProxy.Configuration;
using Observability;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("api-gateway");

// Add services for the API Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Pet App API Gateway", Version = "v1" });
});

// Load configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("yarpconfig.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseTraceContextPropagation();

// Use CORS
app.UseCors("AllowAll");

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
