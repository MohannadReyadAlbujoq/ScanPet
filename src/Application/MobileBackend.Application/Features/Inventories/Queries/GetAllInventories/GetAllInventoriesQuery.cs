using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetAllInventories;

/// <summary>
/// Query to get all inventories/warehouses
/// </summary>
public class GetAllInventoriesQuery : BasePagedQuery<InventoryDto>
{
}
