namespace PetService.Domain.Events;

/// <summary>
/// Domain event raised when a pet is created
/// </summary>
public sealed class PetCreatedEvent : DomainEvent
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
}

/// <summary>
/// Domain event raised when pet information is updated
/// </summary>
public sealed class PetUpdatedEvent : DomainEvent
{
    public Guid PetId { get; set; }
    public string? Name { get; set; }
    public string? Breed { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

/// <summary>
/// Domain event raised when a photo is added to a pet
/// </summary>
public sealed class PhotoAddedToPetEvent : DomainEvent
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid PhotoId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Domain event raised when a document is added to a pet
/// </summary>
public sealed class DocumentAddedToPetEvent : DomainEvent
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
