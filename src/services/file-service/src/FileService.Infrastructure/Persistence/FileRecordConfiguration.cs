using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileService.Domain.Entities;
using SharedKernel.Infrastructure;

namespace FileService.Infrastructure.Persistence;

/// <summary>
/// Entity configuration for FileRecord.
/// </summary>
public class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        builder.ToTable("FileRecords");

        // Primary key
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        // Properties
        builder.Property(f => f.UserId).IsRequired();
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(500);
        builder.Property(f => f.FileType).IsRequired().HasMaxLength(100);
        builder.Property(f => f.FileSize);
        builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(f => f.IsVirusSafe);
        builder.Property(f => f.Category).HasMaxLength(50);
        builder.Property(f => f.RelatedEntityId);
        builder.Property(f => f.UploadedAt);
        builder.Property(f => f.DeletedAt);
        builder.Property(f => f.IsDeleted);

        // Indices
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.RelatedEntityId);
        builder.HasIndex(f => f.IsDeleted);
        builder.HasIndex(f => f.UploadedAt);

        // Navigation ignored
        builder.Ignore(f => f.DomainEvents);
    }
}
