namespace MobileBackend.Application.DTOs.Auth;

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// The refresh token to use for getting a new access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Device information (optional for tracking)
    /// </summary>
    public string? DeviceInfo { get; set; }
}
