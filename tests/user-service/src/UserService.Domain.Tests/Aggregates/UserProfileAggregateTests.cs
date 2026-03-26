using FluentAssertions;
using Xunit;
using UserProfileService.Domain.Aggregates;
using UserProfileService.Domain.Entities;
using UserProfileService.Domain.Events;

namespace UserService.Domain.Tests.Aggregates;

/// <summary>
/// Tests for UserProfile aggregate root.
/// </summary>
public class UserProfileAggregateTests
{
    [Fact]
    public void CreateUserProfile_WithValidInput_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "john@example.com";

        // Act
        var userProfile = new UserProfile(Guid.NewGuid(), userId, email);

        // Assert
        userProfile.UserId.Should().Be(userId);
        userProfile.Email.Should().Be(email);
        userProfile.IsActive.Should().BeTrue();
        (userProfile.CreatedAt <= DateTime.UtcNow.AddSeconds(1)).Should().BeTrue();
        userProfile.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void CreateProfileFromRegistration_WithFullName_ShouldParseName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "jane@example.com";
        var fullName = "Jane Smith";

        // Act
        var userProfile = UserProfile.CreateFromRegistration(userId, email, fullName, "https://picture.url");

        // Assert
        userProfile.Profile.FirstName.Should().Be("Jane");
        userProfile.Profile.LastName.Should().Be("Smith");
        userProfile.Profile.ProfilePictureUrl.Should().Be("https://picture.url");
    }

    [Fact]
    public void CreateProfileFromRegistration_WithoutFullName_ShouldKeepEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";

        // Act
        var userProfile = UserProfile.CreateFromRegistration(userId, email, null, null);

        // Assert
        userProfile.Profile.FirstName.Should().BeEmpty();
        userProfile.Profile.LastName.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProfile_WithNewData_ShouldUpdateAllFields()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        var originalUpdatedAt = userProfile.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        userProfile.UpdateProfile(
            "John",
            "Doe",
            "I like pets",
            new DateTime(1990, 5, 15),
            "+1234567890",
            "123 Main St",
            "Springfield",
            "USA",
            "https://avatar.url");

        // Assert
        userProfile.Profile.FirstName.Should().Be("John");
        userProfile.Profile.LastName.Should().Be("Doe");
        userProfile.Profile.Bio.Should().Be("I like pets");
        userProfile.Profile.DateOfBirth.Should().Be(new DateTime(1990, 5, 15));
        userProfile.Profile.PhoneNumber.Should().Be("+1234567890");
        userProfile.Profile.Address.Should().Be("123 Main St");
        userProfile.Profile.City.Should().Be("Springfield");
        userProfile.Profile.Country.Should().Be("USA");
        userProfile.Profile.ProfilePictureUrl.Should().Be("https://avatar.url");
        userProfile.UpdatedAt.Should().NotBeNull();
        (userProfile.UpdatedAt >= (originalUpdatedAt ?? DateTime.MinValue)).Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_WithPartialData_ShouldOnlyUpdateNonNull()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        userProfile.UpdateProfile("John", "Doe", null, null, null, null, null, null, null);

        // Act
        userProfile.UpdateProfile("Jane", null, "New bio", null, null, null, null, null, null);

        // Assert
        userProfile.Profile.FirstName.Should().Be("Jane");
        userProfile.Profile.LastName.Should().Be("Doe"); // Should remain unchanged
        userProfile.Profile.Bio.Should().Be("New bio");
    }

    [Fact]
    public void UpdateNotificationPreferences_WithValidInput_ShouldUpdate()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateNotificationPreferences(true, false, true, false, true);

        // Assert
        userProfile.Preferences.EmailNotifications.Should().BeTrue();
        userProfile.Preferences.PushNotifications.Should().BeFalse();
        userProfile.Preferences.SmsNotifications.Should().BeTrue();
        userProfile.Preferences.ReceivePromotions.Should().BeFalse();
        userProfile.Preferences.ReceiveNewsletter.Should().BeTrue();
    }

    [Fact]
    public void UpdateLanguageAndTimezone_WithValidInput_ShouldUpdate()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateLanguageAndTimezone("es", "America/New_York");

        // Assert
        userProfile.Preferences.Language.Should().Be("es");
        userProfile.Preferences.Timezone.Should().Be("America/New_York");
    }



    [Fact]
    public void Deactivate_WhenActive_ShouldMarkedInactive()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        userProfile.IsActive.Should().BeTrue();

        // Act
        userProfile.Deactivate();

        // Assert
        userProfile.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_WhenInactive_ShouldMarkActive()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        userProfile.Deactivate();

        // Act
        userProfile.Reactivate();

        // Assert
        userProfile.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("admin+tag@company.org")]
    [InlineData("user.name+123@test.co.uk")]
    public void CreateUserProfile_WithVariousEmails_ShouldAcceptAll(string email)
    {
        // Act
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), email);

        // Assert
        userProfile.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Giovanni")]
    [InlineData("José")]
    public void UpdateProfile_FirstName_WithVariousNames_ShouldAcceptAll(string firstName)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateProfile(firstName, null, null, null, null, null, null, null, null);

        // Assert
        userProfile.Profile.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void UpdateProfile_WithLongBio_ShouldAccept()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        var longBio = string.Concat(Enumerable.Repeat("I love pets and animals. ", 50));

        // Act
        userProfile.UpdateProfile(null, null, longBio, null, null, null, null, null, null);

        // Assert
        userProfile.Profile.Bio.Should().Be(longBio);
    }

    [Theory]
    [InlineData("2000-01-15")]
    [InlineData("1985-12-31")]
    [InlineData("1950-06-15")]
    public void UpdateProfile_WithVariousDateOfBirth_ShouldAccept(string dateStr)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        var dateOfBirth = DateTime.Parse(dateStr);

        // Act
        userProfile.UpdateProfile(null, null, null, dateOfBirth, null, null, null, null, null);

        // Assert
        userProfile.Profile.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Theory]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("+81 3-1234-5678")]
    [InlineData("555-123-4567")]
    public void UpdateProfile_WithVariousPhoneFormats_ShouldAccept(string phoneNumber)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateProfile(null, null, null, null, phoneNumber, null, null, null, null);

        // Assert
        userProfile.Profile.PhoneNumber.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("123 Main Street", "Springfield", "USA")]
    [InlineData("456 Oak Avenue", "London", "UK")]
    [InlineData("Tokyo Tower, Chiyoda", "Tokyo", "Japan")]
    public void UpdateProfile_WithVariousAddresses_ShouldAccept(string address, string city, string country)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateProfile(null, null, null, null, null, address, city, country, null);

        // Assert
        userProfile.Profile.Address.Should().Be(address);
        userProfile.Profile.City.Should().Be(city);
        userProfile.Profile.Country.Should().Be(country);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    [InlineData("fr")]
    [InlineData("ja")]
    [InlineData("zh")]
    public void UpdateLanguageAndTimezone_WithVariousLanguages_ShouldAccept(string language)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateLanguageAndTimezone(language, "UTC");

        // Assert
        userProfile.Preferences.Language.Should().Be(language);
    }

    [Theory]
    [InlineData("UTC")]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    public void UpdateLanguageAndTimezone_WithVariousTimezones_ShouldAccept(string timezone)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.UpdateLanguageAndTimezone("en", timezone);

        // Assert
        userProfile.Preferences.Timezone.Should().Be(timezone);
    }

    [Fact]
    public void Multiple_Updates_ShouldPreserveHistory()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");
        var initialCreatedAt = userProfile.CreatedAt;

        // Act
        userProfile.UpdateProfile("John", "Doe", null, null, null, null, null, null, null);
        System.Threading.Thread.Sleep(10);
        var afterFirstUpdate = userProfile.UpdatedAt;

        userProfile.UpdateNotificationPreferences(false, true, true, false, false);
        System.Threading.Thread.Sleep(10);
        var afterSecondUpdate = userProfile.UpdatedAt;

        // Assert
        userProfile.CreatedAt.Should().Be(initialCreatedAt);
        (afterSecondUpdate >= (afterFirstUpdate ?? DateTime.MinValue)).Should().BeTrue();
        userProfile.IsActive.Should().BeTrue();
        userProfile.Profile.FirstName.Should().Be("John");
        userProfile.Preferences.PushNotifications.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ThenSetPreferences_ShouldStillWork()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        // Act
        userProfile.Deactivate();
        userProfile.UpdateNotificationPreferences(true, false, true, false, true);

        // Assert
        userProfile.IsActive.Should().BeFalse();
        userProfile.Preferences.EmailNotifications.Should().BeTrue();
    }
}

