namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current user context information
/// Provides user ID, IP address, user agent, etc.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current user's ID (from JWT token)
    /// </summary>
    Guid? UserId { get; }
    
    /// <summary>
    /// Get the current user's username
    /// </summary>
    string? Username { get; }
    
    /// <summary>
    /// Get the current user's email
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Get the IP address of the current request
    /// </summary>
    string? IpAddress { get; }
    
    /// <summary>
    /// Get the User-Agent of the current request
    /// </summary>
    string? UserAgent { get; }
}
