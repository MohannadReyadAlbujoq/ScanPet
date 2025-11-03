using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetActiveInventories;

/// <summary>
/// Query to get only active inventories
/// </summary>
public class GetActiveInventoriesQuery : IRequest<Result<List<InventoryDto>>>
{
}
