using SharedKernel;
using UserProfileService.Domain.Entities;
using UserProfileService.Domain.Events;

namespace UserProfileService.Domain.Aggregates;

/// <summary>
/// UserProfile aggregate root - manages user profile information.
/// Users have profiles created automatically when registered via Identity Service.
/// </summary>
public class UserProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public ProfileData Profile { get; private set; } = null!;
    public UserPreferences Preferences { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected UserProfile() { }

    public UserProfile(Guid id, Guid userId, string email) : base(id)
    {
        UserId = userId;
        Email = email;
        Profile = new ProfileData();
        Preferences = new UserPreferences();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        AddDomainEvent(new UserProfileCreatedEvent(id, userId, email));
    }

    public static UserProfile CreateFromRegistration(Guid userId, string email, string? fullName, string? avatar)
    {
        var userProfile = new UserProfile(Guid.NewGuid(), userId, email);

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var names = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = names.Length > 0 ? names[0] : string.Empty;
            var lastName = names.Length > 1 ? names[1] : string.Empty;

            userProfile.Profile.Update(firstName, lastName, null, null, null, null, null, null, avatar);
        }

        return userProfile;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? bio, DateTime? dateOfBirth,
        string? phoneNumber, string? address, string? city, string? country, string? profilePictureUrl)
    {
        Profile.Update(firstName, lastName, bio, dateOfBirth, phoneNumber, address, city, country, profilePictureUrl);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(Id, UserId));
    }

    public void UpdateNotificationPreferences(bool email, bool push, bool sms, bool promotions, bool newsletter)
    {
        Preferences.UpdateNotificationPreferences(email, push, sms, promotions, newsletter);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferencesUpdatedEvent(Id, UserId, email, push, sms, promotions, newsletter));
    }

    public void UpdateLanguageAndTimezone(string language, string timezone)
    {
        Preferences.Language = language;
        Preferences.Timezone = timezone;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(Id, UserId));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileDeactivatedEvent(Id, UserId));
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileActivatedEvent(Id, UserId));
    }
}
