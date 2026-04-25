using MediatR;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Features.Orders.Commands.CancelOrder;
using MobileBackend.Application.Features.Orders.Commands.ConfirmOrder;
using MobileBackend.Application.Features.Orders.Commands.CreateOrder;
using MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;
using MobileBackend.Application.Features.Orders.Queries.GetAllOrders;
using MobileBackend.Application.Features.Orders.Queries.GetOrderById;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Order management controller
/// Handles order CRUD and status operations
/// </summary>
[Route("api/[controller]")]
public class OrdersController : BaseApiController
{
    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get all orders with optional pagination and status filter
    /// </summary>
    /// <param name="pageNumber">Page number (optional)</param>
    /// <param name="pageSize">Page size (optional)</param>
    /// <param name="status">Filter by status: 0=Pending, 1=Confirmed, 2=Cancelled (optional)</param>
    /// <returns>List of all orders</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] int? status, [FromQuery] string? keyword)
    {
        var query = new GetAllOrdersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            Keyword = keyword
        };
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Get order by ID with items
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details with items</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderByIdQuery { OrderId = id };
        var result = await Mediator.Send(query);

        return result.Success 
            ? OkResponse(result.Data) 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="dto">Order creation data with items</param>
    /// <returns>Created order ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] OrderDto dto)
    {
        var command = new CreateOrderCommand
        {
            ClientName = dto.ClientName ?? string.Empty,
            ClientPhone = dto.ClientPhone ?? string.Empty,
            InventoryId = dto.InventoryId ?? Guid.Empty,
            Description = dto.Description,
            OrderItems = dto.OrderItems ?? new List<OrderItemDto>()
        };

        var result = await Mediator.Send(command);

        return result.Success 
            ? CreatedResponse(result.Data, "Order") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Confirm a pending order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var command = new ConfirmOrderCommand { OrderId = id };
        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Order confirmed successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Cancel an order and restore item quantities
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string? reason)
    {
        var command = new CancelOrderCommand 
        { 
            OrderId = id,
            CancellationReason = reason
        };
        var result = await Mediator.Send(command);

        return result.Success 
            ? OkResponse("Order cancelled successfully") 
            : ErrorResponse(result);
    }

    /// <summary>
    /// Refund one or more lines of an order (v5).
    /// Body: { orderId, refundToInventoryId, refundReason, items:[{ orderItemId|itemId, quantity }] }
    /// Returns the refunded items, refunded percent and refunded amount; the order is auto-promoted to
    /// Refunded (all lines fully refunded) or PartiallyRefunded.
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(Result<RefundResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundOrderItem([FromBody] RefundOrderItemRequest request)
    {
        var command = new RefundOrderItemCommand
        {
            OrderId = request.OrderId,
            RefundToInventoryId = request.RefundToInventoryId,
            RefundReason = request.RefundReason,
            Items = request.Items
        };

        var result = await Mediator.Send(command);

        return result.Success
            ? OkResponse(result.Data)
            : StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
    }
}
