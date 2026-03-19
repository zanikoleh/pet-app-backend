using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Queries;

/// <summary>
/// Query to get file by ID.
/// </summary>
public sealed record GetFileQuery(Guid FileId) : IRequest<FileDto>;

/// <summary>
/// Query to get file download URL.
/// </summary>
public sealed record GetFileDownloadUrlQuery(Guid FileId, int ExpirationMinutes = 60) : IRequest<string>;

/// <summary>
/// Query to list files by user.
/// </summary>
public sealed record ListUserFilesQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedListDto<FileDto>>;

/// <summary>
/// Query to list files by related entity.
/// </summary>
public sealed record ListFilesByEntityQuery(Guid RelatedEntityId, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedListDto<FileDto>>;
