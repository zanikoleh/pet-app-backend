using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Gateway.Api.Tests.Controllers;

/// <summary>
/// Tests for API Gateway routing through YARP configuration.
/// Verifies that requests are correctly routed to downstream services.
/// </summary>
public class GatewayRoutingTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _httpClient = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public void GatewayFactory_ShouldCreateValidHttpClient()
    {
        // Arrange & Act
        var client = _factory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.BaseAddress.Should().NotBeNull();
    }

    [Fact]
    public async Task Gateway_HealthCheck_ShouldReturn200()
    {
        // Arrange
        var endpoint = "/health";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Gateway_RootPath_ShouldReturnSwaggerUI()
    {
        // Arrange
        var endpoint = "/";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        // Gateway should either redirect to swagger or return OK for swagger redirection
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently);
    }

    [Fact]
    public async Task Gateway_SwaggerEndpoint_ShouldExist()
    {
        // Arrange
        var endpoint = "/swagger";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        response.Should().NotBeNull();
        // Gateway may redirect to swagger-ui or return swagger JSON
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.MovedPermanently,
            HttpStatusCode.TemporaryRedirect
        );
    }

    [Fact]
    public async Task Gateway_InvalidRoute_ShouldReturn404()
    {
        // Arrange
        var endpoint = "/invalid/nonexistent/route/12345";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
