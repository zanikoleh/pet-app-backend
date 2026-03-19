using MediatR;
using SharedKernel.Infrastructure.Events;
using NotificationService.Application.Commands;

namespace NotificationService.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for UserRegistered event from Identity Service.
/// Sends welcome email with activation link.
/// </summary>
public class UserRegisteredEventHandler : INotificationHandler<IntegrationEventNotification>
{
    private readonly IMediator _mediator;

    public UserRegisteredEventHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Handle(IntegrationEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.EventType != "user.registered")
            return;

        var email = notification.Data["email"].GetString() ?? throw new InvalidOperationException();
        var fullName = notification.Data["fullName"]?.GetString() ?? "User";
        
        // In production, generate a proper activation link
        var activationLink = $"https://app.example.com/verify-email?token=xxx";

        var command = new SendUserRegistrationEmailCommand(email, fullName, activationLink);
        await _mediator.Send(command, cancellationToken);
    }
}

/// <summary>
/// Event handler for UserDeleted event from Identity Service.
/// Sends account deactivation confirmation email.
/// </summary>
public class UserDeletedEventHandler : INotificationHandler<IntegrationEventNotification>
{
    private readonly IMediator _mediator;

    public UserDeletedEventHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Handle(IntegrationEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.EventType != "user.deleted")
            return;

        var email = notification.Data["email"].GetString() ?? throw new InvalidOperationException();
        var fullName = notification.Data["fullName"]?.GetString() ?? "User";

        // Note: This would be sent from the account deactivation endpoint, not from event
        // This is here as an example of how to handle user events
    }
}

/// <summary>
/// Event handler for NotificationPreferencesUpdated event from User Profile Service.
/// Sends confirmation email.
/// </summary>
public class NotificationPreferencesUpdatedEventHandler : INotificationHandler<IntegrationEventNotification>
{
    private readonly IMediator _mediator;

    public NotificationPreferencesUpdatedEventHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Handle(IntegrationEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.EventType != "notification.preferences.updated")
            return;

        var email = notification.Data["email"].GetString() ?? throw new InvalidOperationException();
        var fullName = notification.Data["fullName"]?.GetString() ?? "User";

        var command = new SendNotificationPreferencesUpdatedCommand(email, fullName);
        await _mediator.Send(command, cancellationToken);
    }
}
