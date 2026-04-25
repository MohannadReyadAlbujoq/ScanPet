using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class DiscountRepository : GenericRepository<Discount>, IDiscountRepository
{
    public DiscountRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Discount>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.ItemId == itemId && !d.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
