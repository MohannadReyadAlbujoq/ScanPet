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
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsColorNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);
    }
}
