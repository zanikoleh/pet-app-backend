using FluentAssertions;
using FileService.Infrastructure.Services;
using Xunit;

namespace FileService.Infrastructure.Tests.Services;

/// <summary>
/// Tests for Mock File Storage Service.
/// </summary>
public class MockFileStorageServiceTests : IAsyncLifetime
{
    private readonly string _testBasePath;
    private MockFileStorageService? _storageService;

    public MockFileStorageServiceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"fileservice_storage_test_{Guid.NewGuid()}");
    }

    public Task InitializeAsync()
    {
        if (!Directory.Exists(_testBasePath))
            Directory.CreateDirectory(_testBasePath);

        _storageService = new MockFileStorageService(_testBasePath);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Directory.Exists(_testBasePath))
        {
            try
            {
                Directory.Delete(_testBasePath, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }

        if (_storageService != null)
        {
            await Task.CompletedTask;
        }
    }

    private MockFileStorageService StorageService => 
        _storageService ?? throw new InvalidOperationException("StorageService not initialized");

    #region Upload Tests

    [Fact]
    public async Task UploadFileAsync_WithValidContent_ShouldReturnStoragePath()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "document.pdf";

        // Act
        var storagePath = await StorageService.UploadFileAsync(fileName, fileContent);

        // Assert
        storagePath.Should().NotBeNullOrEmpty();
        storagePath.Should().StartWith("files/");
        storagePath.Should().Contain(fileName);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldCreatePhysicalFile()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "test.txt";

        // Act
        var storagePath = await StorageService.UploadFileAsync(fileName, fileContent);

        // Assert
        var filePath = Path.Combine(_testBasePath, Path.GetFileName(storagePath));
        File.Exists(filePath).Should().BeTrue();
        var uploadedContent = await File.ReadAllBytesAsync(filePath);
        uploadedContent.Should().Equal(fileContent);
    }

    [Fact]
    public async Task UploadFileAsync_WithLargeFile_ShouldSucceed()
    {
        // Arrange
        var largFileContent = new byte[10 * 1024 * 1024]; // 10 MB
        Random.Shared.NextBytes(largFileContent);
        var fileName = "largefile.bin";

        // Act
        var storagePath = await StorageService.UploadFileAsync(fileName, largFileContent);

        // Assert
        storagePath.Should().NotBeNullOrEmpty();
        var filePath = Path.Combine(_testBasePath, Path.GetFileName(storagePath));
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_MultipleUploads_ShouldCreateUniqueNames()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3 };
        var fileName = "duplicate.txt";

        // Act
        var path1 = await StorageService.UploadFileAsync(fileName, fileContent);
        var path2 = await StorageService.UploadFileAsync(fileName, fileContent);

        // Assert
        path1.Should().NotBe(path2);
        path1.Should().Contain(fileName);
        path2.Should().Contain(fileName);
    }

    #endregion

    #region Download URL Tests

    [Fact]
    public async Task GetDownloadUrlAsync_ShouldReturnStoragePath()
    {
        // Arrange
        var storagePath = "files/document_123.pdf";

        // Act
        var url = await StorageService.GetDownloadUrlAsync(storagePath);

        // Assert
        url.Should().Be(storagePath);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_WithDifferentExpirations_ShouldReturnPath()
    {
        // Arrange
        var storagePath = "files/document.pdf";

        // Act - In mock, expiration doesn't affect output
        var url30 = await StorageService.GetDownloadUrlAsync(storagePath, 30);
        var url60 = await StorageService.GetDownloadUrlAsync(storagePath, 60);
        var url1440 = await StorageService.GetDownloadUrlAsync(storagePath, 1440);

        // Assert - All should return the same path (mock doesn't use expiration)
        url30.Should().Be(storagePath);
        url60.Should().Be(storagePath);
        url1440.Should().Be(storagePath);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_ShouldDeleteFile()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "todelete.txt";
        var storagePath = await StorageService.UploadFileAsync(fileName, fileContent);
        var filePath = Path.Combine(_testBasePath, Path.GetFileName(storagePath));

        File.Exists(filePath).Should().BeTrue();

        // Act
        await StorageService.DeleteFileAsync(storagePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_ShouldNotThrow()
    {
        // Arrange
        var nonExistentPath = "files/nonexistent.pdf";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            StorageService.DeleteFileAsync(nonExistentPath));

        exception.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3 };
        var storagePath = await StorageService.UploadFileAsync("file.txt", fileContent);

        // Act
        await StorageService.DeleteFileAsync(storagePath);
        var exception = await Record.ExceptionAsync(() =>
            StorageService.DeleteFileAsync(storagePath));

        // Assert
        exception.Should().BeNull();
    }

    #endregion

    #region Virus Scan Tests

    [Fact]
    public async Task ScanFileAsync_WithCleanContent_ShouldReturnTrue()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "clean.pdf";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(fileContent, fileName);

        // Assert
        isClean.Should().BeTrue();
        scanResult.Should().BeNull();
    }

    [Fact]
    public async Task ScanFileAsync_WithSuspiciousContent_ShouldReturnFalse()
    {
        // Arrange
        var suspiciousText = "This file contains virus code for testing purposes";
        var fileContent = System.Text.Encoding.UTF8.GetBytes(suspiciousText);
        var fileName = "suspicious.txt";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(fileContent, fileName);

        // Assert
        isClean.Should().BeFalse();
        scanResult.Should().NotBeNullOrEmpty();
        scanResult.Should().Contain("Suspicious");
    }

    [Theory]
    [InlineData("virus")]
    [InlineData("malware")]
    [InlineData("trojan")]
    [InlineData("ransomware")]
    public async Task ScanFileAsync_WithDangerousKeywords_ShouldReturnFalse(string keyword)
    {
        // Arrange
        var fileContent = System.Text.Encoding.UTF8.GetBytes($"This contains {keyword}");
        var fileName = "dangerous.txt";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(fileContent, fileName);

        // Assert
        isClean.Should().BeFalse();
        scanResult.Should().NotBeNull();
    }

    [Fact]
    public async Task ScanFileAsync_CaseInsensitive_ShouldDetectUppercase()
    {
        // Arrange
        var fileContent = System.Text.Encoding.UTF8.GetBytes("This contains VIRUS code");
        var fileName = "uppercase.txt";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(fileContent, fileName);

        // Assert
        isClean.Should().BeFalse();
        scanResult.Should().NotBeNull();
    }

    [Fact]
    public async Task ScanFileAsync_WithBinaryContent_ShouldHandleGracefully()
    {
        // Arrange
        var binaryContent = new byte[1024];
        Random.Shared.NextBytes(binaryContent);
        var fileName = "binary.bin";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(binaryContent, fileName);

        // Assert - Should not throw and return result
        // Binary files won't contain text keywords
        isClean.Should().BeTrue();
    }

    [Fact]
    public async Task ScanFileAsync_WithEmptyFile_ShouldReturnClean()
    {
        // Arrange
        var fileContent = new byte[] { };
        var fileName = "empty.txt";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(fileContent, fileName);

        // Assert
        isClean.Should().BeTrue();
        scanResult.Should().BeNull();
    }

    [Fact]
    public async Task ScanFileAsync_WithLargeFile_ShouldScanFirst1KB()
    {
        // Arrange
        var largeContent = new byte[10 * 1024];
        Random.Shared.NextBytes(largeContent);
        // Add virus signature at position 2000 (beyond first 1KB)
        var virusSignature = System.Text.Encoding.UTF8.GetBytes(" virus ");
        System.Buffer.BlockCopy(virusSignature, 0, largeContent, 2000, virusSignature.Length);

        var fileName = "large.bin";

        // Act
        var (isClean, scanResult) = await StorageService.ScanFileAsync(largeContent, fileName);

        // Assert - Should not detect virus since it's beyond first 1KB
        isClean.Should().BeTrue();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task WorkflowTest_UploadScanAndDelete()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "workflow_test.pdf";

        // Act - Upload
        var storagePath = await StorageService.UploadFileAsync(fileName, fileContent);
        var filePath = Path.Combine(_testBasePath, Path.GetFileName(storagePath));
        File.Exists(filePath).Should().BeTrue();

        // Act - Scan
        var (isClean, _) = await StorageService.ScanFileAsync(fileContent, fileName);
        isClean.Should().BeTrue();

        // Act - Get download URL
        var downloadUrl = await StorageService.GetDownloadUrlAsync(storagePath);
        downloadUrl.Should().Be(storagePath);

        // Act - Delete
        await StorageService.DeleteFileAsync(storagePath);
        File.Exists(filePath).Should().BeFalse();
    }

    #endregion
}
