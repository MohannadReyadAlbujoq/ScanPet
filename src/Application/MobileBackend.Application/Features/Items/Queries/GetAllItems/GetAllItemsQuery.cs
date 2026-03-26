using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;

namespace MobileBackend.Application.Features.Items.Queries.GetAllItems;

/// <summary>
/// Query to get all items with pagination and optional filtering.
/// Returns PagedResult with total count, page info, etc.
/// </summary>
public class GetAllItemsQuery : IRequest<Result<PagedResult<ItemDto>>>
{
    /// <summary>
    /// Page number (1-based, defaults to 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (defaults to 10, max 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional inventory/section ID to filter items by
    /// </summary>
    public Guid? InventoryId { get; set; }
}
