using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Stores aggregated permissions for a role using bitwise operations.
/// PermissionsBitmask is the sum of all permission bits assigned to the role.
/// To check if a role has a permission: (PermissionsBitmask & PermissionBit) == PermissionBit
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// Bitwise sum of all permissions for this role.
    /// Example: If role has ColorCreate(1), ColorEdit(2), ItemCreate(8)
    /// Then PermissionsBitmask = 1 + 2 + 8 = 11
    /// </summary>
    public long PermissionsBitmask { get; set; } = 0;
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
}
