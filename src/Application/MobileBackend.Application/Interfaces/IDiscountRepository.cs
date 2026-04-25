using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository for Discount entity (v5).
/// </summary>
public interface IDiscountRepository : IRepository<Discount>
{
    Task<List<Discount>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
}
