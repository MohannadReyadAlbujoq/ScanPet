namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// Request DTO for updating user role
/// </summary>
public class UpdateUserRoleDto
{
    /// <summary>
    /// The role ID to assign to the user
    /// </summary>
    public Guid RoleId { get; set; }
}
