# ??? ARCHITECTURE & DESIGN PATTERNS SUMMARY

**Date:** December 2024  
**Project:** ScanPet Mobile Backend  
**Architecture:** Clean Architecture + CQRS

---

## ?? SYSTEM PURPOSE

### What This System Does

**ScanPet Mobile Backend** is an enterprise-grade REST API for a pet inventory management and order tracking mobile application.

**Core Functionality:**
1. **User Management** - Registration, authentication, authorization
2. **Inventory Management** - Track colored items with quantities
3. **Location Management** - Warehouse/storage locations
4. **Order Processing** - Multi-item orders with serial numbers
5. **Refund System** - Refund orders by serial number with inventory restoration
6. **Role-Based Access Control** - 30+ permissions, 3 default roles
7. **Audit Logging** - Complete action tracking for compliance

---

## ??? ARCHITECTURE PATTERN: CLEAN ARCHITECTURE

### Overview

```
????????????????????????????????????????????????????????
?                     API Layer                         ?
?  (Controllers, Middleware, Filters, Health Checks)    ?
????????????????????????????????????????????????????????
                     ?
                     ?
????????????????????????????????????????????????????????
?                Application Layer                      ?
?    (Commands, Queries, Handlers, Validators, DTOs)    ?
????????????????????????????????????????????????????????
                     ?
                     ?
????????????????????????????????????????????????????????
?                  Domain Layer                         ?
?          (Entities, Enums, Interfaces)                ?
????????????????????????????????????????????????????????
                     ?
                     ?
????????????????????????????????????????????????????????
?             Infrastructure Layer                      ?
?  (DbContext, Repositories, Services, Configurations)   ?
????????????????????????????????????????????????????????
                     
????????????????????????????????????????????????????????
?               Framework Layer                         ?
?         (JWT Services, Security, Extensions)          ?
????????????????????????????????????????????????????????
```

### Layer Responsibilities

**1. Domain Layer (Core)**
- **Purpose:** Business entities and rules
- **Dependencies:** NONE
- **Contains:**
  - Entities (User, Order, Item, Color, etc.)
  - Enums (OrderStatus, OrderItemStatus)
  - Domain interfaces (ISoftDelete, IAuditableEntity)
- **Principle:** Pure business logic, no external dependencies

**2. Application Layer (Use Cases)**
- **Purpose:** Business logic orchestration
- **Dependencies:** Domain only
- **Contains:**
  - Commands (CreateOrder, RefundOrderItem)
  - Queries (GetAllOrders, GetOrderById)
  - Handlers (MediatR handlers)
  - Validators (FluentValidation)
  - DTOs (Data Transfer Objects)
  - Interfaces (IUnitOfWork, Repositories)
- **Principle:** CQRS pattern, single responsibility

**3. Infrastructure Layer (External)**
- **Purpose:** Data access and external services
- **Dependencies:** Application (interfaces)
- **Contains:**
  - DbContext (Entity Framework Core)
  - Repositories (implementations)
  - Services (AuditService, EmailService)
  - Configurations (entity configurations)
  - Migrations
- **Principle:** Dependency Inversion

**4. API Layer (Presentation)**
- **Purpose:** HTTP endpoints and middleware
- **Dependencies:** Application
- **Contains:**
  - Controllers (REST endpoints)
  - Middleware (exception handling, logging, JWT)
  - Filters (authorization, validation)
  - Health checks
- **Principle:** Thin controllers, delegate to handlers

**5. Framework Layer (Shared)**
- **Purpose:** Cross-cutting concerns
- **Dependencies:** NONE
- **Contains:**
  - JWT services
  - Encryption utilities
  - Extension methods
- **Principle:** Reusable across projects

---

## ?? DESIGN PATTERNS IMPLEMENTED

### 1. **CQRS (Command Query Responsibility Segregation)**

**Purpose:** Separate read and write operations

**Implementation:**
```
Commands (Write)              Queries (Read)
??? CreateOrderCommand        ??? GetAllOrdersQuery
??? RefundOrderItemCommand    ??? GetOrderByIdQuery
??? UpdateOrderCommand        ??? GetOrdersByUserQuery
```

**Benefits:**
- ? Optimized read/write operations separately
- ? Different models for commands vs queries
- ? Scalability (can split to different databases)
- ? Clear separation of concerns

**Example:**
```csharp
// Command (Write)
public class CreateOrderCommand : IRequest<Result<Guid>>
{
    public string ClientName { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
}

// Query (Read)
public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }
}
```

---

