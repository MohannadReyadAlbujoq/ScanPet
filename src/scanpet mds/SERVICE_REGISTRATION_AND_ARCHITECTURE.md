# ?? SERVICE REGISTRATION & ARCHITECTURE COMPARISON

**Date:** December 2024  
**Purpose:** Service registration location + Clean Architecture vs N-Tier  
**Project:** ScanPet Mobile Backend

---

## ?? WHERE SERVICES ARE REGISTERED

### Primary Location: Program.cs

**File:** `src/API/MobileBackend.API/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ===============================================
// SERVICE REGISTRATION (DI Container Setup)
// ===============================================

// 1. Application Layer Services
builder.Services.AddApplicationServices();

// 2. Infrastructure Layer Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Framework Layer Services
builder.Services.AddFrameworkServices(builder.Configuration);

// 4. API Layer Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddHealthChecks();

var app = builder.Build();

// ===============================================
// MIDDLEWARE PIPELINE CONFIGURATION
// ===============================================
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

---

## ?? EXTENSION METHODS (Best Practice)

Each layer registers its own services using extension methods.

### 1. Application Layer Registration

**File:** `src/Application/MobileBackend.Application/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace MobileBackend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // MediatR - for CQRS pattern
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // FluentValidation - for input validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // AutoMapper - for DTO mapping (if used)
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
```

**What's Registered:**
- ? All Command Handlers (21)
- ? All Query Handlers (15)
- ? All Validators (15)
- ? AutoMapper profiles

---

### 2. Infrastructure Layer Registration

**File:** `src/Infrastructure/MobileBackend.Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MobileBackend.Application.Interfaces;
using MobileBackend.Infrastructure.Data;
using MobileBackend.Infrastructure.Repositories;
using MobileBackend.Infrastructure.Services;

namespace MobileBackend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IColorRepository, ColorRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        // Services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IEmailService, EmailService>();
        
        // Database Seeder
        services.AddScoped<DbSeeder>();
        
        return services;
    }
}
```

**What's Registered:**
- ? DbContext (Scoped)
- ? UnitOfWork (Scoped)
- ? 10 Repositories (Scoped)
- ? 3 Services (Scoped/Transient)
- ? DbSeeder (Scoped)

---

### 3. Framework Layer Registration

**File:** `src/Framework/MobileBackend.Framework/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MobileBackend.Framework.Security;

namespace MobileBackend.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameworkServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Service
        services.AddSingleton<IJwtService>(provider =>
        {
            var privateKeyPath = configuration["Jwt:PrivateKeyPath"];
            var publicKeyPath = configuration["Jwt:PublicKeyPath"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var accessTokenExpiry = int.Parse(configuration["Jwt:AccessTokenExpiryMinutes"]);
            var refreshTokenExpiry = int.Parse(configuration["Jwt:RefreshTokenExpiryDays"]);
            
            return new JwtService(
                privateKeyPath,
                publicKeyPath,
                issuer,
                audience,
                accessTokenExpiry,
                refreshTokenExpiry);
        });
        
        // Encryption Services
        services.AddSingleton<IEncryptionService, EncryptionService>();
        
        return services;
    }
}
```

**What's Registered:**
- ? JwtService (Singleton)
- ? EncryptionService (Singleton)

---

### 4. API Layer Registration

**In Program.cs directly:**

```csharp
// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
        options.JsonSerializerOptions.WriteIndented = true;        // Pretty print
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ScanPet Mobile Backend API",
        Version = "v1"
    });
    
    // JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // JWT configuration
    });

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<DetailedHealthCheck>("detailed");

// HTTP Context Accessor (for CurrentUserService)
builder.Services.AddHttpContextAccessor();
```

---

## ?? SERVICE LIFETIMES

### Singleton (Application Lifetime)
**Created once, shared across entire app**

```csharp
services.AddSingleton<IJwtService, JwtService>();
services.AddSingleton<IEncryptionService, EncryptionService>();
```

**Use For:**
- Stateless services
- Configuration services
- Services without dependencies on scoped services

**Examples in Your System:**
- JwtService (stateless, thread-safe)
- EncryptionService (stateless)

---

### Scoped (Per Request)
**Created once per HTTP request**

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IAuditService, AuditService>();
```

