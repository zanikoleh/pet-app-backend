using AutoMapper;
using FluentAssertions;
using Moq;
using MediatR;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Application.Handlers;
using PetService.Domain.Aggregates;
using PetService.Domain.ValueObjects;
using SharedKernel;
using Xunit;

namespace PetService.Application.Tests.Handlers;

/// <summary>
/// Tests for Pet Command Handlers - demonstrates testing patterns for application handlers.
/// Shows mocking patterns for IPetRepository and IMapper.
/// </summary>
public class PetCommandHandlerTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _petId = Guid.NewGuid();
    private readonly DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-2);

    public PetCommandHandlerTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task CreatePetCommandHandler_ValidInput_ShouldCallRepository()
    {
        // Arrange
        var handler = new CreatePetCommandHandler(_petRepositoryMock.Object, _mapperMock.Object);
        var command = new CreatePetCommand(
            _ownerId,
            "Fluffy",
            "cat",
            "Siamese",
            _dateOfBirth,
            "A beautiful cat");

        var expectedDto = new PetDto
        {
            Id = _petId,
            OwnerId = _ownerId,
            Name = "Fluffy",
            Type = "cat",
            Breed = "Siamese",
            DateOfBirth = _dateOfBirth,
            Description = "A beautiful cat",
            CreatedAt = DateTime.UtcNow
        };

        _mapperMock
            .Setup(m => m.Map<PetDto>(It.IsAny<Pet>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Fluffy");
        result.Type.Should().Be("cat");
        
        // Verify repository was called
        _petRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PetDto>(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public async Task CreatePetCommandHandler_WithoutBreed_ShouldCreatePetSuccessfully()
    {
        // Arrange
        var handler = new CreatePetCommandHandler(_petRepositoryMock.Object, _mapperMock.Object);
        var command = new CreatePetCommand(
            _ownerId,
            "Max",
            "dog",
            null,
            _dateOfBirth,
            null);

        var expectedDto = new PetDto
        {
            Id = _petId,
            OwnerId = _ownerId,
            Name = "Max",
            Type = "dog",
            Breed = null,
            DateOfBirth = _dateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        _mapperMock
            .Setup(m => m.Map<PetDto>(It.IsAny<Pet>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Breed.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePetCommandHandler_ValidInput_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var handler = new UpdatePetCommandHandler(_petRepositoryMock.Object, _mapperMock.Object);
        var command = new UpdatePetCommand(
            _petId,
            _ownerId,
            "UpdatedName",
            "Persian",
            "Updated description");

        var existingPet = new Pet(_ownerId, "OldName", PetType.Cat, _dateOfBirth);

        var expectedDto = new PetDto
        {
            Id = _petId,
            OwnerId = _ownerId,
            Name = "UpdatedName",
            Type = "cat",
            Breed = "Persian",
            Description = "Updated description",
            CreatedAt = DateTime.UtcNow
        };

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        _mapperMock
            .Setup(m => m.Map<PetDto>(It.IsAny<Pet>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("UpdatedName");
        
        _petRepositoryMock.Verify(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()), Times.Once);
        _petRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePetCommandHandler_PetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new UpdatePetCommandHandler(_petRepositoryMock.Object, _mapperMock.Object);
        var command = new UpdatePetCommand(_petId, _ownerId, "Name", null, null);

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdatePetCommandHandler_DifferentOwner_ShouldThrowNotFoundException()
    {
        // Arrange
        var differentOwnerId = Guid.NewGuid();
        var handler = new UpdatePetCommandHandler(_petRepositoryMock.Object, _mapperMock.Object);
        var command = new UpdatePetCommand(_petId, differentOwnerId, "Name", null, null);

        var existingPet = new Pet(_ownerId, "OldName", PetType.Cat, _dateOfBirth);

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task DeletePetCommandHandler_PetExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var handler = new DeletePetCommandHandler(_petRepositoryMock.Object);
        var command = new DeletePetCommand(_petId, _ownerId);
        var existingPet = new Pet(_ownerId, "Fluffy", PetType.Cat, _dateOfBirth);

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        _petRepositoryMock.Verify(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()), Times.Once);
        _petRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePetCommandHandler_PetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new DeletePetCommandHandler(_petRepositoryMock.Object);
        var command = new DeletePetCommand(_petId, _ownerId);

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task DeletePetCommandHandler_WrongOwner_ShouldThrowNotFoundException()
    {
        // Arrange
        var differentOwnerId = Guid.NewGuid();
        var handler = new DeletePetCommandHandler(_petRepositoryMock.Object);
        var command = new DeletePetCommand(_petId, differentOwnerId);
        var existingPet = new Pet(_ownerId, "Fluffy", PetType.Cat, _dateOfBirth);

        _petRepositoryMock
            .Setup(r => r.GetByIdAsync(_petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}

