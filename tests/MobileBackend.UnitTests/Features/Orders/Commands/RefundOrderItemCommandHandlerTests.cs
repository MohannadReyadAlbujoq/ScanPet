using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Orders.Commands;

/// <summary>
/// Unit tests for the v5 order-level RefundOrderItemCommandHandler.
/// </summary>
public class RefundOrderItemCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IOrderRepository> _orders;
    private readonly Mock<IOrderItemRepository> _orderItems;
    private readonly Mock<IInventoryRepository> _inventories;
    private readonly Mock<IItemInventoryRepository> _itemInventories;
    private readonly Mock<IAuditService> _audit;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<ILogger<RefundOrderItemCommandHandler>> _logger;
    private readonly RefundOrderItemCommandHandler _handler;

    public RefundOrderItemCommandHandlerTests()
    {
        _uow = CreateMock<IUnitOfWork>();
        _orders = CreateMock<IOrderRepository>();
        _orderItems = CreateMock<IOrderItemRepository>();
        _inventories = CreateMock<IInventoryRepository>();
        _itemInventories = CreateMock<IItemInventoryRepository>();
        _audit = CreateMock<IAuditService>();
        _currentUser = CreateMock<ICurrentUserService>();
        _logger = CreateMock<ILogger<RefundOrderItemCommandHandler>>();

        _uow.Setup(x => x.Orders).Returns(_orders.Object);
        _uow.Setup(x => x.OrderItems).Returns(_orderItems.Object);
        _uow.Setup(x => x.Inventories).Returns(_inventories.Object);
        _uow.Setup(x => x.ItemInventories).Returns(_itemInventories.Object);
        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _handler = new RefundOrderItemCommandHandler(
            _uow.Object, _audit.Object, _currentUser.Object, _logger.Object);
    }

    private (Order order, OrderItem item, Inventory inv) Seed(int qty = 5, int alreadyRefunded = 0)
    {
        var inventoryId = Guid.NewGuid();
        var inv = new Inventory { Id = inventoryId, Name = "WH", IsActive = true };
        var oi = new OrderItem
        {
            Id = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            SerialNumber = "PF-001",
            Quantity = qty,
            SalePrice = 100m,
            Status = alreadyRefunded == 0 ? OrderItemStatus.Successful
                    : alreadyRefunded >= qty ? OrderItemStatus.Refunded
                    : OrderItemStatus.PartiallyRefunded,
            RefundedQuantity = alreadyRefunded
        };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            InventoryId = inventoryId,
            OrderStatus = OrderStatus.Confirmed,
            OrderItems = new List<OrderItem> { oi }
        };

        _orders.Setup(x => x.GetWithItemsAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventories.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>())).ReturnsAsync(inv);
        _itemInventories.Setup(x => x.AdjustInventoryAsync(oi.ItemId, inventoryId, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (order, oi, inv);
    }

    [Fact]
    public async Task Handle_FullRefund_SetsOrderStatusToRefunded()
    {
        var (order, item, inv) = Seed(qty: 5);
        var cmd = new RefundOrderItemCommand
        {
            OrderId = order.Id,
            RefundToInventoryId = inv.Id,
            RefundReason = "Defect",
            Items = new List<RefundLineDto> { new() { OrderItemId = item.Id, Quantity = 5 } }
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.OrderStatus.Should().Be((int)OrderStatus.Refunded);
        result.Data.RefundedPercent.Should().Be(100m);
        result.Data.RefundedAmount.Should().Be(500m);
        item.Status.Should().Be(OrderItemStatus.Refunded);
        item.RefundedQuantity.Should().Be(5);
        order.OrderStatus.Should().Be(OrderStatus.Refunded);
    }

    [Fact]
    public async Task Handle_PartialRefund_SetsOrderStatusToPartiallyRefunded()
    {
        var (order, item, inv) = Seed(qty: 5);
        var cmd = new RefundOrderItemCommand
        {
            OrderId = order.Id,
            RefundToInventoryId = inv.Id,
            Items = new List<RefundLineDto> { new() { OrderItemId = item.Id, Quantity = 2 } }
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.OrderStatus.Should().Be((int)OrderStatus.PartiallyRefunded);
        result.Data.RefundedPercent.Should().Be(40m);
        result.Data.RefundedAmount.Should().Be(200m);
        item.Status.Should().Be(OrderItemStatus.PartiallyRefunded);
        order.OrderStatus.Should().Be(OrderStatus.PartiallyRefunded);
    }

    [Fact]
    public async Task Handle_OrderNotFound_Returns404()
    {
        var orderId = Guid.NewGuid();
        _orders.Setup(x => x.GetWithItemsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var result = await _handler.Handle(new RefundOrderItemCommand
        {
            OrderId = orderId,
            RefundToInventoryId = Guid.NewGuid(),
            Items = new List<RefundLineDto> { new() { ItemId = Guid.NewGuid(), Quantity = 1 } }
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_EmptyItems_Returns400()
    {
        var result = await _handler.Handle(new RefundOrderItemCommand
        {
            OrderId = Guid.NewGuid(),
            RefundToInventoryId = Guid.NewGuid(),
            Items = new List<RefundLineDto>()
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ExceedRemaining_Returns400()
    {
        var (order, item, inv) = Seed(qty: 5, alreadyRefunded: 4);
        var result = await _handler.Handle(new RefundOrderItemCommand
        {
            OrderId = order.Id,
            RefundToInventoryId = inv.Id,
            Items = new List<RefundLineDto> { new() { OrderItemId = item.Id, Quantity = 2 } }
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_InactiveInventory_Returns400()
    {
        var (order, item, inv) = Seed();
        inv.IsActive = false;

        var result = await _handler.Handle(new RefundOrderItemCommand
        {
            OrderId = order.Id,
            RefundToInventoryId = inv.Id,
            Items = new List<RefundLineDto> { new() { OrderItemId = item.Id, Quantity = 1 } }
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorMessage.Should().Contain("inactive");
    }
}
