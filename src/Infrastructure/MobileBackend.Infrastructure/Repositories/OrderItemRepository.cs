using Microsoft.EntityFrameworkCore;
using MobileBackend.Domain.Entities;
using MobileBackend.Application.Interfaces;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrderItem entity
/// </summary>
public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get order item by serial number
    /// </summary>
    public async Task<OrderItem?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(oi => oi.Order)
            .Include(oi => oi.Item)
            .FirstOrDefaultAsync(oi => oi.SerialNumber == serialNumber && !oi.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get all order items for a specific order
    /// </summary>
    public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(oi => oi.Item)
                .ThenInclude(i => i.Color)
            .Where(oi => oi.OrderId == orderId && !oi.IsDeleted)
            .OrderBy(oi => oi.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get order items by item ID
    /// </summary>
    public async Task<List<OrderItem>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(oi => oi.Order)
            .Where(oi => oi.ItemId == itemId && !oi.IsDeleted)
            .OrderByDescending(oi => oi.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
