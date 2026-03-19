namespace Contracts.Events;

/// <summary>
/// Raised when a file is uploaded to the system.
/// Published by File Service.
/// </summary>
public class FileUploadedEvent : DomainEventNotification
{
    public Guid FileId { get; set; }
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Raised when a file is deleted from the system.
/// Published by File Service.
/// </summary>
public class FileDeletedEvent : DomainEventNotification
{
    public Guid FileId { get; set; }
    public Guid OwnerId { get; set; }
}
