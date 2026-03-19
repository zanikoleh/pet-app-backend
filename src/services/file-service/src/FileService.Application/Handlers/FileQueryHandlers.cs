using MediatR;
using AutoMapper;
using FileService.Application.DTOs;
using FileService.Application.Interfaces;
using FileService.Application.Queries;

namespace FileService.Application.Handlers;

/// <summary>
/// Handler for getting file by ID.
/// </summary>
public sealed class GetFileQueryHandler : IRequestHandler<GetFileQuery, FileDto>
{
    private readonly IFileRepository _repository;
    private readonly IMapper _mapper;

    public GetFileQueryHandler(IFileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<FileDto> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        var fileRecord = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (fileRecord == null || fileRecord.IsDeleted)
            throw new NotFoundException("File not found.", "FILE_NOT_FOUND");

        return _mapper.Map<FileDto>(fileRecord);
    }
}

/// <summary>
/// Handler for getting file download URL.
/// </summary>
public sealed class GetFileDownloadUrlQueryHandler : IRequestHandler<GetFileDownloadUrlQuery, string>
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageService _storageService;

    public GetFileDownloadUrlQueryHandler(IFileRepository repository, IFileStorageService storageService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task<string> Handle(GetFileDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var fileRecord = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (fileRecord == null || fileRecord.IsDeleted)
            throw new NotFoundException("File not found.", "FILE_NOT_FOUND");

        return await _storageService.GetDownloadUrlAsync(fileRecord.StoragePath, request.ExpirationMinutes, cancellationToken);
    }
}

/// <summary>
/// Handler for listing user files.
/// </summary>
public sealed class ListUserFilesQueryHandler : IRequestHandler<ListUserFilesQuery, PaginatedListDto<FileDto>>
{
    private readonly IFileRepository _repository;
    private readonly IMapper _mapper;

    public ListUserFilesQueryHandler(IFileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedListDto<FileDto>> Handle(ListUserFilesQuery request, CancellationToken cancellationToken)
    {
        var (files,  totalCount) = await _repository.GetUserFilesAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PaginatedListDto<FileDto>
        {
            Items = _mapper.Map<List<FileDto>>(files),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for listing files by related entity.
/// </summary>
public sealed class ListFilesByEntityQueryHandler : IRequestHandler<ListFilesByEntityQuery, PaginatedListDto<FileDto>>
{
    private readonly IFileRepository _repository;
    private readonly IMapper _mapper;

    public ListFilesByEntityQueryHandler(IFileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PaginatedListDto<FileDto>> Handle(ListFilesByEntityQuery request, CancellationToken cancellationToken)
    {
        var (files, totalCount) = await _repository.GetFilesByEntityAsync(
            request.RelatedEntityId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PaginatedListDto<FileDto>
        {
            Items = _mapper.Map<List<FileDto>>(files),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
