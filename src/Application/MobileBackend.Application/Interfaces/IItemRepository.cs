using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Item-specific operations
/// </summary>
public interface IItemRepository : IRepository<Item>
{
    Task<Item?> GetWithDetailsAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetByColorIdAsync(Guid colorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetAvailableItemsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsItemAvailableAsync(Guid itemId, CancellationToken cancellationToken = default);
}
