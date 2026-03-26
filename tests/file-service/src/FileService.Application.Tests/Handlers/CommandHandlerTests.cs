using AutoMapper;
using FluentAssertions;
using Moq;
using MediatR;
using FileService.Application.Commands;
using FileService.Application.DTOs;
using FileService.Application.Handlers;
using FileService.Application.Interfaces;
using FileService.Domain.Entities;
using SharedKernel;
using Xunit;

namespace FileService.Application.Tests.Handlers;

/// <summary>
/// Tests for File Upload Command Handler.
/// </summary>
public class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IFileStorageService> _storageServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();

    public UploadFileCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _storageServiceMock = new Mock<IFileStorageService>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldUploadSuccessfully()
    {
        // Arrange
        var handler = new UploadFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var command = new UploadFileCommand(
            _userId,
            "test.pdf",
            "application/pdf",
            fileContent,
            "pet-document",
            Guid.NewGuid());

        var storagePath = "files/test_abc123.pdf";
        var expectedDto = new FileDto
        {
            Id = _fileId,
            UserId = _userId,
            FileName = "test.pdf",
            FileType = "application/pdf",
            FileSize = fileContent.Length,
            Category = "pet-document",
            IsVirusSafe = true,
            UploadedAt = DateTime.UtcNow
        };

        _storageServiceMock
            .Setup(s => s.ScanFileAsync(fileContent, "test.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));

        _storageServiceMock
            .Setup(s => s.UploadFileAsync("test.pdf", fileContent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storagePath);

        _mapperMock
            .Setup(m => m.Map<FileDto>(It.IsAny<FileRecord>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.FileSize.Should().Be(fileContent.Length);
        result.IsVirusSafe.Should().BeTrue();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FileRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _storageServiceMock.Verify(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFile_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var handler = new UploadFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        var command = new UploadFileCommand(
            _userId,
            "empty.pdf",
            "application/pdf",
            new byte[] { },
            null,
            null);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
        exception!.Message.Should().Contain("File size must be between 0 and 50 MB");
    }

    [Fact]
    public async Task Handle_WithFileTooLarge_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var handler = new UploadFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        var largeFile = new byte[50 * 1024 * 1024 + 1]; // Over 50 MB limit
        var command = new UploadFileCommand(
            _userId,
            "large.pdf",
            "application/pdf",
            largeFile,
            null,
            null);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
        exception!.Message.Should().Contain("File size must be between 0 and 50 MB");
    }

    [Fact]
    public async Task Handle_WithMalwareDetected_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var handler = new UploadFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var command = new UploadFileCommand(
            _userId,
            "malware.pdf",
            "application/pdf",
            fileContent,
            null,
            null);

        _storageServiceMock
            .Setup(s => s.ScanFileAsync(fileContent, "malware.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Virus detected: Win32.Trojan"));

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
        exception!.Message.Should().Contain("malware");
    }

    [Fact]
    public async Task Handle_WithValidationError_ShouldNotCallStorage()
    {
        // Arrange
        var handler = new UploadFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        var command = new UploadFileCommand(
            _userId,
            "test.pdf",
            "application/pdf",
            new byte[] { },
            null,
            null);

        // Act & Assert
        await Record.ExceptionAsync(() => handler.Handle(command, CancellationToken.None));

        _storageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

/// <summary>
/// Tests for File Delete Command Handler.
/// </summary>
public class DeleteFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IFileStorageService> _storageServiceMock;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();

    public DeleteFileCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _storageServiceMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldDeleteSuccessfully()
    {
        // Arrange
        var handler = new DeleteFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var fileRecord = new FileRecord(
            _fileId,
            _userId,
            "document.pdf",
            "application/pdf",
            1024,
            "files/document.pdf");
        fileRecord.MarkAsVirusSafe();

        var command = new DeleteFileCommand(_fileId, _userId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        fileRecord.IsDeleted.Should().BeTrue();

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _storageServiceMock.Verify(s => s.DeleteFileAsync(fileRecord.StoragePath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new DeleteFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var command = new DeleteFileCommand(Guid.NewGuid(), _userId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord?)null);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDifferentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new DeleteFileCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var fileRecord = new FileRecord(
            _fileId,
            Guid.NewGuid(), // Different user
            "document.pdf",
            "application/pdf",
            1024,
            "files/document.pdf");

        var command = new DeleteFileCommand(_fileId, _userId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
        exception!.Message.Should().Contain("access denied");
    }
}

/// <summary>
/// Tests for Mark File as Virus Safe Command Handler.
/// </summary>
public class MarkFileAsVirusSafeCommandHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Guid _fileId = Guid.NewGuid();

    public MarkFileAsVirusSafeCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldMarkAsVirusSafe()
    {
        // Arrange
        var handler = new MarkFileAsVirusSafeCommandHandler(_repositoryMock.Object);
        var fileRecord = new FileRecord(
            _fileId,
            Guid.NewGuid(),
            "document.pdf",
            "application/pdf",
            1024,
            "files/document.pdf");

        var command = new MarkFileAsVirusSafeCommand(_fileId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        fileRecord.IsVirusSafe.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new MarkFileAsVirusSafeCommandHandler(_repositoryMock.Object);
        var command = new MarkFileAsVirusSafeCommand(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord?)null);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
    }
}

/// <summary>
/// Tests for Mark File as Virus Detected Command Handler.
/// </summary>
public class MarkFileAsVirusDetectedCommandHandlerTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IFileStorageService> _storageServiceMock;
    private readonly Guid _fileId = Guid.NewGuid();

    public MarkFileAsVirusDetectedCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _storageServiceMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldMarkAsDeletedAndDeleteFromStorage()
    {
        // Arrange
        var handler = new MarkFileAsVirusDetectedCommandHandler(
            _repositoryMock.Object,
            _storageServiceMock.Object);

        var fileRecord = new FileRecord(
            _fileId,
            Guid.NewGuid(),
            "malware.exe",
            "application/octet-stream",
            2048,
            "files/malware.exe");
        fileRecord.MarkAsVirusSafe();

        var command = new MarkFileAsVirusDetectedCommand(_fileId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileRecord);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        fileRecord.IsDeleted.Should().BeTrue();
        fileRecord.IsVirusSafe.Should().BeFalse();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _storageServiceMock.Verify(s => s.DeleteFileAsync(fileRecord.StoragePath, It.IsAny<CancellationToken>()), Times.Once);
    }
}
