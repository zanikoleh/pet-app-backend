using AutoMapper;
using MediatR;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Domain.Aggregates;

namespace PetService.Application.Handlers;

/// <summary>
/// Handler for adding a document to a pet.
/// </summary>
public sealed class AddDocumentToPetCommandHandler : IRequestHandler<AddDocumentToPetCommand, DocumentDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public AddDocumentToPetCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentDto> Handle(AddDocumentToPetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        var documentId = Guid.NewGuid();
        pet.AddDocument(
            documentId,
            request.FileName,
            request.FileType,
            request.FileSizeBytes,
            request.Url,
            request.Category,
            request.Description);

        _petRepository.Update(pet);
        await _petRepository.SaveChangesAsync(cancellationToken);

        var document = pet.Documents.FirstOrDefault(d => d.Id == documentId);
        return _mapper.Map<DocumentDto>(document);
    }
}

/// <summary>
/// Handler for updating a pet document.
/// </summary>
public sealed class UpdatePetDocumentCommandHandler : IRequestHandler<UpdatePetDocumentCommand, DocumentDto>
{
    private readonly IPetRepository _petRepository;
    private readonly IMapper _mapper;

    public UpdatePetDocumentCommandHandler(IPetRepository petRepository, IMapper mapper)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentDto> Handle(UpdatePetDocumentCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        pet.UpdateDocument(request.DocumentId, request.Description);

        _petRepository.Update(pet);
        await _petRepository.SaveChangesAsync(cancellationToken);

        var document = pet.Documents.FirstOrDefault(d => d.Id == request.DocumentId);
        return _mapper.Map<DocumentDto>(document);
    }
}

/// <summary>
/// Handler for removing a document from a pet.
/// </summary>
public sealed class RemoveDocumentFromPetCommandHandler : IRequestHandler<RemoveDocumentFromPetCommand>
{
    private readonly IPetRepository _petRepository;

    public RemoveDocumentFromPetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
    }

    public async Task Handle(RemoveDocumentFromPetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null || pet.OwnerId != request.OwnerId)
            throw new NotFoundException($"Pet with id {request.PetId} not found.", "PET_NOT_FOUND");

        pet.RemoveDocument(request.DocumentId);

        _petRepository.Update(pet);
        await _petRepository.SaveChangesAsync(cancellationToken);
    }
}
