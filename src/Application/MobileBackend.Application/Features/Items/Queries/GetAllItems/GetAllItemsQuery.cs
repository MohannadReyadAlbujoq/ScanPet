using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;

namespace MobileBackend.Application.Features.Items.Queries.GetAllItems;

/// <summary>
/// Query to get all items with optional pagination
/// </summary>
public class GetAllItemsQuery : BasePagedQuery<ItemDto>
{
}
