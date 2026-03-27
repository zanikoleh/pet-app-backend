using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Middleware for adding security headers to HTTP responses.
/// Implements OWASP security best practices.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add Strict-Transport-Security header (HTTPS enforcement)
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Add X-Content-Type-Options header (prevent MIME type sniffing)
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Add X-Frame-Options header (clickjacking protection)
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // Add X-XSS-Protection header (XSS protection)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Add Content-Security-Policy header
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";

        // Add Referrer-Policy header
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Add Permissions-Policy header
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding security headers middleware to the application pipeline.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security headers middleware to the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
