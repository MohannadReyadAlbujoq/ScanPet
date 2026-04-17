# N-Tier vs Clean Architecture + CQRS — Users & Items CRUD

This document explains how CRUD for the `User` and `Item` entities looks in
two different architectural styles:

1. **Classic N-Tier** — `Controller ? Manager ? Repository ? DbContext`
2. **Clean Architecture + CQRS** (what this project uses) — `Controller ? MediatR Handler ? UnitOfWork/Repository ? DbContext`

It covers file layout, request flow, and real code examples.

---

## 1. Classic N-Tier (Controller ? Manager ? Repository)

### 1.1 Philosophy

- Three **horizontal** layers stacked on top of each other.
- Each layer depends **downwards** on the layer below.
- "Manager" (a.k.a. **Service** / **Business Logic Layer**) holds all business rules.
- "Repository" is a thin data-access wrapper over `DbContext`.
- A single `Manager` class typically contains **all** operations for one entity
  (Create, Read, Update, Delete, plus extra business methods).

### 1.2 File Layout

```
MyApp.sln
??? MyApp.Api/                     ? Presentation
?   ??? Controllers/
?       ??? UsersController.cs
?       ??? ItemsController.cs
?
??? MyApp.Business/                ? Business Logic (Managers)
?   ??? Managers/
?   ?   ??? UserManager.cs
?   ?   ??? ItemManager.cs
?   ??? Dtos/
?       ??? UserDto.cs
?       ??? ItemDto.cs
?
??? MyApp.Data/                    ? Data Access
?   ??? Repositories/
?   ?   ??? UserRepository.cs
?   ?   ??? ItemRepository.cs
?   ??? Entities/
?   ?   ??? User.cs
?   ?   ??? Item.cs
?   ??? AppDbContext.cs
```

### 1.3 Request flow

```
HTTP request
   ?
   ?
UsersController.Create(dto)
   ?
   ?
IUserManager.CreateAsync(dto)          ? validation, hashing, business rules
   ?
   ?
IUserRepository.AddAsync(user)         ? pure persistence
   ?
   ?
AppDbContext.SaveChangesAsync()
```

### 1.4 Code example — `User` CRUD

#### Repository (data access only)

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task<int> SaveAsync();
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?>      GetByIdAsync(Guid id) => _db.Users.FindAsync(id).AsTask();
    public Task<List<User>> GetAllAsync()          => _db.Users.ToListAsync();
    public Task AddAsync(User u)                   => _db.Users.AddAsync(u).AsTask();
    public void Update(User u)                     => _db.Users.Update(u);
    public void Remove(User u)                     => _db.Users.Remove(u);
    public Task<int> SaveAsync()                   => _db.SaveChangesAsync();
}
```

#### Manager (all business rules live here)

```csharp
public interface IUserManager
{
    Task<Guid>      CreateAsync(CreateUserDto dto);
    Task<UserDto?>  GetAsync(Guid id);
    Task<List<UserDto>> GetAllAsync();
    Task UpdateAsync(Guid id, UpdateUserDto dto);
    Task DeleteAsync(Guid id);
}

public class UserManager : IUserManager
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;

    public UserManager(IUserRepository repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto)
    {
        // business rule: username must be unique
        if ((await _repo.GetAllAsync()).Any(u => u.Username == dto.Username))
            throw new InvalidOperationException("Username already taken");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email    = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password)
        };

        await _repo.AddAsync(user);
        await _repo.SaveAsync();
        return user.Id;
    }

    public async Task<UserDto?> GetAsync(Guid id)
    {
        var u = await _repo.GetByIdAsync(id);
        return u == null ? null : new UserDto { Id = u.Id, Username = u.Username, Email = u.Email };
    }

    // ... UpdateAsync / DeleteAsync / GetAllAsync similar
}
```

#### Controller (thin — just routes + shape)

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserManager _users;
    public UsersController(IUserManager users) => _users = users;

    [HttpGet]         public Task<List<UserDto>> GetAll() => _users.GetAllAsync();
    [HttpGet("{id}")] public async Task<IActionResult> Get(Guid id)
    {
        var u = await _users.GetAsync(id);
        return u == null ? NotFound() : Ok(u);
    }
    [HttpPost]        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
                          => CreatedAtAction(nameof(Get), new { id = await _users.CreateAsync(dto) }, null);
    [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
                      { await _users.UpdateAsync(id, dto); return NoContent(); }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id)
                      { await _users.DeleteAsync(id); return NoContent(); }
}
```

#### Same structure for `Item`

