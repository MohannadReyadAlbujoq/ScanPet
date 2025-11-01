using FluentAssertions;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Features.Colors.Queries.GetColorById;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Colors.Queries;

/// <summary>
/// Unit tests for GetColorByIdQueryHandler
/// </summary>
public class GetColorByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<ILogger<GetColorByIdQueryHandler>> _mockLogger;
    private readonly GetColorByIdQueryHandler _handler;

    public GetColorByIdQueryHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockLogger = CreateMock<ILogger<GetColorByIdQueryHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        
        _handler = new GetColorByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnColor()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var query = new GetColorByIdQuery { ColorId = colorId }; // Fix: Changed from Id to ColorId

        var color = new Color
        {
            Id = colorId,
            Name = "Test Color",
            Description = "Test Description",
            RedValue = 128,
            GreenValue = 64,
            BlueValue = 192
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(colorId);
        result.Data.Name.Should().Be("Test Color");
        result.Data.RedValue.Should().Be(128);
        result.Data.GreenValue.Should().Be(64);
        result.Data.BlueValue.Should().Be(192);
    }

    [Fact]
    public async Task Handle_InvalidId_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var query = new GetColorByIdQuery { ColorId = colorId }; // Fix: Changed from Id to ColorId

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DeletedColor_ShouldNotReturn()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var query = new GetColorByIdQuery { ColorId = colorId }; // Fix: Changed from Id to ColorId

        // Repository should not return deleted colors
        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var query = new GetColorByIdQuery { ColorId = colorId };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("retrieving the color"); // Fixed: added "the"
    }

    [Fact]
    public async Task Handle_ShouldMapAllProperties()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow.AddDays(-5);
        var query = new GetColorByIdQuery { ColorId = colorId }; // Fix: Changed from Id to ColorId

        var color = new Color
        {
            Id = colorId,
            Name = "Full Test Color",
            Description = "Complete Description",
            RedValue = 255,
            GreenValue = 128,
            BlueValue = 64,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = result.Data!;
        dto.Id.Should().Be(colorId);
        dto.Name.Should().Be("Full Test Color");
        dto.Description.Should().Be("Complete Description");
        dto.RedValue.Should().Be(255);
        dto.GreenValue.Should().Be(128);
        dto.BlueValue.Should().Be(64);
        dto.HexCode.Should().NotBeNullOrEmpty();
        dto.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        dto.UpdatedAt.Should().BeCloseTo(updatedAt, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    [InlineData(0, 0, 0)]
    [InlineData(255, 255, 255)]
    public async Task Handle_VariousRGBValues_ShouldReturnCorrectly(int red, int green, int blue)
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var query = new GetColorByIdQuery { ColorId = colorId }; // Fix: Changed from Id to ColorId

        var color = new Color
        {
            Id = colorId,
            Name = $"RGB({red},{green},{blue})",
            RedValue = red,
            GreenValue = green,
            BlueValue = blue
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RedValue.Should().Be(red);
        result.Data.GreenValue.Should().Be(green);
        result.Data.BlueValue.Should().Be(blue);
    }
}
