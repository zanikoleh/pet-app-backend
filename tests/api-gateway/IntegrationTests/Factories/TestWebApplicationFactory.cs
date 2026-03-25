using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Factories;

/// <summary>
/// Base WebApplicationFactory for integration tests that configures the test environment.
/// </summary>
public abstract class TestWebApplicationFactory<T> : WebApplicationFactory<T> where T : class
{
    protected readonly Dictionary<string, string?> _configuration = new();

    protected TestWebApplicationFactory()
    {
        SetupTestConfiguration();
    }

    /// <summary>
    /// Override to setup test-specific configuration.
    /// </summary>
    protected virtual void SetupTestConfiguration()
    {
        // Add common test configuration
        _configuration["ASPNETCORE_ENVIRONMENT"] = "Testing";
    }

    /// <summary>
    /// Set a configuration value for the test application.
    /// </summary>
    protected void SetConfiguration(string key, string? value)
    {
        _configuration[key] = value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration on top of the default configuration
            config.AddInMemoryCollection(_configuration);
        });

        builder.ConfigureServices(services =>
        {
            // Override specific services for testing if needed
            ConfigureTestServices(services);
        });
    }

    /// <summary>
    /// Override to configure services for testing.
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Can be overridden by subclasses
    }
}
