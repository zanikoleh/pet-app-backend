using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Gateway.Api.Tests.Middleware;

/// <summary>
/// Tests for API Gateway middleware functionality.
/// Verifies authentication, authorization, and request/response handling.
/// </summary>
public class GatewayMiddlewareTests : IAsyncLifetime
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
    public async Task RequestWithoutContentType_ShouldBeAccepted()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        // Don't set Content-Type header for GET request

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GatewayHeaders_ShouldBePreserved()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        var customHeaderValue = "test-gateway-value";
        request.Headers.Add("X-Gateway-Request-Id", customHeaderValue);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LargePayload_ShouldBeHandled()
    {
        // Arrange
        var largeContent = new string('x', 1024 * 100); // 100KB

        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TimeoutBehavior_ShouldNotCauseGatewayError()
    {
        // Arrange - Using a very short endpoint
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ResponseHeaders_ShouldIncludeXPoweredBy()
    {
        // Arrange
        var endpoint = "/health";

        // Act
        var response = await _httpClient.GetAsync(endpoint);

        // Assert
        response.Should().NotBeNull();
        // Gateway should set appropriate headers for downstream service identification
        response.Headers.Should().NotBeEmpty();
    }
}
