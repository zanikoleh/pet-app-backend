using MediatR;
using PetService.Application.DTOs;

namespace PetService.Application.Queries;

/// <summary>
/// Query to get a pet by ID.
/// </summary>
public sealed record GetPetQuery(Guid PetId, Guid OwnerId) : IRequest<PetDto>;

/// <summary>
/// Query to get all pets of an owner.
/// </summary>
public sealed record GetOwnerPetsQuery(
    Guid OwnerId,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResponse<PetDto>>;

/// <summary>
/// Query to search pets by name.
/// </summary>
public sealed record SearchPetsQuery(
    Guid OwnerId,
    string SearchTerm,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResponse<PetDto>>;

/// <summary>
/// Query to get pets of a specific type.
/// </summary>
public sealed record GetPetsByTypeQuery(
    Guid OwnerId,
    string Type,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResponse<PetDto>>;

/// <summary>
/// Query to get photos of a pet.
/// </summary>
public sealed record GetPetPhotosQuery(
    Guid PetId,
    Guid OwnerId) : IRequest<List<PhotoDto>>;

/// <summary>
/// Query to get a specific photo.
/// </summary>
public sealed record GetPhotoQuery(
    Guid PetId,
    Guid PhotoId,
    Guid OwnerId) : IRequest<PhotoDto>;

/// <summary>
/// Query to get documents of a pet.
/// </summary>
public sealed record GetPetDocumentsQuery(
    Guid PetId,
    Guid OwnerId) : IRequest<List<DocumentDto>>;

/// <summary>
/// Query to get a specific document.
/// </summary>
public sealed record GetDocumentQuery(
    Guid PetId,
    Guid DocumentId,
    Guid OwnerId) : IRequest<DocumentDto>;