```csharp
public class ItemManager : IItemManager
{
    private readonly IItemRepository _items;
    public ItemManager(IItemRepository items) => _items = items;

    public async Task<Guid> CreateAsync(CreateItemDto dto)
    {
        // business rule: SKU must be unique
        if (await _items.ExistsBySkuAsync(dto.Sku))
            throw new InvalidOperationException("Duplicate SKU");

        var item = new Item { Id = Guid.NewGuid(), Name = dto.Name, Sku = dto.Sku, BasePrice = dto.BasePrice };
        await _items.AddAsync(item);
        await _items.SaveAsync();
        return item.Id;
    }
    // ...
}
```

### 1.5 Strengths / Weaknesses

? Simple, familiar, easy to onboard juniors.
? Manager classes grow very large (God classes).
? Hard to add cross-cutting concerns (validation, logging, transactions) without duplication.
? Read and write logic mixed together — hard to optimize reads.
? Business layer depends on an interface defined inside itself; tight coupling of read & write models.

---

## 2. Clean Architecture + CQRS (this project)

### 2.1 Philosophy

- **Dependency Rule**: outer layers depend on inner layers, **never the other way around**.
- **CQRS** (Command Query Responsibility Segregation): every operation is a
  separate class — **Commands** change state, **Queries** only read.
- **MediatR** dispatches each request to its **single** handler.
- Cross-cutting concerns (validation, logging, transactions, audit) are
  implemented once as **Pipeline Behaviors** and run for every command automatically.
- **UnitOfWork + Repositories** wrap EF Core but sit in the Infrastructure layer.

### 2.2 File Layout (actual project structure)

```
MobileBackend.sln
??? src/
?   ??? Domain/                               ? Core business entities (no dependencies)
?   ?   ??? MobileBackend.Domain/
?   ?       ??? Common/BaseEntity.cs
?   ?       ??? Entities/
?   ?           ??? User.cs
?   ?           ??? Item.cs
?   ?           ??? UserDefaultInventory.cs
?   ?
?   ??? Application/                          ? Use cases (depends only on Domain)
?   ?   ??? MobileBackend.Application/
?   ?       ??? DTOs/
?   ?       ?   ??? Users/UserDto.cs
?   ?       ?   ??? Items/ItemDto.cs
?   ?       ??? Features/
?   ?       ?   ??? Users/
?   ?       ?   ?   ??? Commands/
?   ?       ?   ?   ?   ??? CreateUser/
?   ?       ?   ?   ?   ?   ??? CreateUserCommand.cs          ? the request
?   ?       ?   ?   ?   ?   ??? CreateUserCommandHandler.cs   ? the code that runs it
?   ?       ?   ?   ?   ??? SetDefaultInventory/
?   ?       ?   ?   ?   ??? SetDefaultLocations/
?   ?       ?   ?   ?   ??? ToggleUserStatus/
?   ?       ?   ?   ??? Queries/
?   ?       ?   ?       ??? GetAllUsers/
?   ?       ?   ?       ??? GetUserById/
?   ?       ?   ??? Items/
?   ?       ?       ??? Commands/{CreateItem, UpdateItem, DeleteItem}/
?   ?       ?       ??? Queries/{GetAllItems, GetItemById}/
?   ?       ??? Interfaces/
?   ?       ?   ??? IUnitOfWork.cs
?   ?       ?   ??? IUserRepository.cs
?   ?       ?   ??? IItemRepository.cs
?   ?       ??? Common/Handlers/             ? reusable base handlers
?   ?       ?   ??? BaseCreateHandler.cs
?   ?       ?   ??? BaseUpdateHandler.cs
?   ?       ??? Validators/                  ? FluentValidation
?   ?
?   ??? Infrastructure/                       ? EF Core, external services
?   ?   ??? MobileBackend.Infrastructure/
?   ?       ??? Data/
?   ?       ?   ??? ApplicationDbContext.cs
?   ?       ?   ??? Configurations/UserConfiguration.cs
?   ?       ??? Repositories/
?   ?           ??? UserRepository.cs
?   ?           ??? ItemRepository.cs
?   ?
?   ??? API/                                  ? Thin HTTP layer
?       ??? MobileBackend.API/
?           ??? Controllers/
?               ??? UsersController.cs
?               ??? ItemsController.cs
?
??? tests/
    ??? MobileBackend.UnitTests/
```

### 2.3 Request flow

```
HTTP request
   ?
   ?
UsersController.Create(UserDto dto)
   ?
   ?
Mediator.Send(new CreateUserCommand { ... })
   ?
   ?  (MediatR pipeline)
ValidationBehavior    ? FluentValidation runs
LoggingBehavior       ? structured logs
TransactionBehavior   ? wraps in DB transaction
   ?
   ?
CreateUserCommandHandler (inherits BaseCreateHandler<CreateUserCommand, User>)
   ?
   ?
IUnitOfWork.Users.AddAsync(user)  ?  _context.Users.Add(user)
IUnitOfWork.SaveChangesAsync()
   ?
   ?
Result<Guid>.SuccessResult(user.Id, 201)
```

