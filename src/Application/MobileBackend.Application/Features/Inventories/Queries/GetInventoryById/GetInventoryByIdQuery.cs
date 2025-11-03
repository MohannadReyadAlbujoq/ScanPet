using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryById;

/// <summary>
/// Query to get inventory by ID with items
/// </summary>
public class GetInventoryByIdQuery : BaseGetByIdQuery<InventoryDto>, IRequest<Result<InventoryDto>>
{
}
