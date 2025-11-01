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
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLocationNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(l => l.Name == name, cancellationToken);
    }
}
