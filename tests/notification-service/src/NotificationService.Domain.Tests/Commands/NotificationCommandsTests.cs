using FluentAssertions;
using NotificationService.Application.Commands;
using NotificationService.Application.DTOs;
using Xunit;

namespace NotificationService.Domain.Tests.Commands;

/// <summary>
/// Tests for Notification Commands - mainly DTO validation.
/// </summary>
public class NotificationCommandsTests
{
    [Fact]
    public void SendEmailNotificationCommand_WithValidInput_ShouldCreate()
    {
        // Arrange
        var email = "user@example.com";
        var subject = "Test Email";
        var htmlContent = "<html><body>Test</body></html>";

        // Act
        var command = new SendEmailNotificationCommand(email, subject, htmlContent);

        // Assert
        command.RecipientEmail.Should().Be(email);
        command.Subject.Should().Be(subject);
        command.HtmlContent.Should().Be(htmlContent);
        command.PlainTextContent.Should().BeNull();
    }

    [Fact]
    public void SendEmailNotificationCommand_WithPlainText_ShouldIncludeBoth()
    {
        // Arrange
        var email = "user@example.com";
        var subject = "Test Email";
        var htmlContent = "<html><body>Test</body></html>";
        var plainText = "This is plain text";

        // Act
        var command = new SendEmailNotificationCommand(email, subject, htmlContent, plainText);

        // Assert
        command.PlainTextContent.Should().Be(plainText);
    }

    [Fact]
    public void SendSmsNotificationCommand_WithValidInput_ShouldCreate()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "Hello, this is an SMS";

        // Act
        var command = new SendSmsNotificationCommand(phoneNumber, message);

        // Assert
        command.PhoneNumber.Should().Be(phoneNumber);
        command.Message.Should().Be(message);
    }

    [Fact]
    public void SendUserRegistrationEmailCommand_WithValidInput_ShouldCreate()
    {
        // Arrange
        var email = "newuser@example.com";
        var fullName = "John Doe";
        var activationLink = "https://app.example.com/verify?token=abc123";

        // Act
        var command = new SendUserRegistrationEmailCommand(email, fullName, activationLink);

        // Assert
        command.Email.Should().Be(email);
        command.FullName.Should().Be(fullName);
        command.ActivationLink.Should().Be(activationLink);
    }

    [Fact]
    public void SendPasswordResetEmailCommand_WithValidInput_ShouldCreate()
    {
        // Arrange
        var email = "user@example.com";
        var resetLink = "https://app.example.com/reset?token=xyz789";

        // Act
        var command = new SendPasswordResetEmailCommand(email, resetLink);

        // Assert
        command.Email.Should().Be(email);
        command.ResetLink.Should().Be(resetLink);
    }

    [Fact]
    public void SendNotificationPreferencesUpdatedCommand_WithValidInput_ShouldCreate()
    {
        // Arrange
        var email = "user@example.com";
        var fullName = "Jane Smith";

        // Act
        var command = new SendNotificationPreferencesUpdatedCommand(email, fullName);

        // Assert
        command.Email.Should().Be(email);
        command.FullName.Should().Be(fullName);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("support@company.org")]
    [InlineData("admin+tag@test.co.uk")]
    public void SendEmailNotificationCommand_WithVariousEmails_ShouldAcceptAll(string email)
    {
        // Act
        var command = new SendEmailNotificationCommand(email, "Subject", "<html>Content</html>");

        // Assert
        command.RecipientEmail.Should().Be(email);
    }

    [Theory]
    [InlineData("Welcome!", "Welcome to Pet App")]
    [InlineData("Password Reset", "Please reset your password")]
    [InlineData("Account Confirmation", "Confirm your account")]
    public void SendEmailNotificationCommand_WithVariousSubjects_ShouldAcceptAll(string subject, string htmlContent)
    {
        // Act
        var command = new SendEmailNotificationCommand("user@example.com", subject, htmlContent);

        // Assert
        command.Subject.Should().Be(subject);
    }

    [Fact]
    public void SendEmailNotificationCommand_WithLongHtmlContent_ShouldAccept()
    {
        // Arrange
        var htmlContent = string.Concat(Enumerable.Repeat("<p>This is a test paragraph.</p>", 100));

        // Act
        var command = new SendEmailNotificationCommand("user@example.com", "Test", htmlContent);

        // Assert
        command.HtmlContent.Length.Should().BeGreaterThan(100);
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+441234567890")]
    [InlineData("1234567890")]
    public void SendSmsNotificationCommand_WithVariousPhoneNumbers_ShouldAcceptAll(string phoneNumber)
    {
        // Act
        var command = new SendSmsNotificationCommand(phoneNumber, "Test message");

        // Assert
        command.PhoneNumber.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("Welcome to Pet App!")]
    [InlineData("Your verification code is: 123456")]
    [InlineData("Account alert: Unusual activity detected")]
    public void SendSmsNotificationCommand_WithVariousMessages_ShouldAcceptAll(string message)
    {
        // Act
        var command = new SendSmsNotificationCommand("+1234567890", message);

        // Assert
        command.Message.Should().Be(message);
    }
}

