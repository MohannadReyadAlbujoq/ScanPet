using MobileBackend.Domain.Common;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for OrderItem entity
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    /// <summary>
    /// Get order item by serial number
    /// </summary>
    Task<OrderItem?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all order items for a specific order
    /// </summary>
    Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order items by item ID
    /// </summary>
    Task<List<OrderItem>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
}
