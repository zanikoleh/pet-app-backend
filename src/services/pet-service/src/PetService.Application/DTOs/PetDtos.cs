namespace PetService.Application.DTOs;

/// <summary>
/// DTO for pet information response.
/// </summary>
public class PetDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PhotoDto> Photos { get; set; } = new();
    public List<DocumentDto> Documents { get; set; } = new();
}

/// <summary>
/// DTO for photo information.
/// </summary>
public class PhotoDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Caption { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for document information.
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for creating a new pet.
/// </summary>
public class CreatePetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating pet information.
/// </summary>
public class UpdatePetRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for adding a photo to a pet.
/// </summary>
public class AddPhotoRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for updating a photo.
/// </summary>
public class UpdatePhotoRequest
{
    public string? Caption { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO for adding a document to a pet.
/// </summary>
public class AddDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating a document.
/// </summary>
public class UpdateDocumentRequest
{
    public string? Description { get; set; }
}
