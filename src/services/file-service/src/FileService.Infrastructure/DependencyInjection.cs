using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FileService.Application.Interfaces;
using FileService.Infrastructure.Persistence;
using FileService.Infrastructure.Services;

namespace FileService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add database context
        var connectionString = configuration.GetConnectionString("FileServiceDb") ?? 
            throw new InvalidOperationException("Connection string 'FileServiceDb' not found.");

        services.AddDbContext<FileServiceDbContext>((provider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("FileService.Infrastructure");
                sqlOptions.CommandTimeout(30);
            });

            if (configuration.GetValue<bool>("Logging:EnableSqlLogging"))
            {
                options.LogTo(Console.WriteLine);
            }
        });

        // Add repositories
        services.AddScoped<IFileRepository, FileRepository>();

        // Add file storage service
        var storageType = configuration.GetValue<string>("FileStorage:Type", "local");
        
        if (storageType == "azure")
        {
            // In production, implement Azure Blob Storage
            services.AddScoped<IFileStorageService>(provider =>
                new MockFileStorageService("./files"));
        }
        else
        {
            // Local storage for development
            var basePath = configuration.GetValue<string>("FileStorage:BasePath", "./files");
            services.AddScoped<IFileStorageService>(provider =>
                new MockFileStorageService(basePath));
        }

        // Add event bus
        services.AddEventBus(configuration);

        return services;
    }
}
