using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;

namespace MobileBackend.Application.Features.Items.Queries.SearchItems;

/// <summary>
/// Query to search items by name, description, or SKU
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchItemsQuery : BaseSearchQuery<ItemDto>, IRequest<Result<List<ItemDto>>>
{
}
