using MediatR;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a valid refresh token
/// </summary>
public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}
