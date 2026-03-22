using FluentAssertions;
using PetService.Domain.Aggregates;
using PetService.Domain.Entities;
using PetService.Domain.Events;
using PetService.Domain.ValueObjects;
using SharedKernel;
using Xunit;

namespace PetService.Domain.Tests.Aggregates;

public class PetAggregateTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly string _petName = "Fluffy";
    private readonly PetType _petType = PetType.Cat;
    private readonly DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-2);

    [Fact]
    public void Create_ValidInput_ShouldCreatePet()
    {
        // Act
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);

        // Assert
        pet.Id.Should().NotBe(Guid.Empty);
        pet.OwnerId.Should().Be(_ownerId);
        pet.Name.Should().Be(_petName);
        pet.Type.Should().Be(_petType);
        pet.DateOfBirth.Should().Be(_dateOfBirth);
        pet.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        pet.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithBreedAndDescription_ShouldSetAllProperties()
    {
        // Arrange
        var breed = Breed.Create("Siamese");
        var description = "Friendly cat";

        // Act
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth, breed, description);

        // Assert
        pet.Breed.Should().Be(breed);
        pet.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            new Pet(_ownerId, invalidName, _petType, _dateOfBirth));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void Create_FutureDateOfBirth_ShouldThrowDomainException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = Record.Exception(() =>
            new Pet(_ownerId, _petName, _petType, futureDate));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void Create_ShouldRaisePetCreatedEvent()
    {
        // Act
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);

        // Assert
        var domainEvents = pet.DomainEvents.ToList();
        domainEvents.Should().HaveCount(1);
        
        var petCreatedEvent = domainEvents[0] as PetCreatedEvent;
        petCreatedEvent.Should().NotBeNull();
        petCreatedEvent!.PetId.Should().Be(pet.Id);
        petCreatedEvent.OwnerId.Should().Be(_ownerId);
        petCreatedEvent.Name.Should().Be(_petName);
        petCreatedEvent.DateOfBirth.Should().Be(_dateOfBirth);
    }

    [Fact]
    public void UpdateInfo_ValidInput_ShouldUpdateProperties()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var newName = "Whiskers";
        var newBreed = Breed.Create("Persian");
        var newDescription = "Very fluffy";

        // Act
        pet.UpdateInfo(newName, newBreed, newDescription);

        // Assert
        pet.Name.Should().Be(newName);
        pet.Breed.Should().Be(newBreed);
        pet.Description.Should().Be(newDescription);
        pet.UpdatedAt.Should().NotBeNull();
        pet.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        pet.UpdatedAt.Value.Should().BeOnOrAfter(pet.CreatedAt);
    }

    [Fact]
    public void UpdateInfo_ShouldRaisePetUpdatedEvent()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var createdEventCount = pet.DomainEvents.Count;
        
        var newName = "Whiskers";
        var newBreed = Breed.Create("Persian");

        // Act
        pet.UpdateInfo(newName, newBreed, null);

        // Assert
        var domainEvents = pet.DomainEvents.ToList();
        // Should have the creation event plus the update event
        domainEvents.Should().HaveCount(createdEventCount + 1);
        
        var petUpdatedEvent = domainEvents[createdEventCount] as PetUpdatedEvent;
        petUpdatedEvent.Should().NotBeNull();
        petUpdatedEvent!.PetId.Should().Be(pet.Id);
        petUpdatedEvent.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateInfo_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);

        // Act & Assert
        var exception = Record.Exception(() => pet.UpdateInfo(invalidName, null, null));
        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void AddPhoto_ValidInput_ShouldAddPhotoToCollection()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var photoId = Guid.NewGuid();
        var fileName = "photo.jpg";
        var fileType = "image/jpeg";
        long fileSizeBytes = 1024000;
        var url = "https://example.com/photo.jpg";

        // Act
        pet.AddPhoto(photoId, fileName, fileType, fileSizeBytes, url);

        // Assert
        pet.Photos.Should().HaveCount(1);
        pet.Photos.First().Id.Should().Be(photoId);
        pet.Photos.First().FileName.Should().Be(fileName);
        pet.Photos.First().Url.Should().Be(url);
    }

    [Fact]
    public void AddPhoto_WithCaption_ShouldStoreCaption()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var photoId = Guid.NewGuid();
        var caption = "My beloved cat";

        // Act
        pet.AddPhoto(photoId, "photo.jpg", "image/jpeg", 1024000, "https://example.com/photo.jpg", caption);

        // Assert
        pet.Photos.First().Caption.Should().Be(caption);
    }

    [Fact]
    public void AddPhoto_DuplicateId_ShouldThrowDomainException()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var photoId = Guid.NewGuid();
        pet.AddPhoto(photoId, "photo1.jpg", "image/jpeg", 1024000, "https://example.com/photo1.jpg");

        // Act & Assert
        var exception = Record.Exception(() =>
            pet.AddPhoto(photoId, "photo2.jpg", "image/jpeg", 1024000, "https://example.com/photo2.jpg"));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void AddPhoto_ShouldRaisePhotoAddedEvent()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var createdEventCount = pet.DomainEvents.Count;
        var photoId = Guid.NewGuid();
        var url = "https://example.com/photo.jpg";

        // Act
        pet.AddPhoto(photoId, "photo.jpg", "image/jpeg", 1024000, url);

        // Assert
        var domainEvents = pet.DomainEvents.ToList();
        domainEvents.Should().HaveCount(createdEventCount + 1);
        
        var photoAddedEvent = domainEvents[createdEventCount] as PhotoAddedToPetEvent;
        photoAddedEvent.Should().NotBeNull();
        photoAddedEvent!.PhotoId.Should().Be(photoId);
        photoAddedEvent.PhotoUrl.Should().Be(url);
    }

    [Fact]
    public void RemovePhoto_ValidId_ShouldRemovePhoto()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var photoId = Guid.NewGuid();
        pet.AddPhoto(photoId, "photo.jpg", "image/jpeg", 1024000, "https://example.com/photo.jpg");

        // Act
        pet.RemovePhoto(photoId);

        // Assert
        pet.Photos.Should().BeEmpty();
    }

    [Fact]
    public void RemovePhoto_InvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var nonExistentPhotoId = Guid.NewGuid();

        // Act & Assert
        var exception = Record.Exception(() => pet.RemovePhoto(nonExistentPhotoId));
        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void UpdatePhoto_ValidCaption_ShouldUpdateCaption()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var photoId = Guid.NewGuid();
        pet.AddPhoto(photoId, "photo.jpg", "image/jpeg", 1024000, "https://example.com/photo.jpg");
        var newCaption = "Updated caption";

        // Act
        pet.UpdatePhoto(photoId, newCaption, null);

        // Assert
        pet.Photos.First().Caption.Should().Be(newCaption);
    }

    [Fact]
    public void AddMultiplePhotos_ShouldHaveAllPhotos()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);

        // Act
        for (int i = 0; i < 3; i++)
        {
            pet.AddPhoto(Guid.NewGuid(), $"photo{i}.jpg", "image/jpeg", 1024000, $"https://example.com/photo{i}.jpg");
        }

        // Assert
        pet.Photos.Should().HaveCount(3);
    }

    [Fact]
    public void AddDocument_ValidInput_ShouldAddDocumentToCollection()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var documentId = Guid.NewGuid();
        var fileName = "vaccination.pdf";
        var fileType = "application/pdf";
        long fileSizeBytes = 256000;
        var url = "https://example.com/vaccination.pdf";
        var category = "vaccination";

        // Act
        pet.AddDocument(documentId, fileName, fileType, fileSizeBytes, url, category);

        // Assert
        pet.Documents.Should().HaveCount(1);
        pet.Documents.First().Id.Should().Be(documentId);
        pet.Documents.First().FileName.Should().Be(fileName);
        pet.Documents.First().Category.Should().Be(category);
    }

    [Fact]
    public void RemoveDocument_ValidId_ShouldRemoveDocument()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var documentId = Guid.NewGuid();
        pet.AddDocument(documentId, "vaccination.pdf", "application/pdf", 256000, "https://example.com/vaccination.pdf", "vaccination");

        // Act
        pet.RemoveDocument(documentId);

        // Assert
        pet.Documents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDocument_ValidDescription_ShouldUpdateDescription()
    {
        // Arrange
        var pet = new Pet(_ownerId, _petName, _petType, _dateOfBirth);
        var documentId = Guid.NewGuid();
        pet.AddDocument(documentId, "vaccination.pdf", "application/pdf", 256000, "https://example.com/vaccination.pdf", "vaccination");
        var newDescription = "Annual vaccination 2024";

        // Act
        pet.UpdateDocument(documentId, newDescription);

        // Assert
        pet.Documents.First().Description.Should().Be(newDescription);
    }
}
