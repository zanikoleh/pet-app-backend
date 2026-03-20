using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;

namespace IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
