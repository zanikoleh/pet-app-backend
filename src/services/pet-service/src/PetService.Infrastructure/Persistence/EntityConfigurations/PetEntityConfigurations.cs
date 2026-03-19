using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetService.Domain.Aggregates;
using PetService.Domain.Entities;
using PetService.Domain.ValueObjects;

namespace PetService.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for Pet aggregate root.
/// </summary>
public sealed class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.Version)
            .IsConcurrencyToken();

        // Value object configuration - PetType
        builder.Property(p => p.Type)
            .HasConversion(
                pt => pt.Value,
                value => PetType.FromString(value))
            .HasMaxLength(50)
            .IsRequired();

        // Value object configuration - Breed
        builder.Property(p => p.Breed)
            .HasConversion(
                b => b != null ? b.Value : null,
                value => value != null ? Breed.Create(value) : null)
            .HasMaxLength(100);

        // Configure Photos collection
        builder.HasMany<Photo>()
            .WithOwner()
            .HasForeignKey(ph => ph.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Photos)
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        // Configure Documents collection
        builder.HasMany<Document>()
            .WithOwner()
            .HasForeignKey(d => d.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        // Create index for owner to enable fast queries
        builder.HasIndex(p => p.OwnerId)
            .HasName("IX_Pets_OwnerId");

        builder.HasIndex(p => new { p.OwnerId, p.CreatedAt })
            .HasName("IX_Pets_OwnerId_CreatedAt");
    }
}

/// <summary>
/// Entity configuration for Photo entity.
/// </summary>
public sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.PetId)
            .IsRequired();

        builder.Property(ph => ph.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ph => ph.FileType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ph => ph.FileSizeBytes)
            .IsRequired();

        builder.Property(ph => ph.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(ph => ph.UploadedAt)
            .IsRequired();

        builder.Property(ph => ph.Caption)
            .HasMaxLength(500);

        builder.Property(ph => ph.Tags)
            .HasMaxLength(500);

        // Index for finding photos by pet
        builder.HasIndex(ph => ph.PetId)
            .HasName("IX_Photos_PetId");
    }
}

/// <summary>
/// Entity configuration for Document entity.
/// </summary>
public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.PetId)
            .IsRequired();

        builder.Property(d => d.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.FileType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(d => d.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        // Index for finding documents by pet
        builder.HasIndex(d => d.PetId)
            .HasName("IX_Documents_PetId");

        builder.HasIndex(d => new { d.PetId, d.Category })
            .HasName("IX_Documents_PetId_Category");
    }
}
