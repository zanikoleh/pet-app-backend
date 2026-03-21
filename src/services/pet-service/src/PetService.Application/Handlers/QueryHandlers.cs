using AutoMapper;
using Infrastructure;
using MediatR;
using PetService.Application.DTOs;
using PetService.Application.Queries;
using PetService.Domain.Aggregates;
using PetService.Domain.Specifications;
using SharedKernel;

namespace PetService.Application.Handlers;

/// <summary>
/// Handler for getting a pet by ID.
/// </summary>
public sealed class GetPetQueryHandler : IRequestHandler<GetPetQuery, PetDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetPetQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PetDto> Handle(GetPetQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetPetByIdAndOwnerSpecification(request.PetId, request.OwnerId);
        var pet = await _petRepository.FindAsync(specification, cancellationToken);

        if (pet == null)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        return _mapper.Map<PetDto>(pet);
    }
}

/// <summary>
/// Handler for getting all pets of an owner.
/// </summary>
public sealed class GetOwnerPetsQueryHandler : IRequestHandler<GetOwnerPetsQuery, PaginatedResponse<PetDto>>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetOwnerPetsQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedResponse<PetDto>> Handle(GetOwnerPetsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetPetsByOwnerSpecification(request.OwnerId, request.Page, request.PageSize);
        var pets = await _petRepository.FindAsync(specification, cancellationToken);

        var totalCount = await _petRepository.CountAsync(
            new PetsByOwnerCountSpecification(request.OwnerId),
            cancellationToken);

        var dtos = _mapper.Map<List<PetDto>>(pets);
        return PaginatedResponse<PetDto>.Create(dtos, request.Page, request.PageSize, totalCount);
    }
}

/// <summary>
/// Handler for searching pets by name.
/// </summary>
public sealed class SearchPetsQueryHandler : IRequestHandler<SearchPetsQuery, PaginatedResponse<PetDto>>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public SearchPetsQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedResponse<PetDto>> Handle(SearchPetsQuery request, CancellationToken cancellationToken)
    {
        var specification = new SearchPetsByNameSpecification(
            request.OwnerId,
            request.SearchTerm,
            request.Page,
            request.PageSize);

        var pets = await _petRepository.FindAsync(specification, cancellationToken);

        var totalCount = await _petRepository.CountAsync(
            new SearchPetsByNameCountSpecification(request.OwnerId, request.SearchTerm),
            cancellationToken);

        var dtos = _mapper.Map<List<PetDto>>(pets);
        return PaginatedResponse<PetDto>.Create(dtos, request.Page, request.PageSize, totalCount);
    }
}

/// <summary>
/// Handler for getting pets of a specific type.
/// </summary>
public sealed class GetPetsByTypeQueryHandler : IRequestHandler<GetPetsByTypeQuery, PaginatedResponse<PetDto>>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetPetsByTypeQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedResponse<PetDto>> Handle(GetPetsByTypeQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetPetsByOwnerAndTypeSpecification(
            request.OwnerId,
            request.Type,
            request.Page,
            request.PageSize);

        var pets = await _petRepository.FindAsync(specification, cancellationToken);

        var totalCount = await _petRepository.CountAsync(
            new PetsByOwnerAndTypeCountSpecification(request.OwnerId, request.Type),
            cancellationToken);

        var dtos = _mapper.Map<List<PetDto>>(pets);
        return PaginatedResponse<PetDto>.Create(dtos, request.Page, request.PageSize, totalCount);
    }
}

/// <summary>
/// Handler for getting photos of a pet.
/// </summary>
public sealed class GetPetPhotosQueryHandler : IRequestHandler<GetPetPhotosQuery, List<PhotoDto>>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetPetPhotosQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PhotoDto>> Handle(GetPetPhotosQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        return _mapper.Map<List<PhotoDto>>(pet.Photos.ToList());
    }
}

/// <summary>
/// Handler for getting a specific photo.
/// </summary>
public sealed class GetPhotoQueryHandler : IRequestHandler<GetPhotoQuery, PhotoDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetPhotoQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PhotoDto> Handle(GetPhotoQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        var photo = pet.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
        if (photo == null)
            throw new NotFoundException($"Photo with id {request.PhotoId} not found.", "PHOTO_NOT_FOUND");

        return _mapper.Map<PhotoDto>(photo);
    }
}

/// <summary>
/// Handler for getting documents of a pet.
/// </summary>
public sealed class GetPetDocumentsQueryHandler : IRequestHandler<GetPetDocumentsQuery, List<DocumentDto>>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetPetDocumentsQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<DocumentDto>> Handle(GetPetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        return _mapper.Map<List<DocumentDto>>(pet.Documents.ToList());
    }
}

/// <summary>
/// Handler for getting a specific document.
/// </summary>
public sealed class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public GetDocumentQueryHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentDto> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        var document = pet.Documents.FirstOrDefault(d => d.Id == request.DocumentId);
        if (document == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found.", "DOCUMENT_NOT_FOUND");

        return _mapper.Map<DocumentDto>(document);
    }
}
