using MediatR;
using AutoMapper;
using UserProfileService.Application.DTOs;
using UserProfileService.Application.Interfaces;
using UserProfileService.Application.Queries;

namespace UserProfileService.Application.Handlers;

/// <summary>
/// Handler for getting user profile by ID.
/// </summary>
public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null || !userProfile.IsActive)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        return _mapper.Map<UserProfileDto>(userProfile);
    }
}

/// <summary>
/// Handler for getting user profile by UserId.
/// </summary>
public sealed class GetUserProfileByUserIdQueryHandler : IRequestHandler<GetUserProfileByUserIdQuery, UserProfileDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public GetUserProfileByUserIdQueryHandler(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserProfileDto> Handle(GetUserProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (userProfile == null || !userProfile.IsActive)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        return _mapper.Map<UserProfileDto>(userProfile);
    }
}

/// <summary>
/// Handler for getting notification preferences.
/// </summary>
public sealed class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public GetNotificationPreferencesQueryHandler(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _repository.GetByIdAsync(request.UserProfileId, cancellationToken);
        if (userProfile == null || !userProfile.IsActive)
            throw new NotFoundException("User profile not found.", "PROFILE_NOT_FOUND");

        return _mapper.Map<NotificationPreferencesDto>(userProfile.Preferences);
    }
}
