using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Application.Features.Auth.Commands.Login;
using MobileBackend.Application.Features.Auth.Commands.RefreshToken;
using MobileBackend.Application.Features.Auth.Commands.Register;

namespace MobileBackend.API.Controllers;

/// <summary>
/// Authentication controller
/// Handles user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>User ID if successful</returns>
    /// <response code="201">User registered successfully (pending approval)</response>
    /// <response code="400">Invalid request or validation error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = "User registered successfully. Your account is pending approval by an administrator.",
            userId = result.Data
        });
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT tokens and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="403">Account disabled or not approved</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand
        {
            UsernameOrEmail = request.UsernameOrEmail,
            Password = request.Password,
            DeviceInfo = request.DeviceInfo,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return Ok(new
        {
            success = true,
            message = "Login successful",
            data = result.Data
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            DeviceInfo = request.DeviceInfo,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return Ok(new
        {
            success = true,
            message = "Token refreshed successfully",
            data = result.Data
        });
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    /// <returns>Success message</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        // TODO: Implement logout command to revoke refresh token
        // For now, client should just delete tokens from local storage
        
        _logger.LogInformation("User logged out");

        return Ok(new
        {
            success = true,
            message = "Logout successful"
        });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var username = User.FindFirst("name")?.Value;
        var email = User.FindFirst("email")?.Value;

        return Ok(new
        {
            success = true,
            data = new
            {
                userId,
                username,
                email,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            }
        });
    }
}
