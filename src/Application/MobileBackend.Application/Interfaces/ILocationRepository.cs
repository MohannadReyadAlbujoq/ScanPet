using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Location-specific operations
/// </summary>
public interface ILocationRepository : IRepository<Location>
{
    Task<Location?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsLocationNameAvailableAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all locations with order counts and section counts (efficient - single query)
    /// </summary>
    Task<IEnumerable<(Location Location, int OrderCount, int SectionCount)>> GetAllWithCountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get single location with order count, section count, and sections detail
    /// </summary>
    Task<(Location? Location, int OrderCount, List<Inventory> Sections)> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    // Keep old methods for backward compatibility
    Task<IEnumerable<(Location Location, int OrderCount)>> GetAllWithOrderCountsAsync(CancellationToken cancellationToken = default);
    Task<(Location? Location, int OrderCount)> GetByIdWithOrderCountAsync(Guid id, CancellationToken cancellationToken = default);
}
