using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using IdentityService.Api;
using IntegrationTests.Factories;

namespace IntegrationTests.Services;

public class IdentityServiceIntegrationTests : IAsyncLifetime
{
    private readonly IdentityServiceWebApplicationFactory _factory;
    private HttpClient _client;

    public IdentityServiceIntegrationTests()
    {
        _factory = new IdentityServiceWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var registerRequest = new
        {
            email = "test@example.com",
            password = "SecurePassword123!",
            fullName = "Test User"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("\"id\"");
        responseContent.Should().Contain("test@example.com");
        responseContent.Should().Contain("accessToken");
        responseContent.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new
        {
            email = "invalid-email",
            password = "SecurePassword123!",
            fullName = "Test User"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new
        {
            email = "test@example.com",
            password = "weak",
            fullName = "Test User"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange - Register first
        var registerRequest = new
        {
            email = "login-test@example.com",
            password = "SecurePassword123!",
            fullName = "Login Test User"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync("/api/auth/register", registerContent);

        // Act - Login
        var loginRequest = new
        {
            email = "login-test@example.com",
            password = "SecurePassword123!"
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("accessToken");
        responseContent.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            email = "nonexistent@example.com",
            password = "WrongPassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckEmail_WithRegisteredEmail_ReturnsExists()
    {
        // Arrange
        var registerRequest = new
        {
            email = "check-email@example.com",
            password = "SecurePassword123!",
            fullName = "Check Email Test"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync("/api/auth/register", registerContent);

        // Act
        var checkEmailRequest = new { email = "check-email@example.com" };
        var checkContent = new StringContent(
            JsonSerializer.Serialize(checkEmailRequest),
            Encoding.UTF8,
            "application/json");
        var response = await _client.PostAsync("/api/auth/check-email", checkContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("true");
    }

    [Fact]
    public async Task CheckEmail_WithNonRegisteredEmail_ReturnsNotExists()
    {
        // Act
        var checkEmailRequest = new { email = "notregistered@example.com" };
        var checkContent = new StringContent(
            JsonSerializer.Serialize(checkEmailRequest),
            Encoding.UTF8,
            "application/json");
        var response = await _client.PostAsync("/api/auth/check-email", checkContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("false");
    }
}
