using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Infrastructure.Persistence;
using UserProfileService.Application.Interfaces;
using UserProfileService.Domain.Aggregates;

namespace UserService.Infrastructure.Tests.Persistence;

/// <summary>
/// Tests for UserProfile repository using SQLite database.
/// </summary>
public class UserProfileRepositoryTests : IAsyncLifetime
{
    private readonly string _databasePath;
    private readonly ServiceProvider _serviceProvider;
    private readonly UserProfileServiceDbContext _dbContext;
    private readonly IUserProfileRepository _repository;

    public UserProfileRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"userprofile_test_{Guid.NewGuid()}.db");
        
        var services = new ServiceCollection();

        services.AddDbContext<UserProfileServiceDbContext>(options =>
            options.UseSqlite($"Data Source={_databasePath};Cache=Shared"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<UserProfileServiceDbContext>();
        _repository = new UserProfileRepository(_dbContext);
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
        _serviceProvider.Dispose();

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

    [Fact]
    public async Task AddAsync_WithNewUserProfile_ShouldPersist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(Guid.NewGuid(), userId, "john@example.com");

        // Act
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be(userId);
        retrieved.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProfile()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(userProfileId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(userProfileId);
        retrieved.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(Guid.NewGuid(), userId, "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByUserIdAsync(userId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithInvalidUserId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfile_ShouldPersistChanges()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.UpdateProfile("John", "Doe", "New bio", null, null, null, null, null, null);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Profile.FirstName.Should().Be("John");
        retrieved.Profile.LastName.Should().Be("Doe");
        retrieved.Profile.Bio.Should().Be("New bio");
    }

    [Fact]
    public async Task UpdateNotificationPreferences_ShouldPersistChanges()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.UpdateNotificationPreferences(false, true, false, true, false);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Preferences.EmailNotifications.Should().BeFalse();
        retrieved.Preferences.PushNotifications.Should().BeTrue();
        retrieved.Preferences.SmsNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_ShouldPersistInactiveState()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.Deactivate();
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Reactivate_ShouldPersistActiveState()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        userProfile.Deactivate();
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.Reactivate();
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleProfiles_ShouldRetrieveCorrectOne()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var profile1 = new UserProfile(Guid.NewGuid(), userId1, "user1@example.com");
        var profile2 = new UserProfile(Guid.NewGuid(), userId2, "user2@example.com");

        _dbContext.UserProfiles.Add(profile1);
        _dbContext.UserProfiles.Add(profile2);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved1 = await _repository.GetByUserIdAsync(userId1);
        var retrieved2 = await _repository.GetByUserIdAsync(userId2);

        // Assert
        retrieved1.Should().NotBeNull();
        retrieved2.Should().NotBeNull();
        retrieved1!.Email.Should().Be("user1@example.com");
        retrieved2!.Email.Should().Be("user2@example.com");
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleOperations_ShouldPersistAll()
    {
        // Arrange
        var profile1 = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user1@example.com");
        var profile2 = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user2@example.com");
        var profile3 = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user3@example.com");

        // Act
        _dbContext.UserProfiles.Add(profile1);
        _dbContext.UserProfiles.Add(profile2);
        _dbContext.UserProfiles.Add(profile3);
        await _dbContext.SaveChangesAsync();

        // Assert
        var count = _dbContext.UserProfiles.Count();
        count.Should().Be(3);
    }

    [Fact]
    public async Task UpdateLanguageAndTimezone_ShouldPersist()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.UpdateLanguageAndTimezone("fr", "Europe/Paris");
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Preferences.Language.Should().Be("fr");
        retrieved.Preferences.Timezone.Should().Be("Europe/Paris");
    }

    [Fact]
    public async Task ProfileDataWithComplexInfo_ShouldPersist()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        userProfile.UpdateProfile(
            "Jane",
            "Smith",
            "Love traveling and photography",
            new DateTime(1995, 3, 20),
            "+1-555-0123",
            "456 Oak Lane",
            "Portland",
            "USA",
            "https://avatar.example.com/jane.jpg");
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        var profile = retrieved!;
        profile.Profile.FirstName.Should().Be("Jane");
        profile.Profile.LastName.Should().Be("Smith");
        profile.Profile.Bio.Should().Contain("traveling");
        profile.Profile.DateOfBirth.Should().Be(new DateTime(1995, 3, 20));
        profile.Profile.PhoneNumber.Should().Be("+1-555-0123");
        profile.Profile.Address.Should().Be("456 Oak Lane");
        profile.Profile.City.Should().Be("Portland");
        profile.Profile.Country.Should().Be("USA");
        profile.Profile.ProfilePictureUrl.Should().Be("https://avatar.example.com/jane.jpg");
    }

    [Fact]
    public async Task MultipleUpdates_ShouldPreserveAllData()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act - First update
        userProfile.UpdateProfile("John", "Doe", null, null, null, null, null, null, null);
        await _dbContext.SaveChangesAsync();

        // Act - Second update
        userProfile.UpdateNotificationPreferences(false, false, true, false, true);
        await _dbContext.SaveChangesAsync();

        // Act - Third update
        userProfile.UpdateLanguageAndTimezone("es", "America/Mexico_City");
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        var profile = retrieved!;
        profile.Profile.FirstName.Should().Be("John");
        profile.Profile.LastName.Should().Be("Doe");
        profile.Preferences.SmsNotifications.Should().BeTrue();
        profile.Preferences.Language.Should().Be("es");
        profile.Preferences.Timezone.Should().Be("America/Mexico_City");
    }

    [Fact]
    public async Task CreatedAt_ShouldBePreserved()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        var originalCreatedAt = userProfile.CreatedAt;

        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        System.Threading.Thread.Sleep(100);
        userProfile.UpdateProfile("John", null, null, null, null, null, null, null, null);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.CreatedAt.Should().Be(originalCreatedAt);
        retrieved.UpdatedAt.Should().NotBeNull();
        (retrieved.UpdatedAt >= originalCreatedAt).Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithMultipleProfiles_ShouldReturnCorrectOne()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var targetProfile = new UserProfile(Guid.NewGuid(), targetUserId, "target@example.com");
        var otherProfile = new UserProfile(Guid.NewGuid(), otherUserId, "other@example.com");

        _dbContext.UserProfiles.Add(targetProfile);
        _dbContext.UserProfiles.Add(otherProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByUserIdAsync(targetUserId);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("target@example.com");
        retrieved.UserId.Should().Be(targetUserId);
    }

    [Fact]
    public async Task PreferencesDefaults_ShouldBeCorrect()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);

        // Assert
        retrieved.Should().NotBeNull();
        var prefs = retrieved!.Preferences;
        prefs.Language.Should().Be("en");
        prefs.Timezone.Should().Be("UTC");
        prefs.EmailNotifications.Should().BeTrue();
        prefs.PushNotifications.Should().BeTrue();
        prefs.SmsNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task Active_ShouldDefaultToTrue()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        _dbContext.UserProfiles.Add(userProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(userProfile.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.IsActive.Should().BeTrue();
    }
}
