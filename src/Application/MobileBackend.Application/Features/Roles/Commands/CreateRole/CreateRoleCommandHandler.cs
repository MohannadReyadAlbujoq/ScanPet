using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Handler for CreateRoleCommand
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public CreateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole != null)
        {
            return Result<Guid>.FailureResult("Role name already exists", 400);
        }

        // Create new role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = _dateTimeService.UtcNow,
            CreatedBy = _currentUserService.UserId ?? Guid.Empty
        };

        await _unitOfWork.Roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogAsync(
            AuditActions.RoleCreated,
            EntityNames.Role,
            role.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"Role created: {role.Name}",
            cancellationToken);

        return Result<Guid>.SuccessResult(role.Id, 201);
    }
}
