using FileService.Application.Interfaces;

namespace FileService.Infrastructure.Services;

/// <summary>
/// Mock implementation of file storage service for development.
/// In production, this would use Azure Blob Storage.
/// </summary>
public class MockFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public MockFileStorageService(string basePath = "./files")
    {
        _basePath = basePath;
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_basePath, $"{fileId}_{fileName}");
        
        await File.WriteAllBytesAsync(filePath, fileContent, cancellationToken);
        
        return $"files/{fileId}_{fileName}";
    }

    public async Task<string> GetDownloadUrlAsync(
        string storagePath,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        // In production, this would generate a signed URL from Azure
        // For development, return a relative URL
        return await Task.FromResult(storagePath);
    }

    public async Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, Path.GetFileName(storagePath));
        
        if (File.Exists(filePath))
            File.Delete(filePath);
            
        await Task.CompletedTask;
    }

    public async Task<(bool IsClean, string? ScanResult)> ScanFileAsync(
        byte[] fileContent,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        // Mock virus scanning - in production, use ClamAV or similar
        // Check for some basic suspicious patterns
        var content = System.Text.Encoding.UTF8.GetString(fileContent.Take(1024).ToArray());
        
        var suspiciousPatterns = new[] { "virus", "malware", "trojan", "ransomware" };
        var isSuspicious = suspiciousPatterns.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase));
        
        return await Task.FromResult((!isSuspicious, isSuspicious ? "Suspicious content detected" : null));
    }
}
