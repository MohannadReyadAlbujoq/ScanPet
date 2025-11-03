using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Role-specific operations
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsRoleAssignedToUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
