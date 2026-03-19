using AutoMapper;
using MediatR;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Queries;

namespace IdentityService.Application.Handlers;

/// <summary>
/// Handler for getting user profile.
/// </summary>
public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        return _mapper.Map<UserDto>(user);
    }
}

/// <summary>
/// Handler for checking if email exists.
/// </summary>
public sealed class EmailExistsQueryHandler : IRequestHandler<EmailExistsQuery, bool>
{
    private readonly IUserRepository _userRepository;

    public EmailExistsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<bool> Handle(EmailExistsQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
    }
}

/// <summary>
/// Handler for validating access token.
/// </summary>
public sealed class ValidateAccessTokenQueryHandler : IRequestHandler<ValidateAccessTokenQuery, Guid?>
{
    private readonly IJwtTokenService _jwtTokenService;

    public ValidateAccessTokenQueryHandler(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public async Task<Guid?> Handle(ValidateAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId, _) = _jwtTokenService.ValidateAccessToken(request.AccessToken);
        return isValid ? userId : null;
    }
}
