using MobileBackend.Framework.Security.Models;

namespace MobileBackend.Framework.Security;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access token for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    /// <param name="permissionsBitmask">Aggregated permissions bitmask</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(Guid userId, string email, List<string> roles, long permissionsBitmask);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate and decode access token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>JWT validation result</returns>
    JwtValidationResult ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID or null if invalid</returns>
    Guid? GetUserIdFromToken(string token);
}
