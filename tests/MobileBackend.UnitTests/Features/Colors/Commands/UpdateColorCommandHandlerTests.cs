using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Features.Colors.Commands.UpdateColor;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Colors.Commands;

/// <summary>
/// Unit tests for UpdateColorCommandHandler
/// </summary>
public class UpdateColorCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly Mock<ILogger<UpdateColorCommandHandler>> _mockLogger;
    private readonly UpdateColorCommandHandler _handler;

    public UpdateColorCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockDateTimeService = CreateMock<IDateTimeService>();
        _mockLogger = CreateMock<ILogger<UpdateColorCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockDateTimeService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        _handler = new UpdateColorCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateColor()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Updated Color",
            RedValue = 200,
            GreenValue = 100,
            BlueValue = 50,
            Description = "Updated Description"
        };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Old Color",
            RedValue = 100,
            GreenValue = 100,
            BlueValue = 100
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingColor.Name.Should().Be(command.Name);
        existingColor.RedValue.Should().Be(command.RedValue);
        existingColor.GreenValue.Should().Be(command.GreenValue);
        existingColor.BlueValue.Should().Be(command.BlueValue);

        _mockColorRepository.Verify(x => x.Update(existingColor), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ColorNotFound_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Updated Color"
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");

        _mockColorRepository.Verify(x => x.Update(It.IsAny<Color>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Existing Color Name"
        };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Old Name"
        };

        var duplicateColor = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Existing Color Name"
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateColor);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");

        _mockColorRepository.Verify(x => x.Update(It.IsAny<Color>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameNameForSameColor_ShouldUpdate()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Same Name",
            RedValue = 150
        };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Same Name",
            RedValue = 100
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor); // Same color

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingColor.RedValue.Should().Be(command.RedValue);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Updated Color"
        };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Old Color"
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("updating the color");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(255, 255, 255)]
    [InlineData(128, 64, 192)]
    public async Task Handle_VariousRGBValues_ShouldUpdate(int red, int green, int blue)
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new UpdateColorCommand
        {
            ColorId = colorId, // Fix: Changed from Id to ColorId
            Name = "Test Color",
            RedValue = red,
            GreenValue = green,
            BlueValue = blue
        };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Old Color"
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingColor.RedValue.Should().Be(red);
        existingColor.GreenValue.Should().Be(green);
        existingColor.BlueValue.Should().Be(blue);
    }
}
