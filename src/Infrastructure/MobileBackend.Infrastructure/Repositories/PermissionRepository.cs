using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasPermissionAsync(Guid userId, PermissionType permission, CancellationToken cancellationToken = default)
    {
        var userBitmask = await GetUserPermissionBitmaskAsync(userId, cancellationToken);
        long permissionBit = 1L << (int)permission;
        return (userBitmask & permissionBit) == permissionBit;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        // Get the role's permission bitmask and convert to individual permissions
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId, cancellationToken);

        if (rolePermission == null)
            return Enumerable.Empty<Permission>();

        // Get all permissions that match the bitmask
        return await _dbSet
            .Where(p => (rolePermission.PermissionsBitmask & p.PermissionBit) == p.PermissionBit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userBitmask = await GetUserPermissionBitmaskAsync(userId, cancellationToken);

        if (userBitmask == 0)
            return Enumerable.Empty<Permission>();

        return await _dbSet
            .Where(p => (userBitmask & p.PermissionBit) == p.PermissionBit)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetUserPermissionsBitmaskAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get all role permissions for user's roles
        var rolePermissions = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Join(_context.Set<RolePermission>(),
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionsBitmask)
            .ToListAsync(cancellationToken);

        // Combine all permissions using bitwise OR
        long combinedPermissions = 0;
        foreach (var perm in rolePermissions)
        {
            combinedPermissions |= perm;
        }

        return combinedPermissions;
    }

    public async Task<long> GetUserPermissionBitmaskAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetUserPermissionsBitmaskAsync(userId, cancellationToken);
    }

    public async Task<RolePermission?> GetRolePermissionAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId, cancellationToken);
    }

    public async Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        await _context.Set<RolePermission>().AddAsync(rolePermission, cancellationToken);
    }

    public void UpdateRolePermission(RolePermission rolePermission)
    {
        _context.Set<RolePermission>().Update(rolePermission);
    }
}
