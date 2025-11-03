using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout user and revoke refresh token
/// </summary>
public class LogoutCommand : IRequest<Result<bool>>
{
    public string? RefreshToken { get; set; }
}
