using MediatR;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using SharedKernel;

namespace NotificationService.Application.Handlers;

/// <summary>
/// Handler for sending email notifications.
/// </summary>
public sealed class SendEmailNotificationCommandHandler : IRequestHandler<SendEmailNotificationCommand, Unit>
{
    private readonly IEmailService _emailService;

    public SendEmailNotificationCommandHandler(IEmailService emailService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<Unit> Handle(SendEmailNotificationCommand request, CancellationToken cancellationToken)
    {
        var success = await _emailService.SendEmailAsync(
            request.RecipientEmail,
            request.Subject,
            request.HtmlContent,
            request.PlainTextContent,
            cancellationToken);

        if (!success)
            throw new BusinessLogicException("Failed to send email notification.", "EMAIL_SEND_FAILED");

        return Unit.Value;
    }
}

/// <summary>
/// Handler for sending SMS notifications.
/// </summary>
public sealed class SendSmsNotificationCommandHandler : IRequestHandler<SendSmsNotificationCommand, Unit>
{
    private readonly ISmsService _smsService;

    public SendSmsNotificationCommandHandler(ISmsService smsService)
    {
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
    }

    public async Task<Unit> Handle(SendSmsNotificationCommand request, CancellationToken cancellationToken)
    {
        var success = await _smsService.SendSmsAsync(
            request.PhoneNumber,
            request.Message,
            cancellationToken);

        if (!success)
            throw new BusinessLogicException("Failed to send SMS notification.", "SMS_SEND_FAILED");

        return Unit.Value;
    }
}

/// <summary>
/// Handler for user registration email.
/// </summary>
public sealed class SendUserRegistrationEmailCommandHandler : IRequestHandler<SendUserRegistrationEmailCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public SendUserRegistrationEmailCommandHandler(IEmailService emailService, IEmailTemplateService templateService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    public async Task<Unit> Handle(SendUserRegistrationEmailCommand request, CancellationToken cancellationToken)
    {
        var htmlContent = _templateService.GetRegistrationTemplate(request.FullName, request.ActivationLink);
        
        var success = await _emailService.SendEmailAsync(
            request.Email,
            "Welcome to Pet App! Verify Your Email",
            htmlContent,
            null,
            cancellationToken);

        if (!success)
            throw new BusinessLogicException("Failed to send registration email.", "EMAIL_SEND_FAILED");

        return Unit.Value;
    }
}

/// <summary>
/// Handler for password reset email.
/// </summary>
public sealed class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public SendPasswordResetEmailCommandHandler(IEmailService emailService, IEmailTemplateService templateService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    public async Task<Unit> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        var htmlContent = _templateService.GetPasswordResetTemplate(request.ResetLink);
        
        var success = await _emailService.SendEmailAsync(
            request.Email,
            "Reset Your Pet App Password",
            htmlContent,
            null,
            cancellationToken);

        if (!success)
            throw new BusinessLogicException("Failed to send password reset email.", "EMAIL_SEND_FAILED");

        return Unit.Value;
    }
}

/// <summary>
/// Handler for notification preferences updated email.
/// </summary>
public sealed class SendNotificationPreferencesUpdatedCommandHandler : IRequestHandler<SendNotificationPreferencesUpdatedCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public SendNotificationPreferencesUpdatedCommandHandler(IEmailService emailService, IEmailTemplateService templateService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    public async Task<Unit> Handle(SendNotificationPreferencesUpdatedCommand request, CancellationToken cancellationToken)
    {
        var htmlContent = _templateService.GetNotificationPreferencesUpdatedTemplate(request.FullName);
        
        var success = await _emailService.SendEmailAsync(
            request.Email,
            "Your Notification Preferences Have Been Updated",
            htmlContent,
            null,
            cancellationToken);

        if (!success)
            throw new BusinessLogicException("Failed to send notification preferences email.", "EMAIL_SEND_FAILED");

        return Unit.Value;
    }
}
