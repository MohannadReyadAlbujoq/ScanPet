using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Order-specific operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetOrderCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
}