### 2.4 Code example — `User` Create (real code from this project)

#### 1) Command (the request DTO)

`src/Application/.../Features/Users/Commands/CreateUser/CreateUserCommand.cs`

```csharp
public class CreateUserCommand : IRequest<Result<Guid>>
{
    public string Username { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<Guid>? DefaultInventoryIds { get; set; }
    public List<Guid>? DefaultLocationIds  { get; set; }
}
```

#### 2) Handler (one class = one use case)

`src/Application/.../Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`

```csharp
public class CreateUserCommandHandler : BaseCreateHandler<CreateUserCommand, User>
{
    private readonly IPasswordService _passwordService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateUserCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
        _passwordService = passwordService;
    }

    protected override async Task<User> CreateEntityAsync(
        CreateUserCommand command, CancellationToken ct)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username     = command.Username,
            Email        = command.Email,
            PasswordHash = _passwordService.HashPassword(command.Password),
            FullName     = command.FullName,
            PhoneNumber  = command.PhoneNumber
        };

        foreach (var invId in command.DefaultInventoryIds ?? [])
            user.DefaultInventories.Add(new UserDefaultInventory { UserId = user.Id, InventoryId = invId });

        foreach (var locId in command.DefaultLocationIds ?? [])
            user.DefaultLocations.Add(new UserDefaultLocation { UserId = user.Id, LocationId = locId });

        return user;
    }

    protected override Task AddEntityAsync(User e, CancellationToken ct)
        => UnitOfWork.Users.AddAsync(e, ct);

    protected override string GetEntityName()   => EntityNames.User;
    protected override string GetAuditAction()  => AuditActions.UserCreated;
    protected override string GetAuditMessage(User u) => $"User created: {u.Username}";

    // Custom uniqueness validation (username + email)
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateUserCommand cmd, CancellationToken ct)
    {
        if (await UnitOfWork.Users.GetByUsernameAsync(cmd.Username, ct) != null)
            return Result<Guid>.FailureResult("Username already exists", 400);
        if (await UnitOfWork.Users.GetByEmailAsync(cmd.Email, ct) != null)
            return Result<Guid>.FailureResult("Email already exists", 400);
        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
```

#### 3) Validator (FluentValidation — runs automatically via pipeline)

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

#### 4) Controller (truly thin — just shapes HTTP ? Command)

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] UserDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Username) ||
        string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Password))
        return BadRequestResponse("Username, Email, and Password are required");

    var command = new CreateUserCommand
    {
        Username            = dto.Username,
        Email               = dto.Email,
        Password            = dto.Password,
        FullName            = dto.FullName,
        PhoneNumber         = dto.PhoneNumber,
        DefaultInventoryIds = dto.DefaultInventoryIds,
        DefaultLocationIds  = dto.DefaultLocationIds
    };

    var result = await Mediator.Send(command);
    return result.Success ? CreatedResponse(result.Data, "User") : ErrorResponse(result);
}
```

#### 5) Query side — `Get User By Id`

```csharp
public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}

public class GetUserByIdQueryHandler : BaseGetByIdHandler<GetUserByIdQuery, User, UserDto>
{
    private readonly IUnitOfWork _uow;
    public GetUserByIdQueryHandler(IUnitOfWork uow, ILogger<GetUserByIdQueryHandler> log) : base(log)
        => _uow = uow;

    protected override Task<User?> GetEntityByIdAsync(Guid id, CancellationToken ct)
        => _uow.Users.GetByIdWithRolesAsync(id, ct);

    protected override UserDto MapToDto(User e) => new()
    {
        Id                    = e.Id,
        Username              = e.Username,
        Email                 = e.Email,
        DefaultInventoryIds   = e.DefaultInventories.Select(di => di.InventoryId).ToList(),
        DefaultInventoryNames = e.DefaultInventories.Select(di => di.Inventory?.Name ?? "").ToList(),
        DefaultLocationIds    = e.DefaultLocations .Select(dl => dl.LocationId).ToList(),
        DefaultLocationNames  = e.DefaultLocations .Select(dl => dl.Location?.Name ?? "").ToList(),
        Roles                 = e.UserRoles.Select(ur => ur.Role.Name).ToList(),
        CreatedAt             = e.CreatedAt,
        UpdatedAt             = e.UpdatedAt
    };

    protected override string GetEntityName() => EntityNames.User;
}
```

### 2.5 Code example — `Item` Create (same pattern)

```csharp
// Command
public class CreateItemCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal BasePrice { get; set; }
    public Guid? ColorId { get; set; }
    public IFormFile? Image { get; set; }
}

