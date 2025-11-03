using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for User-specific operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetEnabledUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetPendingApprovalUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddRefreshToken(RefreshToken refreshToken);
    
    // UserRole management
    Task<IEnumerable<UserRole>> GetActiveUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddUserRole(UserRole userRole);
    void RemoveUserRole(UserRole userRole);
}
