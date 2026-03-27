namespace MobileBackend.Framework.Security.Models;

/// <summary>
/// Token response containing access and refresh tokens
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// Custom token validation result
/// </summary>
public class JwtValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public long PermissionsBitmask { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
