namespace NotificationService.Application.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends email to multiple recipients.
    /// </summary>
    Task<bool> SendEmailBatchAsync(
        List<string> toEmails,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for sending SMS.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS.
    /// </summary>
    Task<bool> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for building email templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Gets user registration email template.
    /// </summary>
    string GetRegistrationTemplate(string fullName, string activationLink);

    /// <summary>
    /// Gets password reset email template.
    /// </summary>
    string GetPasswordResetTemplate(string resetLink);

    /// <summary>
    /// Gets notification preferences updated template.
    /// </summary>
    string GetNotificationPreferencesUpdatedTemplate(string fullName);

    /// <summary>
    /// Gets welcome email template.
    /// </summary>
    string GetWelcomeTemplate(string fullName);

    /// <summary>
    /// Gets account deactivated email template.
    /// </summary>
    string GetAccountDeactivatedTemplate(string fullName);
}
