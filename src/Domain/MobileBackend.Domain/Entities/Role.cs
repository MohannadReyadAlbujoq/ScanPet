using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class Role : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty; // Admin, User, Admin.User
    public string? Description { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
