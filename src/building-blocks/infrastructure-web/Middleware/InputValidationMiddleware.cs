using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Utility class for input validation and sanitization.
/// </summary>
public static class InputSanitizer
{
    /// <summary>
    /// Sanitizes a string input by removing potentially harmful content.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // HTML entity encoding
        input = System.Net.WebUtility.HtmlEncode(input);

        // Remove null bytes
        input = input.Replace("\0", string.Empty);

        // Remove potential path traversal patterns
        input = Regex.Replace(input, @"\.\\|\.\.\/|\.\.\\", "", RegexOptions.IgnoreCase);

        return input;
    }

    /// <summary>
    /// Validates if a string length is within acceptable limits.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="maxLength">Maximum allowed length.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>Tuple of (isValid, errorMessage).</returns>
    public static (bool isValid, string errorMessage) ValidateLength(string? input, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (false, $"{fieldName} is required");

        if (input.Length > maxLength)
            return (false, $"{fieldName} exceeds maximum length of {maxLength} characters");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates if a string matches a specific pattern.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="pattern">The regex pattern to match.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <returns>Tuple of (isValid, errorMessage).</returns>
    public static (bool isValid, string errorMessage) ValidatePattern(string input, string pattern, string fieldName)
    {
        if (!Regex.IsMatch(input, pattern))
            return (false, $"{fieldName} format is invalid");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates if a string is a valid email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        const string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Validates if a file name is safe (no path traversal).
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>True if safe; otherwise false.</returns>
    public static bool ValidateFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // Reject paths with directory separators
        if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
            return false;

        // Reject very long names
        if (fileName.Length > 255)
            return false;

        return true;
    }

    /// <summary>
    /// Validates file size.
    /// </summary>
    /// <param name="size">The file size in bytes.</param>
    /// <param name="maxSizeInMB">Maximum allowed size in MB.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool ValidateFileSize(long size, int maxSizeInMB = 50)
    {
        const long bytesPerMB = 1024 * 1024;
        return size > 0 && size <= (maxSizeInMB * bytesPerMB);
    }
}

/// <summary>
/// Middleware for validating and sanitizing input data.
/// Prevents common injection attacks and malformed data.
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Don't validate GET requests or requests without body
        if (context.Request.Method == HttpMethods.Get || 
            context.Request.ContentLength == 0 || 
            string.IsNullOrEmpty(context.Request.ContentType))
        {
            await _next(context);
            return;
        }

        // Only validate JSON requests
        if (!context.Request.ContentType?.Contains("application/json") ?? false)
        {
            await _next(context);
            return;
        }

        // Validate request body size (max 10 MB)
        const long maxBodySize = 10 * 1024 * 1024;
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > maxBodySize)
        {
            _logger.LogWarning("Request body exceeds maximum size limit from {RemoteIP}", 
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Request body too large",
                message = "Request body exceeds maximum size of 10 MB"
            });

            return;
        }

        // Read and validate request body
        context.Request.EnableBuffering();
        var bodyStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(bodyStream);
        var bodyBytes = bodyStream.ToArray();
        context.Request.Body.Position = 0;

        try
        {
            // Validate JSON structure
            var bodyText = Encoding.UTF8.GetString(bodyBytes);
            if (!IsValidJson(bodyText))
            {
                _logger.LogWarning("Invalid JSON in request body from {RemoteIP}", 
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid request format",
                    message = "Request body must be valid JSON"
                });

                return;
            }

            // Check for null bytes in body
            if (bodyBytes.Contains((byte)0))
            {
                _logger.LogWarning("Null bytes detected in request body from {RemoteIP}", 
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid request format",
                    message = "Request contains invalid characters"
                });

                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating request body from {RemoteIP}", 
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Request validation failed",
                message = "Unable to process request"
            });

            return;
        }

        // Validate query string parameters
        var queryErrors = ValidateQueryParameters(context.Request.Query);
        if (queryErrors.Any())
        {
            _logger.LogWarning("Invalid query parameters from {RemoteIP}: {Errors}", 
                context.Connection.RemoteIpAddress, string.Join(", ", queryErrors));

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid query parameters",
                message = "Request contains invalid parameters",
                details = queryErrors
            });

            return;
        }

        await _next(context);
    }

    private static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        json = json.Trim();
        if (!((json.StartsWith("{") && json.EndsWith("}")) ||
              (json.StartsWith("[") && json.EndsWith("]"))))
        {
            return false;
        }

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> ValidateQueryParameters(IQueryCollection queryParameters)
    {
        var errors = new List<string>();

        foreach (var parameter in queryParameters)
        {
            var paramName = parameter.Key;
            var paramValue = parameter.Value.ToString();

            // Validate parameter name length
            if (paramName.Length > 100)
            {
                errors.Add($"Parameter name '{paramName}' is too long");
                continue;
            }

            // Validate parameter value length
            if (paramValue.Length > 1000)
            {
                errors.Add($"Parameter '{paramName}' value exceeds maximum length");
                continue;
            }

            // Check for null bytes in parameter value
            if (paramValue.Contains('\0'))
            {
                errors.Add($"Parameter '{paramName}' contains invalid characters");
            }
        }

        return errors;
    }
}

/// <summary>
/// Extension methods for adding input validation middleware to the application pipeline.
/// </summary>
public static class InputValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds input validation middleware to the HTTP request pipeline.
    /// Should be applied early in the middleware chain, after security headers.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseInputValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<InputValidationMiddleware>();
    }
}
