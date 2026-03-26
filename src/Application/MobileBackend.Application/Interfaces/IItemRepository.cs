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
    Task<Item?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetItemsByColorAsync(Guid colorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default);
    Task<bool> IsSkuAvailableAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetAllWithColorsAsync(CancellationToken cancellationToken = default);
    Task<Item?> GetByIdWithColorAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    Task<(List<Item> Items, int TotalCount)> GetPagedWithColorsAsync(
        int pageNumber,
        int pageSize,
        Guid? inventoryId = null,
        CancellationToken cancellationToken = default);
}
