using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InfrastructureWeb.Middleware;

/// <summary>
/// Adds custom handling for model validation errors to ensure they are properly logged.
/// </summary>
public static class ModelValidationErrorHandlingExtensions
{
    /// <summary>
    /// Configures custom handling for model validation errors.
    /// Ensures invalid model state responses are logged and formatted consistently.
    /// </summary>
    public static IServiceCollection AddModelValidationErrorHandling(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<object>>();
                var errors = new Dictionary<string, string[]>();

                foreach (var keyValuePair in context.ModelState)
                {
                    var key = keyValuePair.Key;
                    var value = keyValuePair.Value;

                    if (value?.Errors.Count > 0)
                    {
                        errors[key] = value.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray();
                    }
                }

                logger.LogWarning(
                    "Model validation failed for {Path}. Errors: {@Errors}",
                    context.HttpContext.Request.Path,
                    errors);

                return new BadRequestObjectResult(new
                {
                    error = "One or more validation errors occurred.",
                    code = "VALIDATION_ERROR",
                    errors = errors
                });
            };
        });

        return services;
    }
}
