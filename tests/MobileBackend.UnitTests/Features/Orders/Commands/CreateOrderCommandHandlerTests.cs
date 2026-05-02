using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Features.Orders.Commands.CreateOrder;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq.Expressions;

namespace MobileBackend.UnitTests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<IInventoryRepository> _mockInventoryRepository;
    private readonly Mock<IItemInventoryRepository> _mockItemInventoryRepository;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
    private readonly Mock<IDiscountRepository> _mockDiscountRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockOrderRepository = CreateMock<IOrderRepository>();
        _mockItemRepository = CreateMock<IItemRepository>();
        _mockInventoryRepository = CreateMock<IInventoryRepository>();
        _mockItemInventoryRepository = CreateMock<IItemInventoryRepository>();
        _mockOrderItemRepository = CreateMock<IOrderItemRepository>();
        _mockDiscountRepository = CreateMock<IDiscountRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<CreateOrderCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Orders).Returns(_mockOrderRepository.Object);
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Inventories).Returns(_mockInventoryRepository.Object);
        _mockUnitOfWork.Setup(x => x.ItemInventories).Returns(_mockItemInventoryRepository.Object);
        _mockUnitOfWork.Setup(x => x.OrderItems).Returns(_mockOrderItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Discounts).Returns(_mockDiscountRepository.Object);

        // Default: no discounts active
        _mockDiscountRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Discount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Discount>());

        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _handler = new CreateOrderCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    private void SetupItemWithInventory(Guid itemId, string name, string sku, decimal basePrice, int availableQty)
    {
        var item = new Item { Id = itemId, Name = name, SKU = sku, BasePrice = basePrice };
        var invId = Guid.NewGuid();
        var ii = new ItemInventory
        {
            Id = Guid.NewGuid(), ItemId = itemId, InventoryId = invId, Quantity = availableQty,
            Inventory = new Inventory { Id = invId, Name = "Test Warehouse", IsActive = true }
        };
        _mockItemRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Item, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Item> { item });
        _mockItemInventoryRepository
            .Setup(x => x.GetTotalQuantityForItemAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableQty);
        _mockItemInventoryRepository
            .Setup(x => x.GetByItemIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemInventory> { ii });
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrder()
    {
        var itemId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe", ClientPhone = "1234567890", InventoryId = inventoryId,
            Description = "Test order",
            OrderItems = new List<OrderItemDto> { new() { ItemId = itemId, Quantity = 5, UnitPrice = 10.00m } }
        };
        _mockInventoryRepository.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Inventory { Id = inventoryId, Name = "Test Inventory", IsActive = true });
        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10);
        _mockOrderItemRepository.Setup(x => x.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderItem?)null);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldReturnFailure()
    {
        var itemId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe", ClientPhone = "1234567890", InventoryId = inventoryId,
            OrderItems = new List<OrderItemDto> { new() { ItemId = itemId, Quantity = 20, UnitPrice = 10.00m } }
        };
        _mockInventoryRepository.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Inventory { Id = inventoryId, Name = "Test Inventory", IsActive = true });
        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Insufficient quantity");
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ShouldReturnFailure()
    {
        var itemId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe", ClientPhone = "1234567890", InventoryId = inventoryId,
            OrderItems = new List<OrderItemDto> { new() { ItemId = itemId, Quantity = 5, UnitPrice = 10.00m } }
        };
        _mockInventoryRepository.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Inventory { Id = inventoryId, Name = "Test Inventory", IsActive = true });
        _mockItemRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Item, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Item>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InventoryNotFound_ShouldReturnFailure()
    {
        var inventoryId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe", ClientPhone = "1234567890", InventoryId = inventoryId,
            OrderItems = new List<OrderItemDto> { new() { ItemId = Guid.NewGuid(), Quantity = 5, UnitPrice = 10.00m } }
        };
        _mockInventoryRepository.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Inventory not found");
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        var itemId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe", ClientPhone = "1234567890", InventoryId = inventoryId,
            OrderItems = new List<OrderItemDto> { new() { ItemId = itemId, Quantity = 5, UnitPrice = 10.00m } }
        };
        _mockInventoryRepository.Setup(x => x.GetByIdAsync(inventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Inventory { Id = inventoryId, Name = "Test Inventory", IsActive = true });
        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10);
        _mockOrderItemRepository.Setup(x => x.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderItem?)null);
        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("creating the order");
    }
}