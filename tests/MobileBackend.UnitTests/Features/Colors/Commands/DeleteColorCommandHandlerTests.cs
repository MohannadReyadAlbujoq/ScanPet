using FluentAssertions;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Features.Colors.Commands.DeleteColor;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests.Features.Colors.Commands;

/// <summary>
/// Unit tests for DeleteColorCommandHandler
/// </summary>
public class DeleteColorCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DeleteColorCommandHandler>> _mockLogger;
    private readonly DeleteColorCommandHandler _handler;

    public DeleteColorCommandHandlerTests()
    {
        _mockUnitOfWork = CreateMock<IUnitOfWork>();
        _mockColorRepository = CreateMock<IColorRepository>();
        _mockAuditService = CreateMock<IAuditService>();
        _mockCurrentUserService = CreateMock<ICurrentUserService>();
        _mockLogger = CreateMock<ILogger<DeleteColorCommandHandler>>();
        
        _mockUnitOfWork.Setup(x => x.Colors).Returns(_mockColorRepository.Object);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        
        // Fix: Constructor now uses IUnitOfWork, IAuditService, ICurrentUserService, ILogger
        _handler = new DeleteColorCommandHandler(
            _mockUnitOfWork.Object,
            _mockAuditService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSoftDeleteColor()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteColorCommand { ColorId = colorId }; // Fix: Changed from Id to ColorId

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Test Color",
            IsDeleted = false
        };

        _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingColor.IsDeleted.Should().BeTrue();
        existingColor.DeletedAt.Should().NotBeNull();
        existingColor.DeletedBy.Should().Be(userId);

        _mockColorRepository.Verify(x => x.Update(existingColor), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ColorNotFound_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new DeleteColorCommand { ColorId = colorId }; // Fix: Changed from Id to ColorId

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
    public async Task Handle_AlreadyDeleted_ShouldSucceed()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new DeleteColorCommand { ColorId = colorId };

        var deletedColor = new Color
        {
            Id = colorId,
            Name = "Deleted Color",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedColor);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Note: Handler allows re-deleting (idempotent operation)
        result.Success.Should().BeTrue();
        deletedColor.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new DeleteColorCommand { ColorId = colorId }; // Fix: Changed from Id to ColorId

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Test Color",
            IsDeleted = false
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("deleting the color");
    }

    [Fact]
    public async Task Handle_MultipleDeletes_ShouldBeIdempotent()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var command = new DeleteColorCommand { ColorId = colorId };

        var existingColor = new Color
        {
            Id = colorId,
            Name = "Test Color",
            IsDeleted = false
        };

        _mockColorRepository
            .Setup(x => x.GetByIdAsync(colorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColor);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act - First delete
        var result1 = await _handler.Handle(command, CancellationToken.None);

        // Mark as deleted for second attempt
        existingColor.IsDeleted = true;
        
        // Act - Second delete (should also succeed - idempotent)
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue(); // Handler allows re-deleting
        existingColor.IsDeleted.Should().BeTrue();
    }
}
