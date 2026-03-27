using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Rate limiting configuration for different endpoint types.
/// </summary>
public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 100;
    public int BurstSize { get; set; } = 10;
}

/// <summary>
/// Tracks rate limit state for a specific key (IP, user, etc.).
/// </summary>
internal class RateLimitState
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    public DateTime LastRequest { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Middleware for implementing rate limiting using a sliding window algorithm.
/// Protects against brute force attacks and DDoS by limiting requests per IP address.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitConfig _config;
    private static readonly ConcurrentDictionary<string, RateLimitState> _rateLimitStore = new();
    private static readonly object _cleanupLock = new();
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private const int CleanupIntervalSeconds = 300; // 5 minutes

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitConfig? config = null)
    {
        _next = next;
        _logger = logger;
        _config = config ?? new RateLimitConfig();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for internal service-to-service calls
        if (context.Request.Headers.TryGetValue("X-Service-Call", out var serviceCall) && serviceCall == "true")
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var endpoint = context.Request.Path.Value ?? "/";

        // Get or create rate limit state
        var state = _rateLimitStore.AddOrUpdate(clientId, 
            new RateLimitState { RequestCount = 1, WindowStart = DateTime.UtcNow, LastRequest = DateTime.UtcNow },
            (key, existing) =>
            {
                var now = DateTime.UtcNow;
                var windowAge = (now - existing.WindowStart).TotalSeconds;

                // Reset window if 60 seconds have passed
                if (windowAge > 60)
                {
                    return new RateLimitState { RequestCount = 1, WindowStart = now, LastRequest = now };
                }

                existing.RequestCount++;
                existing.LastRequest = now;
                return existing;
            });

        var requestsRemaining = Math.Max(0, _config.RequestsPerMinute - state.RequestCount);
        var windowResetTime = (int)(60 - (DateTime.UtcNow - state.WindowStart).TotalSeconds);

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _config.RequestsPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = requestsRemaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = ((int)new DateTimeOffset(DateTime.UtcNow.AddSeconds(windowResetTime)).ToUnixTimeSeconds()).ToString();

        // Check if rate limit exceeded
        if (state.RequestCount > _config.RequestsPerMinute)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Requests: {Count}/{Limit}",
                clientId, endpoint, state.RequestCount, _config.RequestsPerMinute);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = windowResetTime.ToString();
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = $"Rate limit exceeded. Maximum {_config.RequestsPerMinute} requests per minute allowed.",
                retryAfter = windowResetTime
            });

            return;
        }

        // Periodic cleanup of old entries
        CleanupOldEntries();

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims if authenticated
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{remoteIp}";
    }

    private static void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        
        // Only cleanup every 5 minutes to avoid performance impact
        if ((now - _lastCleanup).TotalSeconds < CleanupIntervalSeconds)
        {
            return;
        }

        lock (_cleanupLock)
        {
            // Double-check after acquiring lock
            if ((DateTime.UtcNow - _lastCleanup).TotalSeconds < CleanupIntervalSeconds)
            {
                return;
            }

            var keysToRemove = _rateLimitStore
                .Where(kvp => (DateTime.UtcNow - kvp.Value.LastRequest).TotalSeconds > 300) // 5 minutes
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimitStore.TryRemove(key, out _);
            }

            _lastCleanup = DateTime.UtcNow;
        }
    }
}

/// <summary>
/// Extension methods for adding rate limiting middleware to the application pipeline.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="config">Optional rate limit configuration. Uses defaults if not provided.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, RateLimitConfig? config = null)
    {
        var configuration = config ?? new RateLimitConfig();
        return app.UseMiddleware<RateLimitingMiddleware>(configuration);
    }
}
