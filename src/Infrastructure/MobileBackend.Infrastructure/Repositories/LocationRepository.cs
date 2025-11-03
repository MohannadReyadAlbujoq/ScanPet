using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class LocationRepository : GenericRepository<Location>, ILocationRepository
{
    public LocationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Location?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Location>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLocationNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(l => l.Name == name, cancellationToken);
    }

    // Get all locations with order counts (efficient - single query with aggregation)
    public async Task<IEnumerable<(Location Location, int OrderCount)>> GetAllWithOrderCountsAsync(CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Where(l => !l.IsDeleted)
            .Select(l => new
            {
                Location = l,
                OrderCount = l.Orders.Count(o => !o.IsDeleted)  // ? Efficient SQL COUNT
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Location, r.OrderCount));
    }

    // Get single location with order count
    public async Task<(Location? Location, int OrderCount)> GetByIdWithOrderCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(l => l.Id == id && !l.IsDeleted)
            .Select(l => new
            {
                Location = l,
                OrderCount = l.Orders.Count(o => !o.IsDeleted)  // ? Efficient SQL COUNT
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return (null, 0);

        return (result.Location, result.OrderCount);
    }
}
