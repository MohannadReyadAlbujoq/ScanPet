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
    
    // Get all locations with order counts (efficient - single query)
    Task<IEnumerable<(Location Location, int OrderCount)>> GetAllWithOrderCountsAsync(CancellationToken cancellationToken = default);
    
    // Get single location with order count
    Task<(Location? Location, int OrderCount)> GetByIdWithOrderCountAsync(Guid id, CancellationToken cancellationToken = default);
}
