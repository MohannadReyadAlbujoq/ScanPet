using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;

namespace MobileBackend.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : BaseGetByIdQuery<UserDto>
{
    // Backwards compatibility: Allow UserId property
    public Guid UserId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
