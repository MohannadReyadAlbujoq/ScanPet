using FluentValidation;
using MobileBackend.Application.DTOs.Auth;

namespace MobileBackend.Application.Validators.Auth;

/// <summary>
/// Validator for login requests
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required")
            .MaximumLength(255).WithMessage("Username or email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(1).WithMessage("Password is required");
    }
}
