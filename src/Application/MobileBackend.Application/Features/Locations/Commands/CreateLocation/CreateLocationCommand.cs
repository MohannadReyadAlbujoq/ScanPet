using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Locations.Commands.CreateLocation;

/// <summary>
/// Command to create a new location
/// </summary>
public class CreateLocationCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; } = true;
}
