using SharedKernel;

namespace PetService.Domain.ValueObjects;

/// <summary>
/// Value object for photo metadata
/// </summary>
public sealed class PhotoMetadata : ValueObject
{
    public Guid PhotoId { get; }
    public string FileName { get; }
    public string FileType { get; } // "image/jpeg", "image/png", etc.
    public long FileSizeBytes { get; }
    public string Url { get; } // URL or path to the photo
    public DateTime UploadedAt { get; }
    public string? Caption { get; }
    public string? Tags { get; }

    private PhotoMetadata(
        Guid photoId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        DateTime uploadedAt,
        string? caption = null,
        string? tags = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Photo file name cannot be empty.");

        if (fileSizeBytes <= 0)
            throw new DomainException("Photo file size must be greater than 0.");

        if (fileSizeBytes > 50 * 1024 * 1024) // 50 MB limit
            throw new DomainException("Photo file size cannot exceed 50 MB.");

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Photo URL cannot be empty.");

        PhotoId = photoId;
        FileName = fileName;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
        Url = url;
        UploadedAt = uploadedAt;
        Caption = caption;
        Tags = tags;
    }

    public static PhotoMetadata Create(
        Guid photoId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string? caption = null,
        string? tags = null)
    {
        return new PhotoMetadata(
            photoId,
            fileName,
            fileType,
            fileSizeBytes,
            url,
            DateTime.UtcNow,
            caption,
            tags);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return PhotoId;
        yield return FileName;
        yield return Url;
        yield return UploadedAt;
    }
}

/// <summary>
/// Value object for document metadata
/// </summary>
public sealed class DocumentMetadata : ValueObject
{
    public Guid DocumentId { get; }
    public string FileName { get; }
    public string FileType { get; } // "application/pdf", "image/jpeg", etc.
    public long FileSizeBytes { get; }
    public string Url { get; } // URL or path to the document
    public string DocumentCategory { get; } // "medical", "achievement", "vaccination", "other"
    public DateTime UploadedAt { get; }
    public string? Description { get; }

    private DocumentMetadata(
        Guid documentId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string documentCategory,
        DateTime uploadedAt,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Document file name cannot be empty.");

        if (fileSizeBytes <= 0)
            throw new DomainException("Document file size must be greater than 0.");

        if (fileSizeBytes > 100 * 1024 * 1024) // 100 MB limit
            throw new DomainException("Document file size cannot exceed 100 MB.");

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Document URL cannot be empty.");

        DocumentId = documentId;
        FileName = fileName;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
        Url = url;
        DocumentCategory = documentCategory;
        UploadedAt = uploadedAt;
        Description = description;
    }

    public static DocumentMetadata Create(
        Guid documentId,
        string fileName,
        string fileType,
        long fileSizeBytes,
        string url,
        string documentCategory,
        string? description = null)
    {
        return new DocumentMetadata(
            documentId,
            fileName,
            fileType,
            fileSizeBytes,
            url,
            documentCategory,
            DateTime.UtcNow,
            description);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DocumentId;
        yield return FileName;
        yield return Url;
        yield return UploadedAt;
    }
}