/// <summary>
/// Tests for Notification DTOs.
/// </summary>
public class NotificationDtosTests
{
    [Fact]
    public void EmailNotificationDto_WithValidData_ShouldInitialize()
    {
        // Arrange & Act
        var dto = new EmailNotificationDto
        {
            RecipientEmail = "user@example.com",
            Subject = "Test",
            HtmlContent = "<html>Content</html>",
            SentAt = DateTime.UtcNow
        };

        // Assert
        dto.RecipientEmail.Should().Be("user@example.com");
        dto.Subject.Should().Be("Test");
        dto.HtmlContent.Should().Be("<html>Content</html>");
        dto.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EmailNotificationDto_WithPlainText_ShouldInclude()
    {
        // Arrange & Act
        var dto = new EmailNotificationDto
        {
            RecipientEmail = "user@example.com",
            Subject = "Test",
            HtmlContent = "<html>Content</html>",
            PlainTextContent = "Plain text version",
            SentAt = DateTime.UtcNow
        };

        // Assert
        dto.PlainTextContent.Should().Be("Plain text version");
    }

    [Fact]
    public void EmailNotificationDto_WithoutPlainText_ShouldHaveNull()
    {
        // Arrange & Act
        var dto = new EmailNotificationDto
        {
            RecipientEmail = "user@example.com",
            Subject = "Test",
            HtmlContent = "<html>Content</html>",
            SentAt = DateTime.UtcNow
        };

        // Assert
        dto.PlainTextContent.Should().BeNull();
    }

    [Fact]
    public void EmailNotificationDto_AllPropertiesCanBeSet()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var email = "test@example.com";
        var subject = "Important";
        var html = "<html><body>HTML Content</body></html>";
        var plain = "Plain text";

        // Act
        var dto = new EmailNotificationDto();
        dto.RecipientEmail = email;
        dto.Subject = subject;
        dto.HtmlContent = html;
        dto.PlainTextContent = plain;
        dto.SentAt = now;

        // Assert
        dto.RecipientEmail.Should().Be(email);
        dto.Subject.Should().Be(subject);
        dto.HtmlContent.Should().Be(html);
        dto.PlainTextContent.Should().Be(plain);
        dto.SentAt.Should().Be(now);
    }
}

/// <summary>
/// Tests for Email validation patterns.
/// </summary>
public class EmailValidationTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("firstname.lastname@example.co.uk")]
    [InlineData("email+tag@example.org")]
    [InlineData("user123@sub.example.com")]
    public void ValidEmailAddresses_ShouldBeAccepted(string email)
    {
        // Act
        var command = new SendEmailNotificationCommand(email, "Test", "Content");

        // Assert
        command.RecipientEmail.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void EmptyEmailAddresses_CouldBeInvalid(string? email)
    {
        // Note: Actual validation should be in application/API layer
        // Domain allows any value, validation happens upstream
        var command = new SendEmailNotificationCommand(email ?? "", "Test", "Content");
        command.RecipientEmail.Should().Be(email ?? "");
    }
}
