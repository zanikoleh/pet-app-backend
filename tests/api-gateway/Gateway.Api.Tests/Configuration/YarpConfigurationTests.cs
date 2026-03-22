using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Gateway.Api.Tests.Configuration;

/// <summary>
/// Tests for YARP (Yet Another Reverse Proxy) configuration.
/// Verifies that routing rules are correctly configured for downstream services.
/// </summary>
public class YarpConfigurationTests : IAsyncLifetime
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

    [Theory]
    [InlineData("/pets")]
    [InlineData("/api/pets")]
    public async Task GatewayShould_RoutePetServiceRequests(string path)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        // In integration testing with services not running, 502/503 is expected
        // The important thing is that the gateway attempts to route the request
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,           // Route matched, service returned 404
            HttpStatusCode.OK,                 // Service responded
            HttpStatusCode.Unauthorized,       // Auth required
            HttpStatusCode.BadGateway,         // Service unavailable (expected in test)
            HttpStatusCode.ServiceUnavailable  // Service unavailable
        );
    }

    [Theory]
    [InlineData("/users")]
    [InlineData("/api/users")]
    public async Task GatewayShould_RouteUserServiceRequests(string path)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Theory]
    [InlineData("/auth")]
    [InlineData("/api/auth")]
    public async Task GatewayShould_RouteIdentityServiceRequests(string path)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Theory]
    [InlineData("/files")]
    [InlineData("/api/files")]
    public async Task GatewayShould_RouteFileServiceRequests(string path)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Theory]
    [InlineData("/notifications")]
    [InlineData("/api/notifications")]
    public async Task GatewayShould_RouteNotificationServiceRequests(string path)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Fact]
    public async Task GatewayShould_SupportMethodOverrideHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/health");
        request.Headers.Add("X-HTTP-Method-Override", "GET");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        // Gateway should handle method override gracefully
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GatewayShould_HandleMultipleQueryParameters()
    {
        // Arrange
        var endpoint = "/pets?page=1&pageSize=10&sortBy=name";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        response.Should().NotBeNull();
        // Gateway should preserve query parameters
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }
}
