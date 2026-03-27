using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Features.Orders.Commands.CreateOrder;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;
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
    private readonly Mock<ILocationRepository> _mockLocationRepository;
    private readonly Mock<IItemInventoryRepository> _mockItemInventoryRepository;
    private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockOrderRepository = CreateMock<IOrderRepository>();
        _mockItemRepository = CreateMock<IItemRepository>();
        _mockLocationRepository = CreateMock<ILocationRepository>();
        _mockItemInventoryRepository = CreateMock<IItemInventoryRepository>();
        _mockOrderItemRepository = CreateMock<IOrderItemRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<CreateOrderCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Orders).Returns(_mockOrderRepository.Object);
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Locations).Returns(_mockLocationRepository.Object);
        _mockUnitOfWork.Setup(x => x.ItemInventories).Returns(_mockItemInventoryRepository.Object);
        _mockUnitOfWork.Setup(x => x.OrderItems).Returns(_mockOrderItemRepository.Object);
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        
        _handler = new CreateOrderCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    private void SetupItemWithInventory(Guid itemId, string name, string sku, decimal basePrice, int inventoryQuantity)
    {
        var item = new Item
        {
            Id = itemId,
            Name = name,
            SKU = sku,
            BasePrice = basePrice
        };

        var inventoryId = Guid.NewGuid();
        var itemInventory = new ItemInventory
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            InventoryId = inventoryId,
            Quantity = inventoryQuantity,
            Inventory = new Inventory { Id = inventoryId, Name = "Test Warehouse", IsActive = true }
        };

        _mockItemRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Item, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Item> { item });

        _mockItemInventoryRepository
            .Setup(x => x.GetTotalQuantityForItemAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryQuantity);

        _mockItemInventoryRepository
            .Setup(x => x.GetByItemIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ItemInventory> { itemInventory });
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            Description = "Test order",
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ItemId = itemId,
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };

        var location = new Location { Id = locationId, Name = "Test Location" };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10);

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderItem?)null);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ItemId = itemId,
                    Quantity = 20, // More than available
                    UnitPrice = 10.00m
                }
            }
        };

        var location = new Location { Id = locationId, Name = "Test Location" };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10); // Only 10 available

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Insufficient quantity");
        
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ItemId = itemId,
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };

        var location = new Location { Id = locationId, Name = "Test Location" };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Item, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Item>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LocationNotFound_ShouldReturnFailure()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ItemId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Location not found");
        
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ItemId = itemId,
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
        };

        var location = new Location { Id = locationId, Name = "Test Location" };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        SetupItemWithInventory(itemId, "Test Item", "TEST-001", 10.00m, 10);

        _mockOrderItemRepository
            .Setup(x => x.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderItem?)null);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("creating the order");
    }
}
