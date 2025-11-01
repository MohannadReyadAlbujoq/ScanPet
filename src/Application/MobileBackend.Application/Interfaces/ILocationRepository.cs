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
}
