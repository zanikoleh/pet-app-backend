using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using PetService.Domain.Aggregates;
using PetService.Domain.ValueObjects;
using PetService.Domain.Events;
using SharedKernel;

namespace IntegrationTests.Domain;

public class PetAggregateTests
{
    [Fact]
    public void CreatePet_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "Fluffy";
        var type = PetType.Dog;
        var breed = Breed.Create("Golden Retriever");
        var dateOfBirth = DateTime.UtcNow.AddYears(-5);

        // Act
        var pet = new Pet(ownerId, name, type, dateOfBirth, breed);

        // Assert
        pet.Should().NotBeNull();
        pet.Name.Should().Be(name);
        pet.Type.Should().Be(type);
        pet.Breed!.Value.Should().Be("Golden Retriever");
        pet.DateOfBirth.Should().Be(dateOfBirth);
        pet.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public void CreatePet_PublishesPetCreatedEvent()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var pet = new Pet(ownerId, "Fluffy", PetType.Dog, DateTime.UtcNow.AddYears(-5));

        // Assert
        pet.DomainEvents.Should().HaveCount(1);
        pet.DomainEvents.First().Should().BeOfType<PetCreatedEvent>();
    }

    [Fact]
    public void UpdatePet_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var pet = new Pet(Guid.NewGuid(), "Fluffy", PetType.Dog, DateTime.UtcNow.AddYears(-5), Breed.Create("Golden Retriever"));
        var newName = "Lucky";

        // Act
        pet.UpdateInfo(newName, Breed.Create("Labrador"), null);

        // Assert
        pet.Name.Should().Be(newName);
        pet.Breed!.Value.Should().Be("Labrador");
    }

    [Fact]
    public void UpdatePet_WithEmptyName_ThrowsException()
    {
        // Arrange
        var pet = new Pet(Guid.NewGuid(), "Fluffy", PetType.Dog, DateTime.UtcNow.AddYears(-5));

        // Act & Assert
        var act = () => pet.UpdateInfo("", null, null);
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("name");
    }

    [Fact]
    public void CreatePet_WithEmptyName_ThrowsException()
    {
        // Act & Assert
        var act = () => new Pet(Guid.NewGuid(), "", PetType.Dog, DateTime.UtcNow.AddYears(-5));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("name");
    }

    [Fact]
    public void CreatePet_WithFutureDateOfBirth_ThrowsException()
    {
        // Act & Assert
        var act = () => new Pet(Guid.NewGuid(), "Fluffy", PetType.Dog, DateTime.UtcNow.AddYears(1));
        act.Should().Throw<DomainException>().Which.Message.Should().Contain("future");
    }
}
