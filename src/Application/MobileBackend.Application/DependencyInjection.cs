using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MobileBackend.Application.Common.Behaviors;

namespace MobileBackend.Application;

/// <summary>
/// Application layer dependency injection configuration.
/// Registers MediatR (CQRS), FluentValidation, AutoMapper, and Pipeline Behaviors.
/// 
/// Usage in Program.cs:
/// builder.Services.AddApplication();
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Register all Application layer services.
    /// This includes CQRS handlers, validators, mappers, and behaviors.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. MediatR - Command Query Responsibility Segregation (CQRS)
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // 2. FluentValidation - Input validation
        services.AddValidatorsFromAssembly(assembly);

        // 3. AutoMapper - Object-to-object mapping
        services.AddAutoMapper(assembly);

        // 4. MediatR Pipeline Behaviors (cross-cutting concerns)
        // Order matters: Logging ? Validation ? Transaction ? Performance ? Handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to add custom MediatR handlers.
    /// This enables plugin architecture for extending functionality.
    /// 
    /// Example usage:
    /// services.AddApplication()
    ///     .AddCustomHandler<MyCustomCommand, MyCustomCommandHandler>();
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCustomHandler<TRequest, THandler>(this IServiceCollection services)
        where TRequest : IRequest
        where THandler : class, IRequestHandler<TRequest>
    {
        services.AddScoped<IRequestHandler<TRequest>, THandler>();
        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to add custom validators.
    /// This enables validation rule customization per deployment.
    /// 
    /// Example usage:
    /// services.AddApplication()
    ///     .AddCustomValidator<LoginRequestDto, CustomLoginValidator>();
    /// </summary>
    /// <typeparam name="TDto">DTO type to validate</typeparam>
    /// <typeparam name="TValidator">Validator implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCustomValidator<TDto, TValidator>(this IServiceCollection services)
        where TValidator : class, IValidator<TDto>
    {
        services.AddScoped<IValidator<TDto>, TValidator>();
        return services;
    }

    /// <summary>
    /// EXTENSION POINT: Allow customers to add custom pipeline behaviors.
    /// This enables additional cross-cutting concerns.
    /// 
    /// Example usage:
    /// services.AddApplication()
    ///     .AddCustomBehavior<CustomCachingBehavior>();
    /// </summary>
    /// <typeparam name="TBehavior">Custom behavior type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCustomBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TBehavior));
        return services;
    }
}
