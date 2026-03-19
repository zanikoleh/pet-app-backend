using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Entities;
using SharedKernel.Infrastructure;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Entity configuration for User aggregate root.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        // Value object: Email
        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(254);

        // Value object: PasswordHash
        builder.Property(u => u.PasswordHash)
            .HasConversion(
                ph => ph.Hash,
                v => new PasswordHash(v))
            .HasColumnName("PasswordHash")
            .IsRequired();

        // Scalar properties
        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.Avatar)
            .HasMaxLength(500);

        builder.Property(u => u.IsEmailVerified);
        builder.Property(u => u.IsActive);
        builder.Property(u => u.LastLoginAt);

        // Owned collections
        builder.OwnsMany(u => u.OAuthLinks, oal =>
        {
            oal.ToTable("OAuthProviderLinks");
            oal.WithOwner().HasForeignKey("UserId");
            oal.HasKey("Id");
            oal.Property(o => o.Id).ValueGeneratedNever();
            oal.Property(o => o.Provider).HasMaxLength(50).IsRequired();
            oal.Property(o => o.ProviderUserId).HasMaxLength(500).IsRequired();
            oal.Property(o => o.LinkedAt);
            oal.Property(o => o.UnlinkedAt);
        });

        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshTokens");
            rt.WithOwner().HasForeignKey("UserId");
            rt.HasKey("Id");
            rt.Property(r => r.Id).ValueGeneratedNever();
            rt.Property(r => r.Token).HasMaxLength(500).IsRequired();
            rt.Property(r => r.ExpiresAt);
            rt.Property(r => r.RevokedAt);
            rt.Property(r => r.IsRevoked);
        });

        // Value object: Role
        builder.Property(u => u.Role)
            .HasConversion(r => r.Value, v => Role.Create(v))
            .HasColumnName("Role")
            .IsRequired()
            .HasMaxLength(20);

        // Audit properties
        builder.Property(u => u.CreatedAt);
        builder.Property(u => u.UpdatedAt);

        // Indices
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.CreatedAt);

        // Navigation ignored (owned collections handled above)
        builder.Ignore(u => u.DomainEvents);
    }
}
