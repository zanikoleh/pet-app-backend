using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using System.Diagnostics;

namespace InfrastructureWeb;

/// <summary>
/// Extension methods for adding structured logging using Serilog to ASP.NET Core services.
/// 
/// Architecture: Hybrid observability approach
/// - Serilog: Handles local file logging (development + immediate access)
/// - OpenTelemetry: Handles backend export (production + correlation with traces/metrics)
/// - Bridge: Correlation IDs and trace context propagate through both systems
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds structured logging using Serilog to the web application builder.
    /// Configures console and file sinks with JSON formatting for easy parsing and analysis.
    /// Optionally adds Elasticsearch sink for centralized log aggregation in development/test environments.
    /// Logs are automatically enriched with OTEL trace context for correlation with distributed traces.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="serviceName">The name of the service for telemetry and log identification.</param>
    /// <param name="environment">The environment name (Development, Staging, Production).</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddStructuredLogging(
        this WebApplicationBuilder builder,
        string serviceName,
        string? environment = null)
    {
        // Enable Serilog self-logging to console for debugging
        Serilog.Debugging.SelfLog.Enable(Console.Out);
        
        environment ??= builder.Environment.EnvironmentName;
        
        // Ensure logs directory exists
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Clear default providers
        builder.Logging.ClearProviders();

        // Get Elasticsearch configuration (optional)
        var elasticsearchUrl = builder.Configuration.GetValue<string>("Logging:Elasticsearch:Url");
        var enableElasticsearch = !string.IsNullOrEmpty(elasticsearchUrl);
        
        // Debug: List all Logging:* configuration keys
        Console.WriteLine("=== DEBUG: All Configuration Keys ===");
        var loggingSection = builder.Configuration.GetSection("Logging");
        Console.WriteLine($"Logging section exists: {loggingSection.Exists()}");
        Console.WriteLine($"Logging section value: {loggingSection.Value}");
        
        foreach (var child in loggingSection.GetChildren())
        {
            Console.WriteLine($"  {child.Key} = {child.Value}");
            foreach (var subchild in child.GetChildren())
            {
                Console.WriteLine($"    {subchild.Key} = {subchild.Value}");
            }
        }
        
        // Also try direct key
        Console.WriteLine($"Direct Logging:Elasticsearch:Url = {builder.Configuration["Logging:Elasticsearch:Url"]}");
        Console.WriteLine($"Final elasticsearchUrl value: {elasticsearchUrl ?? "NULL"}");
        Console.WriteLine("=== DEBUG: All Configuration Keys END ===");

        // Configure Serilog with OTEL context enrichment
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            // Enrich with OTEL trace context: TraceId, SpanId, for correlation with distributed traces
            .Enrich.When(le => Activity.Current != null, e => e.WithProperty("TraceId", Activity.Current?.Id))
            .Enrich.When(le => Activity.Current != null, e => e.WithProperty("SpanId", Activity.Current?.SpanId))
            .WriteTo.Console(new CompactJsonFormatter());

        // Add file sink (always enabled for local debugging)
        loggerConfig.WriteTo.File(
            path: Path.Combine(logDirectory, $"log-{serviceName}-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Service}] [TraceId: {TraceId}] {Message:lj}{NewLine}{Exception}");

        // Add Elasticsearch sink if configured (for centralized log aggregation)
        if (enableElasticsearch && !string.IsNullOrEmpty(elasticsearchUrl))
        {
            loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl!))
            {
                AutoRegisterTemplate = true,
                DetectElasticsearchVersion = true,
                // Use non-data-stream naming pattern (avoid logs-* prefix which triggers ES 8.x data stream mode)
                IndexFormat = $"logindex-{serviceName.ToLower()}-{{0:yyyy.MM.dd}}",
                NumberOfShards = 1,
                NumberOfReplicas = 0,
                // Ensure bulk operations include all properties for searching
                InlineFields = true,
            });
        }

        var logger = loggerConfig.CreateLogger();

        builder.Host.UseSerilog(logger);
        builder.Logging.AddSerilog(logger);

        return builder;
    }

    /// <summary>
    /// Adds correlation ID middleware to track requests across the system.
    /// This middleware ensures each request has a unique ID for distributed tracing and logging.
    /// Correlation IDs are propagated through both Serilog (local) and OpenTelemetry (backend) pipelines.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseCorrelationIdMiddleware(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? context.TraceIdentifier;
            
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Push correlation ID to both Serilog LogContext and Activity (for OTEL)
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // Set correlation ID in current Activity for OTEL propagation
                Activity.Current?.SetTag("correlation_id", correlationId);
                
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RequestLogging");
                
                logger.LogInformation(
                    "Request started: {Method} {Path} [CorrelationId: {CorrelationId}]",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);

                try
                {
                    await next();
                    
                    logger.LogInformation(
                        "Request completed with status {StatusCode} [CorrelationId: {CorrelationId}]",
                        context.Response.StatusCode,
                        correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Request failed with exception [CorrelationId: {CorrelationId}]",
                        correlationId);
                    throw;
                }
            }
        });

        return app;
    }
}
