using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetEnabledUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IsEnabled && u.IsApproved)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetPendingApprovalUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => !u.IsApproved)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Username.Contains(searchTerm) ||
                       u.Email.Contains(searchTerm) ||
                       (u.FullName != null && u.FullName.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
    }

    public async Task<IEnumerable<UserRole>> GetActiveUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public void AddUserRole(UserRole userRole)
    {
        _context.Set<UserRole>().Add(userRole);
    }

    public void RemoveUserRole(UserRole userRole)
    {
        _context.Set<UserRole>().Remove(userRole);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedWithRolesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<string> Roles, long PermissionsBitmask)> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Join(_context.Set<Role>(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.RoleId, r.Name })
            .GroupJoin(_context.Set<RolePermission>(),
                r => r.RoleId,
                rp => rp.RoleId,
                (r, rps) => new { r.Name, Permissions = rps })
            .SelectMany(
                x => x.Permissions.DefaultIfEmpty(),
                (x, rp) => new { x.Name, PermissionsBitmask = rp != null ? rp.PermissionsBitmask : 0L })
            .ToListAsync(cancellationToken);

        var roles = query.Select(q => q.Name).Distinct().ToList();
        
        var permissionsBitmask = query
            .Select(q => q.PermissionsBitmask)
            .Aggregate(0L, (acc, val) => acc | val);

        return (roles, permissionsBitmask);
    }
}