**Use For:**
- Database contexts
- Repositories
- Services that depend on DbContext
- Request-specific services

**Examples in Your System:**
- ApplicationDbContext
- All Repositories
- UnitOfWork
- AuditService
- CurrentUserService

---

### Transient (Per Injection)
**Created every time it's injected**

```csharp
services.AddTransient<IEmailService, EmailService>();
```

**Use For:**
- Lightweight services
- Stateless operations
- Services that shouldn't be reused

**Examples in Your System:**
- EmailService (shouldn't be shared)

---

## ?? FUTURE SERVICE ADDITIONS

### Where to Add New Services

**1. New Repository**
```csharp
// Interface in Application/Interfaces/
public interface INewEntityRepository : IRepository<NewEntity>
{
    // Custom methods
}

// Implementation in Infrastructure/Repositories/
public class NewEntityRepository : GenericRepository<NewEntity>, INewEntityRepository
{
    // Implementation
}

// Register in Infrastructure/DependencyInjection.cs
services.AddScoped<INewEntityRepository, NewEntityRepository>();

// Add to UnitOfWork
INewEntityRepository NewEntities { get; }
```

**2. New Service**
```csharp
// Interface in Application/Common/Interfaces/
public interface INewService
{
    Task DoSomethingAsync();
}

// Implementation in Infrastructure/Services/
public class NewService : INewService
{
    public async Task DoSomethingAsync()
    {
        // Implementation
    }
}

// Register in Infrastructure/DependencyInjection.cs
services.AddScoped<INewService, NewService>();
```

**3. New Handler (CQRS)**
```csharp
// Command in Application/Features/NewFeature/Commands/
public class NewCommand : IRequest<Result>
{
    // Properties
}

// Handler
public class NewCommandHandler : IRequestHandler<NewCommand, Result>
{
    // Auto-registered by MediatR
}
```

---

## ??? CLEAN ARCHITECTURE VS N-TIER

### Traditional N-Tier (3-Layer)

```
???????????????????????????
?   Presentation Layer    ? (UI, Controllers)
?      (depends on)       ?
???????????????????????????
            ?
???????????????????????????
?  Business Logic Layer   ? (Services, Business Rules)
?      (depends on)       ?
???????????????????????????
            ?
???????????????????????????
?  Data Access Layer      ? (Repositories, SQL)
?      (depends on)       ?
???????????????????????????
            ?
???????????????????????????
?       Database          ?
???????????????????????????
```

**Characteristics:**
- ? Top-down dependencies (each layer depends on layer below)
- ? Business logic depends on database
- ? Hard to test (need database)
- ? Tight coupling
- ? Database-centric

**Example Code:**
```csharp
// Business layer depends on data layer
public class OrderService
{
    private readonly OrderRepository _repository;  // Concrete class!
    
    public void CreateOrder(Order order)
    {
        // Business logic mixed with data access
        _repository.Save(order);
    }
}
```

---

### Clean Architecture (This Project)

```
??????????????????????????????????????????
?            API Layer                   ?
?         (Controllers, UI)              ?
??????????????????????????????????????????
               ?
??????????????????????????????????????????
?       Application Layer                ?
?    (Commands, Queries, Handlers)       ?
??????????????????????????????????????????
               ?
??????????????????????????????????????????
?          Domain Layer                  ?
?    (Entities, Business Rules)          ?
?         NO DEPENDENCIES                ?
??????????????????????????????????????????
               ?
               ? (Implements interfaces)
??????????????????????????????????????????
?      Infrastructure Layer              ?
?  (DbContext, Repositories, Services)   ?
??????????????????????????????????????????
```

**Characteristics:**
- ? Dependency Inversion (outer depends on inner)
- ? Domain is independent
- ? Easy to test (mock interfaces)
- ? Loose coupling
- ? Business-centric

**Example Code:**
```csharp
// Application layer depends on interface
public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;  // Interface!
    
    public async Task<Result> Handle(CreateOrderCommand command)
    {
        // Business logic independent of database
        var order = new Order { /* ... */ };
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### Side-by-Side Comparison

| Aspect | N-Tier | Clean Architecture |
|--------|--------|-------------------|
| **Layers** | 3 (Presentation, Business, Data) | 5 (API, Application, Domain, Infrastructure, Framework) |
| **Dependencies** | Top-down (A?B?C) | Dependency Inversion (all ?Domain) |
| **Core** | Database | Domain |
| **Business Logic** | Business Layer | Domain + Application |
| **Data Access** | Data Layer | Infrastructure |
| **Coupling** | Tight | Loose |
| **Testability** | Hard (needs DB) | Easy (mock interfaces) |
| **Flexibility** | Low | High |
| **Complexity** | Simple | More complex |
| **Scalability** | Vertical | Horizontal |
| **Maintenance** | Hard (changes ripple) | Easy (isolated changes) |

---

### Dependency Flow Comparison

**N-Tier:**
```
Presentation ? Business ? Data ? Database
  (depends)     (depends)  (depends)
```
All dependencies point down. Business layer NEEDS Data layer.

**Clean Architecture:**
```
        API ? Application ? Infrastructure
                ?
              Domain
```
All dependencies point to Domain. Infrastructure IMPLEMENTS interfaces FROM Application.

---

### Testing Comparison

**N-Tier Testing:**
```csharp
[Test]
public void CreateOrder_Should_Save()
{
    // Need real database or complex mocking
    var dbContext = new TestDbContext();
    var repository = new OrderRepository(dbContext);
    var service = new OrderService(repository);  // Concrete!
    
    service.CreateOrder(order);
    
    Assert.That(dbContext.Orders.Count(), Is.EqualTo(1));
}
```
? Hard to test, requires database

**Clean Architecture Testing:**
```csharp
[Test]
public async Task CreateOrder_Should_Call_Repository()
{
    // Easy mocking with interfaces
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var handler = new CreateOrderCommandHandler(mockUnitOfWork.Object);
    
    await handler.Handle(command, CancellationToken.None);
    
    mockUnitOfWork.Verify(x => x.Orders.AddAsync(It.IsAny<Order>()), Times.Once);
}
```
? Easy to test, no database needed

---

### Real-World Example

**Scenario:** Switch from PostgreSQL to MongoDB

**N-Tier:**
```csharp
// Business layer breaks!
public class OrderService
{
    private readonly SqlRepository _repo;  // Tightly coupled!
    
    // Need to rewrite business logic
}
```
? Must change business logic code

**Clean Architecture:**
```csharp
// Business logic unchanged!
public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;  // Interface!
    
    // Business logic stays the same
}

