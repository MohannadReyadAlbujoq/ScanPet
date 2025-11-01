using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.DTOs.Roles;

/// <summary>
/// DTO for assigning permissions to a role
/// Uses PermissionType enum for type-safe permission assignment
/// </summary>
public class AssignPermissionsDto
{
    public Guid RoleId { get; set; }
    public List<PermissionType> Permissions { get; set; } = new();
}