/// <summary>
/// Tests for ProfileData entity.
/// </summary>
public class ProfileDataTests
{
    [Fact]
    public void CreateProfileData_ShouldInitializeEmpty()
    {
        // Act
        var profileData = new ProfileData();

        // Assert
        profileData.FirstName.Should().BeEmpty();
        profileData.LastName.Should().BeEmpty();
        profileData.Bio.Should().BeNull();
        profileData.DateOfBirth.Should().BeNull();
        profileData.PhoneNumber.Should().BeNull();
        profileData.Address.Should().BeNull();
        profileData.City.Should().BeNull();
        profileData.Country.Should().BeNull();
        profileData.ProfilePictureUrl.Should().BeNull();
    }

    [Fact]
    public void Update_WithAllFields_ShouldSetAll()
    {
        // Arrange
        var profileData = new ProfileData();

        // Act
        profileData.Update("John", "Doe", "Bio text", new DateTime(1990, 1, 1),
            "+1234567890", "Street", "City", "Country", "https://pic.url");

        // Assert
        profileData.FirstName.Should().Be("John");
        profileData.LastName.Should().Be("Doe");
        profileData.Bio.Should().Be("Bio text");
        profileData.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        profileData.PhoneNumber.Should().Be("+1234567890");
        profileData.Address.Should().Be("Street");
        profileData.City.Should().Be("City");
        profileData.Country.Should().Be("Country");
        profileData.ProfilePictureUrl.Should().Be("https://pic.url");
    }

