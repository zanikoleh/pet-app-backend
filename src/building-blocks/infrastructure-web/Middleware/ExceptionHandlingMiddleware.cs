using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Middleware for handling exceptions and converting them to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response = new { error = exception.Message, code = "ERROR" };

        switch (exception)
        {
            case NotFoundException nfe:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = new { error = nfe.Message, code = nfe.Code ?? "NOT_FOUND" };
                break;
            
            case BusinessLogicException ble:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { error = ble.Message, code = ble.Code ?? "BUSINESS_ERROR" };
                break;
            
            case ValidationException ve:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { error = ve.Message, code = ve.Code ?? "VALIDATION_ERROR", errors = ve.Errors };
                break;
            
            case DomainException de:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { error = de.Message, code = de.Code ?? "DOMAIN_ERROR" };
                break;
            
            case ArgumentException ae:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { error = ae.Message, code = "INVALID_ARGUMENT" };
                break;
            
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new { error = "An unexpected error occurred", code = "INTERNAL_ERROR" };
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension methods for adding exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
