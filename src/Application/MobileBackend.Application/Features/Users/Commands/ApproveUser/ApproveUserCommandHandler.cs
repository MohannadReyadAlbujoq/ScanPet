using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Commands.ApproveUser;

/// <summary>
/// Handler for ApproveUserCommand
/// Approves and/or enables user account
/// </summary>
public class ApproveUserCommandHandler : IRequestHandler<ApproveUserCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public ApproveUserCommandHandler(
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

    public async Task<Result<bool>> Handle(ApproveUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.FailureResult("User not found", 404);
        }

        // Update approval status
        user.IsApproved = request.IsApproved;
        user.IsEnabled = request.IsEnabled;
        user.UpdatedAt = _dateTimeService.UtcNow;
        user.UpdatedBy = _currentUserService.UserId;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        var action = request.IsApproved ? AuditActions.UserApproved : "UserRejected";
        await _auditService.LogAsync(
            action,
            EntityNames.User,
            user.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"User {(request.IsApproved ? "approved" : "rejected")}: {user.Username}, Enabled: {request.IsEnabled}",
            cancellationToken);

        return Result<bool>.SuccessResult(true);
    }
}
