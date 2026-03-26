using FluentAssertions;
using FileService.Domain.Entities;
using Xunit;

namespace FileService.Domain.Tests.Entities;

/// <summary>
/// Tests for FileRecord domain entity.
/// </summary>
public class FileRecordTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly string _fileName = "document.pdf";
    private readonly string _fileType = "application/pdf";
    private readonly long _fileSize = 1024 * 100; // 100 KB
    private readonly string _storagePath = "files/document_001.pdf";

    [Fact]
    public void Create_WithValidInput_ShouldCreateFileRecord()
    {
        // Act
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);

        // Assert
        fileRecord.Id.Should().Be(_fileId);
        fileRecord.UserId.Should().Be(_userId);
        fileRecord.FileName.Should().Be(_fileName);
        fileRecord.FileType.Should().Be(_fileType);
        fileRecord.FileSize.Should().Be(_fileSize);
        fileRecord.StoragePath.Should().Be(_storagePath);
        fileRecord.IsDeleted.Should().BeFalse();
        fileRecord.IsVirusSafe.Should().BeFalse();
        fileRecord.DeletedAt.Should().BeNull();
        fileRecord.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithCategoryAndRelatedEntity_ShouldSetProperties()
    {
        // Arrange
        var category = "pet-document";
        var relatedEntityId = Guid.NewGuid();

        // Act
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath,
            category,
            relatedEntityId);

        // Assert
        fileRecord.Category.Should().Be(category);
        fileRecord.RelatedEntityId.Should().Be(relatedEntityId);
    }

    [Fact]
    public void Create_WithNullCategory_ShouldAllowNullValue()
    {
        // Act
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath,
            null,
            null);

        // Assert
        fileRecord.Category.Should().BeNull();
        fileRecord.RelatedEntityId.Should().BeNull();
    }

    [Fact]
    public void MarkAsVirusSafe_ShouldSetIsVirusSafeToTrue()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);

        // Act
        fileRecord.MarkAsVirusSafe();

        // Assert
        fileRecord.IsVirusSafe.Should().BeTrue();
    }

    [Fact]
    public void MarkAsVirusDetected_ShouldMarkAsDeletedAndSetIsVirusSafeToFalse()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);
        fileRecord.MarkAsVirusSafe();

        // Act
        fileRecord.MarkAsVirusDetected();

        // Assert
        fileRecord.IsVirusSafe.Should().BeFalse();
        fileRecord.IsDeleted.Should().BeTrue();
        fileRecord.DeletedAt.Should().NotBeNull();
        fileRecord.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_ShouldMarkFileAsDeleted()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);

        // Act
        fileRecord.Delete();

        // Assert
        fileRecord.IsDeleted.Should().BeTrue();
        fileRecord.DeletedAt.Should().NotBeNull();
        fileRecord.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_CalledMultipleTimes_ShouldUpdateDeletedAtEachTime()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);

        // Act
        fileRecord.Delete();
        var firstDeletionTime = fileRecord.DeletedAt;
        
        // Small delay to ensure different timestamps
        System.Threading.Thread.Sleep(10);
        fileRecord.Delete();
        var secondDeletionTime = fileRecord.DeletedAt;

        // Assert
        firstDeletionTime.Should().NotBeNull();
        secondDeletionTime.Should().NotBeNull();
        (secondDeletionTime >= firstDeletionTime).Should().BeTrue();
    }

    [Fact]
    public void Restore_ShouldUnmarkFileAsDeleted()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);
        fileRecord.Delete();

        // Act
        fileRecord.Restore();

        // Assert
        fileRecord.IsDeleted.Should().BeFalse();
        fileRecord.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotDeleted_ShouldNotThrow()
    {
        // Arrange
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath);

        // Act & Assert
        var exception = Record.Exception(() => fileRecord.Restore());
        exception.Should().BeNull();
        fileRecord.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50 * 1024 * 1024 + 1)] // Just over 50 MB limit
    public void Create_WithVariousFileSizes_ShouldAcceptAnySizeInDomain(long fileSize)
    {
        // Act
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            fileSize,
            _storagePath);

        // Assert - Domain doesn't enforce size limits, only application layer does
        fileRecord.FileSize.Should().Be(fileSize);
    }

    [Fact]
    public void Create_WithDifferentFileTypes_ShouldStoreFileTypeCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain",
            "application/octet-stream"
        };

        // Act & Assert
        foreach (var fileType in testCases)
        {
            var fileRecord = new FileRecord(
                Guid.NewGuid(),
                _userId,
                $"file.{fileType.Split('/')[1]}",
                fileType,
                _fileSize,
                _storagePath);

            fileRecord.FileType.Should().Be(fileType);
        }
    }

    [Fact]
    public void Create_WithDifferentCategories_ShouldStoreCategoryCorrectly()
    {
        // Arrange
        var categories = new[] { "pet-document", "profile-picture", "prescription", "identity-verification" };

        // Act & Assert
        foreach (var category in categories)
        {
            var fileRecord = new FileRecord(
                Guid.NewGuid(),
                _userId,
                _fileName,
                _fileType,
                _fileSize,
                _storagePath,
                category);

            fileRecord.Category.Should().Be(category);
        }
    }

    [Fact]
    public void WorkflowTest_CompleteFileLifecycle()
    {
        // Arrange & Act
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            _fileName,
            _fileType,
            _fileSize,
            _storagePath,
            "pet-document",
            Guid.NewGuid());

        // Step 1: File created but not safe yet
        fileRecord.IsVirusSafe.Should().BeFalse();
        fileRecord.IsDeleted.Should().BeFalse();

        // Step 2: After virus scan passes
        fileRecord.MarkAsVirusSafe();
        fileRecord.IsVirusSafe.Should().BeTrue();

        // Step 3: User deletes file
        fileRecord.Delete();
        fileRecord.IsDeleted.Should().BeTrue();
        fileRecord.IsVirusSafe.Should().BeTrue(); // Remains safe even after deletion

        // Step 4: User restores file
        fileRecord.Restore();
        fileRecord.IsDeleted.Should().BeFalse();
        fileRecord.IsVirusSafe.Should().BeTrue();
    }
}
