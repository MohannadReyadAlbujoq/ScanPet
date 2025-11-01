using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Features.Orders.Commands.CancelOrder;
using MobileBackend.Application.Features.Orders.Commands.ConfirmOrder;
using MobileBackend.Application.Features.Orders.Commands.CreateOrder;
using MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;
using MobileBackend.Application.Features.Orders.Queries.GetAllOrders;
using MobileBackend.Application.Features.Orders.Queries.GetOrderById;
using MobileBackend.API.Filters;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Order management controller
/// Handles order CRUD and status operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    /// <param name="pageNumber">Page number (optional)</param>
    /// <param name="pageSize">Page size (optional)</param>
    /// <returns>List of all orders</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var query = new GetAllOrdersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            data = result.Data
        });
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
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            data = result.Data
        });
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
            LocationId = dto.LocationId ?? Guid.Empty,
            Description = dto.Description,
            OrderItems = dto.OrderItems ?? new List<OrderItemDto>()
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = "Order created successfully",
            orderId = result.Data
        });
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
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            message = "Order confirmed successfully"
        });
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
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            success = true,
            message = "Order cancelled successfully"
        });
    }

    /// <summary>
    /// Refund an order item by serial number
    /// </summary>
    /// <param name="serialNumber">Serial number of the order item to refund</param>
    /// <param name="request">Refund details including quantity and reason</param>
    /// <returns>Result of refund operation</returns>
    [HttpPost("refund/{serialNumber}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundOrderItem(string serialNumber, [FromBody] RefundOrderItemRequest request)
    {
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = request.RefundQuantity,
            RefundReason = request.RefundReason
        };

        var result = await _mediator.Send(command);

        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}

/// <summary>
/// Request model for refunding an order item
/// </summary>
public class RefundOrderItemRequest
{
    /// <summary>
    /// Quantity to refund (must be greater than 0 and not exceed ordered quantity)
    /// </summary>
    public int RefundQuantity { get; set; }

    /// <summary>
    /// Optional reason for the refund
    /// </summary>
    public string? RefundReason { get; set; }
}
