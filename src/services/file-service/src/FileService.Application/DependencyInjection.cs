using Microsoft.Extensions.DependencyInjection;

namespace FileService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        return services;
    }
}