### 2. **Repository Pattern**

**Purpose:** Abstract data access logic

**Implementation:**
```csharp
// Interface in Application layer
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<List<Order>> GetByUserIdAsync(Guid userId);
}

// Implementation in Infrastructure layer
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }
}
```

**Benefits:**
- ? Testable (can mock repositories)
- ? Changeable data source without affecting business logic
- ? Centralized data access logic

---

### 3. **Unit of Work Pattern**

**Purpose:** Coordinate multiple repository operations in a transaction

**Implementation:**
```csharp
public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    IItemRepository Items { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
}
```

**Benefits:**
- ? Atomic operations (all or nothing)
- ? Single point to manage transactions
- ? Consistent data state

---

### 4. **Mediator Pattern (MediatR)**

**Purpose:** Decouple request/response handling

**Implementation:**
```csharp
// Controller sends command
var command = new CreateOrderCommand { /* ... */ };
var result = await _mediator.Send(command);

// Mediator routes to handler
public class CreateOrderCommandHandler 
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

**Benefits:**
- ? Loose coupling between controllers and handlers
- ? Single Responsibility Principle
- ? Easy to add new handlers
- ? Pipeline behaviors (validation, logging)

---

### 5. **Dependency Injection**

**Purpose:** Inversion of Control, manage dependencies

**Implementation:**
```csharp
// Registration in Program.cs
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Injection in handler
public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

**Benefits:**
- ? Testable (inject mocks)
- ? Flexible (swap implementations)
- ? Lifetime management by container

---

### 6. **Factory Pattern**

**Purpose:** Create objects without specifying exact class

**Implementation:**
```csharp
// DbContext Factory for migrations
public class ApplicationDbContextFactory 
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
```

---

### 7. **Strategy Pattern**

**Purpose:** Select algorithm at runtime

**Implementation:**
```csharp
// Different authentication strategies
public interface IAuthenticationStrategy
{
    Task<AuthResult> AuthenticateAsync(string identifier, string password);
}

public class EmailAuthenticationStrategy : IAuthenticationStrategy { }
public class PhoneAuthenticationStrategy : IAuthenticationStrategy { }
```

---

### 8. **Decorator Pattern**

**Purpose:** Add behavior to objects dynamically

**Implementation:**
```csharp
// MediatR Pipeline Behaviors
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Validate before handler
        await ValidateAsync(request);
        
        // Call next in pipeline
        return await next();
    }
}
```

---

### 9. **Builder Pattern**

**Purpose:** Construct complex objects step by step

**Implementation:**
```csharp
// FluentValidation is a builder pattern
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .Must(items => items.Count > 0);
    }
}
```

---

### 10. **Specification Pattern**

**Purpose:** Encapsulate business rules for querying

**Implementation:**
```csharp
// In repositories
public async Task<List<Order>> GetActiveOrdersAsync()
{
    return await _dbSet
        .Where(o => !o.IsDeleted && 
                    o.OrderStatus == OrderStatus.Confirmed)
        .ToListAsync();
}
```

---

## ?? SECURITY PATTERNS

### 1. **Authentication - JWT (RS256)**

**Pattern:** Token-based authentication
- Asymmetric encryption (RSA)
- Access tokens (short-lived: 15 min)
- Refresh tokens (long-lived: 7 days)

### 2. **Authorization - RBAC (Role-Based Access Control)**

**Pattern:** Permission-based authorization
- 30+ fine-grained permissions
- Bitwise permission checking (performance)
- Hierarchical roles (Admin, Manager, User)

**Example:**
```csharp
[RequirePermission("Orders.Create")]
public async Task<IActionResult> CreateOrder()
```

### 3. **Password Hashing - BCrypt**

**Pattern:** One-way cryptographic hash
- Work factor: 12 (adjustable for security)
- Salt included automatically
- Slow by design (brute-force protection)

---

## ?? DATA PATTERNS

### 1. **Soft Delete**

**Pattern:** Logical deletion, not physical

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
```

**Benefits:**
- ? Data recovery possible
- ? Audit trail maintained
- ? Referential integrity preserved

### 2. **Audit Trail**

**Pattern:** Track all changes

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}
```

### 3. **Pagination**

**Pattern:** Efficient data retrieval

```csharp
public class GetAllOrdersQuery
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}
```

---

## ?? ARCHITECTURAL PRINCIPLES

### SOLID Principles

**S - Single Responsibility**
- Each handler does ONE thing
- Each repository manages ONE entity
- Each validator validates ONE command