// Handler
public class CreateItemCommandHandler : BaseCreateHandler<CreateItemCommand, Item>
{
    private readonly IFileService _files;
    public CreateItemCommandHandler(IUnitOfWork uow, IFileService files, /* ...base deps... */)
        : base(/* ... */) { _files = files; }

    protected override async Task<Item> CreateEntityAsync(CreateItemCommand cmd, CancellationToken ct)
    {
        string? imageUrl = cmd.Image != null
            ? await _files.UploadAsync(cmd.Image, "items", ct)
            : null;

        return new Item
        {
            Id = Guid.NewGuid(),
            Name = cmd.Name,
            SKU = cmd.SKU,
            BasePrice = cmd.BasePrice,
            ColorId = cmd.ColorId,
            ImageUrl = imageUrl
        };
    }

    protected override Task AddEntityAsync(Item e, CancellationToken ct)
        => UnitOfWork.Items.AddAsync(e, ct);

    protected override async Task<Result<Guid>> ValidateUniquenessAsync(CreateItemCommand cmd, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(cmd.SKU) && await UnitOfWork.Items.ExistsBySkuAsync(cmd.SKU, ct))
            return Result<Guid>.FailureResult("SKU already exists", 409);
        return Result<Guid>.SuccessResult(Guid.Empty);
    }

    protected override string GetEntityName()  => EntityNames.Item;
    protected override string GetAuditAction() => AuditActions.ItemCreated;
    protected override string GetAuditMessage(Item i) => $"Item created: {i.Name} ({i.SKU})";
}
```

### 2.6 Strengths / Weaknesses

? Each use case is a **single, small, testable class** ? easy to reason about.
? **Read** and **write** models are separate ? free to optimize reads (projection DTOs, caching).
? Cross-cutting concerns live in **one place** (pipeline behaviors) ? no duplication.
? Domain is **framework-free** ? reusable, testable, portable.
? Controllers are truly dumb — no business rules leak to the API layer.
? More files per operation (Command + Handler + Validator vs. a method in a Manager).
? Steeper learning curve — devs must understand MediatR, pipeline, dependency rule.
? Can feel ceremonial for tiny apps.

---

## 3. Side-by-side comparison

| Concern                       | N-Tier                                      | Clean Arch + CQRS (this project)                     |
| ----------------------------- | ------------------------------------------- | ---------------------------------------------------- |
| Business logic location       | `UserManager.cs` (big class)                | `CreateUserCommandHandler.cs` (one per use case)     |
| Read vs write                 | Mixed in the same Manager                   | Separate — `Commands/` and `Queries/`                |
| Data access                   | `IUserRepository`                           | `IUnitOfWork.Users` (+ `IUserRepository`)            |
| Validation                    | Manual `if` checks in Manager               | FluentValidation + pipeline behavior                 |
| Transactions                  | Call `SaveAsync` manually                   | `TransactionBehavior` wraps every command            |
| Logging / audit               | Added by each Manager                       | Pipeline behaviors + `IAuditService`                 |
| Controller size               | Medium (calls Manager)                      | Tiny (maps DTO ? Command, sends through Mediator)    |
| Dependency direction          | Controller ? Business ? Data (downward)     | All outer layers ? Application ? Domain (inward)     |
| Testability                   | Test Manager with repo mock                 | Test each Handler in isolation                       |
| Scaling team                  | Merge conflicts on big Manager              | Each feature isolated in its own folder              |
| Best suited for               | Small/medium CRUD apps                      | Medium/large domain-rich apps, long-lived codebases  |

---

## 4. Mapping the current project to the diagram

| Layer          | Project                                     | Example file for `User`                                                                      |
| -------------- | ------------------------------------------- | -------------------------------------------------------------------------------------------- |
| Domain         | `MobileBackend.Domain`                      | `Entities/User.cs`, `Entities/UserDefaultInventory.cs`                                       |
| Application    | `MobileBackend.Application`                 | `Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`                             |
| Infrastructure | `MobileBackend.Infrastructure`              | `Repositories/UserRepository.cs`, `Data/ApplicationDbContext.cs`                             |
| Presentation   | `MobileBackend.API`                         | `Controllers/UsersController.cs`                                                             |

The arrow of dependencies is **only inward**:
`API ? Infrastructure ? Application ? Domain`.
`Domain` has no references to EF Core, ASP.NET, or any other framework.

---

## 5. When to pick which

- **Pick N-Tier** when: the app is small, the team is small, business rules are
  simple, and you want to ship fast with the least ceremony.
- **Pick Clean Arch + CQRS** when: the app will live for years, has many
  features, needs a clear separation between reads and writes, benefits from
  cross-cutting behaviors (validation, logging, transactions, audit), and
  you want to keep the domain framework-free and easy to test.
