using Xunit;
using FluentAssertions;
using IdentityService.Domain.ValueObjects;
using SharedKernel;

namespace IdentityService.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("john.doe@company.co.uk")]
    [InlineData("test+tag@example.org")]
    [InlineData("123@example.com")]
    public void Create_WithValidEmail_ShouldSucceed(string emailAddress)
    {
        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(emailAddress.ToLower());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("invalid")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string? emailAddress)
    {
        // Act & Assert
        var action = () => Email.Create(emailAddress!);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Email_ShouldBeCaseInsensitive()
    {
        // Arrange
        var email1 = Email.Create("User@Example.COM");
        var email2 = Email.Create("user@example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Email_ShouldBeEquatable()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");
        var email3 = Email.Create("different@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Should().NotBe(email3);
    }

    [Fact]
    public void Email_ShouldHaveConsistentHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }
}

public class PasswordHashTests
{
    [Theory]
    [InlineData("ValidPassword123!")]
    [InlineData("SuperSecurePass456@")]
    [InlineData("LongPassword1234567890")]
    public void Create_WithValidPassword_ShouldSucceed(string password)
    {
        // Act
        var hash = PasswordHash.Create(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Value.Should().NotBeNullOrEmpty();
        hash.Value.Should().NotBe(password); // Should be hashed, not plain text
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Create_WithInvalidPassword_ShouldThrowDomainException(string password)
    {
        // Act & Assert
        var action = () => PasswordHash.Create(password);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNull_ShouldThrowDomainException()
    {
        // Act & Assert
        var action = () => PasswordHash.Create(null!);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "MySecurePassword123!";
        var hash = PasswordHash.Create(password);

        // Act
        var result = hash.Verify(password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = PasswordHash.Create("CorrectPassword123!");

        // Act
        var result = hash.Verify("WrongPassword123!");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        // Arrange
        var hash = PasswordHash.Create("MyPassword123!");

        // Act
        var result = hash.Verify("mypassword123!");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Password123!", "password123!", false)]
    [InlineData("Test@1234", "Test@1234", true)]
    [InlineData("Test@1234", "Test@12345", false)]
    public void Verify_WithVariousInputs_ShouldBehaveCorrectly(
        string hashedPassword, string verifyPassword, bool shouldMatch)
    {
        // Arrange
        var hash = PasswordHash.Create(hashedPassword);

        // Act
        var result = hash.Verify(verifyPassword);

        // Assert
        result.Should().Be(shouldMatch);
    }

    [Fact]
    public void PasswordHash_ShouldBeDifferentEachTime()
    {
        // Arrange
        const string password = "SamePassword123!";
        var hash1 = PasswordHash.Create(password);
        var hash2 = PasswordHash.Create(password);

        // Act & Assert
        // BCrypt produces different hashes for the same password (due to salt)
        hash1.Value.Should().NotBe(hash2.Value);
        // Both should correctly verify the password
        hash1.Verify(password).Should().BeTrue();
        hash2.Verify(password).Should().BeTrue();
    }

    [Fact]
    public void PasswordHash_IsNotReversible()
    {
        // Arrange
        const string password = "SecretPassword123!";
        var hash = PasswordHash.Create(password);

        // Act & Assert
        // The hash value should not contain the password
        hash.Value.Should().NotContain(password);
    }
}

public class RoleTests
{
    [Fact]
    public void UserRole_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Role.User.Value.Should().Be("User");
    }

    [Fact]
    public void AdminRole_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Role.Admin.Value.Should().Be("Admin");
    }

    [Fact]
    public void DefaultUserRole_ShouldBeUser()
    {
        // Act & Assert
        Role.User.Should().NotBeNull();
    }

    [Fact]
    public void AllRoles_ShouldBeDistinct()
    {
        // Act
        var roles = new[] { Role.User, Role.Admin };

        // Assert
        var uniqueRoles = roles.Distinct().Count();
        uniqueRoles.Should().Be(2);
    }
}
