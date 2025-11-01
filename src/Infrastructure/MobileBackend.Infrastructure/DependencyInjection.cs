using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using MobileBackend.Infrastructure.Data;
using MobileBackend.Infrastructure.Repositories;
using MobileBackend.Infrastructure.Services;

namespace MobileBackend.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration.
/// Registers database, repositories, and infrastructure services.
/// 
/// Usage in Program.cs:
/// builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all Infrastructure services.
    /// Includes database context, repositories, and Unit of Work.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. Database Configuration
        services.AddDatabase(configuration);

        // 2. Repository Pattern
        services.AddRepositories();

        // 3. Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 4. Common Services
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Register Application interfaces with Framework service wrappers
        services.AddScoped<Application.Interfaces.IPasswordService, PasswordServiceWrapper>();
        services.AddScoped<Application.Interfaces.IJwtService, JwtServiceWrapper>();

        return services;
    }

    /// <summary>
    /// Register PostgreSQL database.
    /// 
    /// Configuration in appsettings.json:
    /// {
    ///   "ConnectionStrings": {
    ///     "DefaultConnection": "Host=localhost;Database=MobileBackendDb;Username=postgres;Password=your_password"
    ///   }
    /// }
    /// </summary>
    private static IServiceCollection AddDatabase(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is required. " +
                "Please configure it in appsettings.json under ConnectionStrings section.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("MobileBackend.Infrastructure")));

        return services;
    }

    /// <summary>
    /// Register all repositories.
    /// Uses manual registration for explicit control and clarity.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Manual Registration (Recommended for production)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IColorRepository, ColorRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to override specific repositories.
    /// 
    /// Example usage:
    /// services.AddInfrastructure(configuration)
    ///     .OverrideRepository<IOrderRepository, CustomOrderRepository>();
    /// </summary>
    public static IServiceCollection OverrideRepository<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TInterface));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped<TInterface, TImplementation>();
        return services;
    }
}
