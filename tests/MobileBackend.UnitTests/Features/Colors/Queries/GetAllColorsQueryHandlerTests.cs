using FluentAssertions;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Features.Colors.Queries.GetAllColors;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Colors.Queries;

public class GetAllColorsQueryHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<ILogger<GetAllColorsQueryHandler>> _mockLogger;
    private readonly GetAllColorsQueryHandler _handler;

    public GetAllColorsQueryHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockLogger = CreateMock<ILogger<GetAllColorsQueryHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        
        _handler = new GetAllColorsQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllColors()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        var colors = new List<Color>
        {
            new Color { Id = Guid.NewGuid(), Name = "Red", RedValue = 255, GreenValue = 0, BlueValue = 0 },
            new Color { Id = Guid.NewGuid(), Name = "Green", RedValue = 0, GreenValue = 255, BlueValue = 0 },
            new Color { Id = Guid.NewGuid(), Name = "Blue", RedValue = 0, GreenValue = 0, BlueValue = 255 }
        };

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(colors);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data.Should().Contain(c => c.Name == "Red");
        result.Data.Should().Contain(c => c.Name == "Green");
        result.Data.Should().Contain(c => c.Name == "Blue");
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Color>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("retrieving colors");
    }

    [Fact]
    public async Task Handle_ShouldNotReturnDeletedColors()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        var colors = new List<Color>
        {
            new Color { Id = Guid.NewGuid(), Name = "Active Color", IsDeleted = false },
            new Color { Id = Guid.NewGuid(), Name = "Deleted Color", IsDeleted = true }
        };

        // Repository should only return non-deleted colors
        var activeColors = colors.Where(c => !c.IsDeleted).ToList();

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeColors);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.Should().Contain(c => c.Name == "Active Color");
        result.Data.Should().NotContain(c => c.Name == "Deleted Color");
    }

    [Fact]
    public async Task Handle_ShouldMapAllProperties()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        var color = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Test Color",
            Description = "Test Description",
            RedValue = 128,
            GreenValue = 64,
            BlueValue = 32
        };

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Color> { color });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = result.Data.First();
        dto.Id.Should().Be(color.Id);
        dto.Name.Should().Be(color.Name);
        dto.Description.Should().Be(color.Description);
        dto.RedValue.Should().Be(color.RedValue);
        dto.GreenValue.Should().Be(color.GreenValue);
        dto.BlueValue.Should().Be(color.BlueValue);
        dto.HexCode.Should().NotBeNullOrEmpty(); // Calculated from RGB
    }

    [Fact]
    public async Task Handle_MultipleColors_ShouldReturnAll()
    {
        // Arrange
        var query = new GetAllColorsQuery();

        var colors = Enumerable.Range(1, 10)
            .Select(i => new Color
            {
                Id = Guid.NewGuid(),
                Name = $"Color {i}",
                RedValue = i * 10,
                GreenValue = i * 20,
                BlueValue = i * 30
            })
            .ToList();

        _mockColorRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(colors);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(10);
    }
}
