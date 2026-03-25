using PetService.Domain.Entities;
using PetService.Domain.Events;
using PetService.Domain.ValueObjects;
using SharedKernel;

namespace PetService.Domain.Aggregates;

/// <summary>
/// Pet aggregate root - represents a pet record
/// Manages photos and documents as part of the aggregate
/// </summary>
public sealed class Pet : AggregateRoot<Guid>
{
    // Pet properties
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PetType Type { get; private set; } = PetType.Dog;
    public Breed? Breed { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Collections managed by this aggregate
    private readonly List<Photo> _photos = new();
    private readonly List<Document> _documents = new();

    public IReadOnlyCollection<Photo> Photos => _photos.AsReadOnly();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private Pet() { }

    /// <summary>
    /// Creates a new pet aggregate
    /// </summary>
    public Pet(
        Guid ownerId,
        string name,
        PetType type,
        DateTime dateOfBirth,
        Breed? breed = null,
        string? description = null)
        : base(Guid.NewGuid())
    {
        ValidateInvariants(name, dateOfBirth);

        OwnerId = ownerId;
        Name = name;
        Type = type;
        // Ensure DateOfBirth is UTC for PostgreSQL timestamp with time zone
        DateOfBirth = dateOfBirth.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc) 
            : dateOfBirth.ToUniversalTime();
        Breed = breed;
        Description = description;
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
        RaiseDomainEvent(new PetCreatedEvent
        {
            PetId = Id,
            OwnerId = OwnerId,
            Name = Name,
            Type = Type.Value,
            Breed = Breed?.Value,
            DateOfBirth = DateOfBirth
        });
    }

    /// <summary>
    /// Updates pet information
    /// </summary>
    public void UpdateInfo(string name, Breed? breed, string? description)
    {
        ValidateInvariants(name, DateOfBirth);

        Name = name;
        Breed = breed;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PetUpdatedEvent
        {
            PetId = Id,
            Name = Name,
            Breed = Breed?.Value,
            DateOfBirth = DateOfBirth
        });
    }

    /// <summary>
    /// Adds a photo to the pet's record
    /// </summary>
    public void AddPhoto(
        Guid photoId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string? caption = null,
        string? tags = null)
    {
        if (_photos.Any(p => p.Id == photoId))
            throw new DomainException("A photo with this ID already exists for this pet.");

        var photo = new Photo(Id, photoId, fileName, fileType, fileSizeBytes, url, caption, tags);
        _photos.Add(photo);

        RaiseDomainEvent(new PhotoAddedToPetEvent
        {
            PetId = Id,
            OwnerId = OwnerId,
            PhotoId = photoId,
            PhotoUrl = url,
            UploadedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Removes a photo from the pet's record
    /// </summary>
    public void RemovePhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            throw new NotFoundException($"Photo with id {photoId} not found.", "PHOTO_NOT_FOUND");

        _photos.Remove(photo);
    }

    /// <summary>
    /// Updates a photo
    /// </summary>
    public void UpdatePhoto(Guid photoId, string? caption, string? tags)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            throw new NotFoundException($"Photo with id {photoId} not found.", "PHOTO_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(caption))
            photo.UpdateCaption(caption);

        if (!string.IsNullOrWhiteSpace(tags))
            photo.UpdateTags(tags);
    }

    /// <summary>
    /// Adds a document to the pet's record
    /// </summary>
    public void AddDocument(
        Guid documentId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string category,
        string? description = null)
    {
        if (_documents.Any(d => d.Id == documentId))
            throw new DomainException("A document with this ID already exists for this pet.");

        var document = new Document(Id, documentId, fileName, fileType, fileSizeBytes, url, category, description);
        _documents.Add(document);

        RaiseDomainEvent(new DocumentAddedToPetEvent
        {
            PetId = Id,
            OwnerId = OwnerId,
            DocumentId = documentId,
            DocumentUrl = url,
            DocumentType = category,
            UploadedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Removes a document from the pet's record
    /// </summary>
    public void RemoveDocument(Guid documentId)
    {
        var document = _documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new NotFoundException($"Document with id {documentId} not found.", "DOCUMENT_NOT_FOUND");

        _documents.Remove(document);
    }

    /// <summary>
    /// Updates document description
    /// </summary>
    public void UpdateDocument(Guid documentId, string? description)
    {
        var document = _documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            throw new NotFoundException($"Document with id {documentId} not found.", "DOCUMENT_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(description))
            document.UpdateDescription(description);
    }

    /// <summary>
    /// Validates domain invariants
    /// </summary>
    private static void ValidateInvariants(string name, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Pet name cannot be empty.");

        if (name.Length > 100)
            throw new DomainException("Pet name cannot exceed 100 characters.");

        if (dateOfBirth == default)
            throw new DomainException("Pet date of birth must be specified.");

        if (dateOfBirth >= DateTime.UtcNow)
            throw new DomainException("Pet date of birth cannot be in the future.");

        if (DateTime.UtcNow.AddYears(-200) > dateOfBirth)
            throw new DomainException("Pet date of birth seems invalid (too far in the past).");
    }
}
