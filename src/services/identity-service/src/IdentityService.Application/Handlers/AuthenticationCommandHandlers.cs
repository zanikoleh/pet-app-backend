using AutoMapper;
using MediatR;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Application.Handlers;

/// <summary>
/// Handler for user registration with email and password.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<AuthenticationResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new BusinessLogicException("Email is already registered.", "EMAIL_ALREADY_EXISTS");

        // Create user with email and password
        var email = Email.Create(request.Email);
        var passwordHash = PasswordHash.Create(request.Password);
        var user = new User(email, passwordHash, request.FullName);

        // Save user
        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email.Value, user.Role.Value);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, _jwtTokenService.GetRefreshTokenExpirationMinutes());
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return new AuthenticationResponseDto
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpirationMinutes())
        };
    }
}

/// <summary>
/// Handler for user login with email and password.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<AuthenticationResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            throw new NotFoundException("Invalid email or password.", "INVALID_CREDENTIALS");

        // Verify password
        if (!user.VerifyPassword(request.Password))
            throw new NotFoundException("Invalid email or password.", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            throw new BusinessLogicException("Account is deactivated.", "ACCOUNT_DEACTIVATED");

        // Record login
        user.RecordLogin();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email.Value, user.Role.Value);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, _jwtTokenService.GetRefreshTokenExpirationMinutes());
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return new AuthenticationResponseDto
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpirationMinutes())
        };
    }
}

/// <summary>
/// Handler for OAuth login/registration.
/// </summary>
public sealed class OAuthLoginCommandHandler : IRequestHandler<OAuthLoginCommand, AuthenticationResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public OAuthLoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<AuthenticationResponseDto> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists with this OAuth provider
        var existingUser = await _userRepository.GetByOAuthProviderAsync(
            request.Provider,
            request.ProviderUserId,
            cancellationToken);

        User user;
        if (existingUser != null)
        {
            user = existingUser;
        }
        else
        {
            // Check if email already registered with different provider
            var userByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (userByEmail != null)
            {
                // Link the OAuth provider to existing account
                userByEmail.LinkOAuthProvider(request.Provider, request.ProviderUserId, request.Email);
                user = userByEmail;
            }
            else
            {
                // Create new user from OAuth
                var email = Email.Create(request.Email);
                user = User.CreateFromOAuth(request.Provider, request.ProviderUserId, email, request.FullName, request.Avatar);
                _userRepository.Add(user);
            }
        }

        // Record login
        user.RecordLogin();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email.Value, user.Role.Value);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, _jwtTokenService.GetRefreshTokenExpirationMinutes());
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return new AuthenticationResponseDto
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpirationMinutes())
        };
    }
}

/// <summary>
/// Handler for refreshing access token.
/// </summary>
public sealed class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, AuthenticationResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public RefreshAccessTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<AuthenticationResponseDto> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        var validToken = user.GetValidRefreshToken(request.RefreshToken);
        if (validToken == null)
            throw new BusinessLogicException("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");

        // Generate new tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email.Value, user.Role.Value);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        
        user.RevokeRefreshToken(validToken.Id);
        user.AddRefreshToken(newRefreshToken, _jwtTokenService.GetRefreshTokenExpirationMinutes());
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return new AuthenticationResponseDto
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpirationMinutes())
        };
    }
}

/// <summary>
/// Handler for changing user password.
/// </summary>
public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;

    public ChangePasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        if (!user.VerifyPassword(request.CurrentPassword))
            throw new BusinessLogicException("Current password is incorrect.", "INVALID_PASSWORD");

        var newPasswordHash = PasswordHash.Create(request.NewPassword);
        user.ChangePassword(newPasswordHash);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for updating user profile.
/// </summary>
public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.UpdateProfile(request.FullName, request.Avatar);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

/// <summary>
/// Handler for verifying user email.
/// </summary>
public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IUserRepository _userRepository;

    public VerifyEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.VerifyEmail();
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for linking OAuth provider.
/// </summary>
public sealed class LinkOAuthProviderCommandHandler : IRequestHandler<LinkOAuthProviderCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public LinkOAuthProviderCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(LinkOAuthProviderCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.LinkOAuthProvider(request.Provider, request.ProviderUserId, request.ProviderEmail);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

/// <summary>
/// Handler for unlinking OAuth provider.
/// </summary>
public sealed class UnlinkOAuthProviderCommandHandler : IRequestHandler<UnlinkOAuthProviderCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UnlinkOAuthProviderCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(UnlinkOAuthProviderCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.UnlinkOAuthProvider(request.Provider);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

/// <summary>
/// Handler for deactivating user account.
/// </summary>
public sealed class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IUserRepository _userRepository;

    public DeactivateAccountCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.Deactivate();
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for logout (revoke all refresh tokens).
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        // Revoke all refresh tokens
        foreach (var token in user.RefreshTokens.Where(t => !t.IsRevoked))
        {
            user.RevokeRefreshToken(token.Id);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
