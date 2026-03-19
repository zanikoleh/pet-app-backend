using Microsoft.EntityFrameworkCore;
using FileService.Domain.Entities;
using SharedKernel.Infrastructure;

namespace FileService.Infrastructure.Persistence;

/// <summary>
/// Database context for File Service.
/// </summary>
public class FileServiceDbContext : ApplicationDbContextBase
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileRecord> FileRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new FileRecordConfiguration());
    }
}
