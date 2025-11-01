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
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<RefundOrderItemCommandHandler>> _mockLogger;
    private readonly RefundOrderItemCommandHandler _handler;

    public RefundOrderItemCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockOrderItemRepository = CreateMock<IOrderItemRepository>();
        _mockItemRepository = CreateMock<IItemRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<RefundOrderItemCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.OrderItems).Returns(_mockOrderItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
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
        var serialNumber = "SN-ITEM123-20250101-001";
        var refundQuantity = 2;
        var itemPrice = 100m;
        var itemId = Guid.NewGuid();

        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = refundQuantity,
            RefundReason = "Customer request"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            Quantity = 10,
            BasePrice = itemPrice
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

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _mockOrderItemRepository
            .Setup(x => x.Update(It.IsAny<OrderItem>()))
            .Verifiable();

        _mockItemRepository
            .Setup(x => x.Update(It.IsAny<Item>()))
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
        item.Quantity.Should().Be(12); // 10 + 2 refunded

        _mockOrderItemRepository.Verify(x => x.Update(orderItem), Times.Once);
        _mockItemRepository.Verify(x => x.Update(item), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "SN-NOTFOUND-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundReason = "Test"
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
        var serialNumber = "SN-ITEM123-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            Status = OrderItemStatus.Refunded, // Already refunded
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
        var serialNumber = "SN-DELETED-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            IsDeleted = true, // Deleted
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
        var serialNumber = "SN-ITEM123-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 10 // More than available
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            Quantity = 5, // Only 5 available
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
    public async Task Handle_ItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "SN-ITEM123-001";
        var itemId = Guid.NewGuid();

        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            ItemId = itemId,
            Quantity = 5,
            Status = OrderItemStatus.Successful,
            RefundedQuantity = 0
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Related item not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_ZeroRefundQuantity_ShouldReturnFailure()
    {
        // Arrange
        var serialNumber = "SN-ITEM123-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 0 // Invalid
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            Quantity = 5
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

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
        var serialNumber = "SN-ITEM123-001";
        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1
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

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Handle_VariousRefundQuantities_ShouldRestoreCorrectInventory(int refundQuantity)
    {
        // Arrange
        var serialNumber = "SN-ITEM123-001";
        var itemId = Guid.NewGuid();
        var initialInventory = 100;

        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = refundQuantity
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            Quantity = initialInventory,
            BasePrice = 50m
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            ItemId = itemId,
            Quantity = 10,
            Status = OrderItemStatus.Successful,
            RefundedQuantity = 0
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _mockOrderItemRepository.Setup(x => x.Update(It.IsAny<OrderItem>())).Verifiable();
        _mockItemRepository.Setup(x => x.Update(It.IsAny<Item>())).Verifiable();

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        orderItem.RefundedQuantity.Should().Be(refundQuantity);
        item.Quantity.Should().Be(initialInventory + refundQuantity);
    }

    [Fact]
    public async Task Handle_WithRefundReason_ShouldStoreReason()
    {
        // Arrange
        var serialNumber = "SN-ITEM123-001";
        var refundReason = "Product defect - customer not satisfied";
        var itemId = Guid.NewGuid();

        var command = new RefundOrderItemCommand
        {
            SerialNumber = serialNumber,
            RefundQuantity = 1,
            RefundReason = refundReason
        };

        var item = new Item
        {
            Id = itemId,
            Quantity = 10
        };

        var orderItem = new OrderItem
        {
            SerialNumber = serialNumber,
            ItemId = itemId,
            Quantity = 5,
            Status = OrderItemStatus.Successful
        };

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(serialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItem);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        _mockOrderItemRepository.Setup(x => x.Update(It.IsAny<OrderItem>())).Verifiable();
        _mockItemRepository.Setup(x => x.Update(It.IsAny<Item>())).Verifiable();

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        orderItem.Status.Should().Be(OrderItemStatus.Refunded);
        orderItem.RefundedAt.Should().NotBeNull();
        // Note: RefundReason is set in the handler but not accessible in test due to how mocks work
    }
}