// Just create new implementation
public class MongoOrderRepository : IOrderRepository
{
    // New MongoDB implementation
}

// Change registration
services.AddScoped<IOrderRepository, MongoOrderRepository>();
```
? Only change Infrastructure layer, business logic untouched

---

## ?? WHY CLEAN ARCHITECTURE?

### Problems It Solves

1. **Database Changes**
   - N-Tier: Rewrite everything
   - Clean: Change Infrastructure only

2. **UI Changes**
   - N-Tier: May affect business logic
   - Clean: Change API only

3. **Testing**
   - N-Tier: Need database
   - Clean: Mock interfaces

4. **Team Collaboration**
   - N-Tier: Merge conflicts
   - Clean: Work on separate layers

5. **Scaling**
   - N-Tier: Scale all together
   - Clean: Scale layers independently

---

## ?? SUMMARY

### Service Registration Location
**File:** `src/API/MobileBackend.API/Program.cs`

**Method:** Extension methods per layer
- `AddApplicationServices()`
- `AddInfrastructureServices()`
- `AddFrameworkServices()`

### Clean vs N-Tier

**Use N-Tier When:**
- ? Small, simple projects
- ? Tight deadlines
- ? Single developer
- ? Won't change much

**Use Clean Architecture When:**
- ? Medium to large projects
- ? Long-term maintenance
- ? Team development
- ? Testability important
- ? May change database/UI
- ? **Your project!** ?

**Your System Uses Clean Architecture Because:**
1. ? Business rules are complex
2. ? Needs to be testable
3. ? May scale in future
4. ? Professional quality required
5. ? Team collaboration expected

---

**Status:** ? **SERVICE REGISTRATION GUIDE COMPLETE**  
**Architecture:** Clean Architecture  
**Advantages:** Testability, Maintainability, Flexibility  
**Worth The Complexity:** ? **YES!**

---

**END OF SERVICE REGISTRATION GUIDE**
