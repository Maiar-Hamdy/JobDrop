using FluentValidation;
using MediatR;

namespace JobDrop.Application.Features.Auth;

public sealed record RegisterCommand(string Email, string Password, string Handle, bool IsRecruiter) : IRequest<Guid>;
public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresUtc);

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10);
        RuleFor(x => x.Handle).NotEmpty().Matches("^[a-zA-Z0-9_]{3,20}$");
    }
}

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}