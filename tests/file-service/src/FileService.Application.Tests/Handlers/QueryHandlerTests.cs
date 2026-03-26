using AutoMapper;
using FluentAssertions;
using Moq;
using FileService.Application.DTOs;
using FileService.Application.Handlers;
using FileService.Application.Interfaces;
using FileService.Application.Queries;
using FileService.Domain.Entities;
using SharedKernel;
using Xunit;

namespace FileService.Application.Tests.Handlers;

/// <summary>
/// Tests for File Query Handlers.
/// </summary>
public class GetFileQueryHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();

    public GetFileQueryHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_WithValidFileId_ShouldReturnFileDto()
    {
        // Arrange
        var handler = new GetFileQueryHandler(_repositoryMock.Object, _mapperMock.Object);
        var query = new GetFileQuery(_fileId);

        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            "document.pdf",
            "application/pdf",
            1024,
            "files/document.pdf");
        fileRecord.MarkAsVirusSafe();

        var expectedDto = new FileDto
        {
            Id = _fileId,
            UserId = _userId,
            FileName = "document.pdf",
            FileType = "application/pdf",
            FileSize = 1024,
            IsVirusSafe = true,
            UploadedAt = fileRecord.UploadedAt
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        _mapperMock
            .Setup(m => m.Map<FileDto>(fileRecord))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_fileId);
        result.FileName.Should().Be("document.pdf");
        result.IsVirusSafe.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetFileQueryHandler(_repositoryMock.Object, _mapperMock.Object);
        var query = new GetFileQuery(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord?)null);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(query, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDeletedFile_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetFileQueryHandler(_repositoryMock.Object, _mapperMock.Object);
        var query = new GetFileQuery(_fileId);

        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            "deleted.pdf",
            "application/pdf",
            1024,
            "files/deleted.pdf");
        fileRecord.Delete();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(query, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }
}

/// <summary>
/// Tests for Get File Download URL Query Handler.
/// </summary>
public class GetFileDownloadUrlQueryHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IFileStorageService> _storageServiceMock;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();

    public GetFileDownloadUrlQueryHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _storageServiceMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldReturnDownloadUrl()
    {
        // Arrange
        var handler = new GetFileDownloadUrlQueryHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var query = new GetFileDownloadUrlQuery(_fileId, 60);
        var storagePath = "files/document.pdf";
        var expectedUrl = "https://storage.example.com/signed-url";

        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            "document.pdf",
            "application/pdf",
            1024,
            storagePath);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        _storageServiceMock
            .Setup(s => s.GetDownloadUrlAsync(storagePath, 60, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUrl);
        _storageServiceMock.Verify(
            s => s.GetDownloadUrlAsync(storagePath, 60, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentExpirationTimes_ShouldPassToStorageService()
    {
        // Arrange
        var handler = new GetFileDownloadUrlQueryHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var expirationMinutes = new[] { 30, 60, 1440 };

        foreach (var expiration in expirationMinutes)
        {
            var query = new GetFileDownloadUrlQuery(_fileId, expiration);
            var fileRecord = new FileRecord(
                _fileId,
                _userId,
                "document.pdf",
                "application/pdf",
                1024,
                "files/document.pdf");

            _repositoryMock
                .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileRecord);

            _storageServiceMock
                .Setup(s => s.GetDownloadUrlAsync("files/document.pdf", expiration, It.IsAny<CancellationToken>()))
                .ReturnsAsync($"https://url-expires-in-{expiration}-minutes");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().Contain(expiration.ToString());
        }
    }

    [Fact]
    public async Task Handle_WithDeletedFile_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetFileDownloadUrlQueryHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var query = new GetFileDownloadUrlQuery(_fileId);
        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            "deleted.pdf",
            "application/pdf",
            1024,
            "files/deleted.pdf");
        fileRecord.Delete();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(query, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }
}

/// <summary>
/// Tests for List User Files Query Handler.
/// </summary>
public class ListUserFilesQueryHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Guid _userId = Guid.NewGuid();

    public ListUserFilesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnPaginatedList()
    {
        // Arrange
        var handler = new ListUserFilesQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object);

        var query = new ListUserFilesQuery(_userId, 1, 10);

        var fileRecords = new List<FileRecord>
        {
            new FileRecord(Guid.NewGuid(), _userId, "file1.pdf", "application/pdf", 1024, "files/file1.pdf"),
            new FileRecord(Guid.NewGuid(), _userId, "file2.jpg", "image/jpeg", 2048, "files/file2.jpg"),
            new FileRecord(Guid.NewGuid(), _userId, "file3.png", "image/png", 4096, "files/file3.png")
        };

        var fileDtos = fileRecords.Select(f => new FileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            FileSize = f.FileSize
        }).ToList();

        _repositoryMock
            .Setup(r => r.GetUserFilesAsync(_userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((fileRecords, 3));

        _mapperMock
            .Setup(m => m.Map<List<FileDto>>(fileRecords))
            .Returns(fileDtos);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var handler = new ListUserFilesQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object);

        var query = new ListUserFilesQuery(_userId, 2, 5);
        var fileRecords = new List<FileRecord>();
        var fileDtos = new List<FileDto>();

        _repositoryMock
            .Setup(r => r.GetUserFilesAsync(_userId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((fileRecords, 12)); // 12 total files

        _mapperMock
            .Setup(m => m.Map<List<FileDto>>(fileRecords))
            .Returns(fileDtos);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithNoFiles_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new ListUserFilesQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object);

        var query = new ListUserFilesQuery(Guid.NewGuid(), 1, 10);

        _repositoryMock
            .Setup(r => r.GetUserFilesAsync(It.IsAny<Guid>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<FileRecord>(), 0));

        _mapperMock
            .Setup(m => m.Map<List<FileDto>>(It.IsAny<List<FileRecord>>()))
            .Returns(new List<FileDto>());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

    }
}

/// <summary>
/// Tests for List Files by Entity Query Handler.
/// </summary>
public class ListFilesByEntityQueryHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Guid _entityId = Guid.NewGuid();

    public ListFilesByEntityQueryHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_WithValidEntity_ShouldReturnRelatedFiles()
    {
        // Arrange
        var handler = new ListFilesByEntityQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object);

        var query = new ListFilesByEntityQuery(_entityId, 1, 10);

        var fileRecords = new List<FileRecord>
        {
            new FileRecord(Guid.NewGuid(), Guid.NewGuid(), "pet-doc1.pdf", "application/pdf", 1024, "files/pet-doc1.pdf", relatedEntityId: _entityId),
            new FileRecord(Guid.NewGuid(), Guid.NewGuid(), "pet-doc2.pdf", "application/pdf", 2048, "files/pet-doc2.pdf", relatedEntityId: _entityId)
        };

        var fileDtos = fileRecords.Select(f => new FileDto
        {
            Id = f.Id,
            RelatedEntityId = f.RelatedEntityId
        }).ToList();

        _repositoryMock
            .Setup(r => r.GetFilesByEntityAsync(_entityId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((fileRecords, 2));

        _mapperMock
            .Setup(m => m.Map<List<FileDto>>(fileRecords))
            .Returns(fileDtos);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.All(f => f.RelatedEntityId == _entityId).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new ListFilesByEntityQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object);

        var query = new ListFilesByEntityQuery(Guid.NewGuid(), 1, 10);

        _repositoryMock
            .Setup(r => r.GetFilesByEntityAsync(It.IsAny<Guid>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<FileRecord>(), 0));

        _mapperMock
            .Setup(m => m.Map<List<FileDto>>(It.IsAny<List<FileRecord>>()))
            .Returns(new List<FileDto>());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
