using MediatR;
using AutoMapper;
using UserProfileService.Application.Commands;
using UserProfileService.Application.DTOs;
using UserProfileService.Application.Interfaces;
using UserProfileService.Domain.Aggregates;

namespace UserProfileService.Application.Handlers;

/// <summary>
/// Handler for updating user profile.
/// </summary>
public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        userProfile.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Bio,
            request.DateOfBirth,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.Country,
            request.ProfilePictureUrl);

        await _repository.SaveChangesAsync(cancellationToken);
        return _mapper.Map<UserProfileDto>(userProfile);
    }
}

/// <summary>
/// Handler for updating notification preferences.
/// </summary>
public sealed class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public UpdateNotificationPreferencesCommandHandler(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<NotificationPreferencesDto> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        userProfile.UpdateNotificationPreferences(
            request.EmailNotifications,
            request.PushNotifications,
            request.SmsNotifications,
            request.ReceivePromotions,
            request.ReceiveNewsletter);

        await _repository.SaveChangesAsync(cancellationToken);
        return _mapper.Map<NotificationPreferencesDto>(userProfile.Preferences);
    }
}

/// <summary>
/// Handler for updating language and timezone.
/// </summary>
public sealed class UpdateLanguageAndTimezoneCommandHandler : IRequestHandler<UpdateLanguageAndTimezoneCommand>
{
    private readonly IUserProfileRepository _repository;

    public UpdateLanguageAndTimezoneCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Handle(UpdateLanguageAndTimezoneCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        userProfile.UpdateLanguageAndTimezone(request.Language, request.Timezone);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for deactivating user profile.
/// </summary>
public sealed class DeactivateProfileCommandHandler : IRequestHandler<DeactivateProfileCommand>
{
    private readonly IUserProfileRepository _repository;

    public DeactivateProfileCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Handle(DeactivateProfileCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        userProfile.Deactivate();
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for creating user profile from registration.
/// </summary>
public sealed class CreateProfileFromRegistrationCommandHandler : IRequestHandler<CreateProfileFromRegistrationCommand, Guid>
{
    private readonly IUserProfileRepository _repository;

    public CreateProfileFromRegistrationCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> Handle(CreateProfileFromRegistrationCommand request, CancellationToken cancellationToken)
    {
        // Check if profile already exists
        var existingProfile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingProfile != null)
            return existingProfile.Id;

        var userProfile = UserProfile.CreateFromRegistration(request.UserId, request.Email, request.FullName, request.Avatar);
        _repository.Add(userProfile);
        await _repository.SaveChangesAsync(cancellationToken);

        return userProfile.Id;
    }
}
