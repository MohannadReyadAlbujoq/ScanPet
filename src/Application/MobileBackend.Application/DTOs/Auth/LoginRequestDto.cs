namespace MobileBackend.Application.DTOs.Auth;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Username or email
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Device information (optional for tracking refresh tokens)
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP Address (will be populated by middleware)
    /// </summary>
    public string? IpAddress { get; set; }
}
