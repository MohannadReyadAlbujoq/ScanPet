using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MobileBackend.Framework.Security;
using MobileBackend.Framework.Security.Models;

namespace MobileBackend.Framework;

/// <summary>
/// Framework layer dependency injection configuration.
/// Registers security services (JWT, Password, BitManipulation).
/// 
/// Usage in Program.cs:
/// builder.Services.AddFramework(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all Framework services.
    /// Includes JWT service, password service, and bit manipulation service.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddFramework(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. JWT Settings Configuration
        services.ConfigureJwtSettings(configuration);

        // 2. Security Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IBitManipulationService, BitManipulationService>();

        return services;
    }

    /// <summary>
    /// Configure JWT settings from appsettings.json.
    /// 
    /// Required configuration in appsettings.json:
    /// {
    ///   "JwtSettings": {
    ///     "SecretKey": "your-secret-key-at-least-32-characters-long",
    ///     "Issuer": "MobileBackendAPI",
    ///     "Audience": "MobileApp",
    ///     "AccessTokenExpiryMinutes": 15,
    ///     "RefreshTokenExpiryDays": 7
    ///   }
    /// }
    /// </summary>
    private static IServiceCollection ConfigureJwtSettings(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        
        if (jwtSettings == null)
        {
            throw new InvalidOperationException(
                "JwtSettings configuration is missing in appsettings.json. " +
                "Please add the JwtSettings section with SecretKey, Issuer, Audience, " +
                "AccessTokenExpiryMinutes, and RefreshTokenExpiryDays.");
        }

        // Validate required settings
        ValidateJwtSettings(jwtSettings);

        // Register as singleton (settings don't change during app lifetime)
        services.AddSingleton(jwtSettings);

        return services;
    }

    /// <summary>
    /// Validate JWT settings to ensure all required values are present.
    /// </summary>
    private static void ValidateJwtSettings(JwtSettings settings)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            errors.Add("JwtSettings.SecretKey is required");
        else if (Encoding.UTF8.GetByteCount(settings.SecretKey) < 32)
            errors.Add("JwtSettings.SecretKey must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(settings.Issuer))
            errors.Add("JwtSettings.Issuer is required");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            errors.Add("JwtSettings.Audience is required");

        if (settings.AccessTokenExpiryMinutes <= 0)
            errors.Add("JwtSettings.AccessTokenExpiryMinutes must be greater than 0");

        if (settings.RefreshTokenExpiryDays <= 0)
            errors.Add("JwtSettings.RefreshTokenExpiryDays must be greater than 0");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                "JWT configuration is invalid:\n" + string.Join("\n", errors));
        }
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to use custom JWT service implementation.
    /// This enables customization of token generation/validation logic.
    /// 
    /// Example usage:
    /// services.AddFramework(configuration)
    ///     .UseCustomJwtService<CustomJwtService>();
    /// </summary>
    /// <typeparam name="TJwtService">Custom JWT service implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection UseCustomJwtService<TJwtService>(this IServiceCollection services)
        where TJwtService : class, IJwtService
    {
        // Remove existing JWT service
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IJwtService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register custom JWT service
        services.AddScoped<IJwtService, TJwtService>();

        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to use custom password service.
    /// This enables different password hashing algorithms.
    /// 
    /// Example usage:
    /// services.AddFramework(configuration)
    ///     .UseCustomPasswordService<Argon2PasswordService>();
    /// </summary>
    /// <typeparam name="TPasswordService">Custom password service</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection UseCustomPasswordService<TPasswordService>(this IServiceCollection services)
        where TPasswordService : class, IPasswordService
    {
        // Remove existing password service
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPasswordService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register custom password service
        services.AddScoped<IPasswordService, TPasswordService>();

        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Configure JWT settings at runtime.
    /// Useful for multi-tenant scenarios where each tenant has different JWT settings.
    /// 
    /// Example usage:
    /// services.AddFramework(configuration)
    ///     .WithJwtSettings(customJwtSettings);
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="jwtSettings">Custom JWT settings</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection WithJwtSettings(
        this IServiceCollection services, 
        JwtSettings jwtSettings)
    {
        // Validate settings
        ValidateJwtSettings(jwtSettings);

        // Remove existing settings
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(JwtSettings));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register custom settings
        services.AddSingleton(jwtSettings);

        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Disable bit manipulation for testing or specific deployments.
    /// 
    /// Example usage:
    /// services.AddFramework(configuration)
    ///     .DisableBitManipulation();
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection DisableBitManipulation(this IServiceCollection services)
    {
        // Remove bit manipulation service
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBitManipulationService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Register no-op implementation
        services.AddScoped<IBitManipulationService, NoOpBitManipulationService>();

        return services;
    }
}

/// <summary>
/// No-op implementation of IBitManipulationService for testing or when bit manipulation is disabled.
/// </summary>
internal class NoOpBitManipulationService : IBitManipulationService
{
    public byte[] EncryptData(byte[] data) => data;
    public byte[] DecryptData(byte[] data) => data;
    public string EncryptString(string data) => data;
    public string DecryptString(string encryptedData) => encryptedData;
}
