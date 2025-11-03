using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Handler for creating a new order
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate location exists
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
            if (location == null)
            {
                return Result<Guid>.FailureResult("Location not found", 404);
            }

            // Validate items exist and have sufficient quantity
            if (request.OrderItems == null || !request.OrderItems.Any())
            {
                return Result<Guid>.FailureResult("Order must contain at least one item", 400);
            }

            // ? FIX N+1: Fetch all items in single query before loop
            var itemIds = request.OrderItems
                .Where(oi => oi.ItemId.HasValue)
                .Select(oi => oi.ItemId!.Value)
                .Distinct()
                .ToList();

            var items = await _unitOfWork.Items.FindAsync(
                i => itemIds.Contains(i.Id), 
                cancellationToken);
            
            var itemsDict = items.ToDictionary(i => i.Id);

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in request.OrderItems)
            {
                // ? Now using dictionary lookup instead of database query
                if (!itemDto.ItemId.HasValue || !itemsDict.TryGetValue(itemDto.ItemId.Value, out var item))
                {
                    return Result<Guid>.FailureResult($"Item with ID {itemDto.ItemId} not found", 404);
                }

                // Check quantity
                if (item.Quantity < (itemDto.Quantity ?? 0))
                {
                    return Result<Guid>.FailureResult($"Insufficient quantity for item {item.Name}. Available: {item.Quantity}, Requested: {itemDto.Quantity}", 400);
                }

                // Calculate price
                var salePrice = itemDto.UnitPrice ?? item.BasePrice;
                var quantity = itemDto.Quantity ?? 0;
                var itemTotal = salePrice * quantity;

                // Generate or validate serial number
                string serialNumber;
                if (!string.IsNullOrEmpty(itemDto.SerialNumber))
                {
                    // Serial number provided by frontend - validate uniqueness
                    var existingOrderItem = await _unitOfWork.OrderItems.GetBySerialNumberAsync(itemDto.SerialNumber, cancellationToken);
                    if (existingOrderItem != null)
                    {
                        return Result<Guid>.FailureResult($"Serial number {itemDto.SerialNumber} already exists", 400);
                    }
                    serialNumber = itemDto.SerialNumber;
                }
                else
                {
                    // Auto-generate serial number
                    serialNumber = $"SN-{item.SKU}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 3).ToUpper()}";
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    SerialNumber = serialNumber,
                    Quantity = quantity,
                    SalePrice = salePrice,
                    Status = OrderItemStatus.Successful,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                orderItems.Add(orderItem);
                totalAmount += itemTotal;

                // Decrease item quantity
                item.Quantity -= quantity;
                _unitOfWork.Items.Update(item);
            }

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                ClientName = request.ClientName,
                ClientPhone = request.ClientPhone,
                LocationId = request.LocationId,
                Description = request.Description,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderStatus = OrderStatus.Pending,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.OrderCreated,
                entityName: EntityNames.Order,
                entityId: order.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Order {orderNumber} created for client {request.ClientName}, Total: {totalAmount:C}, Items: {orderItems.Count}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Order created successfully: {OrderId} - {OrderNumber} - Total: {Total}", order.Id, orderNumber, order.TotalAmount);

            return Result<Guid>.SuccessResult(order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for client: {ClientName}", request.ClientName);
            return Result<Guid>.FailureResult("An error occurred while creating the order", 500);
        }
    }
}
