using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Permission-specific operations
/// Uses bitwise operations for efficient permission checks
/// </summary>
public interface IPermissionRepository : IRepository<Permission>
{
    Task<bool> HasPermissionAsync(Guid userId, PermissionType permission, CancellationToken cancellationToken = default);
    Task<long> GetUserPermissionsBitmaskAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<long> GetUserPermissionBitmaskAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RolePermission?> GetRolePermissionAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
    void UpdateRolePermission(RolePermission rolePermission);
}
