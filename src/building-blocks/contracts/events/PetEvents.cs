namespace Contracts.Events;

/// <summary>
/// Raised when a new pet is created in the system.
/// Published by Pet Service.
/// Subscribed by User Profile Service and Notification Service.
/// </summary>
public class PetCreatedEvent : DomainEventNotification
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "dog", "cat", etc.
    public string? Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
}

/// <summary>
/// Raised when a pet's information is updated.
/// Published by Pet Service.
/// </summary>
public class PetUpdatedEvent : DomainEventNotification
{
    public Guid PetId { get; set; }
    public string? Name { get; set; }
    public string? Breed { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

/// <summary>
/// Raised when a photo is added to a pet's record.
/// Published by Pet Service.
/// Subscribed by File Service and Notification Service.
/// </summary>
public class PhotoAddedToPetEvent : DomainEventNotification
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid PhotoId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Raised when a document is added to a pet's record.
/// Published by Pet Service.
/// Subscribed by File Service and Notification Service.
/// </summary>
public class DocumentAddedToPetEvent : DomainEventNotification
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // "medical", "achievement", etc.
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Raised when a pet record is deleted.
/// Published by Pet Service.
/// Subscribed by File Service and Notification Service.
/// </summary>
public class PetDeletedEvent : DomainEventNotification
{
    public Guid PetId { get; set; }
    public Guid OwnerId { get; set; }
}
