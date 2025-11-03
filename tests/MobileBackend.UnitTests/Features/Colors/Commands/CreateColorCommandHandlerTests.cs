using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Features.Colors.Commands.CreateColor;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Colors.Commands;

public class CreateColorCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly Mock<ILogger<CreateColorCommandHandler>> _mockLogger;
    private readonly CreateColorCommandHandler _handler;

    public CreateColorCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockDateTimeService = CreateMock<IDateTimeService>();
        _mockLogger = CreateMock<ILogger<CreateColorCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockDateTimeService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        _handler = new CreateColorCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateColor()
    {
        // Arrange
        var command = new CreateColorCommand
        {
            Name = "Test Color",
            RedValue = 255,
            GreenValue = 128,
            BlueValue = 0,
            Description = "Test Description"
        };

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockColorRepository
            .Setup(x => x.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color c, CancellationToken ct) => c);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _mockColorRepository.Verify(x => x.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateColorCommand
        {
            Name = "Existing Color",
            RedValue = 255,
            GreenValue = 0,
            BlueValue = 0
        };

        var existingColor = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Existing Color"
        };

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");

        _mockColorRepository.Verify(x => x.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateColorCommand
        {
            Name = "Test Color",
            RedValue = 100,
            GreenValue = 150,
            BlueValue = 200
        };

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockColorRepository
            .Setup(x => x.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color c, CancellationToken ct) => c);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("creating the color");
    }

    [Theory]
    [InlineData(255, 0, 0)] // Red
    [InlineData(0, 255, 0)] // Green
    [InlineData(0, 0, 255)] // Blue
    [InlineData(255, 255, 255)] // White
    [InlineData(0, 0, 0)] // Black
    [InlineData(128, 128, 128)] // Gray
    public async Task Handle_VariousRGBValues_ShouldCreateSuccessfully(
        int red, int green, int blue)
    {
        // Arrange
        var command = new CreateColorCommand
        {
            Name = $"Test Color RGB({red},{green},{blue})",
            RedValue = red,
            GreenValue = green,
            BlueValue = blue
        };

        _mockColorRepository
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color?)null);

        _mockColorRepository
            .Setup(x => x.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color c, CancellationToken ct) => c);

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
