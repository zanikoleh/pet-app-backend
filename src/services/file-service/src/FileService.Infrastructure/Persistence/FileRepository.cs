using Microsoft.EntityFrameworkCore;
using FileService.Application.Interfaces;
using FileService.Domain.Entities;
using SharedKernel.Infrastructure;

namespace FileService.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for FileRecord.
/// </summary>
public class FileRepository : RepositoryBase<FileRecord, FileServiceDbContext, Guid>, IFileRepository
{
    public FileRepository(FileServiceDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(List<FileRecord> Files, int TotalCount)> GetUserFilesAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FileRecords
            .Where(f => f.UserId == userId && !f.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var files = await query
            .OrderByDescending(f => f.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (files, totalCount);
    }

    public async Task<(List<FileRecord> Files, int TotalCount)> GetFilesByEntityAsync(
        Guid relatedEntityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FileRecords
            .Where(f => f.RelatedEntityId == relatedEntityId && !f.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var files = await query
            .OrderByDescending(f => f.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (files, totalCount);
    }

    public async Task DeleteUserFilesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var files = await _dbContext.FileRecords
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var file in files)
        {
            file.Delete();
        }
    }
}
