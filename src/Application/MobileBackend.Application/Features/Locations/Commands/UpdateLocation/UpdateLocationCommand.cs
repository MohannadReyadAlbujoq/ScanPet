using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Locations.Commands.UpdateLocation;

/// <summary>
/// Command to update an existing location
/// </summary>
public class UpdateLocationCommand : IRequest<Result<bool>>
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; }
}
