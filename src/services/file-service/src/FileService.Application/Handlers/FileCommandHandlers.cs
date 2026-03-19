using MediatR;
using AutoMapper;
using FileService.Application.Commands;
using FileService.Application.DTOs;
using FileService.Application.Interfaces;
using FileService.Domain.Entities;
using SharedKernel;

namespace FileService.Application.Handlers;

/// <summary>
/// Handler for uploading files.
/// </summary>
public sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileDto>
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageService _storageService;
    private readonly IMapper _mapper;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    public UploadFileCommandHandler(
        IFileRepository repository,
        IFileStorageService storageService,
        IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<FileDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Validate file
        if (request.FileContent.Length == 0 || request.FileContent.Length > MaxFileSize)
            throw new BusinessLogicException("File size must be between 0 and 50 MB.", "INVALID_FILE_SIZE");

        // Scan for viruses
        var (isClean, _) = await _storageService.ScanFileAsync(
            request.FileContent,
            request.FileName,
            cancellationToken);

        if (!isClean)
            throw new BusinessLogicException("File appears to contain malware.", "VIRUS_DETECTED");

        // Upload file
        var storagePath = await _storageService.UploadFileAsync(
            request.FileName,
            request.FileContent,
            cancellationToken);

        // Create record
        var fileRecord = new FileRecord(
            Guid.NewGuid(),
            request.UserId,
            request.FileName,
            request.FileType,
            request.FileContent.Length,
            storagePath,
            request.Category,
            request.RelatedEntityId);

        fileRecord.MarkAsVirusSafe();
        _repository.Add(fileRecord);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FileDto>(fileRecord);
    }
}

/// <summary>
/// Handler for deleting files.
/// </summary>
public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand>
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageService _storageService;

    public DeleteFileCommandHandler(IFileRepository repository, IFileStorageService storageService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var fileRecord = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (fileRecord == null || fileRecord.UserId != request.UserId)
            throw new NotFoundException("File not found or access denied.", "FILE_NOT_FOUND");

        fileRecord.Delete();
        await _repository.SaveChangesAsync(cancellationToken);

        // Delete from storage
        await _storageService.DeleteFileAsync(fileRecord.StoragePath, cancellationToken);
    }
}

/// <summary>
/// Handler for marking file as virus-safe.
/// </summary>
public sealed class MarkFileAsVirusSafeCommandHandler : IRequestHandler<MarkFileAsVirusSafeCommand>
{
    private readonly IFileRepository _repository;

    public MarkFileAsVirusSafeCommandHandler(IFileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Handle(MarkFileAsVirusSafeCommand request, CancellationToken cancellationToken)
    {
        var fileRecord = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (fileRecord == null)
            throw new NotFoundException("File not found.", "FILE_NOT_FOUND");

        fileRecord.MarkAsVirusSafe();
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for marking file as virus detected.
/// </summary>
public sealed class MarkFileAsVirusDetectedCommandHandler : IRequestHandler<MarkFileAsVirusDetectedCommand>
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageService _storageService;

    public MarkFileAsVirusDetectedCommandHandler(IFileRepository repository, IFileStorageService storageService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task Handle(MarkFileAsVirusDetectedCommand request, CancellationToken cancellationToken)
    {
        var fileRecord = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (fileRecord == null)
            throw new NotFoundException("File not found.", "FILE_NOT_FOUND");

        fileRecord.MarkAsVirusDetected();
        await _repository.SaveChangesAsync(cancellationToken);

        // Delete from storage
        await _storageService.DeleteFileAsync(fileRecord.StoragePath, cancellationToken);
    }
}
