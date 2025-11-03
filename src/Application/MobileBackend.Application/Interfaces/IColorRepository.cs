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
    
    // Get all colors with item counts (efficient - single query)
    Task<IEnumerable<(Color Color, int ItemCount)>> GetAllWithItemCountsAsync(CancellationToken cancellationToken = default);
    
    // Get single color with item count
    Task<(Color? Color, int ItemCount)> GetByIdWithItemCountAsync(Guid id, CancellationToken cancellationToken = default);
}
