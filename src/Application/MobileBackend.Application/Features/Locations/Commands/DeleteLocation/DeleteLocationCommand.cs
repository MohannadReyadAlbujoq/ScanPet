using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Locations.Commands.DeleteLocation;

/// <summary>
/// Command to delete (soft delete) a location
/// </summary>
public class DeleteLocationCommand : IRequest<Result<bool>>
{
    public Guid LocationId { get; set; }
}
