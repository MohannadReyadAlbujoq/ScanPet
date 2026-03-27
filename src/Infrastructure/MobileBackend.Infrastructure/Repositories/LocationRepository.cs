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

    // Get all locations with order counts (backward compat)
    public async Task<IEnumerable<(Location Location, int OrderCount)>> GetAllWithOrderCountsAsync(CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Where(l => !l.IsDeleted)
            .Select(l => new
            {
                Location = l,
                OrderCount = l.Orders.Count(o => !o.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Location, r.OrderCount));
    }

    // Get single location with order count (backward compat)
    public async Task<(Location? Location, int OrderCount)> GetByIdWithOrderCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(l => l.Id == id && !l.IsDeleted)
            .Select(l => new
            {
                Location = l,
                OrderCount = l.Orders.Count(o => !o.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return (null, 0);

        return (result.Location, result.OrderCount);
    }

    // Get all locations with order counts AND section counts
    public async Task<IEnumerable<(Location Location, int OrderCount, int SectionCount)>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .Select(l => new
            {
                Location = l,
                OrderCount = l.Orders.Count(o => !o.IsDeleted),
                SectionCount = l.Inventories.Count(i => !i.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Location, r.OrderCount, r.SectionCount));
    }

    // Get single location with order count + full section details
    public async Task<(Location? Location, int OrderCount, List<Inventory> Sections)> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _dbSet
            .Include(l => l.Inventories.Where(i => !i.IsDeleted))
            .Where(l => l.Id == id && !l.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (location == null)
            return (null, 0, new List<Inventory>());

        var orderCount = await _context.Set<Order>()
            .CountAsync(o => o.LocationId == id && !o.IsDeleted, cancellationToken);

        return (location, orderCount, location.Inventories.ToList());
    }
}
