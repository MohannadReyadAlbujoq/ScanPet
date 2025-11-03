using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;

namespace MobileBackend.Application.Features.Items.Queries.GetItemById;

/// <summary>
/// Query to get an item by ID
/// </summary>
public class GetItemByIdQuery : BaseGetByIdQuery<ItemDto>
{
    // Backwards compatibility: Allow ItemId property
    public Guid ItemId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
