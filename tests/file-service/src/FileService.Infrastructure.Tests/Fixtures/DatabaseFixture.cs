using Microsoft.EntityFrameworkCore;
using Xunit;
using FileService.Infrastructure.Persistence;

namespace FileService.Infrastructure.Tests.Fixtures;

/// <summary>
/// Fixture for setting up a SQLite database for testing.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _databasePath;
    public FileServiceDbContext DbContext { get; private set; } = null!;

    public DatabaseFixture()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"fileservice_test_{Guid.NewGuid()}.db");
    }

    public async Task InitializeAsync()
    {
        var connectionString = $"Data Source={_databasePath};Cache=Shared";
        var options = new DbContextOptionsBuilder<FileServiceDbContext>()
            .UseSqlite(connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        DbContext = new FileServiceDbContext(options);
        
        // Ensure database is created with proper schema
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        // Clean up database file
        if (File.Exists(_databasePath))
        {
            try
            {
                File.Delete(_databasePath);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }
}
