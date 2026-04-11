using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.API.Middleware;

/// <summary>
/// Global exception handler middleware
/// Catches all unhandled exceptions and returns consistent error responses
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new Result<object>
        {
            Success = false,
            ErrorMessage = GetUserFriendlyMessage(exception),
            StatusCode = GetStatusCode(exception)
        };

        // Include stack trace in development
        if (_environment.IsDevelopment())
        {
            response.ErrorMessage += $"\n\nStack Trace:\n{exception.StackTrace}";
        }

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            ValidationException validationEx => string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
            ArgumentException => exception.Message,
            InvalidOperationException => exception.Message,
            DbUpdateException dbEx => GetDbUpdateMessage(dbEx),
            _ => "An error occurred while processing your request. Please try again later."
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ValidationException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (int)HttpStatusCode.Conflict,
            DbUpdateException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetDbUpdateMessage(DbUpdateException exception)
    {
        var innerMessage = exception.InnerException?.Message ?? exception.Message;

        if (IsUniqueConstraintViolation(exception))
        {
            return "A record with the same unique value already exists. Please use a different value.";
        }

        if (innerMessage.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
        {
            return "The operation failed because a referenced record does not exist or is still in use.";
        }

        return "A database error occurred while processing your request. Please verify your data and try again.";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var innerMessage = exception.InnerException?.Message ?? exception.Message;
        return innerMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("23505", StringComparison.OrdinalIgnoreCase); // PostgreSQL unique violation code
    }
}

/// <summary>
/// Extension method to register exception handler middleware
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
