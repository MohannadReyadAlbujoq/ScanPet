using MediatR;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and generate JWT tokens
/// </summary>
public class LoginCommand : IRequest<Result<LoginResponseDto>>
{
    /// <summary>
    /// Username or email address
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Device information for refresh token tracking
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }
}
