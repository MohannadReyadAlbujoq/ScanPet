namespace MobileBackend.Application.DTOs.Auth;

/// <summary>
/// Login response DTO containing JWT tokens and user information
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT access token (short-lived, 15 minutes)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token (long-lived, 7 days)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserInfoDto User { get; set; } = new();
}

/// <summary>
/// User information included in login response
/// </summary>
public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsApproved { get; set; }
    public List<string> Roles { get; set; } = new();
    public long PermissionsBitmask { get; set; }
}
