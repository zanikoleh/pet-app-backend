using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfileService.Domain.Aggregates;
using SharedKernel.Infrastructure;

namespace UserProfileService.Infrastructure.Persistence;

/// <summary>
/// Entity configuration for UserProfile aggregate root.
/// </summary>
public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        // Primary key
        builder.HasKey(up => up.Id);
        builder.Property(up => up.Id).ValueGeneratedNever();

        // Scalar properties
        builder.Property(up => up.UserId).IsRequired();
        builder.Property(up => up.Email).IsRequired().HasMaxLength(254);
        builder.Property(up => up.IsActive);
        builder.Property(up => up.CreatedAt);
        builder.Property(up => up.UpdatedAt);

        // Owned entity: Profile
        builder.OwnsOne(up => up.Profile, pd =>
        {
            pd.ToTable("UserProfiles");
            pd.Property(p => p.FirstName).HasMaxLength(100);
            pd.Property(p => p.LastName).HasMaxLength(100);
            pd.Property(p => p.Bio).HasMaxLength(500);
            pd.Property(p => p.DateOfBirth);
            pd.Property(p => p.PhoneNumber).HasMaxLength(20);
            pd.Property(p => p.Address).HasMaxLength(200);
            pd.Property(p => p.City).HasMaxLength(50);
            pd.Property(p => p.Country).HasMaxLength(50);
            pd.Property(p => p.ProfilePictureUrl).HasMaxLength(500);
            
            // Ignore DomainEvents for the owned Profile entity
            pd.Ignore(p => p.DomainEvents);
        });

        // Owned entity: Preferences
        builder.OwnsOne(up => up.Preferences, prefs =>
        {
            prefs.ToTable("UserPreferences");
            prefs.WithOwner().HasForeignKey("UserProfileId");
            prefs.Property(p => p.Language).HasMaxLength(10).HasDefaultValue("en");
            prefs.Property(p => p.Timezone).HasMaxLength(50).HasDefaultValue("UTC");
            prefs.Property(p => p.NotificationsEnabled).HasDefaultValue(true);
            prefs.Property(p => p.EmailNotifications).HasDefaultValue(true);
            prefs.Property(p => p.PushNotifications).HasDefaultValue(true);
            prefs.Property(p => p.SmsNotifications).HasDefaultValue(false);
            prefs.Property(p => p.ReceivePromotions).HasDefaultValue(true);
            prefs.Property(p => p.ReceiveNewsletter).HasDefaultValue(true);
            
            // Ignore DomainEvents for the owned Preferences entity
            prefs.Ignore(p => p.DomainEvents);
        });

        // Indices
        builder.HasIndex(up => up.UserId).IsUnique();
        builder.HasIndex(up => up.Email);
        builder.HasIndex(up => up.IsActive);

        // Navigation ignored (owned collections handled above)
        builder.Ignore(up => up.DomainEvents);
    }
}
