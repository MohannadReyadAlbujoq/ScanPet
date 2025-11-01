using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Items.Commands.DeleteItem;

/// <summary>
/// Command to delete (soft delete) an item
/// </summary>
public class DeleteItemCommand : IRequest<Result<bool>>
{
    public Guid ItemId { get; set; }
}
