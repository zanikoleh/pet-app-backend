using MediatR;

namespace NotificationService.Application.Commands;

/// <summary>
/// Command to send email notification.
/// </summary>
public sealed record SendEmailNotificationCommand(
    string RecipientEmail,
    string Subject,
    string HtmlContent,
    string? PlainTextContent = null) : IRequest<Unit>;

/// <summary>
/// Command to send SMS notification.
/// </summary>
public sealed record SendSmsNotificationCommand(
    string PhoneNumber,
    string Message) : IRequest<Unit>;

/// <summary>
/// Command to queue email for user registration.
/// </summary>
public sealed record SendUserRegistrationEmailCommand(
    string Email,
    string FullName,
    string ActivationLink) : IRequest<Unit>;

/// <summary>
/// Command to queue email for password reset.
/// </summary>
public sealed record SendPasswordResetEmailCommand(
    string Email,
    string ResetLink) : IRequest<Unit>;

/// <summary>
/// Command to queue email for notification preferences update.
/// </summary>
public sealed record SendNotificationPreferencesUpdatedCommand(
    string Email,
    string FullName) : IRequest<Unit>;
