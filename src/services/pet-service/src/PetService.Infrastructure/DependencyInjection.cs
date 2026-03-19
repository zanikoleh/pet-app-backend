using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetService.Application.Handlers;
using PetService.Infrastructure.Persistence;
using PetService.Infrastructure.Persistence.Repositories;

namespace PetService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<PetServiceDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions
                    .MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name)
                    .CommandTimeout(30)
                    .EnableRetryOnFailure(2, TimeSpan.FromSeconds(5), null)));

        // Register repositories
        services.AddScoped<IPetRepository, PetRepository>();

        return services;
    }
}