using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Items.Commands.DeleteItem;

/// <summary>
/// Handler for deleting (soft delete) an item
/// </summary>
public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteItemCommandHandler> _logger;

    public DeleteItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<DeleteItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing item
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId);
            if (item == null)
            {
                return Result<bool>.FailureResult("Item not found", 404);
            }

            // Soft delete
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedBy = _currentUserService.UserId;

            _unitOfWork.Items.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.ItemDeleted,
                entityName: EntityNames.Item,
                entityId: item.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Deleted item: {item.Name}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Item deleted successfully: {ItemId} - {ItemName}", item.Id, item.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item: {ItemId}", request.ItemId);
            return Result<bool>.FailureResult("An error occurred while deleting the item", 500);
        }
    }
}
