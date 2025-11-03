using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Commands.UpdateUserRole;

/// <summary>
/// Handler for UpdateUserRoleCommand
/// Updates a user's role assignment
/// </summary>
public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateUserRoleCommandHandler> _logger;

    public UpdateUserRoleCommandHandler(
        IUnitOfWork _unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateUserRoleCommandHandler> logger)
    {
        this._unitOfWork = _unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.FailureResult("User not found", 404);
        }

        // 2. Validate role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<bool>.FailureResult("Role not found", 404);
        }

        // 3. Get current user roles and remove them
        var existingUserRoles = await _unitOfWork.Users.GetActiveUserRolesAsync(request.UserId, cancellationToken);
        foreach (var existingRole in existingUserRoles)
        {
            _unitOfWork.Users.RemoveUserRole(existingRole);
        }

        // 4. Add new role assignment
        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = _currentUserService.UserId
        };

        _unitOfWork.Users.AddUserRole(userRole);

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} role updated to {RoleId} by {CurrentUserId}", 
            request.UserId, request.RoleId, _currentUserService.UserId);

        return Result<bool>.SuccessResult(true);
    }
}
