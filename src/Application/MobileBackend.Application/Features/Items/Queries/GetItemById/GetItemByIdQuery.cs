using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;

namespace MobileBackend.Application.Features.Items.Queries.GetItemById;

/// <summary>
/// Query to get an item by ID
/// </summary>
public class GetItemByIdQuery : IRequest<Result<ItemDto>>
{
    public Guid ItemId { get; set; }
}
