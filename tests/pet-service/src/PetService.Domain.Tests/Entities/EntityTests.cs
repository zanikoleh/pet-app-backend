using FluentAssertions;
using PetService.Domain.Entities;
using Xunit;

namespace PetService.Domain.Tests.Entities;

public class PhotoEntityTests
{
    private readonly Guid _petId = Guid.NewGuid();
    private readonly Guid _photoId = Guid.NewGuid();
    private readonly string _fileName = "photo.jpg";
    private readonly string _fileType = "image/jpeg";
    private readonly long _fileSizeBytes = 1024000;
    private readonly string _url = "https://example.com/photo.jpg";

    [Fact]
    public void Create_ValidInput_ShouldCreatePhoto()
    {
        // Act
        var photo = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url);

        // Assert
        photo.Id.Should().Be(_photoId);
        photo.PetId.Should().Be(_petId);
        photo.FileName.Should().Be(_fileName);
        photo.FileType.Should().Be(_fileType);
        photo.FileSizeBytes.Should().Be(_fileSizeBytes);
        photo.Url.Should().Be(_url);
        photo.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        photo.Caption.Should().BeNull();
        photo.Tags.Should().BeNull();
    }

    [Fact]
    public void Create_WithCaptionAndTags_ShouldSetProperties()
    {
        // Arrange
        var caption = "My pet photo";
        var tags = "pet,fluffy,cat";

        // Act
        var photo = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url, caption, tags);

        // Assert
        photo.Caption.Should().Be(caption);
        photo.Tags.Should().Be(tags);
    }

    [Fact]
    public void UpdateCaption_ShouldUpdateCaption()
    {
        // Arrange
        var photo = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url);
        var newCaption = "Updated caption";

        // Act
        photo.UpdateCaption(newCaption);

        // Assert
        photo.Caption.Should().Be(newCaption);
    }

    [Fact]
    public void UpdateTags_ShouldUpdateTags()
    {
        // Arrange
        var photo = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url);
        var newTags = "pet,cute,happy";

        // Act
        photo.UpdateTags(newTags);

        // Assert
        photo.Tags.Should().Be(newTags);
    }

    [Fact]
    public void Equality_SameId_ShouldBeEqual()
    {
        // Arrange
        var photo1 = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url);
        var photo2 = new Photo(_petId, _photoId, "different.jpg", "image/png", 2048000, "https://different.com/photo.jpg");

        // Act & Assert
        photo1.Should().Be(photo2);
    }

    [Fact]
    public void Equality_DifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var photo1 = new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url);
        var photo2 = new Photo(_petId, Guid.NewGuid(), _fileName, _fileType, _fileSizeBytes, _url);

        // Act & Assert
        photo1.Should().NotBe(photo2);
    }
}

public class DocumentEntityTests
{
    private readonly Guid _petId = Guid.NewGuid();
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly string _fileName = "vaccination.pdf";
    private readonly string _fileType = "application/pdf";
    private readonly long _fileSizeBytes = 256000;
    private readonly string _url = "https://example.com/vaccination.pdf";
    private readonly string _category = "vaccination";

    [Fact]
    public void Create_ValidInput_ShouldCreateDocument()
    {
        // Act
        var document = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category);

        // Assert
        document.Id.Should().Be(_documentId);
        document.PetId.Should().Be(_petId);
        document.FileName.Should().Be(_fileName);
        document.FileType.Should().Be(_fileType);
        document.FileSizeBytes.Should().Be(_fileSizeBytes);
        document.Url.Should().Be(_url);
        document.Category.Should().Be(_category);
        document.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Arrange
        var description = "Annual vaccination record";

        // Act
        var document = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category, description);

        // Assert
        document.Description.Should().Be(description);
    }

    [Theory]
    [InlineData("vaccination")]
    [InlineData("medical")]
    [InlineData("achievement")]
    [InlineData("other")]
    public void Create_ValidCategories_ShouldCreateDocument(string category)
    {
        // Act
        var document = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, category);

        // Assert
        document.Category.Should().Be(category);
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription()
    {
        // Arrange
        var document = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category);
        var newDescription = "Updated description";

        // Act
        document.UpdateDescription(newDescription);

        // Assert
        document.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Equality_SameId_ShouldBeEqual()
    {
        // Arrange
        var document1 = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category);
        var document2 = new Document(_petId, _documentId, "different.pdf", "application/pdf", 512000, "https://different.com/document.pdf", "medical");

        // Act & Assert
        document1.Should().Be(document2);
    }

    [Fact]
    public void Equality_DifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var document1 = new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category);
        var document2 = new Document(_petId, Guid.NewGuid(), _fileName, _fileType, _fileSizeBytes, _url, _category);

        // Act & Assert
        document1.Should().NotBe(document2);
    }
}
