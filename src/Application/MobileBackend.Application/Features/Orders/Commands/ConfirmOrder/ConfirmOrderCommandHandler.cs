using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Orders.Commands.ConfirmOrder;

/// <summary>
/// Handler for confirming a pending order
/// </summary>
public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ConfirmOrderCommandHandler> _logger;

    public ConfirmOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<ConfirmOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return Result<bool>.FailureResult("Order not found", 404);
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return Result<bool>.FailureResult($"Cannot confirm order with status: {order.OrderStatus}", 400);
            }

            order.OrderStatus = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.OrderConfirmed,
                entityName: EntityNames.Order,
                entityId: order.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Order {order.OrderNumber} confirmed",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Order confirmed successfully: {OrderId} - {OrderNumber}", order.Id, order.OrderNumber);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order: {OrderId}", request.OrderId);
            return Result<bool>.FailureResult("An error occurred while confirming the order", 500);
        }
    }
}
