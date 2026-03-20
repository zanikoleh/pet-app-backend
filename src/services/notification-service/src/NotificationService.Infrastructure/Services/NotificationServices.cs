using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// Mock email service for development.
/// In production, use SendGrid or similar.
/// </summary>
public class MockEmailService : IEmailService
{
    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default)
    {
        // Log email in development
        Console.WriteLine($"[EMAIL] To: {toEmail}");
        Console.WriteLine($"[EMAIL] Subject: {subject}");
        Console.WriteLine($"[EMAIL] Content: {htmlContent}");
        
        return await Task.FromResult(true);
    }

    public async Task<bool> SendEmailBatchAsync(
        List<string> toEmails,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var toEmail in toEmails)
        {
            await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent, cancellationToken);
        }
        return true;
    }
}

/// <summary>
/// Mock SMS service for development.
/// In production, use Twilio or similar.
/// </summary>
public class MockSmsService : ISmsService
{
    public async Task<bool> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        // Log SMS in development
        Console.WriteLine($"[SMS] To: {phoneNumber}");
        Console.WriteLine($"[SMS] Message: {message}");
        
        return await Task.FromResult(true);
    }
}

/// <summary>
/// Email template service with HTML templates.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    public string GetRegistrationTemplate(string fullName, string activationLink)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; }
                    .header {background-color: #007bff; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; }
                    .button { display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Welcome to Pet App, {{fullName}}!</h1>
                    </div>
                    <div class="content">
                        <p>Thank you for registering with Pet App. We're excited to have you on board!</p>
                        <p>To complete your registration, please verify your email address by clicking the button below:</p>
                        <p><a class="button" href="{{activationLink}}">Verify Email</a></p>
                        <p>If the button doesn't work, you can also copy and paste this link in your browser:</p>
                        <p>{{activationLink}}</p>
                        <p>This link will expire in 24 hours.</p>
                        <p>Best regards,<br>The Pet App Team</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public string GetPasswordResetTemplate(string resetLink)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; }
                    .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; }
                    .button { display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Reset Your Password</h1>
                    </div>
                    <div class="content">
                        <p>We received a request to reset your Pet App password.</p>
                        <p>Click the button below to reset your password:</p>
                        <p><a class="button" href="{{resetLink}}">Reset Password</a></p>
                        <p>If the button doesn't work, you can also copy and paste this link in your browser:</p>
                        <p>{{resetLink}}</p>
                        <p>This link will expire in 1 hour.</p>
                        <p>If you didn't request this, you can safely ignore this email.</p>
                        <p>Best regards,<br>The Pet App Team</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public string GetNotificationPreferencesUpdatedTemplate(string fullName)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; }
                    .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Notification Preferences Updated</h1>
                    </div>
                    <div class="content">
                        <p>Hi {{fullName}},</p>
                        <p>We're confirming that your notification preferences have been updated successfully.</p>
                        <p>You can manage your preferences anytime by logging into your account and visiting your settings.</p>
                        <p>Best regards,<br>The Pet App Team</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public string GetWelcomeTemplate(string fullName)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; }
                    .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Welcome to Pet App!</h1>
                    </div>
                    <div class="content">
                        <p>Hi {{fullName}},</p>
                        <p>Your email has been verified and your account is now fully active.</p>
                        <p>You can now start using all features of Pet App.</p>
                        <p>In case you have any questions, don't hesitate to contact our support team.</p>
                        <p>Best regards,<br>The Pet App Team</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public string GetAccountDeactivatedTemplate(string fullName)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; }
                    .header { background-color: #dc3545; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Account Deactivated</h1>
                    </div>
                    <div class="content">
                        <p>Hi {{fullName}},</p>
                        <p>We're confirming that your Pet App account has been deactivated.</p>
                        <p>Your account data will be retained for 30 days. If you change your mind, you can reactivate your account within this period.</p>
                        <p>If you have any questions, please contact our support team.</p>
                        <p>Best regards,<br>The Pet App Team</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }
}
