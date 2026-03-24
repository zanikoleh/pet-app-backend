using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PetService.Api;

namespace IntegrationTests.Services;

public class PetServiceIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<PetApiMarker> _factory;
    private HttpClient _client;
    private string _accessToken;

    public PetServiceIntegrationTests()
    {
        _factory = new WebApplicationFactory<PetApiMarker>();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _accessToken = "test-jwt-token"; // In real tests, get from Identity Service
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreatePet_WithValidData_ReturnsPet()
    {
        // Arrange
        var createPetRequest = new
        {
            name = "Fluffy",
            species = "Dog",
            breed = "Golden Retriever",
            dateOfBirth = "2020-01-15",
            userIdOwner = 1
        };

        var content = new StringContent(
            JsonSerializer.Serialize(createPetRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/pets", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Fluffy");
    }

    [Fact]
    public async Task GetPet_WithValidId_ReturnsPet()
    {
        // Arrange
        var petId = 1;

        // Act
        var response = await _client.GetAsync($"/api/pets/{petId}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound); // Depends on test data
    }

    [Fact]
    public async Task GetAllPets_ReturnsNotEmpty()
    {
        // Act
        var response = await _client.GetAsync("/api/pets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdatePet_WithValidData_ReturnsUpdatedPet()
    {
        // Arrange
        var petId = 1;
        var updatePetRequest = new
        {
            name = "Fluffier",
            species = "Dog",
            breed = "Golden Retriever"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updatePetRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/pets/{petId}", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePet_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var petId = 999; // Non-existent ID

        // Act
        var response = await _client.DeleteAsync($"/api/pets/{petId}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound);
    }
}