    [Fact]
    public void Update_WithNullValues_ShouldClearFields()
    {
        // Arrange
        var profileData = new ProfileData { FirstName = "John", LastName = "Doe", Bio = "Text" };

        // Act
        profileData.Update(null, null, null, null, null, null, null, null, null);

        // Assert
        profileData.FirstName.Should().Be("John"); // Empty string not provided, so no change
        profileData.LastName.Should().Be("Doe");
        profileData.Bio.Should().BeNull();
    }
}

/// <summary>
/// Tests for UserPreferences entity.
/// </summary>
public class UserPreferencesTests
{
    [Fact]
    public void CreateUserPreferences_ShouldHaveDefaults()
    {
        // Act
        var preferences = new UserPreferences();

        // Assert
        preferences.Language.Should().Be("en");
        preferences.Timezone.Should().Be("UTC");
        preferences.NotificationsEnabled.Should().BeTrue();
        preferences.EmailNotifications.Should().BeTrue();
        preferences.PushNotifications.Should().BeTrue();
        preferences.SmsNotifications.Should().BeFalse();
        preferences.ReceivePromotions.Should().BeTrue();
        preferences.ReceiveNewsletter.Should().BeTrue();
    }

    [Fact]
    public void UpdateNotificationPreferences_ShouldChangeAll()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.UpdateNotificationPreferences(false, false, true, false, false);

        // Assert
        preferences.EmailNotifications.Should().BeFalse();
        preferences.PushNotifications.Should().BeFalse();
        preferences.SmsNotifications.Should().BeTrue();
        preferences.ReceivePromotions.Should().BeFalse();
        preferences.ReceiveNewsletter.Should().BeFalse();
    }

    [Fact]
    public void UpdateNotificationPreferences_AllTrue_ShouldEnable()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.UpdateNotificationPreferences(true, true, true, true, true);

        // Assert
        preferences.EmailNotifications.Should().BeTrue();
        preferences.PushNotifications.Should().BeTrue();
        preferences.SmsNotifications.Should().BeTrue();
        preferences.ReceivePromotions.Should().BeTrue();
        preferences.ReceiveNewsletter.Should().BeTrue();
    }

    [Fact]
    public void UpdateNotificationPreferences_AllFalse_ShouldDisable()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.UpdateNotificationPreferences(false, false, false, false, false);

        // Assert
        preferences.EmailNotifications.Should().BeFalse();
        preferences.PushNotifications.Should().BeFalse();
        preferences.SmsNotifications.Should().BeFalse();
        preferences.ReceivePromotions.Should().BeFalse();
        preferences.ReceiveNewsletter.Should().BeFalse();
    }
}
