namespace FileService.Application.DTOs;

/// <summary>
/// DTO for file record.
/// </summary>
public class FileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Category { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsVirusSafe { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Paginated list DTO.
/// </summary>
public class PaginatedListDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>
/// Request to upload file.
/// </summary>
public class UploadFileRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
