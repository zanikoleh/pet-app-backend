using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using PetService.Domain.Entities;
using PetService.Domain.ValueObjects;
using PetService.Domain.Events;

namespace IntegrationTests.Domain;

public class PetAggregateTests
{
    [Fact]
    public void CreatePet_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var name = "Fluffy";
        var species = "Dog";
        var breed = "Golden Retriever";
        var dateOfBirth = DateTime.Now.AddYears(-5);
        var userId = 1;

        // Act
        var pet = Pet.Create(name, species, breed, dateOfBirth, userId);

        // Assert
        pet.Should().NotBeNull();
        pet.Name.Should().Be(name);
        pet.Species.Should().Be(species);
        pet.Breed.Should().Be(breed);
        pet.DateOfBirth.Should().Be(dateOfBirth);
        pet.UserIdOwner.Should().Be(userId);
    }

    [Fact]
    public void CreatePet_PublishesPetCreatedEvent()
    {
        // Arrange
        var name = "Fluffy";
        var species = "Dog";
        var userId = 1;

        // Act
        var pet = Pet.Create(name, species, "Breed", DateTime.Now.AddYears(-5), userId);

        // Assert
        pet.DomainEvents.Should().HaveCount(1);
        pet.DomainEvents.First().Should().BeOfType<PetCreatedEvent>();
    }

    [Fact]
    public void UpdatePet_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var pet = Pet.Create("Fluffy", "Dog", "Golden Retriever", DateTime.Now.AddYears(-5), 1);
        var newName = "Lucky";

        // Act
        pet.Update(newName, "Dog", "Labrador");

        // Assert
        pet.Name.Should().Be(newName);
        pet.Breed.Should().Be("Labrador");
    }

    [Fact]
    public void DeletePet_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var pet = Pet.Create("Fluffy", "Dog", "Golden Retriever", DateTime.Now.AddYears(-5), 1);

        // Act
        pet.Delete();

        // Assert
        pet.IsDeleted.Should().BeTrue();
        pet.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePet_WithEmptyName_ThrowsException()
    {
        // Arrange
        var pet = Pet.Create("Fluffy", "Dog", "Golden Retriever", DateTime.Now.AddYears(-5), 1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            pet.Update("", "Dog", "Breed"));

        ex.Message.Should().Contain("Name cannot be empty");
    }

    [Fact]
    public void Pet_WithZeroUserId_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Pet.Create("Fluffy", "Dog", "Breed", DateTime.Now, 0));
    }
}
