using SharedKernel;

namespace FileService.Domain.Entities;

/// <summary>
/// Represents a file/document in the system.
/// </summary>
public class FileRecord : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty; // MIME type
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public bool IsVirusSafe { get; private set; } = false;
    public string? Category { get; private set; } // "pet-document", "profile-picture", "prescription", etc.
    public Guid? RelatedEntityId { get; private set; } // Pet ID, User ID, etc.
    public DateTime UploadedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    protected FileRecord() { }

    public FileRecord(
        Guid id,
        Guid userId,
        string fileName,
        string fileType,
        long fileSize,
        string storagePath,
        string? category = null,
        Guid? relatedEntityId = null) : base(id)
    {
        UserId = userId;
        FileName = fileName;
        FileType = fileType;
        FileSize = fileSize;
        StoragePath = storagePath;
        Category = category;
        RelatedEntityId = relatedEntityId;
        UploadedAt = DateTime.UtcNow;
        IsVirusSafe = false;
    }

    public void MarkAsVirusSafe()
    {
        IsVirusSafe = true;
    }

    public void MarkAsVirusDetected()
    {
        IsVirusSafe = false;
        Delete();
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
