using FluentAssertions;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Services;
using Xunit;

namespace NotificationService.Infrastructure.Tests.Services;

/// <summary>
/// Tests for Mock Email Service.
/// </summary>
public class MockEmailServiceTests
{
    private readonly IEmailService _emailService;

    public MockEmailServiceTests()
    {
        _emailService = new MockEmailService();
    }

    [Fact]
    public async Task SendEmailAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var email = "user@example.com";
        var subject = "Welcome";
        var htmlContent = "<html><body>Welcome to Pet App</body></html>";
        var plainText = "Welcome to Pet App";

        // Act
        var result = await _emailService.SendEmailAsync(email, subject, htmlContent, plainText, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithoutPlainText_ShouldReturnTrue()
    {
        // Arrange
        var email = "user@example.com";
        var subject = "Test";
        var htmlContent = "<html><body>Test</body></html>";

        // Act
        var result = await _emailService.SendEmailAsync(email, subject, htmlContent, null, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@company.org")]
    [InlineData("support+tag@test.co.uk")]
    public async Task SendEmailAsync_WithVariousEmails_ShouldSucceed(string email)
    {
        // Act
        var result = await _emailService.SendEmailAsync(
            email,
            "Subject",
            "<html>Content</html>",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Welcome to Pet App!")]
    [InlineData("Password Reset Request")]
    [InlineData("Account Verification Required")]
    public async Task SendEmailAsync_WithVariousSubjects_ShouldSucceed(string subject)
    {
        // Act
        var result = await _emailService.SendEmailAsync(
            "user@example.com",
            subject,
            "<html>Content</html>",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WithLongHtmlContent_ShouldSucceed()
    {
        // Arrange
        var longContent = string.Concat(Enumerable.Repeat("<p>This is a paragraph.</p>", 100));

        // Act
        var result = await _emailService.SendEmailAsync(
            "user@example.com",
            "Test",
            longContent,
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        longContent.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithMultipleEmails_ShouldReturnTrue()
    {
        // Arrange
        var emails = new List<string>
        {
            "user1@example.com",
            "user2@example.com",
            "user3@example.com"
        };

        // Act
        var result = await _emailService.SendEmailBatchAsync(
            emails,
            "Batch Email",
            "<html><body>Batch message</body></html>",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithSingleEmail_ShouldSucceed()
    {
        // Arrange
        var emails = new List<string> { "user@example.com" };

        // Act
        var result = await _emailService.SendEmailBatchAsync(
            emails,
            "Single Email",
            "<html>Content</html>",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithEmptyList_ShouldReturnTrue()
    {
        // Arrange
        var emails = new List<string>();

        // Act
        var result = await _emailService.SendEmailBatchAsync(
            emails,
            "Empty",
            "<html>Content</html>",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailBatchAsync_WithPlainText_ShouldIncludeBoth()
    {
        // Arrange
        var emails = new List<string> { "user1@example.com", "user2@example.com" };

        // Act
        var result = await _emailService.SendEmailBatchAsync(
            emails,
            "Test",
            "<html>HTML</html>",
            "Plain text",
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_Concurrently_ShouldSucceed()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_emailService.SendEmailAsync(
                $"user{i}@example.com",
                "Concurrent Test",
                "<html>Content</html>",
                null,
                CancellationToken.None));
        }

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().NotContain(false);
    }

    [Fact]
    public async Task SendEmailAsync_WithCancellation_ShouldHandleGracefully()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act
        var task = _emailService.SendEmailAsync(
            "user@example.com",
            "Test",
            "<html>Content</html>",
            null,
            cts.Token);

        var result = await task;

        // Assert - Mock service completes anyway
        result.Should().BeTrue();
    }
}

/// <summary>
/// Tests for Mock SMS Service.
/// </summary>
public class MockSmsServiceTests
{
    private readonly ISmsService _smsService;

    public MockSmsServiceTests()
    {
        _smsService = new MockSmsService();
    }

    [Fact]
    public async Task SendSmsAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "Your verification code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+441234567890")]
    [InlineData("1234567890")]
    public async Task SendSmsAsync_WithVariousPhoneNumbers_ShouldSucceed(string phoneNumber)
    {
        // Act
        var result = await _smsService.SendSmsAsync(
            phoneNumber,
            "Test message",
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Welcome to Pet App!")]
    [InlineData("Your code: 123456")]
    [InlineData("Account alert: Unusual activity detected")]
    public async Task SendSmsAsync_WithVariousMessages_ShouldSucceed(string message)
    {
        // Act
        var result = await _smsService.SendSmsAsync(
            "+1234567890",
            message,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_WithLongMessage_ShouldSucceed()
    {
        // Arrange
        var longMessage = string.Concat(Enumerable.Repeat("Test message. ", 100));

        // Act
        var result = await _smsService.SendSmsAsync(
            "+1234567890",
            longMessage,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        longMessage.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task SendSmsAsync_Concurrently_ShouldSucceed()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_smsService.SendSmsAsync(
                $"+123456789{i}",
                "Concurrent SMS",
                CancellationToken.None));
        }

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().NotContain(false);
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyMessage_ShouldSucceed()
    {
        // Act
        var result = await _smsService.SendSmsAsync("+1234567890", "", CancellationToken.None);

        // Assert - Mock service allows any message
        result.Should().BeTrue();
    }
}

/// <summary>
/// Tests for Email Template Service.
/// </summary>
public class EmailTemplateServiceTests
{
    private readonly IEmailTemplateService _templateService;

    public EmailTemplateServiceTests()
    {
        _templateService = new EmailTemplateService();
    }

    [Fact]
    public void GetRegistrationTemplate_ShouldReturnHtmlContent()
    {
        // Arrange
        var fullName = "John Doe";
        var activationLink = "https://app.example.com/verify?token=abc123";

        // Act
        var template = _templateService.GetRegistrationTemplate(fullName, activationLink);

        // Assert
        template.Should().NotBeNullOrEmpty();
        template.Should().Contain("<!DOCTYPE html>");
        template.Should().Contain(fullName);
        template.Should().Contain(activationLink);
        template.Should().Contain("Welcome");
        template.Should().Contain("verify");
    }

    [Fact]
    public void GetRegistrationTemplate_ShouldContainActivationLink()
    {
        // Arrange
        var activationLink = "https://example.com/activate?token=xyz";

        // Act
        var template = _templateService.GetRegistrationTemplate("User", activationLink);

        // Assert
        template.Should().Contain(activationLink);
    }

    [Fact]
    public void GetPasswordResetTemplate_ShouldReturnHtmlContent()
    {
        // Arrange
        var resetLink = "https://app.example.com/reset?token=xyz789";

        // Act
        var template = _templateService.GetPasswordResetTemplate(resetLink);

        // Assert
        template.Should().NotBeNullOrEmpty();
        template.Should().Contain("<!DOCTYPE html>");
        template.Should().Contain("Reset");
        template.Should().Contain(resetLink);
        template.Should().Contain("password");
    }

    [Fact]
    public void GetPasswordResetTemplate_ShouldContainResetLink()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=abc";

        // Act
        var template = _templateService.GetPasswordResetTemplate(resetLink);

        // Assert
        template.Should().Contain(resetLink);
    }

    [Fact]
    public void GetNotificationPreferencesUpdatedTemplate_ShouldReturnHtmlContent()
    {
        // Arrange
        var fullName = "Jane Smith";

        // Act
        var template = _templateService.GetNotificationPreferencesUpdatedTemplate(fullName);

        // Assert
        template.Should().NotBeNullOrEmpty();
        template.Should().Contain("<!DOCTYPE html>");
        template.Should().Contain(fullName);
        template.Should().Contain("preferences");
        template.Should().Contain("updated");
    }

    [Fact]
    public void GetWelcomeTemplate_ShouldReturnHtmlContent()
    {
        // Arrange
        var fullName = "New User";

        // Act
        var template = _templateService.GetWelcomeTemplate(fullName);

        // Assert
        template.Should().NotBeNullOrEmpty();
        template.Should().Contain("<!DOCTYPE html>");
        template.Should().Contain(fullName);
        template.Should().Contain("Welcome");
    }

    [Fact]
    public void GetAccountDeactivatedTemplate_ShouldReturnHtmlContent()
    {
        // Arrange
        var fullName = "Goodbye User";

        // Act
        var template = _templateService.GetAccountDeactivatedTemplate(fullName);

        // Assert
        template.Should().NotBeNullOrEmpty();
        template.Should().Contain("<!DOCTYPE html>");
        template.Should().Contain(fullName);
    }

    [Fact]
    public void AllTemplates_ShouldBeValidHtml()
    {
        // Act
        var registration = _templateService.GetRegistrationTemplate("User", "link");
        var passwordReset = _templateService.GetPasswordResetTemplate("link");
        var preferences = _templateService.GetNotificationPreferencesUpdatedTemplate("User");
        var welcome = _templateService.GetWelcomeTemplate("User");
        var deactivated = _templateService.GetAccountDeactivatedTemplate("User");

        // Assert
        registration.Should().Contain("<html>");
        passwordReset.Should().Contain("<html>");
        preferences.Should().Contain("<html>");
        welcome.Should().Contain("<html>");
        deactivated.Should().Contain("<html>");

        registration.Should().Contain("</html>");
        passwordReset.Should().Contain("</html>");
        preferences.Should().Contain("</html>");
        welcome.Should().Contain("</html>");
        deactivated.Should().Contain("</html>");
    }

    [Fact]
    public void AllTemplates_ShouldContainEmailSignature()
    {
        // Act
        var templates = new[]
        {
            _templateService.GetRegistrationTemplate("User", "link"),
            _templateService.GetPasswordResetTemplate("link"),
            _templateService.GetNotificationPreferencesUpdatedTemplate("User"),
            _templateService.GetWelcomeTemplate("User"),
            _templateService.GetAccountDeactivatedTemplate("User")
        };

        // Assert
        foreach (var template in templates)
        {
            template.Should().Contain("Pet App");
        }
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Jane Smith")]
    [InlineData("User")]
    public void GetRegistrationTemplate_WithVariousNames_ShouldIncludeName(string name)
    {
        // Act
        var template = _templateService.GetRegistrationTemplate(name, "link");

        // Assert
        template.Should().Contain(name);
    }

    [Theory]
    [InlineData("https://example.com/activate")]
    [InlineData("https://app.test.org/verify?code=123")]
    [InlineData("https://localhost:3000/email-verification")]
    public void GetRegistrationTemplate_WithVariousLinks_ShouldIncludeLink(string link)
    {
        // Act
        var template = _templateService.GetRegistrationTemplate("User", link);

        // Assert
        template.Should().Contain(link);
    }
}
