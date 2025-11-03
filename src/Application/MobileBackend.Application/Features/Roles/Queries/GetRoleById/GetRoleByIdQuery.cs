using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;

namespace MobileBackend.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Query to get role by ID with permissions
/// </summary>
public class GetRoleByIdQuery : BaseGetByIdQuery<RoleDto>
{
    // Backwards compatibility: Allow RoleId property
    public Guid RoleId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
