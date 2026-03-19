using FluentValidation;
using IdentityService.Application.Commands;

namespace IdentityService.Application.Validators;

/// <summary>
/// Validator for RegisterCommand.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*(),.?""]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");
    }
}

/// <summary>
/// Validator for LoginCommand.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

/// <summary>
/// Validator for OAuthLoginCommand.
/// </summary>
public class OAuthLoginCommandValidator : AbstractValidator<OAuthLoginCommand>
{
    public OAuthLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => new[] { "google", "facebook", "apple" }.Contains(p.ToLower()))
            .WithMessage("Provider must be one of: google, facebook, apple.");

        RuleFor(x => x.ProviderUserId)
            .NotEmpty().WithMessage("Provider user ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");
    }
}

/// <summary>
/// Validator for ChangePasswordCommand.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*(),.?""]").WithMessage("Password must contain at least one special character.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password.");
    }
}

/// <summary>
/// Validator for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Avatar)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");
    }
}

/// <summary>
/// Validator for LinkOAuthProviderCommand.
/// </summary>
public class LinkOAuthProviderCommandValidator : AbstractValidator<LinkOAuthProviderCommand>
{
    public LinkOAuthProviderCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => new[] { "google", "facebook", "apple" }.Contains(p.ToLower()))
            .WithMessage("Provider must be one of: google, facebook, apple.");

        RuleFor(x => x.ProviderUserId)
            .NotEmpty().WithMessage("Provider user ID is required.");

        RuleFor(x => x.ProviderEmail)
            .NotEmpty().WithMessage("Provider email is required.")
            .EmailAddress().WithMessage("Provider email must be a valid email address.");
    }
}

/// <summary>
/// Validator for UnlinkOAuthProviderCommand.
/// </summary>
public class UnlinkOAuthProviderCommandValidator : AbstractValidator<UnlinkOAuthProviderCommand>
{
    public UnlinkOAuthProviderCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => new[] { "google", "facebook", "apple" }.Contains(p.ToLower()))
            .WithMessage("Provider must be one of: google, facebook, apple.");
    }
}