**O - Open/Closed**
- Open for extension (add new handlers)
- Closed for modification (existing code unchanged)

**L - Liskov Substitution**
- Repositories implement interfaces
- Can swap implementations without breaking code

**I - Interface Segregation**
- Small, focused interfaces
- IRepository, IAuditService, ICurrentUserService

**D - Dependency Inversion**
- Depend on abstractions (interfaces)
- High-level modules don't depend on low-level

### DRY (Don't Repeat Yourself)

- BaseEntity for common properties
- GenericRepository for common operations
- Constants for magic strings
- Extension methods for reusable logic

### KISS (Keep It Simple, Stupid)

- Clear folder structure
- Consistent naming conventions
- Simple, readable code

---

## ?? CLEAN ARCHITECTURE VS N-TIER

### Traditional N-Tier Architecture

```
Presentation Layer (UI)
      ?
Business Logic Layer (BLL)
      ?
Data Access Layer (DAL)
      ?
Database
```

**Problems with N-Tier:**
- ? Tight coupling
- ? Database-centric
- ? Hard to test
- ? Difficult to change database
- ? Business logic mixed with data access

### Clean Architecture (This Project)

```
        API (Presentation)
              ?
     Application (Use Cases)
              ?
        Domain (Core)
              ?
     Infrastructure (Data)
```

**Advantages:**
- ? **Independence:** Domain has no dependencies
- ? **Testability:** Can test without database
- ? **Flexibility:** Easy to change UI or database
- ? **Maintainability:** Clear separation of concerns
- ? **Scalability:** Each layer scales independently

### Key Differences

| Aspect | N-Tier | Clean Architecture |
|--------|--------|-------------------|
| **Coupling** | Tight | Loose |
| **Testability** | Hard | Easy |
| **Database** | Center | Outer layer |
| **Business Logic** | Scattered | Domain-centric |
| **Dependencies** | Top-down | Dependency Inversion |
| **Flexibility** | Low | High |
| **Complexity** | Simple | More complex |

---

## ?? SERVICE REGISTRATION (DI Container)

### Where Services Are Registered

**Location:** `Program.cs` in API project

```csharp
// src/API/MobileBackend.API/Program.cs

var builder = WebApplication.CreateBuilder(args);

// Application services (from Application layer)
builder.Services.AddApplicationServices();

// Infrastructure services (from Infrastructure layer)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Framework services (from Framework layer)
builder.Services.AddFrameworkServices(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

### Extension Methods (Best Practice)

**Application/DependencyInjection.cs:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
```

**Infrastructure/DependencyInjection.cs:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        // ... more repositories
        
        // Services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }
}
```

**Why This Approach:**
- ? **Organized:** Each layer registers its own services
- ? **Maintainable:** Easy to find registrations
- ? **Testable:** Can register test services easily
- ? **Reusable:** Extension methods can be used elsewhere

---

## ?? QUICK REFERENCE

### Project Structure
```
ScanPet/
??? src/
?   ??? Domain/              # Entities, Enums
?   ??? Application/         # Commands, Queries, Handlers
?   ??? Infrastructure/      # DbContext, Repositories
?   ??? Framework/           # JWT, Security
?   ??? API/                 # Controllers, Middleware
??? tests/
    ??? UnitTests/           # Unit tests
```

### Adding a New Feature

1. **Domain:** Create entity
2. **Application:** Create command/query, handler, validator, DTO
3. **Infrastructure:** Create repository, add to UnitOfWork
4. **API:** Create controller endpoint

### Request Flow

```
HTTP Request
    ?
Controller
    ?
Mediator (MediatR)
    ?
Validation Pipeline
    ?
Handler (Business Logic)
    ?
Repository (Data Access)
    ?
Database
    ?
Response (DTO)
```

---

## ?? SUMMARY

**Architecture:** Clean Architecture  
**Pattern:** CQRS + Repository + Unit of Work  
**Security:** JWT RS256 + RBAC  
**Database:** PostgreSQL with EF Core  
**Design:** SOLID principles  
**Testing:** Unit + Integration tests ready  

**Key Strengths:**
- ? **Maintainable:** Clear separation of concerns
- ? **Testable:** Domain logic independent
- ? **Scalable:** Each layer scales independently
- ? **Flexible:** Easy to change database or UI
- ? **Professional:** Industry best practices

---

**Status:** ? **ARCHITECTURE DOCUMENTATION COMPLETE**  
**Quality:** Enterprise-Grade  
**Complexity:** Medium-High (worth it!)

---

**END OF ARCHITECTURE GUIDE**
