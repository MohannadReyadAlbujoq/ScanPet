using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Color-specific operations
/// </summary>
public interface IColorRepository : IRepository<Color>
{
    Task<Color?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Color>> GetActiveColorsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsColorNameAvailableAsync(string name, CancellationToken cancellationToken = default);
}
