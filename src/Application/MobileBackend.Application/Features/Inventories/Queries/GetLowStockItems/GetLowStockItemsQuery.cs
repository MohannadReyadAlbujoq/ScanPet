using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetLowStockItems;

/// <summary>
/// Query to get items with low stock across all inventories
/// </summary>
public class GetLowStockItemsQuery : IRequest<Result<List<ItemInventoryDto>>>
{
}
