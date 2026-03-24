using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Observability;

/// <summary>
/// Extension methods for adding OpenTelemetry observability (tracing, metrics, and logging)
/// to ASP.NET Core services.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing, metrics, and logging to the service collection.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="serviceName">The name of the service for telemetry.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        var config = builder.Configuration;
        var otlpEndpoint = config.GetValue<string>("Observability:OtlpEndpoint") 
            ?? "http://localhost:4317";
        var enableConsoleExporter = config.GetValue<bool>("Observability:EnableConsoleExporter");

        var resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: GetServiceVersion())
            .Build();

        // Add OpenTelemetry
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: GetServiceVersion()))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter += context =>
                        {
                            // Skip health check endpoints
                            return !context.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });

                if (enableConsoleExporter)
                {
                    tracingBuilder.AddConsoleExporter();
                }
            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: GetServiceVersion()))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });

                if (enableConsoleExporter)
                {
                    metricsBuilder.AddConsoleExporter();
                }
            });

        // Add OpenTelemetry logging
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName, serviceVersion: GetServiceVersion()))
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });

            if (enableConsoleExporter)
            {
                logging.AddConsoleExporter();
            }
        });

        return builder;
    }

    /// <summary>
    /// Gets the service version from assembly version.
    /// </summary>
    /// <returns>The service version string.</returns>
    private static string GetServiceVersion()
    {
        return typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }

    /// <summary>
    /// Adds a middleware that propagates trace context headers.
    /// Ensures W3C Trace Context is propagated across service boundaries.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseTraceContextPropagation(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // Ensure trace ID is present for logging
            if (!context.Items.ContainsKey("TraceId"))
            {
                var traceId = System.Diagnostics.Activity.Current?.Id 
                    ?? context.TraceIdentifier;
                context.Items["TraceId"] = traceId;
            }

            await next();
        });

        return app;
    }
}
