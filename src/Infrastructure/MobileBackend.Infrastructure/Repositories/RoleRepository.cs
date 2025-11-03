using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await GetByNameAsync(name, cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .CountAsync(ur => ur.RoleId == roleId, cancellationToken);
    }

    public async Task<bool> IsRoleAssignedToUsersAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.RoleId == roleId, cancellationToken);
    }
}
