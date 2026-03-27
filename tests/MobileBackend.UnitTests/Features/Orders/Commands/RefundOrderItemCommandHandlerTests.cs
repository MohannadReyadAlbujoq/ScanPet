using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Orders.Commands;

/// <summary>
/// Unit tests for RefundOrderItemCommandHandler
/// </summary>
public class RefundOrderItemCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<IInventoryRepository> _mockInventoryRepository;
    private readonly Mock<IItemInventoryRepository> _mockItemInventoryRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<RefundOrderItemCommandHandler>> _mockLogger;
    private readonly RefundOrderItemCommandHandler _handler;

    public RefundOrderItemCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockOrderItemRepository = CreateMock<IOrderItemRepository>();
        _mockItemRepository = CreateMock<IItemRepository>();
        _mockInventoryRepository = CreateMock<IInventoryRepository>();
        _mockItemInventoryRepository = CreateMock<IItemInventoryRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<RefundOrderItemCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.OrderItems).Returns(_mockOrderItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Inventories).Returns(_mockInventoryRepository.Object);
        _mockUnitOfWork.Setup(x => x.ItemInventories).Returns(_mockItemInventoryRepository.Object);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _handler = new RefundOrderItemCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRefund_ShouldRefundOrderItemAndRestoreInventory()
    {
        // Arrange
        var serialNumber = "PF-001";
        var refundQuantity = 2;
        var itemPrice = 100m;
        var itemId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();

        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = refundQuantity,
            RefundReason = "Customer request",
            RefundToInventoryId = inventoryId
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            BasePrice = itemPrice
        };

        var inventory = new Inventory
        {
            Id = inventoryId,
            Name = "Test Warehouse",
            IsActive = true
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            ItemId = itemId,
            Quantity = 5,
            SalePrice = itemPrice,
            Status = OrderItemStatus.Successful,
            RefundedQuantity = 0
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _mockInventoryRepository
            .Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _mockItemInventoryRepository
            .Setup(x => x.AdjustInventoryAsync(itemId, inventoryId, refundQuantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockOrderItemRepository
            .Setup(x => x.Update(It.IsAny<OrderItem>()))
            .Verifiable();

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        orderItem.Status.Should().Be(OrderItemStatus.Refunded);
        orderItem.RefundedQuantity.Should().Be(refundQuantity);
        orderItem.RefundedToInventoryId.Should().Be(inventoryId);

        _mockItemInventoryRepository.Verify(x => x.AdjustInventoryAsync(itemId, inventoryId, refundQuantity, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderItemRepository.Verify(x => x.Update(orderItem), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "NOTFOUND-SKU";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundReason = "Test",
            RefundToInventoryId = Guid.NewGuid()
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_AlreadyRefunded_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundToInventoryId = Guid.NewGuid()
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            Status = OrderItemStatus.Refunded,
            RefundedQuantity = 2
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already been refunded");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_DeletedOrderItem_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-DELETED";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundToInventoryId = Guid.NewGuid()
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("been deleted");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_RefundQuantityExceedsAvailable_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 10,
            RefundToInventoryId = Guid.NewGuid()
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            Quantity = 5,
            Status = OrderItemStatus.Successful,
            RefundedQuantity = 0
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot exceed ordered quantity");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ZeroRefundQuantity_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 0,
            RefundToInventoryId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("must be greater than 0");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundToInventoryId = Guid.NewGuid()
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("error occurred");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handle_InactiveInventory_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "PF-001";
        var inventoryId = Guid.NewGuid();
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundToInventoryId = inventoryId
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            ItemId = Guid.NewGuid(),
            Quantity = 5,
            Status = OrderItemStatus.Successful,
            RefundedQuantity = 0
        };

        var inventory = new Inventory
        {
            Id = inventoryId,
            Name = "Inactive Warehouse",
            IsActive = false
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _mockInventoryRepository
            .Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inactive");
        result.StatusCode.Should().Be(400);
    }
}
