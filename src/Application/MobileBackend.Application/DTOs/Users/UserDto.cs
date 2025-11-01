namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// Unified User DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Username (required for create, not updateable)
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Email address (required for create/update)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Password (only for create/update password, never in responses)
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// Full name
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether the user account is enabled
    /// </summary>
    public bool? IsEnabled { get; set; }
    
    /// <summary>
    /// Whether the user account is approved
    /// </summary>
    public bool? IsApproved { get; set; }
    
    /// <summary>
    /// User roles (only in responses)
    /// </summary>
    public List<string>? Roles { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
