using SharedKernel;

namespace UserProfileService.Domain.Entities;

/// <summary>
/// Represents a user profile - child entity of UserProfile aggregate.
/// </summary>
public class ProfileData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public ProfileData()
    {
    }

    public void Update(string? firstName, string? lastName, string? bio, DateTime? dateOfBirth,
        string? phoneNumber, string? address, string? city, string? country, string? profilePictureUrl)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;
        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;
        Bio = bio;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;
        ProfilePictureUrl = profilePictureUrl;
    }
}

/// <summary>
/// Represents user preferences - child entity of UserProfile aggregate.
/// </summary>
public class UserPreferences
{
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
    public bool NotificationsEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool ReceivePromotions { get; set; } = true;
    public bool ReceiveNewsletter { get; set; } = true;

    public UserPreferences()
    {
    }

    public void UpdateNotificationPreferences(bool email, bool push, bool sms, bool promotions, bool newsletter)
    {
        EmailNotifications = email;
        PushNotifications = push;
        SmsNotifications = sms;
        ReceivePromotions = promotions;
        ReceiveNewsletter = newsletter;
    }
}
