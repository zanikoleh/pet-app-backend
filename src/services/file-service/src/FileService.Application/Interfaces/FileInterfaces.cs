using FileService.Domain.Entities;
using SharedKernel.Infrastructure;

namespace FileService.Application.Interfaces;

/// <summary>
/// Service for file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads file content to storage.
    /// </summary>
    Task<string> UploadFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a signed download URL for a file.
    /// </summary>
    Task<string> GetDownloadUrlAsync(string storagePath, int expirationMinutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes file from storage.
    /// </summary>
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks file for viruses.
    /// </summary>
    Task<(bool IsClean, string? ScanResult)> ScanFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for FileRecord.
/// </summary>
public interface IFileRepository : IRepository<FileRecord, Guid>
{
    /// <summary>
    /// Gets files by user ID with pagination.
    /// </summary>
    Task<(List<FileRecord> Files, int TotalCount)> GetUserFilesAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by related entity ID with pagination.
    /// </summary>
    Task<(List<FileRecord> Files, int TotalCount)> GetFilesByEntityAsync(
        Guid relatedEntityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all files for a user.
    /// </summary>
    Task DeleteUserFilesAsync(Guid userId, CancellationToken cancellationToken = default);
}
