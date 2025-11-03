using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Commands.TransferInventory;

/// <summary>
/// Handler for transferring inventory between warehouses
/// </summary>
public class TransferInventoryCommandHandler : IRequestHandler<TransferInventoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TransferInventoryCommandHandler> _logger;

    public TransferInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<TransferInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(TransferInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate quantity
            if (request.Quantity <= 0)
            {
                return Result<bool>.FailureResult("Transfer quantity must be greater than zero", 400);
            }

            // Validate source and destination are different
            if (request.FromInventoryId == request.ToInventoryId)
            {
                return Result<bool>.FailureResult("Source and destination inventories must be different", 400);
            }

            // Validate item exists
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result<bool>.FailureResult($"Item with ID {request.ItemId} not found", 404);
            }

            // Validate source inventory exists and is active
            var fromInventory = await _unitOfWork.Inventories.GetByIdAsync(request.FromInventoryId, cancellationToken);
            if (fromInventory == null)
            {
                return Result<bool>.FailureResult($"Source inventory with ID {request.FromInventoryId} not found", 404);
            }
            if (!fromInventory.IsActive)
            {
                return Result<bool>.FailureResult($"Source inventory '{fromInventory.Name}' is not active", 400);
            }

            // Validate destination inventory exists and is active
            var toInventory = await _unitOfWork.Inventories.GetByIdAsync(request.ToInventoryId, cancellationToken);
            if (toInventory == null)
            {
                return Result<bool>.FailureResult($"Destination inventory with ID {request.ToInventoryId} not found", 404);
            }
            if (!toInventory.IsActive)
            {
                return Result<bool>.FailureResult($"Destination inventory '{toInventory.Name}' is not active", 400);
            }

            // Use repository method to transfer inventory
            var success = await _unitOfWork.ItemInventories.TransferInventoryAsync(
                request.ItemId,
                request.FromInventoryId,
                request.ToInventoryId,
                request.Quantity,
                cancellationToken
            );

            if (!success)
            {
                return Result<bool>.FailureResult("Failed to transfer inventory. Insufficient stock at source or invalid operation.", 400);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            var auditInfo = $"Transferred {request.Quantity} units of {item.Name} from {fromInventory.Name} to {toInventory.Name}";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                auditInfo += $" - Reason: {request.Reason}";
            }

            await _auditService.LogAsync(
                action: AuditActions.Update,
                entityName: "ItemInventory",
                entityId: request.ItemId,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: auditInfo,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Inventory transferred: {Quantity} of {ItemName} from {FromInventory} to {ToInventory}",
                request.Quantity, item.Name, fromInventory.Name, toInventory.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring inventory for Item {ItemId} from {FromInventoryId} to {ToInventoryId}",
                request.ItemId, request.FromInventoryId, request.ToInventoryId);
            return Result<bool>.FailureResult("An error occurred while transferring inventory", 500);
        }
    }
}
