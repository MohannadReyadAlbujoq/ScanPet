using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Features.Items.Commands.CreateItem;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Items.Commands;

public class CreateItemCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly Mock<ILogger<CreateItemCommandHandler>> _mockLogger;
    private readonly CreateItemCommandHandler _handler;

    public CreateItemCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockItemRepository = CreateMock<IItemRepository>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockDateTimeService = CreateMock<IDateTimeService>();
        _mockLogger = CreateMock<ILogger<CreateItemCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Items).Returns(_mockItemRepository.Object);
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockDateTimeService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        _handler = new CreateItemCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateItem()
    {
        // Arrange
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            Description = "Test Description",
            SKU = "TEST-001",
            BasePrice = 99.99m
        };

        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken ct) => i);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _mockItemRepository.Verify(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithColor_ShouldCreateItem()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            SKU = "TEST-001",
            BasePrice = 99.99m,
            ColorId = colorId
        };

        var color = new Color
        {
            Id = colorId,
            Name = "Test Color"
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken ct) => i);

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
    public async Task Handle_InvalidColorId_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            SKU = "TEST-001",
            BasePrice = 99.99m,
            ColorId = colorId
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Color not found");

        _mockItemRepository.Verify(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            SKU = "TEST-001",
            BasePrice = 99.99m
        };

        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken ct) => i);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("creating the item"); // Fixed: added "the"
    }

    [Theory]
    [InlineData(9.99)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    [InlineData(0.01)]
    public async Task Handle_VariousPrices_ShouldCreateCorrectly(decimal price)
    {
        // Arrange
        var command = new CreateItemCommand
        {
            Name = $"Test Item {price}",
            SKU = $"TEST-{price:000}",
            BasePrice = price
        };

        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken ct) => i);

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
    public async Task Handle_ValidCommand_ShouldSetCorrectDefaults()
    {
        // Arrange
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            SKU = "TEST-001",
            BasePrice = 99.99m
        };

        Item? capturedItem = null;
        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((i, ct) => capturedItem = i)
            .ReturnsAsync((Item i, CancellationToken ct) => i);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedItem.Should().NotBeNull();
        capturedItem!.Name.Should().Be(command.Name);
        capturedItem.SKU.Should().Be(command.SKU);
        capturedItem.BasePrice.Should().Be(command.BasePrice);
        capturedItem.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ItemWithNoQuantity_ShouldCreateItem()
    {
        // Arrange
        var command = new CreateItemCommand
        {
            Name = "Test Item",
            SKU = "TEST-001",
            BasePrice = 99.99m
        };

        _mockItemRepository
            .Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken ct) => i);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }
}
