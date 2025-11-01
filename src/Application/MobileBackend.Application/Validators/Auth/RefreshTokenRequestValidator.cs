using FluentValidation;
using MobileBackend.Application.DTOs.Auth;

namespace MobileBackend.Application.Validators.Auth;

/// <summary>
/// Validator for refresh token requests
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
