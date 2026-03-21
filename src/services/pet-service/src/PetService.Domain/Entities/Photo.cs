using SharedKernel;

namespace PetService.Domain.Entities;

/// <summary>
/// Photo entity - child entity of Pet aggregate (not an aggregate root itself)
/// </summary>
public sealed class Photo : Entity<Guid>
{
    public Guid PetId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public string? Caption { get; private set; }
    public string? Tags { get; private set; }

    private Photo() { }

    public Photo(
        Guid petId,
        Guid photoId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string? caption = null,
        string? tags = null)
        : base(photoId)
    {
        PetId = petId;
        FileName = fileName;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
        Url = url;
        UploadedAt = DateTime.UtcNow;
        Caption = caption;
        Tags = tags;
    }

    public void UpdateCaption(string caption)
    {
        Caption = caption;
    }

    public void UpdateTags(string tags)
    {
        Tags = tags;
    }
}

/// <summary>
/// Document entity - child entity of Pet aggregate
/// </summary>
public sealed class Document : Entity<Guid>
{
    public Guid PetId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty; // "medical", "achievement", "vaccination", "other"
    public DateTime UploadedAt { get; private set; }
    public string? Description { get; private set; }

    private Document() { }

    public Document(
        Guid petId,
        Guid documentId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string category,
        string? description = null)
        : base(documentId)
    {
        PetId = petId;
        FileName = fileName;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
        Url = url;
        Category = category;
        UploadedAt = DateTime.UtcNow;
        Description = description;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}
