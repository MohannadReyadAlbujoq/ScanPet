using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Command to create a new role
/// </summary>
public class CreateRoleCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
