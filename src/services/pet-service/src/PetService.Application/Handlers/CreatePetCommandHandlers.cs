using AutoMapper;
using MediatR;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Domain.Aggregates;
using PetService.Domain.ValueObjects;
using SharedKernel;

namespace PetService.Application.Handlers;

/// <summary>
/// Handler for creating a new pet.
/// </summary>
public sealed class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, PetDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public CreatePetCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PetDto> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var petType = PetType.FromString(request.Type);
        var breed = !string.IsNullOrWhiteSpace(request.Breed) ? Breed.Create(request.Breed) : null;

        var pet = new Pet(
            request.OwnerId,
            request.Name,
            petType,
            request.DateOfBirth,
            breed,
            request.Description);

        await _petRepository.AddAsync(pet, cancellationToken);
        await _petRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetDto>(pet);
    }
}

/// <summary>
/// Handler for updating a pet.
/// </summary>
public sealed class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, PetDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public UpdatePetCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PetDto> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        var breed = !string.IsNullOrWhiteSpace(request.Breed) ? Breed.Create(request.Breed) : null;
        pet.UpdateInfo(request.Name, breed, request.Description);

        await _petRepository.UpdateAsync(pet, cancellationToken);
        await _petRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetDto>(pet);
    }
}

/// <summary>
/// Handler for deleting a pet.
/// </summary>
public sealed class DeletePetCommandHandler : IRequestHandler<DeletePetCommand, Unit>
{
    private readonly IPetRepository _petRepository;

    public DeletePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
    }

    public async Task<Unit> Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        await _petRepository.DeleteAsync(pet.Id, cancellationToken);
        await _petRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

/// <summary>
/// Handler for adding a photo to a pet.
/// </summary>
public sealed class AddPhotoToPetCommandHandler : IRequestHandler<AddPhotoToPetCommand, PhotoDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public AddPhotoToPetCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PhotoDto> Handle(AddPhotoToPetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        var photoId = Guid.NewGuid();
        pet.AddPhoto(
            photoId,
            request.FileName,
            request.FileType,
            request.FileSizeBytes,
            request.Url,
            request.Caption,
            request.Tags);

        await _petRepository.UpdateAsync(pet, cancellationToken);
        await _petRepository.SaveChangesAsync(cancellationToken);

        var photo = pet.Photos.FirstOrDefault(p => p.Id == photoId);
        return _mapper.Map<PhotoDto>(photo);
    }
}

/// <summary>
/// Handler for updating a pet photo.
/// </summary>
public sealed class UpdatePetPhotoCommandHandler : IRequestHandler<UpdatePetPhotoCommand, PhotoDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public UpdatePetPhotoCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PhotoDto> Handle(UpdatePetPhotoCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        pet.UpdatePhoto(request.PhotoId, request.Caption, request.Tags);

        await _petRepository.UpdateAsync(pet);
        await _petRepository.SaveChangesAsync(cancellationToken);

        var photo = pet.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
        return _mapper.Map<PhotoDto>(photo);
    }
}

/// <summary>
/// Handler for removing a photo from a pet.
/// </summary>
public sealed class RemovePhotoFromPetCommandHandler : IRequestHandler<RemovePhotoFromPetCommand, Unit>
{
    private readonly IPetRepository _petRepository;

    public RemovePhotoFromPetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
    }

    public async Task<Unit> Handle(RemovePhotoFromPetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        pet.RemovePhoto(request.PhotoId);

        await _petRepository.UpdateAsync(pet);
        await _petRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
