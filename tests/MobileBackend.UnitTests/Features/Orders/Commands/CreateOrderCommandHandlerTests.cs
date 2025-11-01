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

namespace MobileBackend.UnitTests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ILocationRepository> _mockLocationRepository;
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
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<CreateOrderCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Orders).Returns(_mockOrderRepository.Object);
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Locations).Returns(_mockLocationRepository.Object);
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        
        _handler = new CreateOrderCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
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

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            SKU = "TEST-001",
            Quantity = 10,
            BasePrice = 10.00m
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

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

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            Quantity = 10, // Only 10 available
            BasePrice = 10.00m
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

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

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

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
    public async Task Handle_MultipleItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var command = new CreateOrderCommand
        {
            ClientName = "John Doe",
            ClientPhone = "1234567890",
            LocationId = locationId,
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemId = item1Id, Quantity = 5, UnitPrice = 10.00m },
                new OrderItemDto { ItemId = item2Id, Quantity = 3, UnitPrice = 20.00m }
            }
        };

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item1 = new Item
        {
            Id = item1Id,
            Name = "Item 1",
            SKU = "SKU-001",
            Quantity = 10,
            BasePrice = 10.00m
        };

        var item2 = new Item
        {
            Id = item2Id,
            Name = "Item 2",
            SKU = "SKU-002",
            Quantity = 10,
            BasePrice = 20.00m
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(item1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item1);
            
        _mockItemRepository
            .Setup(x => x.GetByIdAsync(item2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item2);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
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

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            SKU = "TEST-001",
            Quantity = 10,
            BasePrice = 10.00m
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

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

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(5, 10.00, 50.00)]
    [InlineData(10, 15.50, 155.00)]
    [InlineData(3, 99.99, 299.97)]
    public async Task Handle_VariousQuantitiesAndPrices_ShouldCalculateCorrectly(
        int quantity, decimal price, decimal expectedTotal)
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
                    Quantity = quantity,
                    UnitPrice = price
                }
            }
        };

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            SKU = "TEST-001",
            Quantity = 100,
            BasePrice = price
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

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
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldGenerateOrderNumber()
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

        var location = new Location
        {
            Id = locationId,
            Name = "Test Location"
        };

        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            SKU = "TEST-001",
            Quantity = 10,
            BasePrice = 10.00m
        };

        _mockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        _mockItemRepository
            .Setup(x => x.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

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
    }
}
