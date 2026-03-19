using MediatR;
using SharedKernel.Infrastructure.Events;
using UserProfileService.Application.Commands;

namespace UserProfileService.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for UserRegistered event from Identity Service.
/// Creates user profile when a new user is registered.
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

        // Extract data from the integration event
        var userId = Guid.Parse(notification.Data["userId"].GetString() ?? throw new InvalidOperationException());
        var email = notification.Data["email"].GetString() ?? throw new InvalidOperationException();
        var fullName = notification.Data["fullName"]?.GetString();
        var avatar = notification.Data["avatar"]?.GetString();

        // Create user profile
        var command = new CreateProfileFromRegistrationCommand(userId, email, fullName, avatar);
        await _mediator.Send(command, cancellationToken);
    }
}

/// <summary>
/// Event handler for UserDeleted event from Identity Service.
/// Deactivates user profile when user deletes account.
/// </summary>
public class UserDeletedEventHandler : INotificationHandler<IntegrationEventNotification>
{
    private readonly IMediator _mediator;
    private readonly IUserProfileRepository _repository;

    public UserDeletedEventHandler(IMediator mediator, IUserProfileRepository repository)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Handle(IntegrationEventNotification notification, CancellationToken cancellationToken)
    {
        if (notification.EventType != "user.deleted")
            return;

        var userId = Guid.Parse(notification.Data["userId"].GetString() ?? throw new InvalidOperationException());
        
        var userProfile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (userProfile != null)
        {
            var command = new DeactivateProfileCommand(userProfile.Id);
            await _mediator.Send(command, cancellationToken);
        }
    }
}
