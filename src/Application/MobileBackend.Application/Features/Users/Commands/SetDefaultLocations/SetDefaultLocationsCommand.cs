using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.SetDefaultLocations;

/// <summary>
/// Command to set a user's default locations
/// </summary>
public class SetDefaultLocationsCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public List<Guid> DefaultLocationIds { get; set; } = new();
}
