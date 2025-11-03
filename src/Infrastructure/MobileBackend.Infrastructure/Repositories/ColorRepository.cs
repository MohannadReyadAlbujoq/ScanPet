using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class ColorRepository : GenericRepository<Color>, IColorRepository
{
    public ColorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Color?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Color>> GetActiveColorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsColorNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);
    }

    // Get all colors with item counts (efficient - single query with aggregation)
    public async Task<IEnumerable<(Color Color, int ItemCount)>> GetAllWithItemCountsAsync(CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Where(c => !c.IsDeleted)
            .Select(c => new
            {
                Color = c,
                ItemCount = c.Items.Count(i => !i.IsDeleted)  // ? Efficient SQL COUNT
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Color, r.ItemCount));
    }

    // Get single color with item count
    public async Task<(Color? Color, int ItemCount)> GetByIdWithItemCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new
            {
                Color = c,
                ItemCount = c.Items.Count(i => !i.IsDeleted)  // ? Efficient SQL COUNT
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return (null, 0);

        return (result.Color, result.ItemCount);
    }
}
