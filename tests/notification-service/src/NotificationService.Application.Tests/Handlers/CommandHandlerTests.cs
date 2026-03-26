using FluentAssertions;
using Moq;
using MediatR;
using NotificationService.Application.Commands;
using NotificationService.Application.Handlers;
using NotificationService.Application.Interfaces;
using SharedKernel;
using Xunit;

namespace NotificationService.Application.Tests.Handlers;

/// <summary>
/// Tests for Send Email Notification Command Handler.
/// </summary>
public class SendEmailNotificationCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;

    public SendEmailNotificationCommandHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
    }

    [Fact]
    public async Task Handle_WithValidEmail_ShouldSendSuccessfully()
    {
        // Arrange
        var handler = new SendEmailNotificationCommandHandler(_emailServiceMock.Object);
        var command = new SendEmailNotificationCommand(
            "user@example.com",
            "Welcome",
            "<html><body>Welcome to Pet App!</body></html>");

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(
                command.RecipientEmail,
                command.Subject,
                command.HtmlContent,
                command.PlainTextContent,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            command.RecipientEmail,
            command.Subject,
            command.HtmlContent,
            command.PlainTextContent,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPlainText_ShouldIncludeBothVersions()
    {
        // Arrange
        var handler = new SendEmailNotificationCommandHandler(_emailServiceMock.Object);
        var command = new SendEmailNotificationCommand(
            "user@example.com",
            "Test",
            "<html>HTML</html>",
            "Plain text version");

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _emailServiceMock.Verify(s => s.SendEmailAsync(
            "user@example.com",
            "Test",
            "<html>HTML</html>",
            "Plain text version",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceFails_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var handler = new SendEmailNotificationCommandHandler(_emailServiceMock.Object);
        var command = new SendEmailNotificationCommand("user@example.com", "Test", "Content");

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
        exception!.Message.Should().Contain("Failed to send email");
    }

    [Theory]
    [InlineData("admin@company.com")]
    [InlineData("support+tag@example.org")]
    [InlineData("user.name@sub.example.co.uk")]
    public async Task Handle_WithVariousEmails_ShouldSendAll(string email)
    {
        // Arrange
        var handler = new SendEmailNotificationCommandHandler(_emailServiceMock.Object);
        var command = new SendEmailNotificationCommand(email, "Test", "Content");

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _emailServiceMock.Verify(s => s.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

/// <summary>
/// Tests for Send SMS Notification Command Handler.
/// </summary>
public class SendSmsNotificationCommandHandlerTests
{
    private readonly Mock<ISmsService> _smsServiceMock;

    public SendSmsNotificationCommandHandlerTests()
    {
        _smsServiceMock = new Mock<ISmsService>();
    }

    [Fact]
    public async Task Handle_WithValidPhone_ShouldSendSuccessfully()
    {
        // Arrange
        var handler = new SendSmsNotificationCommandHandler(_smsServiceMock.Object);
        var command = new SendSmsNotificationCommand("+1234567890", "Your verification code is: 123456");

        _smsServiceMock
            .Setup(s => s.SendSmsAsync("+1234567890", "Your verification code is: 123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _smsServiceMock.Verify(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSmsServiceFails_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var handler = new SendSmsNotificationCommandHandler(_smsServiceMock.Object);
        var command = new SendSmsNotificationCommand("+1234567890", "Test message");

        _smsServiceMock
            .Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
        exception!.Message.Should().Contain("SMS");
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+441234567890")]
    [InlineData("1234567890")]
    public async Task Handle_WithVariousPhoneNumbers_ShouldSendAll(string phoneNumber)
    {
        // Arrange
        var handler = new SendSmsNotificationCommandHandler(_smsServiceMock.Object);
        var command = new SendSmsNotificationCommand(phoneNumber, "Test");

        _smsServiceMock
            .Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _smsServiceMock.Verify(s => s.SendSmsAsync(phoneNumber, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

/// <summary>
/// Tests for Send User Registration Email Command Handler.
/// </summary>
public class SendUserRegistrationEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;

    public SendUserRegistrationEmailCommandHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldSendRegistrationEmail()
    {
        // Arrange
        var handler = new SendUserRegistrationEmailCommandHandler(
            _emailServiceMock.Object,
            _templateServiceMock.Object);

        var command = new SendUserRegistrationEmailCommand(
            "newuser@example.com",
            "John Doe",
            "https://app.example.com/verify?token=abc123");

        var htmlTemplate = "<html><body>Welcome, John Doe!</body></html>";

        _templateServiceMock
            .Setup(s => s.GetRegistrationTemplate("John Doe", "https://app.example.com/verify?token=abc123"))
            .Returns(htmlTemplate);

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(
                "newuser@example.com",
                It.IsAny<string>(),
                htmlTemplate,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _templateServiceMock.Verify(s => s.GetRegistrationTemplate("John Doe", "https://app.example.com/verify?token=abc123"), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailSendFails_ShouldThrowException()
    {
        // Arrange
        var handler = new SendUserRegistrationEmailCommandHandler(
            _emailServiceMock.Object,
            _templateServiceMock.Object);

        var command = new SendUserRegistrationEmailCommand(
            "user@example.com",
            "User",
            "https://link.example.com");

        _templateServiceMock
            .Setup(s => s.GetRegistrationTemplate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("<html>Template</html>");

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessLogicException>();
    }
}

/// <summary>
/// Tests for Send Password Reset Email Command Handler.
/// </summary>
public class SendPasswordResetEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;

    public SendPasswordResetEmailCommandHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldSendPasswordResetEmail()
    {
        // Arrange
        var handler = new SendPasswordResetEmailCommandHandler(
            _emailServiceMock.Object,
            _templateServiceMock.Object);

        var command = new SendPasswordResetEmailCommand(
            "user@example.com",
            "https://app.example.com/reset?token=xyz789");

        var htmlTemplate = "<html><body>Reset your password</body></html>";

        _templateServiceMock
            .Setup(s => s.GetPasswordResetTemplate("https://app.example.com/reset?token=xyz789"))
            .Returns(htmlTemplate);

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _templateServiceMock.Verify(s => s.GetPasswordResetTemplate("https://app.example.com/reset?token=xyz789"), Times.Once);
    }
}

/// <summary>
/// Tests for Send Notification Preferences Updated Command Handler.
/// </summary>
public class SendNotificationPreferencesUpdatedCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;

    public SendNotificationPreferencesUpdatedCommandHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldSendPreferencesEmail()
    {
        // Arrange
        var handler = new SendNotificationPreferencesUpdatedCommandHandler(
            _emailServiceMock.Object,
            _templateServiceMock.Object);

        var command = new SendNotificationPreferencesUpdatedCommand(
            "user@example.com",
            "Jane Smith");

        var htmlTemplate = "<html><body>Your preferences have been updated</body></html>";

        _templateServiceMock
            .Setup(s => s.GetNotificationPreferencesUpdatedTemplate("Jane Smith"))
            .Returns(htmlTemplate);

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _templateServiceMock.Verify(s => s.GetNotificationPreferencesUpdatedTemplate("Jane Smith"), Times.Once);
    }
}
