using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handler for getting an order by ID with items
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetWithItemsAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result<OrderDto>.FailureResult("Order not found", 404);
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ClientName = order.ClientName,
                ClientPhone = order.ClientPhone,
                LocationId = order.LocationId,
                LocationName = order.Location?.Name,
                Description = order.Description,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = (int)order.OrderStatus,
                OrderStatusName = order.OrderStatus.ToString(),
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ItemId = oi.ItemId,
                    ItemName = oi.Item?.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.SalePrice,  // Map SalePrice to UnitPrice in DTO
                    TotalPrice = oi.SalePrice * oi.Quantity,  // Calculate total
                    Status = (int)oi.Status,  // Cast enum to int
                    StatusName = oi.Status.ToString()  // Add status name
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            _logger.LogInformation("Retrieved order: {OrderId} - {OrderNumber}", order.Id, order.OrderNumber);

            return Result<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {OrderId}", request.OrderId);
            return Result<OrderDto>.FailureResult("An error occurred while retrieving the order", 500);
        }
    }
}
