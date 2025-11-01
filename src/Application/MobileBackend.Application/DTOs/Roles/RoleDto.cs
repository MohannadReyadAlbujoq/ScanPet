namespace MobileBackend.Application.DTOs.Roles;

/// <summary>
/// Unified Role DTO for all operations (read, create, update)
/// Nullable properties are optional based on operation context
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Role ID (only for responses, not for create)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Role name (required for create/update)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Role description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Aggregated permissions bitmask (for responses and permission assignment)
    /// </summary>
    public long? PermissionsBitmask { get; set; }
    
    /// <summary>
    /// List of permission names (only in responses)
    /// </summary>
    public List<string>? Permissions { get; set; }
    
    /// <summary>
    /// Number of users with this role (only in responses)
    /// </summary>
    public int? UserCount { get; set; }
    
    /// <summary>
    /// Creation timestamp (only in responses)
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Last update timestamp (only in responses)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
