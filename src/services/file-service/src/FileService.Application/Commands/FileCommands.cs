using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Commands;

/// <summary>
/// Command to upload a file.
/// </summary>
public sealed record UploadFileCommand(
    Guid UserId,
    string FileName,
    string FileType,
    byte[] FileContent,
    string? Category,
    Guid? RelatedEntityId) : IRequest<FileDto>;

/// <summary>
/// Command to delete a file.
/// </summary>
public sealed record DeleteFileCommand(Guid FileId, Guid UserId) : IRequest<Unit>;

/// <summary>
/// Command to mark file as virus-safe.
/// </summary>
public sealed record MarkFileAsVirusSafeCommand(Guid FileId) : IRequest<Unit>;

/// <summary>
/// Command to mark file as containing virus.
/// </summary>
public sealed record MarkFileAsVirusDetectedCommand(Guid FileId) : IRequest<Unit>;
