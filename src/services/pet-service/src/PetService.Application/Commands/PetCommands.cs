using MediatR;
using PetService.Application.DTOs;

namespace PetService.Application.Commands;

/// <summary>
/// Command to create a new pet.
/// </summary>
public sealed record CreatePetCommand(
    Guid OwnerId,
    string Name,
    string Type,
    string? Breed,
    DateTime DateOfBirth,
    string? Description) : IRequest<PetDto>;

/// <summary>
/// Command to update an existing pet.
/// </summary>
public sealed record UpdatePetCommand(
    Guid PetId,
    Guid OwnerId,
    string Name,
    string? Breed,
    string? Description) : IRequest<PetDto>;

/// <summary>
/// Command to delete a pet.
/// </summary>
public sealed record DeletePetCommand(
    Guid PetId,
    Guid OwnerId) : IRequest<Unit>;

/// <summary>
/// Command to add a photo to a pet.
/// </summary>
public sealed record AddPhotoToPetCommand(
    Guid PetId,
    Guid OwnerId,
    string FileName,
    string FileType,
    long FileSizeBytes,
    string Url,
    string? Caption,
    string? Tags) : IRequest<PhotoDto>;

/// <summary>
/// Command to update a pet's photo.
/// </summary>
public sealed record UpdatePetPhotoCommand(
    Guid PetId,
    Guid PhotoId,
    Guid OwnerId,
    string? Caption,
    string? Tags) : IRequest<PhotoDto>;

/// <summary>
/// Command to remove a photo from a pet.
/// </summary>
public sealed record RemovePhotoFromPetCommand(
    Guid PetId,
    Guid PhotoId,
    Guid OwnerId) : IRequest<Unit>;

/// <summary>
/// Command to add a document to a pet.
/// </summary>
public sealed record AddDocumentToPetCommand(
    Guid PetId,
    Guid OwnerId,
    string FileName,
    string FileType,
    long FileSizeBytes,
    string Url,
    string Category,
    string? Description) : IRequest<DocumentDto>;

/// <summary>
/// Command to update a pet's document.
/// </summary>
public sealed record UpdatePetDocumentCommand(
    Guid PetId,
    Guid DocumentId,
    Guid OwnerId,
    string? Description) : IRequest<DocumentDto>;

/// <summary>
/// Command to remove a document from a pet.
/// </summary>
public sealed record RemoveDocumentFromPetCommand(
    Guid PetId,
    Guid DocumentId,
    Guid OwnerId) : IRequest<Unit>;
