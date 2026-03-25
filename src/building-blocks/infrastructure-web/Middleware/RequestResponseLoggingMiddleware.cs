using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Text;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses for debugging purposes.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        var request = context.Request;
        var requestLog = new StringBuilder();
        requestLog.AppendLine($"HTTP {request.Method} {request.Path}{request.QueryString}");
        requestLog.AppendLine($"Content-Type: {request.ContentType}");

        // Capture request body if it's JSON
        if (request.ContentType?.Contains("application/json") == true)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            
            if (!string.IsNullOrEmpty(body))
            {
                requestLog.AppendLine($"Request Body: {body}");
            }
        }

        _logger.LogInformation("Request: {RequestInfo}", requestLog.ToString());

        // Capture response
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Log response
                var responseLog = new StringBuilder();
                responseLog.AppendLine($"HTTP {context.Response.StatusCode}");
                responseLog.AppendLine($"Content-Type: {context.Response.ContentType}");

                if (context.Response.ContentType?.Contains("application/json") == true)
                {
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    if (!string.IsNullOrEmpty(responseBodyText))
                    {
                        responseLog.AppendLine($"Response Body: {responseBodyText}");
                    }
                }

                _logger.LogInformation("Response: {ResponseInfo}", responseLog.ToString());
            }
            finally
            {
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }
    }
}

/// <summary>
/// Extension methods for adding request/response logging middleware.
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
