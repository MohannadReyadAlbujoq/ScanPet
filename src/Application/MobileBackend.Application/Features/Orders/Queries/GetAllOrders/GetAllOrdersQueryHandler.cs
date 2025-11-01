using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Handler for getting all orders
/// </summary>
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllOrdersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                ClientName = o.ClientName,
                ClientPhone = o.ClientPhone,
                LocationId = o.LocationId,
                LocationName = o.Location?.Name,
                Description = o.Description,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderStatus = (int)o.OrderStatus,
                OrderStatusName = o.OrderStatus.ToString(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} orders", orderDtos.Count);

            return Result<List<OrderDto>>.SuccessResult(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return Result<List<OrderDto>>.FailureResult("An error occurred while retrieving orders", 500);
        }
    }
}
