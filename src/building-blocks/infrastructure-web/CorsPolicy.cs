using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfrastructureWeb;

/// <summary>
/// CORS policy configuration for the application.
/// Implements secure cross-origin request handling.
/// </summary>
public static class CorsPolicyExtensions
{
    private const string SecureCorsPolicy = "SecureCorsPolicy";
    private const string ServerToServerPolicy = "ServerToServerPolicy";

    /// <summary>
    /// Adds secure CORS policies to the application services.
    /// Creates two policies: one for browser clients and one for server-to-server communication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSecureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors");
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? GetDefaultAllowedOrigins();
        var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? GetDefaultAllowedMethods();
        var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? GetDefaultAllowedHeaders();
        var credentials = corsSettings.GetValue<bool>("AllowCredentials");
        var maxAge = corsSettings.GetValue<int>("MaxAgeSeconds", 3600);

        services.AddCors(options =>
        {
            // Secure browser client policy
            options.AddPolicy(SecureCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .WithMethods(allowedMethods)
                    .WithHeaders(allowedHeaders)
                    .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");

                if (credentials)
                {
                    policy.AllowCredentials();
                }
                else
                {
                    policy.DisallowCredentials();
                }
            });

            // Server-to-server communication policy (restrictive)
            options.AddPolicy(ServerToServerPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:*", "https://localhost:*")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .WithHeaders("*")
                    .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");
            });
        });

        return services;
    }

    /// <summary>
    /// Applies the secure CORS middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSecureCors(this IApplicationBuilder app)
    {
        return app.UseCors(SecureCorsPolicy);
    }

    /// <summary>
    /// Applies the server-to-server CORS policy.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseServerToServerCors(this IApplicationBuilder app)
    {
        return app.UseCors(ServerToServerPolicy);
    }

    private static string[] GetDefaultAllowedOrigins() => new[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:44300",
        "https://api.petapp.com",
        "https://admin.petapp.com"
    };

    private static string[] GetDefaultAllowedMethods() => new[]
    {
        "GET",
        "POST",
        "PUT",
        "DELETE",
        "PATCH",
        "OPTIONS"
    };

    private static string[] GetDefaultAllowedHeaders() => new[]
    {
        "Authorization",
        "Content-Type",
        "Accept",
        "X-Request-ID",
        "X-Correlation-ID",
        "X-Service-Call",
        "User-Agent"
    };
}

/// <summary>
/// Middleware for validating and enforcing CORS policies.
/// </summary>
public class CorsPolicyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AllowedOrigins = new(StringComparer.OrdinalIgnoreCase)
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:44300",
        "https://api.petapp.com",
        "https://admin.petapp.com"
    };

    public CorsPolicyValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Origin", out var origin))
        {
            var originValue = origin.ToString();
            
            // Validate origin format
            if (!Uri.TryCreate(originValue, UriKind.Absolute, out var uri))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid origin" });
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding CORS policy validation middleware.
/// </summary>
public static class CorsPolicyValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds CORS policy validation middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseCorsPolicyValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorsPolicyValidationMiddleware>();
    }
}
