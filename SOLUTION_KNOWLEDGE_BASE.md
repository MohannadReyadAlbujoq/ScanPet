# ?? ScanPet Mobile Backend — AI Knowledge Base

> **Purpose**: This file is a comprehensive reference for AI assistants working on this codebase.  
> **Last Updated**: Auto-generated from full solution analysis.  
> **Repository**: `https://github.com/mohannadalbujoq-cyber/ScanPet` — branch `main`

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Architecture & Clean Architecture Layers](#2-architecture--clean-architecture-layers)
3. [Project Dependency Graph](#3-project-dependency-graph)
4. [Domain Model — Entities & Relationships](#4-domain-model--entities--relationships)
5. [Enums](#5-enums)
6. [DTOs (Data Transfer Objects)](#6-dtos-data-transfer-objects)
7. [API Endpoints & JSON Contracts](#7-api-endpoints--json-contracts)
8. [CQRS Feature Map](#8-cqrs-feature-map)
9. [Interfaces & Abstractions](#9-interfaces--abstractions)
10. [Middleware Pipeline](#10-middleware-pipeline)
11. [Security & Permission System](#11-security--permission-system)
12. [Database & Infrastructure](#12-database--infrastructure)
13. [Seeded Data & Default Credentials](#13-seeded-data--default-credentials)
14. [Cross-Cutting Concerns](#14-cross-cutting-concerns)
15. [Unit Tests](#15-unit-tests)
16. [NuGet Packages & Frameworks](#16-nuget-packages--frameworks)
17. [File Hierarchy (Complete)](#17-file-hierarchy-complete)
18. [Potential Refactoring & Improvement Notes](#18-potential-refactoring--improvement-notes)

---

## 1. Solution Overview

**ScanPet** is a **.NET 8 Web API** backend for a mobile pet-store management application. It provides:

- **User Management** with registration, approval workflow, role-based access
- **Authentication** via RSA-signed JWT tokens with refresh token rotation
- **Role & Permission System** using bitwise bitmask operations (O(1) permission checks)
- **Product Catalog** — Items with Colors, SKU, pricing
- **Inventory/Warehouse Management** — multi-warehouse stock tracking, transfers, adjustments
- **Order Management** — order lifecycle (Pending ? Confirmed/Cancelled), serial numbers, refunds
- **Location Management** — delivery/shipping locations (separate from warehouses)
- **Audit Logging** — full action tracking for compliance
- **Soft Delete** everywhere via `ISoftDelete` interface + global EF query filters

**Database**: PostgreSQL (via Npgsql)  
**ORM**: Entity Framework Core 9  
**CQRS**: MediatR 13  
**Validation**: FluentValidation 12  
**Mapping**: AutoMapper 12  
**Logging**: NLog  
**Auth**: JWT Bearer (RSA 2048-bit)  
**Target**: .NET 8.0, deployable on Railway

---

## 2. Architecture & Clean Architecture Layers

```
???????????????????????????????????????????????????????
?                    API Layer                         ?
?  (Controllers, Middleware, Filters, Program.cs)      ?
?  MobileBackend.API                                  ?
???????????????????????????????????????????????????????
?               Application Layer                      ?
?  (CQRS Features, DTOs, Validators, Interfaces,      ?
?   Behaviors, Common Handlers)                        ?
?  MobileBackend.Application                          ?
???????????????????????????????????????????????????????
?  Infrastructure      ?      Framework Layer          ?
?  (EF DbContext,      ?  (JWT Service, Password       ?
?   Repositories,      ?   Service, BitManipulation)   ?
?   Migrations,        ?  MobileBackend.Framework      ?
?   UnitOfWork)        ?                               ?
?  MobileBackend.      ?                               ?
?  Infrastructure      ?                               ?
???????????????????????????????????????????????????????
?                   Domain Layer                       ?
?  (Entities, Enums, Base classes, ISoftDelete)         ?
?  MobileBackend.Domain                               ?
???????????????????????????????????????????????????????
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `MobileBackend.Domain` | Entities, Enums, `BaseEntity`, `ISoftDelete`. Zero dependencies. |
| **Application** | `MobileBackend.Application` | CQRS commands/queries/handlers, DTOs, validators, interfaces (repository contracts), pipeline behaviors, base handler templates. Depends on Domain. |
| **Infrastructure** | `MobileBackend.Infrastructure` | EF Core `ApplicationDbContext`, repositories, `UnitOfWork`, `DbSeeder`, entity configurations, migrations, service wrappers. Depends on Domain + Application + Framework. |
| **Framework** | `MobileBackend.Framework` | Security services (JWT generation/validation, password hashing via BCrypt, bit manipulation/encryption). Depends on Domain only. |
| **API** | `MobileBackend.API` | ASP.NET Core controllers, middleware (JWT, Audit, Exception, Logging), filters (`RequirePermission`, `ValidateModel`, `ResponseCache`), `Program.cs` composition root, Swagger, health checks. Depends on Application + Infrastructure + Framework. |
| **Tests** | `MobileBackend.UnitTests` | Unit tests using xUnit, Moq, FluentAssertions. |

---

## 3. Project Dependency Graph

```
MobileBackend.API
??? MobileBackend.Application
?   ??? MobileBackend.Domain
??? MobileBackend.Infrastructure
?   ??? MobileBackend.Domain
?   ??? MobileBackend.Application
?   ??? MobileBackend.Framework
?       ??? MobileBackend.Domain
??? MobileBackend.Framework
    ??? MobileBackend.Domain
```

### Dependency Injection Registration

Each layer exposes an `AddXxx()` extension method in `DependencyInjection.cs`:

```csharp
// Program.cs
builder.Services.AddApplication();                        // MediatR, FluentValidation, AutoMapper, Behaviors
builder.Services.AddInfrastructure(builder.Configuration); // EF Core, Repositories, UnitOfWork, Services
builder.Services.AddFramework(builder.Configuration);      // JWT, Password, BitManipulation
```

---

## 4. Domain Model — Entities & Relationships

### Entity Class Diagram

```
BaseEntity (abstract)
??? Id: Guid (auto-generated)
??? CreatedAt: DateTime
??? CreatedBy: Guid?
??? UpdatedAt: DateTime?
??? UpdatedBy: Guid?

ISoftDelete (interface)
??? IsDeleted: bool
??? DeletedAt: DateTime?
??? DeletedBy: Guid?
```

### All Entities

#### `User` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Username | string | Unique |
| Email | string | Unique |
| PasswordHash | string | BCrypt hash |
| PhoneNumber | string? | |
| FullName | string? | |
| IsEnabled | bool | Default: false |
| IsApproved | bool | Default: false (needs admin approval) |
| **Nav**: UserRoles | `ICollection<UserRole>` | |
| **Nav**: RefreshTokens | `ICollection<RefreshToken>` | |

#### `Role` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Admin", "Manager", "User" |
| Description | string? | |
| **Nav**: UserRoles | `ICollection<UserRole>` | |
| **Nav**: RolePermissions | `ICollection<RolePermission>` | |

#### `UserRole`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK ? User |
| RoleId | Guid | FK ? Role |
| AssignedAt | DateTime | |
| AssignedBy | Guid? | |
| IsDeleted | bool | |

#### `Permission`
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Color Create" |
| Description | string? | |
| PermissionBit | long | Power of 2 value |
| Category | string? | e.g., "Color", "Item", "Order" |
| IsDeleted | bool | |

#### `RolePermission`
| Property | Type | Notes |
|----------|------|-------|
| RoleId | Guid | FK ? Role |
| PermissionsBitmask | long | Bitwise OR of all permission bits |
| IsDeleted | bool | |
| **Nav**: Role | `Role` | |

#### `Color` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| RedValue | int | 0–255 |
| GreenValue | int | 0–255 |
| BlueValue | int | 0–255 |
| Description | string? | |
| HexCode | string | Computed: `#RRGGBB` |
| **Nav**: Items | `ICollection<Item>` | |

#### `Item` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| Description | string? | |
| SKU | string? | |
| BasePrice | decimal | |
| ColorId | Guid? | FK ? Color |
| ImageUrl | string? | |
| **Nav**: Color | `Color?` | |
| **Nav**: OrderItems | `ICollection<OrderItem>` | |
| **Nav**: ItemInventories | `ICollection<ItemInventory>` | Multi-warehouse quantities |
| **Method**: GetTotalQuantity() | int | Sum across all inventories |
| **Method**: GetQuantityAtInventory(Guid) | int | Quantity at specific warehouse |

> **Note**: `Quantity` field has been **removed** from `Item` entity. All quantity tracking is now exclusively through `ItemInventory` records (one per item per warehouse).

#### `Inventory` (ISoftDelete) — Represents a Warehouse
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Warehouse name |
| Location | string? | Physical address |
| Description | string? | |
| IsActive | bool | Default: true |
| **Nav**: ItemInventories | `ICollection<ItemInventory>` | |

#### `ItemInventory` (ISoftDelete) — Junction: Item ? Inventory
| Property | Type | Notes |
|----------|------|-------|
| ItemId | Guid | FK ? Item |
| InventoryId | Guid | FK ? Inventory |
| Quantity | int | Stock at this warehouse |
| MinimumQuantity | int? | Low stock threshold |
| MaximumQuantity | int? | Max capacity |
| Notes | string? | |
| **Nav**: Item | `Item` | |
| **Nav**: Inventory | `Inventory` | |

#### `Location` (ISoftDelete) — Delivery/Shipping Location
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| Address | string? | |
| City | string? | |
| Country | string? | |
| PostalCode | string? | |
| IsActive | bool | |
| **Nav**: Orders | `ICollection<Order>` | |

#### `Order` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| OrderNumber | string | Auto-generated: `ORD-YYYYMMDD-####` |
| ClientName | string | |
| ClientPhone | string | |
| LocationId | Guid | FK ? Location |
| Description | string? | |
| TotalAmount | decimal | Computed from items |
| OrderStatus | OrderStatus | Pending/Confirmed/Cancelled |
| OrderDate | DateTime | |
| **Nav**: Location | `Location` | |
| **Nav**: OrderItems | `ICollection<OrderItem>` | |

#### `OrderItem` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| OrderId | Guid | FK ? Order |
| ItemId | Guid | FK ? Item |
| SerialNumber | string | Uses Item SKU (e.g., `PF-001`) |
| Quantity | int | |
| SalePrice | decimal | Price at time of order |
| Status | OrderItemStatus | Successful/Refunded |
| RefundedQuantity | int | |
| RefundedAt | DateTime? | |
| RefundedBy | Guid? | |
| RefundReason | string? | |
| RefundedToInventoryId | Guid? | FK ? Inventory (refund target) |
| **Nav**: Order | `Order` | |
| **Nav**: Item | `Item` | |
| **Nav**: RefundedToInventory | `Inventory?` | |

#### `RefreshToken`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK ? User |
| Token | string | |
| DeviceInfo | string? | |
| IpAddress | string? | |
| ExpiresAt | DateTime | |
| RevokedAt | DateTime? | |
| IsRevoked | bool | |
| **Computed**: IsExpired | bool | |
| **Computed**: IsActive | bool | !IsRevoked && !IsExpired |

#### `AuditLog`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid? | FK ? User |
| Action | string | Create, Update, Delete, Login, etc. |
| EntityName | string | User, Order, Item, etc. |
| EntityId | Guid? | |
| OldValues | string? | JSON |
| NewValues | string? | JSON |
| IpAddress | string? | |
| UserAgent | string? | |
| Timestamp | DateTime | |
| AdditionalInfo | string? | |

### Entity Relationship Diagram (Simplified)

```
User ??< UserRole >?? Role ??< RolePermission
  ?                              
  ???< RefreshToken             

Permission (standalone, referenced by PermissionBit in RolePermission.PermissionsBitmask)

Color ??< Item ??< ItemInventory >?? Inventory (Warehouse)
                ?
                ???< OrderItem >?? Order >?? Location

OrderItem.RefundedToInventoryId ??> Inventory (optional)
```

---

## 5. Enums

### `OrderStatus`
```csharp
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}
```

### `OrderItemStatus`
```csharp
public enum OrderItemStatus
{
    Successful = 0,
    Refunded = 1
}
```

### `PermissionType : long` (Bitwise Flags)

| Permission | Bit | Value | Category |
|-----------|-----|-------|----------|
| ColorCreate | 0 | 1 | Color |
| ColorEdit | 1 | 2 | Color |
| ColorDelete | 2 | 4 | Color |
| ColorView | 3 | 8 | Color |
| ItemCreate | 4 | 16 | Item |
| ItemEdit | 5 | 32 | Item |
| ItemDelete | 6 | 64 | Item |
| ItemView | 7 | 128 | Item |
| OrderCreate | 8 | 256 | Order |
| OrderView | 9 | 512 | Order |
| OrderEdit | 10 | 1024 | Order |
| OrderConfirm | 11 | 2048 | Order |
| OrderCancel | 12 | 4096 | Order |
| RefundProcess | 13 | 8192 | Refund |
| RefundView | 14 | 16384 | Refund |
| UserView | 15 | 32768 | User |
| UserCreate | 16 | 65536 | User |
| UserEdit | 17 | 131072 | User |
| UserDelete | 18 | 262144 | User |
| UserApprove | 19 | 524288 | User |
| UserEnable | 20 | 1048576 | User |
| UserDisable | 21 | 2097152 | User |
| UserResetPassword | 22 | 4194304 | User |
| LocationCreate | 23 | 8388608 | Location |
| LocationEdit | 24 | 16777216 | Location |
| LocationDelete | 25 | 33554432 | Location |
| LocationView | 26 | 67108864 | Location |
| RoleCreate | 27 | 134217728 | Role |
| RoleEdit | 28 | 268435456 | Role |
| RoleDelete | 29 | 536870912 | Role |
| RoleView | 30 | 1073741824 | Role |
| PermissionManage | 31 | 2147483648 | Role |
| AuditLogView | 32 | 4294967296 | System |
| AuditLogExport | 33 | 8589934592 | System |
| SystemSettings | 34 | 17179869184 | System |
| SystemBackup | 35 | 34359738368 | System |
| SystemRestore | 36 | 68719476736 | System |

**Helper**: `PermissionHelper` class provides static methods:
- `HasPermission(bitmask, permission)` ? bool
- `HasAnyPermission(bitmask, params permissions)` ? bool
- `HasAllPermissions(bitmask, params permissions)` ? bool
- `AddPermission(bitmask, permission)` ? long
- `RemovePermission(bitmask, permission)` ? long
- `CombinePermissions(params permissions)` ? long
- `GetPermissions(bitmask)` ? `List<PermissionType>`

---

## 6. DTOs (Data Transfer Objects)

All DTOs use **unified DTO pattern** — same class for request/response with nullable properties. Context determines which fields are required.

### Auth DTOs
| DTO | Purpose |
|-----|---------|
| `RegisterRequestDto` | `Username`, `Email`, `Password`, `ConfirmPassword`, `FullName`, `PhoneNumber?` |
| `LoginRequestDto` | `UsernameOrEmail`, `Password`, `DeviceInfo?`, `IpAddress?` |
| `LoginResponseDto` | `AccessToken`, `RefreshToken`, `ExpiresAt`, `User` (? `UserInfoDto`) |
| `UserInfoDto` | `Id`, `Username`, `Email`, `FullName`, `IsEnabled`, `IsApproved`, `Roles[]`, `PermissionsBitmask` |
| `RefreshTokenRequestDto` | `RefreshToken`, `DeviceInfo?` |
| `ChangePasswordDto` | *(exists but not exposed in controller currently)* |

### Entity DTOs
| DTO | Key Fields |
|-----|------------|
| `ColorDto` | `Id?`, `Name?`, `Description?`, `RedValue?`, `GreenValue?`, `BlueValue?`, `HexCode?`, `ItemCount?`, `CreatedAt?`, `UpdatedAt?` |
| `ItemDto` | `Id?`, `Name?`, `Description?`, `SKU?`, `BasePrice?`, `ColorId?`, `ColorName?`, `ImageUrl?`, `CreatedAt?`, `UpdatedAt?` |
| `LocationDto` | `Id?`, `Name?`, `Address?`, `City?`, `Country?`, `PostalCode?`, `IsActive?`, `OrderCount?`, `SectionCount?`, `CreatedAt?`, `UpdatedAt?` |
| `OrderDto` | `Id?`, `OrderNumber?`, `ClientName?`, `ClientPhone?`, `LocationId?`, `LocationName?`, `Description?`, `TotalAmount?`, `OrderStatus?`, `OrderStatusName?`, `OrderDate?`, `OrderItems[]?`, `CreatedAt?`, `UpdatedAt?` |
| `OrderItemDto` | `Id?`, `OrderId?`, `ItemId?`, `ItemName?`, `Quantity?`, `UnitPrice?`, `TotalPrice?`, `Status?`, `StatusName?`, `SerialNumber?`, `CreatedAt?` |
| `RoleDto` | `Id?`, `Name?`, `Description?`, `PermissionsBitmask?`, `Permissions[]?`, `UserCount?`, `CreatedAt?`, `UpdatedAt?` |
| `UserDto` | `Id?`, `Username?`, `Email?`, `Password?`, `FullName?`, `PhoneNumber?`, `IsEnabled?`, `IsApproved?`, `Roles[]?`, `CreatedAt?`, `UpdatedAt?` |

### Operation-Specific DTOs
| DTO | Fields |
|-----|--------|
| `AssignPermissionsDto` | `RoleId`, `Permissions: List<PermissionType>` |
| `UserApprovalDto` | `UserId`, `IsApproved`, `IsEnabled` |
| `UpdateUserRoleDto` | `RoleId` |
| `ToggleUserStatusDto` | `IsEnabled` |
| `CreateInventoryDto` | `Name`, `Location?`, `Description?`, `IsActive` |
| `UpdateInventoryDto` | `Name`, `Location?`, `Description?`, `IsActive` |
| `InventoryDto` | `Id`, `Name`, `Location?`, `Description?`, `IsActive`, `TotalItems`, `CreatedAt`, `UpdatedAt?` |
| `ItemInventoryDto` | `Id`, `ItemId`, `ItemName`, `ItemSKU?`, `InventoryId`, `InventoryName`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?`, `IsLowStock`, `CreatedAt`, `UpdatedAt?` |
| `SetItemInventoryDto` | `ItemId`, `InventoryId`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?` |
| `UpdateItemQuantityDto` | `ItemId`, `InventoryId`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?` |
| `IncrementInventoryDto` | `ItemId`, `InventoryId`, `Quantity` (default: 1), `Notes?`, `Reason?` |
| `DecrementInventoryDto` | `ItemId`, `InventoryId`, `Quantity` (default: 1), `Notes?`, `Reason?` |
| `AdjustInventoryDto` | `ItemId`, `InventoryId`, `QuantityChange` (+/-), `Notes?`, `Reason?` |
| `TransferInventoryDto` | `ItemId`, `FromInventoryId`, `ToInventoryId`, `Quantity`, `Notes?`, `Reason?` |

### Common DTOs
| DTO | Purpose |
|-----|---------|
| `Result<T>` | Generic wrapper: `Success`, `Data`, `ErrorMessage`, `ValidationErrors`, `StatusCode`, `Timestamp` |
| `Result` | Non-generic (no data) variant |
| `PagedResult<T>` | `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage` |
| `ValidationError` | Individual validation error details |

---

## 7. API Endpoints & JSON Contracts

**Base URL**: `/api`  
**Auth**: All endpoints require JWT Bearer token except where noted `[AllowAnonymous]`.  
**Response Envelope**: All responses follow:

```json
{
  "success": true|false,
  "message": "string",
  "data": { ... } | null,
  "errors": [ ... ] | null
}
```

---

### 7.1 Auth Controller — `POST /api/auth/*`

#### `POST /api/auth/register` [AllowAnonymous]
**Request:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "P@ssw0rd123",
  "confirmPassword": "P@ssw0rd123",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890"
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "User registered successfully. Your account is pending approval by an administrator.",
  "userId": "guid"
}
```

> **?? Note**: If a user with the same username/email already exists but is **not yet activated**, the response will be:
> ```json
> {
>   "success": false,
>   "message": "This username is already registered but the account is not yet activated. Please contact the administrator to activate your account.",
>   "errors": null
> }
> ```
> HTTP Status: **409 Conflict**

#### `POST /api/auth/login` [AllowAnonymous]
**Request:**
```json
{
  "usernameOrEmail": "admin",
  "password": "Admin@123",
  "deviceInfo": "iPhone 15 Pro"
}
```
**Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbG...",
    "refreshToken": "random-refresh-token-string",
    "expiresAt": "2025-01-01T00:15:00Z",
    "user": {
      "id": "guid",
      "username": "admin",
      "email": "admin@scanpet.com",
      "fullName": "System Administrator",
      "isEnabled": true,
      "isApproved": true,
      "roles": ["Admin"],
      "permissionsBitmask": 137438953471
    }
  }
}
```

#### `POST /api/auth/refresh` [AllowAnonymous]
**Request:**
```json
{
  "refreshToken": "existing-refresh-token",
  "deviceInfo": "iPhone 15 Pro"
}
```
**Response (200):** Same as login response.

#### `POST /api/auth/logout` [Authorized]
**Request Body:** `"optional-refresh-token-string"` (or null to revoke all)  
**Response (200):**
```json
{
  "success": true,
  "message": "Logout successful"
}
```

#### `GET /api/auth/me` [Authorized]
**Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "username": "admin",
    "email": "admin@scanpet.com",
    "isAuthenticated": true
  }
}
```

---

### 7.2 Colors Controller — `/api/colors`

#### `GET /api/colors`
Returns all colors.
```json
{
  "success": true,
  "message": "Operation successful",
  "data": [
    {
      "id": "guid",
      "name": "Red",
      "description": "Bright red",
      "redValue": 255,
      "greenValue": 0,
      "blueValue": 0,
      "hexCode": "#FF0000",
      "itemCount": 5,
      "createdAt": "2025-01-01T00:00:00Z",
      "updatedAt": null
    }
  ]
}
```

#### `GET /api/colors/search?searchTerm=red&pageNumber=1&pageSize=10`
Paginated search by name/description.

#### `GET /api/colors/{id}`
Single color by ID.

#### `POST /api/colors`
**Request:**
```json
{
  "name": "Ocean Blue",
  "description": "Deep ocean blue",
  "redValue": 0,
  "greenValue": 105,
  "blueValue": 180
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "Color",
  "data": "guid"
}
```

#### `PUT /api/colors/{id}`
**Request:** Same body as POST.  
**Response (200):** `{ "success": true, "message": "Color updated successfully" }`

#### `DELETE /api/colors/{id}`
**Response (200):** `{ "success": true, "message": "Color deleted successfully" }`

---

### 7.3 Items Controller — `/api/items`

#### `GET /api/items?pageNumber=1&pageSize=5`
**Paginated** — Returns items with DB-level pagination. Supports inventory filtering.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Page number (1-based) |
| `pageSize` | int | 10 | Items per page (1–100) |
| `inventoryId` | Guid? | null | Filter items by inventory/section |

**Response (200):**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Pet Food Premium",
        "description": "High-quality pet food",
        "sku": "PF-001",
        "basePrice": 29.99,
        "colorId": "guid-or-null",
        "colorName": "Red",
        "imageUrl": null,
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 10,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

#### `GET /api/items/{id}`
Single item by ID.

#### `POST /api/items`
**Request:**
```json
{
  "name": "Pet Food Premium",
  "description": "High-quality pet food",
  "sku": "PF-001",
  "basePrice": 29.99,
  "colorId": "guid-or-null",
  "imageUrl": "https://..."
}
```

#### `PUT /api/items/{id}`
Same body as POST. ? **Now working correctly.**

#### `DELETE /api/items/{id}`
Soft deletes the item. ? **Now working correctly.**

---

### 7.4 Locations Controller — `/api/locations`

#### `GET /api/locations`
Returns all locations with order counts **and section counts**.
```json
{
  "success": true,
  "message": "Operation successful",
  "data": [
    {
      "id": "guid",
      "name": "Main Store",
      "address": "123 Pet Street",
      "city": "Amman",
      "country": "Jordan",
      "postalCode": "11183",
      "isActive": true,
      "orderCount": 5,
      "sectionCount": 2,
      "sections": null,
      "createdAt": "2025-01-01T00:00:00Z",
      "updatedAt": null
    }
  ]
}
```

#### `GET /api/locations/{id}`
Returns location details **with its inventory sections** (warehouses within it).
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": "guid",
    "name": "Main Store",
    "address": "123 Pet Street",
    "city": "Amman",
    "country": "Jordan",
    "postalCode": "11183",
    "isActive": true,
    "orderCount": 5,
    "sectionCount": 2,
    "sections": [
      {
        "id": "guid",
        "name": "Section A - Dry Food",
        "location": "Shelf A1-A5",
        "description": "Dry pet food section",
        "isActive": true,
        "totalItems": 15,
        "locationId": "parent-location-guid",
        "locationName": "Main Store",
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": null
  }
}
```

#### `POST /api/locations`
**Request:**
```json
{
  "name": "Main Store",
  "address": "123 Pet Street",
  "city": "Amman",
  "country": "Jordan",
  "postalCode": "11183",
  "isActive": true
}
```
#### `PUT /api/locations/{id}`
#### `DELETE /api/locations/{id}`

---

### 7.5 Orders Controller — `/api/orders`

#### `GET /api/orders?pageNumber=1&pageSize=10`
#### `GET /api/orders/{id}`
Returns order with all order items.

#### `POST /api/orders`
**Request:**
```json
{
  "clientName": "Ahmad Ali",
  "clientPhone": "+962791234567",
  "locationId": "guid",
  "description": "Rush order",
  "orderItems": [
    {
      "itemId": "guid",
      "quantity": 2,
      "unitPrice": 29.99
    },
    {
      "itemId": "guid",
      "quantity": 1,
      "unitPrice": 15.99
    }
  ]
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "Order",
  "data": "order-guid"
}
```

#### `PUT /api/orders/{id}/confirm`
**Response:** `{ "success": true, "message": "Order confirmed successfully" }`

#### `PUT /api/orders/{id}/cancel?reason=Out of stock`
**Response:** `{ "success": true, "message": "Order cancelled successfully" }`

#### `POST /api/orders/refund/{sku}`
**Request:**
```json
{
  "refundQuantity": 1,
  "refundReason": "Defective product",
  "refundToInventoryId": "warehouse-guid"
}
```
**Response:** `{ "success": true, "message": "Order item refunded successfully" }`

> **Note**: The `{sku}` parameter is the Item SKU (e.g., `PF-001`). The system finds the most recent unrefunded order item matching that SKU.

---

### 7.6 Inventories Controller — `/api/inventories`

> **Transaction Isolation**: All inventory-modifying commands use **Serializable** isolation level via `TransactionBehavior` to ensure full ACID compliance. This prevents dirty reads, non-repeatable reads, phantom reads, and lost updates during concurrent quantity changes.

#### `POST /api/inventories` — Create warehouse
```json
{
  "name": "Main Warehouse",
  "location": "Industrial Zone, Amman",
  "description": "Primary storage facility",
  "isActive": true
}
```

#### `GET /api/inventories` — All warehouses
#### `GET /api/inventories/{id}` — Single warehouse with items
#### `PUT /api/inventories/{id}` — Update warehouse
#### `DELETE /api/inventories/{id}` — Soft delete warehouse
#### `GET /api/inventories/{inventoryId}/items` — Items in warehouse
#### `GET /api/inventories/items/{itemId}` — Item stock across all warehouses
#### `GET /api/inventories/active` — Active warehouses only
#### `GET /api/inventories/low-stock` — Low stock items

#### `POST /api/inventories/items` — Set initial item inventory (create or set)
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 100,
  "minimumQuantity": 10,
  "maximumQuantity": 500,
  "notes": "Initial stock"
}
```

#### `PUT /api/inventories/items` — Update item quantity (Serializable transaction) ? NEW
Sets quantity to an absolute value. Returns warnings if below min or above max threshold.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 100,
  "minimumQuantity": 10,
  "maximumQuantity": 500,
  "notes": "Updated stock"
}
```

#### `POST /api/inventories/increment` — Increment item quantity ? NEW
Adds quantity. Returns warning if result exceeds maximum capacity.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 10,
  "notes": "Restocking",
  "reason": "Weekly supply delivery"
}
```

#### `POST /api/inventories/decrement` — Decrement item quantity ? NEW
Subtracts quantity. **Returns error if result would go below 0.** Returns warning if result falls below minimum threshold.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 1,
  "notes": "Manual removal",
  "reason": "Damaged goods"
}
```

#### `POST /api/inventories/adjust` — Adjust stock (+/-)
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantityChange": -5,
  "notes": "Damaged goods removed",
  "reason": "Quality control"
}
```

#### `POST /api/inventories/transfer` — Transfer between warehouses
```json
{
  "itemId": "guid",
  "fromInventoryId": "guid",
  "toInventoryId": "guid",
  "quantity": 25,
  "notes": "Rebalancing stock",
  "reason": "Customer demand shift"
}
```

---

### 7.7 Roles Controller — `/api/roles`

#### `GET /api/roles`
#### `GET /api/roles/{id}`
Returns role with permission list.

#### `POST /api/roles`
```json
{
  "name": "Supervisor",
  "description": "Shift supervisor role"
}
```

#### `PUT /api/roles/{id}`
#### `DELETE /api/roles/{id}`

#### `PUT /api/roles/{id}/permissions`
```json
{
  "roleId": "guid",
  "permissions": [
    "ColorCreate",
    "ColorEdit",
    "ColorView",
    "ItemView",
    "OrderCreate",
    "OrderView"
  ]
}
```

---

### 7.8 Users Controller — `/api/users`

#### `GET /api/users?pageNumber=1&pageSize=10`
#### `GET /api/users/{id}`

#### `POST /api/users`
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "User@123",
  "fullName": "New User",
  "phoneNumber": "+962791234567"
}
```

#### `PUT /api/users/{id}/activate` ? NEW — No body needed
Activates a user (sets `IsApproved=true, IsEnabled=true`). Just call the URL with the user ID.
**Response:** `{ "success": true, "message": "User activated successfully"}`

#### `PUT /api/users/{id}/deactivate` ? NEW — No body needed
Deactivates a user (sets `IsApproved=false, `IsEnabled=false`).
**Response:** `{ "success": true, "message": "User deactivated successfully"}`

#### `PUT /api/users/{id}/enable` ? NEW — No body needed
Re-enables a disabled user (keeps approval, sets `IsEnabled=true`).
**Response:** `{ "success": true, "message": "User enabled successfully"}`

#### `PUT /api/users/{id}/disable` ? NEW — No body needed
Disables a user temporarily (keeps approval, sets `IsEnabled=false`, revokes tokens).
**Response:** `{ "success": true, "message": "User disabled successfully"}`

#### `PUT /api/users/{id}/role`
```json
{
  "roleId": "guid"
}
```

---

### 7.9 Health Check — `GET /health` [AllowAnonymous]
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-01T00:00:00Z",
  "environment": "Development",
  "database": "PostgreSQL",
  "features": {
    "authentication": 3,
    "userManagement": 4,
    "colorManagement": 5,
    "locationManagement": 5,
    "itemManagement": 5,
    "orderManagement": 5,
    "roleManagement": 6
  },
  "totalEndpoints": 33
}
```

---

## 8. CQRS Feature Map

### Commands

| Feature | Command | Handler Returns |
|---------|---------|-----------------|
| **Auth** | `RegisterCommand` | `Result<Guid>` (userId) |
| | `LoginCommand` | `Result<LoginResponseDto>` |
| | `RefreshTokenCommand` | `Result<LoginResponseDto>` |
| | `LogoutCommand` | `Result<bool>` |
| **Colors** | `CreateColorCommand` | `Result<Guid>` |
| | `UpdateColorCommand` | `Result<bool>` |
| | `DeleteColorCommand` | `Result<bool>` |
| **Items** | `CreateItemCommand` | `Result<Guid>` |
| | `UpdateItemCommand` | `Result<bool>` |
| | `DeleteItemCommand` | `Result<bool>` |
| **Locations** | `CreateLocationCommand` | `Result<Guid>` |
| | `UpdateLocationCommand` | `Result<bool>` |
| | `DeleteLocationCommand` | `Result<bool>` |
| **Orders** | `CreateOrderCommand` | `Result<Guid>` |
| | `ConfirmOrderCommand` | `Result<bool>` |
| | `CancelOrderCommand` | `Result<bool>` |
| | `RefundOrderItemCommand` | `Result<bool>` |
| **Roles** | `CreateRoleCommand` | `Result<Guid>` |
| | `UpdateRoleCommand` | `Result<bool>` |
| | `DeleteRoleCommand` | `Result<bool>` |
| | `AssignPermissionsCommand` | `Result<bool>` |
| **Users** | `CreateUserCommand` | `Result<Guid>` |
| | `ApproveUserCommand` | `Result<bool>` |
| | `UpdateUserRoleCommand` | `Result<bool>` |
| | `ToggleUserStatusCommand` | `Result<bool>` |
| **Inventories** | `CreateInventoryCommand` | `Result<Guid>` |
| | `UpdateInventoryCommand` | `Result<bool>` |
| | `DeleteInventoryCommand` | `Result<bool>` |
| | `SetItemInventoryCommand` | `Result<ItemInventoryDto>` |
| | `UpdateItemQuantityCommand` | `Result<ItemInventoryDto>` |
| | `IncrementInventoryCommand` | `Result<ItemInventoryDto>` |
| | `DecrementInventoryCommand` | `Result<ItemInventoryDto>` |
| | `AdjustInventoryCommand` | `Result<ItemInventoryDto>` |
| | `TransferInventoryCommand` | `Result<bool>` |

### Queries

| Feature | Query | Handler Returns |
|---------|-------|-----------------|
| **Colors** | `GetAllColorsQuery` | `Result<List<ColorDto>>` |
| | `GetColorByIdQuery` | `Result<ColorDto>` |
| | `SearchColorsQuery` | `Result<PagedResult<ColorDto>>` |
| **Items** | `GetAllItemsQuery` | `Result<PagedResult<ItemDto>>` |
| | `GetItemByIdQuery` | `Result<ItemDto>` |
| | `SearchItemsQuery` | `Result<PagedResult<ItemDto>>` |
| **Locations** | `GetAllLocationsQuery` | `Result<List<LocationDto>>` |
| | `GetLocationByIdQuery` | `Result<LocationDto>` |
| | `SearchLocationsQuery` | `Result<PagedResult<LocationDto>>` |
| **Orders** | `GetAllOrdersQuery` | `Result<PagedResult<OrderDto>>` |
| | `GetOrderByIdQuery` | `Result<OrderDto>` |
| | `SearchOrdersQuery` | `Result<PagedResult<OrderDto>>` |
| **Roles** | `GetAllRolesQuery` | `Result<List<RoleDto>>` |
| | `GetRoleByIdQuery` | `Result<RoleDto>` |
| | `SearchRolesQuery` | `Result<PagedResult<RoleDto>>` |
| **Users** | `GetAllUsersQuery` | `Result<PagedResult<UserDto>>` |
| | `GetUserByIdQuery` | `Result<UserDto>` |
| | `SearchUsersQuery` | `Result<PagedResult<UserDto>>` |
| **Inventories** | `GetAllInventoriesQuery` | `Result<List<InventoryDto>>` |
| | `GetInventoryByIdQuery` | `Result<InventoryDto>` |
| | `GetItemsInInventoryQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetItemInventoryQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetLowStockItemsQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetActiveInventoriesQuery` | `Result<List<InventoryDto>>` |

### MediatR Pipeline Behaviors (in order)
1. `ValidationBehavior<,>` — FluentValidation before handler
2. `LoggingBehavior<,>` — Request/response logging
3. `PerformanceBehavior<,>` — Slow query detection
4. `TransactionBehavior<,>` — Automatic transaction wrapping

### Base Handler Templates
| Base Handler | Purpose |
|-------------|---------|
| `BaseCreateHandler<TCommand, TEntity>` | Create entity, audit, standard error handling |
| `BaseUpdateHandler<TCommand, TEntity>` | Update with validation, audit |
| `BaseSoftDeleteHandler<TCommand, TEntity>` | Soft delete with audit |
| `BaseGetByIdHandler<TQuery, TEntity, TDto>` | Get by ID with mapping |
| `BaseGetAllHandler<TQuery, TEntity, TDto>` | Get all with mapping |
| `BaseSearchHandler<TQuery, TEntity, TDto>` | Paged search with mapping |

---

## 9. Interfaces & Abstractions

### Repository Interfaces (Application Layer)

| Interface | Extends | Key Methods |
|-----------|---------|-------------|
| `IRepository<TEntity>` | — | Full CRUD, paging, soft delete, `FromSqlRawAsync` |
| `IUserRepository` | `IRepository<User>` | `GetByUsernameAsync`, `GetByEmailAsync`, `ExistsByUsernameAsync`, `ExistsByEmailAsync`, `GetWithRolesAsync` |
| `IRoleRepository` | `IRepository<Role>` | `GetByNameAsync`, `GetWithPermissionsAsync`, `ExistsByNameAsync` |
| `IPermissionRepository` | `IRepository<Permission>` | `HasPermissionAsync`, `GetUserPermissionsBitmaskAsync`, `GetRolePermissionAsync`, `AddRolePermissionAsync` |
| `IOrderRepository` | `IRepository<Order>` | `GetByStatusAsync`, `GetOrderCountByStatusAsync`, `GetWithItemsAsync` |
| `IOrderItemRepository` | `IRepository<OrderItem>` | |
| `IItemRepository` | `IRepository<Item>` | |
| `IColorRepository` | `IRepository<Color>` | |
| `ILocationRepository` | `IRepository<Location>` | |
| `IAuditLogRepository` | `IRepository<AuditLog>` | |
| `IRefreshTokenRepository` | `IRepository<RefreshToken>` | |
| `IInventoryRepository` | `IRepository<Inventory>` | |
| `IItemInventoryRepository` | `IRepository<ItemInventory>` | |
| `IUnitOfWork` | `IDisposable` | All repositories + `SaveChangesAsync`, `BeginTransaction`, `Commit`, `Rollback` |

### Service Interfaces

| Interface | Layer | Implementation |
|-----------|-------|----------------|
| `IJwtService` (Application) | Application | `JwtServiceWrapper` (Infrastructure) ? wraps Framework `JwtService` |
| `IPasswordService` (Application) | Application | `PasswordServiceWrapper` (Infrastructure) ? wraps Framework `PasswordService` |
| `IJwtService` (Framework) | Framework | `JwtService` (RSA 2048-bit JWT) |
| `IPasswordService` (Framework) | Framework | `PasswordService` (BCrypt) |
| `IBitManipulationService` | Framework | `BitManipulationService` |
| `IAuditService` | Application | `AuditService` (Infrastructure) |
| `ICurrentUserService` | Application | `CurrentUserService` (API) |
| `IDateTimeService` | Application | `DateTimeService` (Infrastructure) |

---

## 10. Middleware Pipeline

Order in `Program.cs` (execution order):

```
1. ExceptionHandlerMiddleware     ? Global exception handling
2. EnhancedLoggingMiddleware      ? HTML request logging (NLog)
3. Swagger (dev only)
4. HTTPS Redirection
5. CORS ("AllowAll" policy)
6. Authentication                 ? JWT Bearer validation
7. JwtMiddleware                  ? Extract user context from claims
8. Authorization
9. AuditLoggingMiddleware         ? Log actions to AuditLog table
10. MapControllers
```

---

## 11. Security & Permission System

### Authentication Flow
1. User registers ? `IsEnabled=false`, `IsApproved=false` ? awaits admin approval
2. Admin approves user ? `IsApproved=true`, `IsEnabled=true`
3. User logs in ? receives JWT access token (15 min) + refresh token (7 days)
4. Access token expires ? client calls `/api/auth/refresh` with refresh token
5. Refresh token rotation: old token revoked, new pair issued

### JWT Token Structure
- **Algorithm**: RS256 (RSA 2048-bit)
- **Claims**: `sub` (userId), `name` (username), `email`, `roles`, `permissions` (bitmask)
- **Access Token Expiry**: 15 minutes
- **Refresh Token Expiry**: 7 days
- **Issuer**: `MobileBackendAPI`
- **Audience**: `MobileApp`

### Permission System (Bitwise)
- Each permission is a power of 2 (`1L << N`)
- Role has `RolePermission.PermissionsBitmask` = bitwise OR of all assigned permissions
- Check: `(bitmask & permissionBit) == permissionBit` ? O(1)
- The `[RequirePermission(PermissionType.XYZ)]` attribute on controller actions checks via `IPermissionRepository.HasPermissionAsync`

### Seeded Roles
| Role | Description | Permissions |
|------|-------------|------------|
| Admin | Full system access | ALL permissions (bitmask = sum of all) |
| Manager | Operational management | Color, Item, Order, Location, RefundView, UserView |
| User | Basic access | View-only on Colors, Items, Locations, Orders |

---

## 12. Database & Infrastructure

### Database
- **Provider**: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4
- **Connection**: Configured in `appsettings.json` ? `ConnectionStrings:DefaultConnection`
- **Railway Deployment**: Auto-parses `DATABASE_URL` env var

### Global Query Filters
All entities implementing `ISoftDelete` have automatic EF filter: `WHERE IsDeleted = false`

### Migrations
| Migration | Description |
|-----------|-------------|
| `20251101094605_InitialCreate` | All core tables |
| `20251103210811_AddInventorySystem` | Inventory, ItemInventory tables |
| `20251103210956_AddInventorySystemFixed` | Fixes to inventory system |

### Entity Configurations
Each entity has a dedicated `*Configuration.cs` file in `Data/Configurations/` with:
- Property constraints (max length, required, etc.)
- Indexes
- Relationships
- Default values
- String conversion for enums (`OrderStatus` stored as string)

---

## 13. Seeded Data & Default Credentials

### Users
| Username | Password | Role | IsEnabled | IsApproved |
|----------|----------|------|-----------|------------|
| `admin` | `Admin@123` | Admin | ? | ? |
| `manager` | `Manager@123` | Manager | ? | ? |
| `user` | `User@123` | User | ? | ? |

### Seeded Permissions
30 permissions across categories: Color (4), Item (4), Order (5), Refund (2), User (5), Location (4), Role (5), System (3).

### Seeded Sample Data
- Colors: Red (#FF0000), Blue (#0000FF), Green (#00FF00)
- Locations: 3 default locations
- Items: 10 pet store items (PF-001 through PG-010)

---

## 14. Cross-Cutting Concerns

### Validation
- FluentValidation validators in `Validators/` folder per feature
- `ValidationBehavior` in MediatR pipeline auto-validates before handler
- `ValidateModelAttribute` global filter on all controllers

### Audit Logging
- `AuditLoggingMiddleware` logs HTTP requests
- `AuditHelper` + `IAuditService` log business operations (CRUD, login, refunds)
- Stored in `AuditLogs` table with old/new values as JSON

### Logging
- NLog with HTML rendering (`HtmlRequestLayoutRenderer`)
- `LoggingBehavior` logs all MediatR requests
- `PerformanceBehavior` flags slow queries (>500ms)
- HTML log files at `C:\AppLogs\ScanPet\`

### Error Handling
- `ExceptionHandlerMiddleware` — global exception catch
- `Result<T>` pattern — no exceptions for business logic failures
- Consistent error envelope across all endpoints

### Soft Delete
- All main entities implement `ISoftDelete`
- Global EF query filter auto-excludes `IsDeleted = true`
- `SoftDeleteAsync` in generic repository sets fields + timestamp

---

## 15. Unit Tests

**Project**: `MobileBackend.UnitTests`  
**Framework**: xUnit + Moq + FluentAssertions

| Test File | Covers |
|-----------|--------|
| `CreateColorCommandHandlerTests` | Color creation |
| `UpdateColorCommandHandlerTests` | Color update |
| `DeleteColorCommandHandlerTests` | Color soft delete |
| `GetAllColorsQueryHandlerTests` | Get all colors |
| `GetColorByIdQueryHandlerTests` | Get color by ID |
| `CreateItemCommandHandlerTests` | Item creation |
| `CreateOrderCommandHandlerTests` | Order creation |
| `RefundOrderItemCommandHandlerTests` | Order item refund |

---

## 16. NuGet Packages & Frameworks

### Domain Layer
*(No external packages — pure C#)*

### Application Layer
| Package | Version | Purpose |
|---------|---------|---------|
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.1 | Object mapping |
| FluentValidation | 12.0.0 | Request validation |
| FluentValidation.DependencyInjectionExtensions | 12.0.0 | DI integration |
| MediatR | 13.1.0 | CQRS mediator |

### Infrastructure Layer
| Package | Version | Purpose |
|---------|---------|---------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | PostgreSQL provider |
| Microsoft.EntityFrameworkCore.Tools | 9.0.10 | EF migrations |
| Microsoft.Extensions.Configuration.Json | 9.0.10 | Config |

### Framework Layer
| Package | Version | Purpose |
|---------|---------|---------|
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| System.IdentityModel.Tokens.Jwt | 8.14.0 | JWT tokens |
| Microsoft.Extensions.Configuration.* | 9.0.10 | Config abstractions |

### API Layer
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT auth middleware |
| NLog.Web.AspNetCore | 6.0.5 | Logging |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger/OpenAPI |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | (for migrations CLI) |

---

## 17. File Hierarchy (Complete)

```
D:\Project\
??? MobileBackend.sln
??? SOLUTION_KNOWLEDGE_BASE.md          ? THIS FILE
?
??? src/
?   ??? Domain/MobileBackend.Domain/
?   ?   ??? MobileBackend.Domain.csproj
?   ?   ??? Common/
?   ?   ?   ??? BaseEntity.cs
?   ?   ?   ??? ISoftDelete.cs
?   ?   ??? Entities/
?   ?   ?   ??? AuditLog.cs
?   ?   ?   ??? Color.cs
?   ?   ?   ??? Inventory.cs
?   ?   ?   ??? Item.cs
?   ?   ?   ??? ItemInventory.cs
?   ?   ?   ??? Location.cs
?   ?   ?   ??? Order.cs
?   ?   ?   ??? OrderItem.cs
?   ?   ?   ??? Permission.cs
?   ?   ?   ??? RefreshToken.cs
?   ?   ?   ??? Role.cs
?   ?   ?   ??? RolePermission.cs
?   ?   ?   ??? User.cs
?   ?   ?   ??? UserRole.cs
?   ?   ??? Enums/
?   ?       ??? OrderItemStatus.cs
?   ?       ??? OrderStatus.cs
?   ?       ??? PermissionType.cs
?   ?
?   ??? Application/MobileBackend.Application/
?   ?   ??? MobileBackend.Application.csproj
?   ?   ??? DependencyInjection.cs
?   ?   ??? Common/
?   ?   ?   ??? Behaviors/
?   ?   ?   ?   ??? LoggingBehavior.cs
?   ?   ?   ?   ??? PerformanceBehavior.cs
?   ?   ?   ?   ??? TransactionBehavior.cs
?   ?   ?   ?   ??? ValidationBehavior.cs
?   ?   ?   ??? Constants/
?   ?   ?   ?   ??? AuditConstants.cs
?   ?   ?   ?   ??? ErrorMessages.cs
?   ?   ?   ??? Extensions/
?   ?   ?   ?   ??? ResultExtensions.cs
?   ?   ?   ??? Handlers/
?   ?   ?   ?   ??? BaseCreateHandler.cs
?   ?   ?   ?   ??? BaseGetAllHandler.cs
?   ?   ?   ?   ??? BaseGetByIdHandler.cs
?   ?   ?   ?   ??? BaseSearchHandler.cs
?   ?   ?   ?   ??? BaseSoftDeleteHandler.cs
?   ?   ?   ?   ??? BaseUpdateHandler.cs
?   ?   ?   ??? Helpers/
?   ?   ?   ?   ??? AuditHelper.cs
?   ?   ?   ??? Interfaces/
?   ?   ?   ?   ??? IAuditService.cs
?   ?   ?   ?   ??? ICurrentUserService.cs
?   ?   ?   ?   ??? IDateTimeService.cs
?   ?   ?   ??? Mappings/
?   ?   ?   ?   ??? AuthMappingProfile.cs
?   ?   ?   ??? Queries/
?   ?   ?       ??? BaseGetByIdQuery.cs
?   ?   ?       ??? BasePagedQuery.cs
?   ?   ?       ??? BaseSearchQuery.cs
?   ?   ??? DTOs/
?   ?   ?   ??? Auth/ (LoginRequestDto, LoginResponseDto, RegisterRequestDto, RefreshTokenRequestDto, ChangePasswordDto)
?   ?   ?   ??? Colors/ (ColorDto)
?   ?   ?   ??? Common/ (Result, PagedResult, ValidationError)
?   ?   ?   ??? Inventories/ (CreateInventoryDto, UpdateInventoryDto, InventoryDto, ItemInventoryDto, SetItemInventoryDto, UpdateItemQuantityDto, IncrementInventoryDto, DecrementInventoryDto, AdjustInventoryDto, TransferInventoryDto)
?   ?   ?   ??? Items/ (ItemDto)
?   ?   ?   ??? Locations/ (LocationDto)
?   ?   ?   ??? Orders/ (OrderDto, OrderItemDto)
?   ?   ?   ??? Roles/ (RoleDto, RoleDtoConsolidated, AssignPermissionsDto)
?   ?   ?   ??? Users/ (UserDto, UserDtoConsolidated, UserApprovalDto, ToggleUserStatusDto, UpdateUserRoleDto)
?   ?   ??? Features/
?   ?   ?   ??? Auth/Commands/ (Login, Logout, RefreshToken, Register)
?   ?   ?   ??? Colors/Commands/ (CreateColor, DeleteColor, UpdateColor)
?   ?   ?   ??? Colors/Queries/ (GetAllColors, GetColorById, SearchColors)
?   ?   ?   ??? Inventories/Commands/ (AdjustInventory, CreateInventory, DeleteInventory, SetItemInventory, TransferInventory, UpdateInventory)
?   ?   ?   ??? Inventories/Queries/ (GetActiveInventories, GetAllInventories, GetInventoryById, GetItemInventory, GetItemsInInventory, GetLowStockItems)
?   ?   ?   ??? Items/Commands/ (CreateItem, DeleteItem, UpdateItem)
?   ?   ?   ??? Items/Queries/ (GetAllItems, GetItemById, SearchItems)
?   ?   ?   ??? Locations/Commands/ (CreateLocation, DeleteLocation, UpdateLocation)
?   ?   ?   ??? Locations/Queries/ (GetAllLocations, GetLocationById, SearchLocations)
?   ?   ?   ??? Orders/Commands/ (CancelOrder, ConfirmOrder, CreateOrder, RefundOrderItem)
?   ?   ?   ??? Orders/Queries/ (GetAllOrders, GetOrderById, SearchOrders)
?   ?   ?   ??? Roles/Commands/ (AssignPermissions, CreateRole, DeleteRole, UpdateRole)
?   ?   ?   ??? Roles/Queries/ (GetAllRoles, GetRoleById, SearchRoles)
?   ?   ?   ??? Users/Commands/ (ApproveUser, CreateUser, ToggleUserStatus, UpdateUserRole)
?   ?   ?   ??? Users/Queries/ (GetAllUsers, GetUserById, SearchUsers)
?   ?
?   ??? Infrastructure/MobileBackend.Infrastructure/
?   ?   ??? MobileBackend.Infrastructure.csproj
?   ?   ??? DependencyInjection.cs
?   ?   ??? Data/
?   ?   ?   ??? ApplicationDbContext.cs
?   ?   ?   ??? ApplicationDbContextFactory.cs
?   ?   ?   ??? DbSeeder.cs
?   ?   ?   ??? Configurations/ (18 entity configuration files)
?   ?   ??? Migrations/ (3 migrations + snapshot)
?   ?   ??? Repositories/ (GenericRepository + 11 specific repos + UnitOfWork)
?   ?   ??? Services/ (AuditService, DateTimeService, JwtServiceWrapper, PasswordServiceWrapper)
?   ?
?   ??? Framework/MobileBackend.Framework/
?       ??? MobileBackend.Framework.csproj
?       ??? DependencyInjection.cs
?       ??? Security/
?           ??? BitManipulationService.cs
?           ??? IBitManipulationService.cs
?           ??? IJwtService.cs
?           ??? IPasswordService.cs
?           ??? JwtService.cs
?           ??? PasswordService.cs
?           ??? Models/TokenModels.cs
?
??? tests/MobileBackend.UnitTests/
?   ??? MobileBackend.UnitTests.csproj
?   ??? TestBase.cs
?   ??? UnitTest1.cs
?   ??? Features/ (Colors + Items + Orders tests)
?
??? docs/ (50+ markdown files, Postman collections, scripts)
```

---

## 18. Potential Refactoring & Improvement Notes

### ?? Issues / Concerns

1. ~~**Deprecated `Item.Quantity` field**~~: ? **RESOLVED in Session 5** — `Quantity` property removed from `Item` entity, commands, DTOs, and all handlers. All quantity tracking now uses `ItemInventory` exclusively.

2. **`IInventoryRepository` and `IItemInventoryRepository` missing from Infrastructure DI**: The `AddRepositories()` in `Infrastructure/DependencyInjection.cs` does NOT register `IInventoryRepository` or `IItemInventoryRepository`, but they are in `IUnitOfWork`. If they're resolved only through `UnitOfWork`, this may work, but standalone injection would fail.

3. **Duplicate JWT/Password interfaces**: Both `Application.Interfaces.IJwtService` and `Framework.Security.IJwtService` exist. Infrastructure provides wrappers (`JwtServiceWrapper`, `PasswordServiceWrapper`) to bridge them — this works but adds complexity.

4. **`AuthController` doesn't extend `BaseApiController`**: `AuthController` extends `ControllerBase` directly while all other controllers extend `BaseApiController`. This means auth endpoints have inconsistent response formatting.

5. ~~**`RefundOrderItemRequest` class defined inside `OrdersController.cs`**~~: ? **RESOLVED in Session 6** — Moved to `Application/DTOs/Orders/RefundOrderItemRequest.cs`.

6. **`ChangePasswordDto` exists but no controller endpoint exposes it**: The feature is defined but not wired.

7. **No `SearchItems` endpoint on `ItemsController`**: The query handler `SearchItemsQuery`/`SearchItemsQueryHandler` exists but there's no controller action to call it.

8. **No `SearchLocations` endpoint on `LocationsController`**: Same issue — handler exists but no route.

9. **No `SearchOrders` endpoint on `OrdersController`**: Handler exists but not exposed.

10. **No `SearchUsers` endpoint on `UsersController`**: Handler exists but not exposed.

11. **No `SearchRoles` endpoint on `RolesController`**: Handler exists but not exposed.

12. **`RoleDtoConsolidated` and `UserDtoConsolidated`**: These DTOs exist but appear unused / leftover from refactoring.

13. **`[RequirePermission]` attribute is defined but NOT applied to any controller actions**: The permission system is built but endpoints are not protected by it. Any authenticated user can access all endpoints.

14. ~~**`ItemsController` has `[ApiController]` attribute AND inherits from `BaseApiController`**~~: ? **RESOLVED in Session 6** — Removed redundant `[ApiController]`.

15. ~~**OrdersController has both `[Authorize]` and inherits `BaseApiController`**~~: ? **RESOLVED in Session 6** — Removed redundant attributes.

### ?? Improvements

1. **Apply `[RequirePermission]` to all CRUD endpoints** to enforce the bitwise permission system.
2. **Expose all Search endpoints** that have handlers but no controller routes.
3. ~~**Remove deprecated `Item.Quantity`**~~: ? Resolved in Session 5.
4. **Make `AuthController` extend `BaseApiController`** for consistent response formatting.
5. ~~**Move `RefundOrderItemRequest` to Application DTOs layer.**~~: ? Resolved in Session 6.
6. **Add `ChangePassword` endpoint to `AuthController`.**
7. **Clean up unused `*Consolidated` DTOs.**
8. **Add response caching** using the `ResponseCacheAttribute` filter that exists but isn't applied.
9. **Add rate limiting** — configured in `appsettings.json` but not implemented.
10. **Consider adding an `IInventoryRepository`/`IItemInventoryRepository` registration** in DI for direct injection scenarios.

````````

This is the description of what the code block changes:
Update issues and improvements lists to mark items resolved in Session 6

This is the code block that represents the suggested code change:

````````markdown
5. ~~**`RefundOrderItemRequest` class defined inside `OrdersController.cs`**~~: ? **RESOLVED in Session 6** — Moved to `Application/DTOs/Orders/RefundOrderItemRequest.cs`.

6. **`ChangePasswordDto` exists but no controller endpoint exposes it**: The feature is defined but not wired.

### ?? Improvements

1. **Apply `[RequirePermission]` to all CRUD endpoints** to enforce the bitwise permission system.
2. **Expose all Search endpoints** that have handlers but no controller routes.
3. ~~**Remove deprecated `Item.Quantity`**~~: ? Resolved in Session 5.
4. **Make `AuthController` extend `BaseApiController`** for consistent response formatting.
5. ~~**Move `RefundOrderItemRequest` to Application DTOs layer.**~~: ? Resolved in Session 6.
6. **Add `ChangePassword` endpoint to `AuthController`.**
7. **Clean up unused `*Consolidated` DTOs.**
8. **Add response caching** using the `ResponseCacheAttribute` filter that exists but isn't applied.
9. **Add rate limiting** — configured in `appsettings.json` but not implemented.
10. **Consider adding an `IInventoryRepository`/`IItemInventoryRepository` registration** in DI for direct injection scenarios.

````````


# Response
````````markdown
# ?? ScanPet Mobile Backend — AI Knowledge Base

> **Purpose**: This file is a comprehensive reference for AI assistants working on this codebase.  
> **Last Updated**: Auto-generated from full solution analysis.  
> **Repository**: `https://github.com/mohannadalbujoq-cyber/ScanPet` — branch `main`

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Architecture & Clean Architecture Layers](#2-architecture--clean-architecture-layers)
3. [Project Dependency Graph](#3-project-dependency-graph)
4. [Domain Model — Entities & Relationships](#4-domain-model--entities--relationships)
5. [Enums](#5-enums)
6. [DTOs (Data Transfer Objects)](#6-dtos-data-transfer-objects)
7. [API Endpoints & JSON Contracts](#7-api-endpoints--json-contracts)
8. [CQRS Feature Map](#8-cqrs-feature-map)
9. [Interfaces & Abstractions](#9-interfaces--abstractions)
10. [Middleware Pipeline](#10-middleware-pipeline)
11. [Security & Permission System](#11-security--permission-system)
12. [Database & Infrastructure](#12-database--infrastructure)
13. [Seeded Data & Default Credentials](#13-seeded-data--default-credentials)
14. [Cross-Cutting Concerns](#14-cross-cutting-concerns)
15. [Unit Tests](#15-unit-tests)
16. [NuGet Packages & Frameworks](#16-nuget-packages--frameworks)
17. [File Hierarchy (Complete)](#17-file-hierarchy-complete)
18. [Potential Refactoring & Improvement Notes](#18-potential-refactoring--improvement-notes)

---

## 1. Solution Overview

**ScanPet** is a **.NET 8 Web API** backend for a mobile pet-store management application. It provides:

- **User Management** with registration, approval workflow, role-based access
- **Authentication** via RSA-signed JWT tokens with refresh token rotation
- **Role & Permission System** using bitwise bitmask operations (O(1) permission checks)
- **Product Catalog** — Items with Colors, SKU, pricing
- **Inventory/Warehouse Management** — multi-warehouse stock tracking, transfers, adjustments
- **Order Management** — order lifecycle (Pending ? Confirmed/Cancelled), serial numbers, refunds
- **Location Management** — delivery/shipping locations (separate from warehouses)
- **Audit Logging** — full action tracking for compliance
- **Soft Delete** everywhere via `ISoftDelete` interface + global EF query filters

**Database**: PostgreSQL (via Npgsql)  
**ORM**: Entity Framework Core 9  
**CQRS**: MediatR 13  
**Validation**: FluentValidation 12  
**Mapping**: AutoMapper 12  
**Logging**: NLog  
**Auth**: JWT Bearer (RSA 2048-bit)  
**Target**: .NET 8.0, deployable on Railway

---

## 2. Architecture & Clean Architecture Layers

```
???????????????????????????????????????????????????????
?                    API Layer                         ?
?  (Controllers, Middleware, Filters, Program.cs)      ?
?  MobileBackend.API                                  ?
???????????????????????????????????????????????????????
?               Application Layer                      ?
?  (CQRS Features, DTOs, Validators, Interfaces,      ?
?   Behaviors, Common Handlers)                        ?
?  MobileBackend.Application                          ?
???????????????????????????????????????????????????????
?  Infrastructure      ?      Framework Layer          ?
?  (EF DbContext,      ?  (JWT Service, Password       ?
?   Repositories,      ?   Service, BitManipulation)   ?
?   Migrations,        ?  MobileBackend.Framework      ?
?   UnitOfWork)        ?                               ?
?  MobileBackend.      ?                               ?
?  Infrastructure      ?                               ?
???????????????????????????????????????????????????????
?                   Domain Layer                       ?
?  (Entities, Enums, Base classes, ISoftDelete)         ?
?  MobileBackend.Domain                               ?
???????????????????????????????????????????????????????
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `MobileBackend.Domain` | Entities, Enums, `BaseEntity`, `ISoftDelete`. Zero dependencies. |
| **Application** | `MobileBackend.Application` | CQRS commands/queries/handlers, DTOs, validators, interfaces (repository contracts), pipeline behaviors, base handler templates. Depends on Domain. |
| **Infrastructure** | `MobileBackend.Infrastructure` | EF Core `ApplicationDbContext`, repositories, `UnitOfWork`, `DbSeeder`, entity configurations, migrations, service wrappers. Depends on Domain + Application + Framework. |
| **Framework** | `MobileBackend.Framework` | Security services (JWT generation/validation, password hashing via BCrypt, bit manipulation/encryption). Depends on Domain only. |
| **API** | `MobileBackend.API` | ASP.NET Core controllers, middleware (JWT, Audit, Exception, Logging), filters (`RequirePermission`, `ValidateModel`, `ResponseCache`), `Program.cs` composition root, Swagger, health checks. Depends on Application + Infrastructure + Framework. |
| **Tests** | `MobileBackend.UnitTests` | Unit tests using xUnit, Moq, FluentAssertions. |

---

## 3. Project Dependency Graph

```
MobileBackend.API
??? MobileBackend.Application
?   ??? MobileBackend.Domain
??? MobileBackend.Infrastructure
?   ??? MobileBackend.Domain
?   ??? MobileBackend.Application
?   ??? MobileBackend.Framework
?       ??? MobileBackend.Domain
??? MobileBackend.Framework
    ??? MobileBackend.Domain
```

### Dependency Injection Registration

Each layer exposes an `AddXxx()` extension method in `DependencyInjection.cs`:

```csharp
// Program.cs
builder.Services.AddApplication();                        // MediatR, FluentValidation, AutoMapper, Behaviors
builder.Services.AddInfrastructure(builder.Configuration); // EF Core, Repositories, UnitOfWork, Services
builder.Services.AddFramework(builder.Configuration);      // JWT, Password, BitManipulation
```

---

## 4. Domain Model — Entities & Relationships

### Entity Class Diagram

```
BaseEntity (abstract)
??? Id: Guid (auto-generated)
??? CreatedAt: DateTime
??? CreatedBy: Guid?
??? UpdatedAt: DateTime?
??? UpdatedBy: Guid?

ISoftDelete (interface)
??? IsDeleted: bool
??? DeletedAt: DateTime?
??? DeletedBy: Guid?
```

### All Entities

#### `User` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Username | string | Unique |
| Email | string | Unique |
| PasswordHash | string | BCrypt hash |
| PhoneNumber | string? | |
| FullName | string? | |
| IsEnabled | bool | Default: false |
| IsApproved | bool | Default: false (needs admin approval) |
| **Nav**: UserRoles | `ICollection<UserRole>` | |
| **Nav**: RefreshTokens | `ICollection<RefreshToken>` | |

#### `Role` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Admin", "Manager", "User" |
| Description | string? | |
| **Nav**: UserRoles | `ICollection<UserRole>` | |
| **Nav**: RolePermissions | `ICollection<RolePermission>` | |

#### `UserRole`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK ? User |
| RoleId | Guid | FK ? Role |
| AssignedAt | DateTime | |
| AssignedBy | Guid? | |
| IsDeleted | bool | |

#### `Permission`
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Color Create" |
| Description | string? | |
| PermissionBit | long | Power of 2 value |
| Category | string? | e.g., "Color", "Item", "Order" |
| IsDeleted | bool | |

#### `RolePermission`
| Property | Type | Notes |
|----------|------|-------|
| RoleId | Guid | FK ? Role |
| PermissionsBitmask | long | Bitwise OR of all permission bits |
| IsDeleted | bool | |
| **Nav**: Role | `Role` | |

#### `Color` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| RedValue | int | 0–255 |
| GreenValue | int | 0–255 |
| BlueValue | int | 0–255 |
| Description | string? | |
| HexCode | string | Computed: `#RRGGBB` |
| **Nav**: Items | `ICollection<Item>` | |

#### `Item` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| Description | string? | |
| SKU | string? | |
| BasePrice | decimal | |
| ColorId | Guid? | FK ? Color |
| ImageUrl | string? | |
| **Nav**: Color | `Color?` | |
| **Nav**: OrderItems | `ICollection<OrderItem>` | |
| **Nav**: ItemInventories | `ICollection<ItemInventory>` | Multi-warehouse quantities |
| **Method**: GetTotalQuantity() | int | Sum across all inventories |
| **Method**: GetQuantityAtInventory(Guid) | int | Quantity at specific warehouse |

> **Note**: `Quantity` field has been **removed** from `Item` entity. All quantity tracking is now exclusively through `ItemInventory` records (one per item per warehouse).

#### `Inventory` (ISoftDelete) — Represents a Warehouse
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Warehouse name |
| Location | string? | Physical address |
| Description | string? | |
| IsActive | bool | Default: true |
| **Nav**: ItemInventories | `ICollection<ItemInventory>` | |

#### `ItemInventory` (ISoftDelete) — Junction: Item ? Inventory
| Property | Type | Notes |
|----------|------|-------|
| ItemId | Guid | FK ? Item |
| InventoryId | Guid | FK ? Inventory |
| Quantity | int | Stock at this warehouse |
| MinimumQuantity | int? | Low stock threshold |
| MaximumQuantity | int? | Max capacity |
| Notes | string? | |
| **Nav**: Item | `Item` | |
| **Nav**: Inventory | `Inventory` | |

#### `Location` (ISoftDelete) — Delivery/Shipping Location
| Property | Type | Notes |
|----------|------|-------|
| Name | string | |
| Address | string? | |
| City | string? | |
| Country | string? | |
| PostalCode | string? | |
| IsActive | bool | |
| **Nav**: Orders | `ICollection<Order>` | |

#### `Order` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| OrderNumber | string | Auto-generated: `ORD-YYYYMMDD-####` |
| ClientName | string | |
| ClientPhone | string | |
| LocationId | Guid | FK ? Location |
| Description | string? | |
| TotalAmount | decimal | Computed from items |
| OrderStatus | OrderStatus | Pending/Confirmed/Cancelled |
| OrderDate | DateTime | |
| **Nav**: Location | `Location` | |
| **Nav**: OrderItems | `ICollection<OrderItem>` | |

#### `OrderItem` (ISoftDelete)
| Property | Type | Notes |
|----------|------|-------|
| OrderId | Guid | FK ? Order |
| ItemId | Guid | FK ? Item |
| SerialNumber | string | Uses Item SKU (e.g., `PF-001`) |
| Quantity | int | |
| SalePrice | decimal | Price at time of order |
| Status | OrderItemStatus | Successful/Refunded |
| RefundedQuantity | int | |
| RefundedAt | DateTime? | |
| RefundedBy | Guid? | |
| RefundReason | string? | |
| RefundedToInventoryId | Guid? | FK ? Inventory (refund target) |
| **Nav**: Order | `Order` | |
| **Nav**: Item | `Item` | |
| **Nav**: RefundedToInventory | `Inventory?` | |

#### `RefreshToken`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK ? User |
| Token | string | |
| DeviceInfo | string? | |
| IpAddress | string? | |
| ExpiresAt | DateTime | |
| RevokedAt | DateTime? | |
| IsRevoked | bool | |
| **Computed**: IsExpired | bool | |
| **Computed**: IsActive | bool | !IsRevoked && !IsExpired |

#### `AuditLog`
| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid? | FK ? User |
| Action | string | Create, Update, Delete, Login, etc. |
| EntityName | string | User, Order, Item, etc. |
| EntityId | Guid? | |
| OldValues | string? | JSON |
| NewValues | string? | JSON |
| IpAddress | string? | |
| UserAgent | string? | |
| Timestamp | DateTime | |
| AdditionalInfo | string? | |

### Entity Relationship Diagram (Simplified)

```
User ??< UserRole >?? Role ??< RolePermission
  ?                              
  ???< RefreshToken             

Permission (standalone, referenced by PermissionBit in RolePermission.PermissionsBitmask)

Color ??< Item ??< ItemInventory >?? Inventory (Warehouse)
                ?
                ???< OrderItem >?? Order >?? Location

OrderItem.RefundedToInventoryId ??> Inventory (optional)
```

---

## 5. Enums

### `OrderStatus`
```csharp
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}
```

### `OrderItemStatus`
```csharp
public enum OrderItemStatus
{
    Successful = 0,
    Refunded = 1
}
```

### `PermissionType : long` (Bitwise Flags)

| Permission | Bit | Value | Category |
|-----------|-----|-------|----------|
| ColorCreate | 0 | 1 | Color |
| ColorEdit | 1 | 2 | Color |
| ColorDelete | 2 | 4 | Color |
| ColorView | 3 | 8 | Color |
| ItemCreate | 4 | 16 | Item |
| ItemEdit | 5 | 32 | Item |
| ItemDelete | 6 | 64 | Item |
| ItemView | 7 | 128 | Item |
| OrderCreate | 8 | 256 | Order |
| OrderView | 9 | 512 | Order |
| OrderEdit | 10 | 1024 | Order |
| OrderConfirm | 11 | 2048 | Order |
| OrderCancel | 12 | 4096 | Order |
| RefundProcess | 13 | 8192 | Refund |
| RefundView | 14 | 16384 | Refund |
| UserView | 15 | 32768 | User |
| UserCreate | 16 | 65536 | User |
| UserEdit | 17 | 131072 | User |
| UserDelete | 18 | 262144 | User |
| UserApprove | 19 | 524288 | User |
| UserEnable | 20 | 1048576 | User |
| UserDisable | 21 | 2097152 | User |
| UserResetPassword | 22 | 4194304 | User |
| LocationCreate | 23 | 8388608 | Location |
| LocationEdit | 24 | 16777216 | Location |
| LocationDelete | 25 | 33554432 | Location |
| LocationView | 26 | 67108864 | Location |
| RoleCreate | 27 | 134217728 | Role |
| RoleEdit | 28 | 268435456 | Role |
| RoleDelete | 29 | 536870912 | Role |
| RoleView | 30 | 1073741824 | Role |
| PermissionManage | 31 | 2147483648 | Role |
| AuditLogView | 32 | 4294967296 | System |
| AuditLogExport | 33 | 8589934592 | System |
| SystemSettings | 34 | 17179869184 | System |
| SystemBackup | 35 | 34359738368 | System |
| SystemRestore | 36 | 68719476736 | System |

**Helper**: `PermissionHelper` class provides static methods:
- `HasPermission(bitmask, permission)` ? bool
- `HasAnyPermission(bitmask, params permissions)` ? bool
- `HasAllPermissions(bitmask, params permissions)` ? bool
- `AddPermission(bitmask, permission)` ? long
- `RemovePermission(bitmask, permission)` ? long
- `CombinePermissions(params permissions)` ? long
- `GetPermissions(bitmask)` ? `List<PermissionType>`

---

## 6. DTOs (Data Transfer Objects)

All DTOs use **unified DTO pattern** — same class for request/response with nullable properties. Context determines which fields are required.

### Auth DTOs
| DTO | Purpose |
|-----|---------|
| `RegisterRequestDto` | `Username`, `Email`, `Password`, `ConfirmPassword`, `FullName`, `PhoneNumber?` |
| `LoginRequestDto` | `UsernameOrEmail`, `Password`, `DeviceInfo?`, `IpAddress?` |
| `LoginResponseDto` | `AccessToken`, `RefreshToken`, `ExpiresAt`, `User` (? `UserInfoDto`) |
| `UserInfoDto` | `Id`, `Username`, `Email`, `FullName`, `IsEnabled`, `IsApproved`, `Roles[]`, `PermissionsBitmask` |
| `RefreshTokenRequestDto` | `RefreshToken`, `DeviceInfo?` |
| `ChangePasswordDto` | *(exists but not exposed in controller currently)* |

### Entity DTOs
| DTO | Key Fields |
|-----|------------|
| `ColorDto` | `Id?`, `Name?`, `Description?`, `RedValue?`, `GreenValue?`, `BlueValue?`, `HexCode?`, `ItemCount?`, `CreatedAt?`, `UpdatedAt?` |
| `ItemDto` | `Id?`, `Name?`, `Description?`, `SKU?`, `BasePrice?`, `ColorId?`, `ColorName?`, `ImageUrl?`, `CreatedAt?`, `UpdatedAt?` |
| `LocationDto` | `Id?`, `Name?`, `Address?`, `City?`, `Country?`, `PostalCode?`, `IsActive?`, `OrderCount?`, `SectionCount?`, `CreatedAt?`, `UpdatedAt?` |
| `OrderDto` | `Id?`, `OrderNumber?`, `ClientName?`, `ClientPhone?`, `LocationId?`, `LocationName?`, `Description?`, `TotalAmount?`, `OrderStatus?`, `OrderStatusName?`, `OrderDate?`, `OrderItems[]?`, `CreatedAt?`, `UpdatedAt?` |
| `OrderItemDto` | `Id?`, `OrderId?`, `ItemId?`, `ItemName?`, `Quantity?`, `UnitPrice?`, `TotalPrice?`, `Status?`, `StatusName?`, `SerialNumber?`, `CreatedAt?` |
| `RoleDto` | `Id?`, `Name?`, `Description?`, `PermissionsBitmask?`, `Permissions[]?`, `UserCount?`, `CreatedAt?`, `UpdatedAt?` |
| `UserDto` | `Id?`, `Username?`, `Email?`, `Password?`, `FullName?`, `PhoneNumber?`, `IsEnabled?`, `IsApproved?`, `Roles[]?`, `CreatedAt?`, `UpdatedAt?` |

### Operation-Specific DTOs
| DTO | Fields |
|-----|--------|
| `AssignPermissionsDto` | `RoleId`, `Permissions: List<PermissionType>` |
| `UserApprovalDto` | `UserId`, `IsApproved`, `IsEnabled` |
| `UpdateUserRoleDto` | `RoleId` |
| `ToggleUserStatusDto` | `IsEnabled` |
| `CreateInventoryDto` | `Name`, `Location?`, `Description?`, `IsActive` |
| `UpdateInventoryDto` | `Name`, `Location?`, `Description?`, `IsActive` |
| `InventoryDto` | `Id`, `Name`, `Location?`, `Description?`, `IsActive`, `TotalItems`, `CreatedAt`, `UpdatedAt?` |
| `ItemInventoryDto` | `Id`, `ItemId`, `ItemName`, `ItemSKU?`, `InventoryId`, `InventoryName`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?`, `IsLowStock`, `CreatedAt`, `UpdatedAt?` |
| `SetItemInventoryDto` | `ItemId`, `InventoryId`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?` |
| `UpdateItemQuantityDto` | `ItemId`, `InventoryId`, `Quantity`, `MinimumQuantity?`, `MaximumQuantity?`, `Notes?` |
| `IncrementInventoryDto` | `ItemId`, `InventoryId`, `Quantity` (default: 1), `Notes?`, `Reason?` |
| `DecrementInventoryDto` | `ItemId`, `InventoryId`, `Quantity` (default: 1), `Notes?`, `Reason?` |
| `AdjustInventoryDto` | `ItemId`, `InventoryId`, `QuantityChange` (+/-), `Notes?`, `Reason?` |
| `TransferInventoryDto` | `ItemId`, `FromInventoryId`, `ToInventoryId`, `Quantity`, `Notes?`, `Reason?` |

### Common DTOs
| DTO | Purpose |
|-----|---------|
| `Result<T>` | Generic wrapper: `Success`, `Data`, `ErrorMessage`, `ValidationErrors`, `StatusCode`, `Timestamp` |
| `Result` | Non-generic (no data) variant |
| `PagedResult<T>` | `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage` |
| `ValidationError` | Individual validation error details |

---

## 7. API Endpoints & JSON Contracts

**Base URL**: `/api`  
**Auth**: All endpoints require JWT Bearer token except where noted `[AllowAnonymous]`.  
**Response Envelope**: All responses follow:

```json
{
  "success": true|false,
  "message": "string",
  "data": { ... } | null,
  "errors": [ ... ] | null
}
```

---

### 7.1 Auth Controller — `POST /api/auth/*`

#### `POST /api/auth/register` [AllowAnonymous]
**Request:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "P@ssw0rd123",
  "confirmPassword": "P@ssw0rd123",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890"
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "User registered successfully. Your account is pending approval by an administrator.",
  "userId": "guid"
}
```

> **?? Note**: If a user with the same username/email already exists but is **not yet activated**, the response will be:
> ```json
> {
>   "success": false,
>   "message": "This username is already registered but the account is not yet activated. Please contact the administrator to activate your account.",
>   "errors": null
> }
> ```
> HTTP Status: **409 Conflict**

#### `POST /api/auth/login` [AllowAnonymous]
**Request:**
```json
{
  "usernameOrEmail": "admin",
  "password": "Admin@123",
  "deviceInfo": "iPhone 15 Pro"
}
```
**Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbG...",
    "refreshToken": "random-refresh-token-string",
    "expiresAt": "2025-01-01T00:15:00Z",
    "user": {
      "id": "guid",
      "username": "admin",
      "email": "admin@scanpet.com",
      "fullName": "System Administrator",
      "isEnabled": true,
      "isApproved": true,
      "roles": ["Admin"],
      "permissionsBitmask": 137438953471
    }
  }
}
```

#### `POST /api/auth/refresh` [AllowAnonymous]
**Request:**
```json
{
  "refreshToken": "existing-refresh-token",
  "deviceInfo": "iPhone 15 Pro"
}
```
**Response (200):** Same as login response.

#### `POST /api/auth/logout` [Authorized]
**Request Body:** `"optional-refresh-token-string"` (or null to revoke all)  
**Response (200):**
```json
{
  "success": true,
  "message": "Logout successful"
}
```

#### `GET /api/auth/me` [Authorized]
**Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "username": "admin",
    "email": "admin@scanpet.com",
    "isAuthenticated": true
  }
}
```

---

### 7.2 Colors Controller — `/api/colors`

#### `GET /api/colors`
Returns all colors.
```json
{
  "success": true,
  "message": "Operation successful",
  "data": [
    {
      "id": "guid",
      "name": "Red",
      "description": "Bright red",
      "redValue": 255,
      "greenValue": 0,
      "blueValue": 0,
      "hexCode": "#FF0000",
      "itemCount": 5,
      "createdAt": "2025-01-01T00:00:00Z",
      "updatedAt": null
    }
  ]
}
```

#### `GET /api/colors/search?searchTerm=red&pageNumber=1&pageSize=10`
Paginated search by name/description.

#### `GET /api/colors/{id}`
Single color by ID.

#### `POST /api/colors`
**Request:**
```json
{
  "name": "Ocean Blue",
  "description": "Deep ocean blue",
  "redValue": 0,
  "greenValue": 105,
  "blueValue": 180
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "Color",
  "data": "guid"
}
```

#### `PUT /api/colors/{id}`
**Request:** Same body as POST.  
**Response (200):** `{ "success": true, "message": "Color updated successfully" }`

#### `DELETE /api/colors/{id}`
**Response (200):** `{ "success": true, "message": "Color deleted successfully" }`

---

### 7.3 Items Controller — `/api/items`

#### `GET /api/items?pageNumber=1&pageSize=5`
**Paginated** — Returns items with DB-level pagination. Supports inventory filtering.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Page number (1-based) |
| `pageSize` | int | 10 | Items per page (1–100) |
| `inventoryId` | Guid? | null | Filter items by inventory/section |

**Response (200):**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Pet Food Premium",
        "description": "High-quality pet food",
        "sku": "PF-001",
        "basePrice": 29.99,
        "colorId": "guid-or-null",
        "colorName": "Red",
        "imageUrl": null,
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 10,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

#### `GET /api/items/{id}`
Single item by ID.

#### `POST /api/items`
**Request:**
```json
{
  "name": "Pet Food Premium",
  "description": "High-quality pet food",
  "sku": "PF-001",
  "basePrice": 29.99,
  "colorId": "guid-or-null",
  "imageUrl": "https://..."
}
```

#### `PUT /api/items/{id}`
Same body as POST. ? **Now working correctly.**

#### `DELETE /api/items/{id}`
Soft deletes the item. ? **Now working correctly.**

---

### 7.4 Locations Controller — `/api/locations`

#### `GET /api/locations`
Returns all locations with order counts **and section counts**.
```json
{
  "success": true,
  "message": "Operation successful",
  "data": [
    {
      "id": "guid",
      "name": "Main Store",
      "address": "123 Pet Street",
      "city": "Amman",
      "country": "Jordan",
      "postalCode": "11183",
      "isActive": true,
      "orderCount": 5,
      "sectionCount": 2,
      "sections": null,
      "createdAt": "2025-01-01T00:00:00Z",
      "updatedAt": null
    }
  ]
}
```

#### `GET /api/locations/{id}`
Returns location details **with its inventory sections** (warehouses within it).
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": "guid",
    "name": "Main Store",
    "address": "123 Pet Street",
    "city": "Amman",
    "country": "Jordan",
    "postalCode": "11183",
    "isActive": true,
    "orderCount": 5,
    "sectionCount": 2,
    "sections": [
      {
        "id": "guid",
        "name": "Section A - Dry Food",
        "location": "Shelf A1-A5",
        "description": "Dry pet food section",
        "isActive": true,
        "totalItems": 15,
        "locationId": "parent-location-guid",
        "locationName": "Main Store",
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": null
  }
}
```

#### `POST /api/locations`
**Request:**
```json
{
  "name": "Main Store",
  "address": "123 Pet Street",
  "city": "Amman",
  "country": "Jordan",
  "postalCode": "11183",
  "isActive": true
}
```
#### `PUT /api/locations/{id}`
#### `DELETE /api/locations/{id}`

---

### 7.5 Orders Controller — `/api/orders`

#### `GET /api/orders?pageNumber=1&pageSize=10`
#### `GET /api/orders/{id}`
Returns order with all order items.

#### `POST /api/orders`
**Request:**
```json
{
  "clientName": "Ahmad Ali",
  "clientPhone": "+962791234567",
  "locationId": "guid",
  "description": "Rush order",
  "orderItems": [
    {
      "itemId": "guid",
      "quantity": 2,
      "unitPrice": 29.99
    },
    {
      "itemId": "guid",
      "quantity": 1,
      "unitPrice": 15.99
    }
  ]
}
```
**Response (201):**
```json
{
  "success": true,
  "message": "Order",
  "data": "order-guid"
}
```

#### `PUT /api/orders/{id}/confirm`
**Response:** `{ "success": true, "message": "Order confirmed successfully" }`

#### `PUT /api/orders/{id}/cancel?reason=Out of stock`
**Response:** `{ "success": true, "message": "Order cancelled successfully" }`

#### `POST /api/orders/refund/{sku}`
**Request:**
```json
{
  "refundQuantity": 1,
  "refundReason": "Defective product",
  "refundToInventoryId": "warehouse-guid"
}
```
**Response:** `{ "success": true, "message": "Order item refunded successfully" }`

> **Note**: The `{sku}` parameter is the Item SKU (e.g., `PF-001`). The system finds the most recent unrefunded order item matching that SKU.

---

### 7.6 Inventories Controller — `/api/inventories`

> **Transaction Isolation**: All inventory-modifying commands use **Serializable** isolation level via `TransactionBehavior` to ensure full ACID compliance. This prevents dirty reads, non-repeatable reads, phantom reads, and lost updates during concurrent quantity changes.

#### `POST /api/inventories` — Create warehouse
```json
{
  "name": "Main Warehouse",
  "location": "Industrial Zone, Amman",
  "description": "Primary storage facility",
  "isActive": true
}
```

#### `GET /api/inventories` — All warehouses
#### `GET /api/inventories/{id}` — Single warehouse with items
#### `PUT /api/inventories/{id}` — Update warehouse
#### `DELETE /api/inventories/{id}` — Soft delete warehouse
#### `GET /api/inventories/{inventoryId}/items` — Items in warehouse
#### `GET /api/inventories/items/{itemId}` — Item stock across all warehouses
#### `GET /api/inventories/active` — Active warehouses only
#### `GET /api/inventories/low-stock` — Low stock items

#### `POST /api/inventories/items` — Set initial item inventory (create or set)
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 100,
  "minimumQuantity": 10,
  "maximumQuantity": 500,
  "notes": "Initial stock"
}
```

#### `PUT /api/inventories/items` — Update item quantity (Serializable transaction) ? NEW
Sets quantity to an absolute value. Returns warnings if below min or above max threshold.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 100,
  "minimumQuantity": 10,
  "maximumQuantity": 500,
  "notes": "Updated stock"
}
```

#### `POST /api/inventories/increment` — Increment item quantity ? NEW
Adds quantity. Returns warning if result exceeds maximum capacity.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 10,
  "notes": "Restocking",
  "reason": "Weekly supply delivery"
}
```

#### `POST /api/inventories/decrement` — Decrement item quantity ? NEW
Subtracts quantity. **Returns error if result would go below 0.** Returns warning if result falls below minimum threshold.
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantity": 1,
  "notes": "Manual removal",
  "reason": "Damaged goods"
}
```

#### `POST /api/inventories/adjust` — Adjust stock (+/-)
```json
{
  "itemId": "guid",
  "inventoryId": "guid",
  "quantityChange": -5,
  "notes": "Damaged goods removed",
  "reason": "Quality control"
}
```

#### `POST /api/inventories/transfer` — Transfer between warehouses
```json
{
  "itemId": "guid",
  "fromInventoryId": "guid",
  "toInventoryId": "guid",
  "quantity": 25,
  "notes": "Rebalancing stock",
  "reason": "Customer demand shift"
}
```

---

### 7.7 Roles Controller — `/api/roles`

#### `GET /api/roles`
#### `GET /api/roles/{id}`
Returns role with permission list.

#### `POST /api/roles`
```json
{
  "name": "Supervisor",
  "description": "Shift supervisor role"
}
```

#### `PUT /api/roles/{id}`
#### `DELETE /api/roles/{id}`

#### `PUT /api/roles/{id}/permissions`
```json
{
  "roleId": "guid",
  "permissions": [
    "ColorCreate",
    "ColorEdit",
    "ColorView",
    "ItemView",
    "OrderCreate",
    "OrderView"
  ]
}
```

---

### 7.8 Users Controller — `/api/users`

#### `GET /api/users?pageNumber=1&pageSize=10`
#### `GET /api/users/{id}`

#### `POST /api/users`
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "User@123",
  "fullName": "New User",
  "phoneNumber": "+962791234567"
}
```

#### `PUT /api/users/{id}/activate` ? NEW — No body needed
Activates a user (sets `IsApproved=true, IsEnabled=true`). Just call the URL with the user ID.
**Response:** `{ "success": true, "message": "User activated successfully"}`

#### `PUT /api/users/{id}/deactivate` ? NEW — No body needed
Deactivates a user (sets `IsApproved=false, `IsEnabled=false`).
**Response:** `{ "success": true, "message": "User deactivated successfully"}`

#### `PUT /api/users/{id}/enable` ? NEW — No body needed
Re-enables a disabled user (keeps approval, sets `IsEnabled=true`).
**Response:** `{ "success": true, "message": "User enabled successfully"}`

#### `PUT /api/users/{id}/disable` ? NEW — No body needed
Disables a user temporarily (keeps approval, sets `IsEnabled=false`, revokes tokens).
**Response:** `{ "success": true, "message": "User disabled successfully"}`

#### `PUT /api/users/{id}/role`
```json
{
  "roleId": "guid"
}
```

---

### 7.9 Health Check — `GET /health` [AllowAnonymous]
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-01T00:00:00Z",
  "environment": "Development",
  "database": "PostgreSQL",
  "features": {
    "authentication": 3,
    "userManagement": 4,
    "colorManagement": 5,
    "locationManagement": 5,
    "itemManagement": 5,
    "orderManagement": 5,
    "roleManagement": 6
  },
  "totalEndpoints": 33
}
```

---

## 8. CQRS Feature Map

### Commands

| Feature | Command | Handler Returns |
|---------|---------|-----------------|
| **Auth** | `RegisterCommand` | `Result<Guid>` (userId) |
| | `LoginCommand` | `Result<LoginResponseDto>` |
| | `RefreshTokenCommand` | `Result<LoginResponseDto>` |
| | `LogoutCommand` | `Result<bool>` |
| **Colors** | `CreateColorCommand` | `Result<Guid>` |
| | `UpdateColorCommand` | `Result<bool>` |
| | `DeleteColorCommand` | `Result<bool>` |
| **Items** | `CreateItemCommand` | `Result<Guid>` |
| | `UpdateItemCommand` | `Result<bool>` |
| | `DeleteItemCommand` | `Result<bool>` |
| **Locations** | `CreateLocationCommand` | `Result<Guid>` |
| | `UpdateLocationCommand` | `Result<bool>` |
| | `DeleteLocationCommand` | `Result<bool>` |
| **Orders** | `CreateOrderCommand` | `Result<Guid>` |
| | `ConfirmOrderCommand` | `Result<bool>` |
| | `CancelOrderCommand` | `Result<bool>` |
| | `RefundOrderItemCommand` | `Result<bool>` |
| **Roles** | `CreateRoleCommand` | `Result<Guid>` |
| | `UpdateRoleCommand` | `Result<bool>` |
| | `DeleteRoleCommand` | `Result<bool>` |
| | `AssignPermissionsCommand` | `Result<bool>` |
| **Users** | `CreateUserCommand` | `Result<Guid>` |
| | `ApproveUserCommand` | `Result<bool>` |
| | `UpdateUserRoleCommand` | `Result<bool>` |
| | `ToggleUserStatusCommand` | `Result<bool>` |
| **Inventories** | `CreateInventoryCommand` | `Result<Guid>` |
| | `UpdateInventoryCommand` | `Result<bool>` |
| | `DeleteInventoryCommand` | `Result<bool>` |
| | `SetItemInventoryCommand` | `Result<ItemInventoryDto>` |
| | `UpdateItemQuantityCommand` | `Result<ItemInventoryDto>` |
| | `IncrementInventoryCommand` | `Result<ItemInventoryDto>` |
| | `DecrementInventoryCommand` | `Result<ItemInventoryDto>` |
| | `AdjustInventoryCommand` | `Result<ItemInventoryDto>` |
| | `TransferInventoryCommand` | `Result<bool>` |

### Queries

| Feature | Query | Handler Returns |
|---------|-------|-----------------|
| **Colors** | `GetAllColorsQuery` | `Result<List<ColorDto>>` |
| | `GetColorByIdQuery` | `Result<ColorDto>` |
| | `SearchColorsQuery` | `Result<PagedResult<ColorDto>>` |
| **Items** | `GetAllItemsQuery` | `Result<PagedResult<ItemDto>>` |
| | `GetItemByIdQuery` | `Result<ItemDto>` |
| | `SearchItemsQuery` | `Result<PagedResult<ItemDto>>` |
| **Locations** | `GetAllLocationsQuery` | `Result<List<LocationDto>>` |
| | `GetLocationByIdQuery` | `Result<LocationDto>` |
| | `SearchLocationsQuery` | `Result<PagedResult<LocationDto>>` |
| **Orders** | `GetAllOrdersQuery` | `Result<PagedResult<OrderDto>>` |
| | `GetOrderByIdQuery` | `Result<OrderDto>` |
| | `SearchOrdersQuery` | `Result<PagedResult<OrderDto>>` |
| **Roles** | `GetAllRolesQuery` | `Result<List<RoleDto>>` |
| | `GetRoleByIdQuery` | `Result<RoleDto>` |
| | `SearchRolesQuery` | `Result<PagedResult<RoleDto>>` |
| **Users** | `GetAllUsersQuery` | `Result<PagedResult<UserDto>>` |
| | `GetUserByIdQuery` | `Result<UserDto>` |
| | `SearchUsersQuery` | `Result<PagedResult<UserDto>>` |
| **Inventories** | `GetAllInventoriesQuery` | `Result<List<InventoryDto>>` |
| | `GetInventoryByIdQuery` | `Result<InventoryDto>` |
| | `GetItemsInInventoryQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetItemInventoryQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetLowStockItemsQuery` | `Result<List<ItemInventoryDto>>` |
| | `GetActiveInventoriesQuery` | `Result<List<InventoryDto>>` |

### MediatR Pipeline Behaviors (in order)
1. `ValidationBehavior<,>` — FluentValidation before handler
2. `LoggingBehavior<,>` — Request/response logging
3. `PerformanceBehavior<,>` — Slow query detection
4. `TransactionBehavior<,>` — Automatic transaction wrapping

### Base Handler Templates
| Base Handler | Purpose |
|-------------|---------|
| `BaseCreateHandler<TCommand, TEntity>` | Create entity, audit, standard error handling |
| `BaseUpdateHandler<TCommand, TEntity>` | Update with validation, audit |
| `BaseSoftDeleteHandler<TCommand, TEntity>` | Soft delete with audit |
| `BaseGetByIdHandler<TQuery, TEntity, TDto>` | Get by ID with mapping |
| `BaseGetAllHandler<TQuery, TEntity, TDto>` | Get all with mapping |
| `BaseSearchHandler<TQuery, TEntity, TDto>` | Paged search with mapping |

---

## 9. Interfaces & Abstractions

### Repository Interfaces (Application Layer)

| Interface | Extends | Key Methods |
|-----------|---------|-------------|
| `IRepository<TEntity>` | — | Full CRUD, paging, soft delete, `FromSqlRawAsync` |
| `IUserRepository` | `IRepository<User>` | `GetByUsernameAsync`, `GetByEmailAsync`, `ExistsByUsernameAsync`, `ExistsByEmailAsync`, `GetWithRolesAsync` |
| `IRoleRepository` | `IRepository<Role>` | `GetByNameAsync`, `GetWithPermissionsAsync`, `ExistsByNameAsync` |
| `IPermissionRepository` | `IRepository<Permission>` | `HasPermissionAsync`, `GetUserPermissionsBitmaskAsync`, `GetRolePermissionAsync`, `AddRolePermissionAsync` |
| `IOrderRepository` | `IRepository<Order>` | `GetByStatusAsync`, `GetOrderCountByStatusAsync`, `GetWithItemsAsync` |
| `IOrderItemRepository` | `IRepository<OrderItem>` | |
| `IItemRepository` | `IRepository<Item>` | |
| `IColorRepository` | `IRepository<Color>` | |
| `ILocationRepository` | `IRepository<Location>` | |
| `IAuditLogRepository` | `IRepository<AuditLog>` | |
| `IRefreshTokenRepository` | `IRepository<RefreshToken>` | |
| `IInventoryRepository` | `IRepository<Inventory>` | |
| `IItemInventoryRepository` | `IRepository<ItemInventory>` | |
| `IUnitOfWork` | `IDisposable` | All repositories + `SaveChangesAsync`, `BeginTransaction`, `Commit`, `Rollback` |

### Service Interfaces

| Interface | Layer | Implementation |
|-----------|-------|----------------|
| `IJwtService` (Application) | Application | `JwtServiceWrapper` (Infrastructure) ? wraps Framework `JwtService` |
| `IPasswordService` (Application) | Application | `PasswordServiceWrapper` (Infrastructure) ? wraps Framework `PasswordService` |
| `IJwtService` (Framework) | Framework | `JwtService` (RSA 2048-bit JWT) |
| `IPasswordService` (Framework) | Framework | `PasswordService` (BCrypt) |
| `IBitManipulationService` | Framework | `BitManipulationService` |
| `IAuditService` | Application | `AuditService` (Infrastructure) |
| `ICurrentUserService` | Application | `CurrentUserService` (API) |
| `IDateTimeService` | Application | `DateTimeService` (Infrastructure) |

---

## 10. Middleware Pipeline

Order in `Program.cs` (execution order):

```
1. ExceptionHandlerMiddleware     ? Global exception handling
2. EnhancedLoggingMiddleware      ? HTML request logging (NLog)
3. Swagger (dev only)
4. HTTPS Redirection
5. CORS ("AllowAll" policy)
6. Authentication                 ? JWT Bearer validation
7. JwtMiddleware                  ? Extract user context from claims
8. Authorization
9. AuditLoggingMiddleware         ? Log actions to AuditLog table
10. MapControllers
```

---

## 11. Security & Permission System

### Authentication Flow
1. User registers ? `IsEnabled=false`, `IsApproved=false` ? awaits admin approval
2. Admin approves user ? `IsApproved=true`, `IsEnabled=true`
3. User logs in ? receives JWT access token (15 min) + refresh token (7 days)
4. Access token expires ? client calls `/api/auth/refresh` with refresh token
5. Refresh token rotation: old token revoked, new pair issued

### JWT Token Structure
- **Algorithm**: RS256 (RSA 2048-bit)
- **Claims**: `sub` (userId), `name` (username), `email`, `roles`, `permissions` (bitmask)
- **Access Token Expiry**: 15 minutes
- **Refresh Token Expiry**: 7 days
- **Issuer**: `MobileBackendAPI`
- **Audience**: `MobileApp`

### Permission System (Bitwise)
- Each permission is a power of 2 (`1L << N`)
- Role has `RolePermission.PermissionsBitmask` = bitwise OR of all assigned permissions
- Check: `(bitmask & permissionBit) == permissionBit` ? O(1)
- The `[RequirePermission(PermissionType.XYZ)]` attribute on controller actions checks via `IPermissionRepository.HasPermissionAsync`

### Seeded Roles
| Role | Description | Permissions |
|------|-------------|------------|
| Admin | Full system access | ALL permissions (bitmask = sum of all) |
| Manager | Operational management | Color, Item, Order, Location, RefundView, UserView |
| User | Basic access | View-only on Colors, Items, Locations, Orders |

---

## 12. Database & Infrastructure

### Database
- **Provider**: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4
- **Connection**: Configured in `appsettings.json` ? `ConnectionStrings:DefaultConnection`
- **Railway Deployment**: Auto-parses `DATABASE_URL` env var

### Global Query Filters
All entities implementing `ISoftDelete` have automatic EF filter: `WHERE IsDeleted = false`

### Migrations
| Migration | Description |
|-----------|-------------|
| `20251101094605_InitialCreate` | All core tables |
| `20251103210811_AddInventorySystem` | Inventory, ItemInventory tables |
| `20251103210956_AddInventorySystemFixed` | Fixes to inventory system |

### Entity Configurations
Each entity has a dedicated `*Configuration.cs` file in `Data/Configurations/` with:
- Property constraints (max length, required, etc.)
- Indexes
- Relationships
- Default values
- String conversion for enums (`OrderStatus` stored as string)

---

## 13. Seeded Data & Default Credentials

### Users
| Username | Password | Role | IsEnabled | IsApproved |
|----------|----------|------|-----------|------------|
| `admin` | `Admin@123` | Admin | ? | ? |
| `manager` | `Manager@123` | Manager | ? | ? |
| `user` | `User@123` | User | ? | ? |

### Seeded Permissions
30 permissions across categories: Color (4), Item (4), Order (5), Refund (2), User (5), Location (4), Role (5), System (3).

### Seeded Sample Data
- Colors: Red (#FF0000), Blue (#0000FF), Green (#00FF00)
- Locations: 3 default locations
- Items: 10 pet store items (PF-001 through PG-010)

---

## 14. Cross-Cutting Concerns

### Validation
- FluentValidation validators in `Validators/` folder per feature
- `ValidationBehavior` in MediatR pipeline auto-validates before handler
- `ValidateModelAttribute` global filter on all controllers

### Audit Logging
- `AuditLoggingMiddleware` logs HTTP requests
- `AuditHelper` + `IAuditService` log business operations (CRUD, login, refunds)
- Stored in `AuditLogs` table with old/new values as JSON

### Logging
- NLog with HTML rendering (`HtmlRequestLayoutRenderer`)
- `LoggingBehavior` logs all MediatR requests
- `PerformanceBehavior` flags slow queries (>500ms)
- HTML log files at `C:\AppLogs\ScanPet\`

### Error Handling
- `ExceptionHandlerMiddleware` — global exception catch
- `Result<T>` pattern — no exceptions for business logic failures
- Consistent error envelope across all endpoints

### Soft Delete
- All main entities implement `ISoftDelete`
- Global EF query filter auto-excludes `IsDeleted = true`
- `SoftDeleteAsync` in generic repository sets fields + timestamp

---

## 15. Unit Tests

**Project**: `MobileBackend.UnitTests`  
**Framework**: xUnit + Moq + FluentAssertions

| Test File | Covers |
|-----------|--------|
| `CreateColorCommandHandlerTests` | Color creation |
| `UpdateColorCommandHandlerTests` | Color update |
| `DeleteColorCommandHandlerTests` | Color soft delete |
| `GetAllColorsQueryHandlerTests` | Get all colors |
| `GetColorByIdQueryHandlerTests` | Get color by ID |
| `CreateItemCommandHandlerTests` | Item creation |
| `CreateOrderCommandHandlerTests` | Order creation |
| `RefundOrderItemCommandHandlerTests` | Order item refund |

---

## 16. NuGet Packages & Frameworks

### Domain Layer
*(No external packages — pure C#)*

### Application Layer
| Package | Version | Purpose |
|---------|---------|---------|
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.1 | Object mapping |
| FluentValidation | 12.0.0 | Request validation |
| FluentValidation.DependencyInjectionExtensions | 12.0.0 | DI integration |
| MediatR | 13.1.0 | CQRS mediator |

### Infrastructure Layer
| Package | Version | Purpose |
|---------|---------|---------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | PostgreSQL provider |
| Microsoft.EntityFrameworkCore.Tools | 9.0.10 | EF migrations |
| Microsoft.Extensions.Configuration.Json | 9.0.10 | Config |

### Framework Layer
| Package | Version | Purpose |
|---------|---------|---------|
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| System.IdentityModel.Tokens.Jwt | 8.14.0 | JWT tokens |
| Microsoft.Extensions.Configuration.* | 9.0.10 | Config abstractions |

### API Layer
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT auth middleware |
| NLog.Web.AspNetCore | 6.0.5 | Logging |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger/OpenAPI |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | (for migrations CLI) |

---

## 17. File Hierarchy (Complete)

```
D:\Project\
??? MobileBackend.sln
??? SOLUTION_KNOWLEDGE_BASE.md          ? THIS FILE
?
??? src/
?   ??? Domain/MobileBackend.Domain/
?   ?   ??? MobileBackend.Domain.csproj
?   ?   ??? Common/
?   ?   ?   ??? BaseEntity.cs
?   ?   ?   ??? ISoftDelete.cs
?   ?   ??? Entities/
?   ?   ?   ??? AuditLog.cs
?   ?   ?   ??? Color.cs
?   ?   ?   ??? Inventory.cs
?   ?   ?   ??? Item.cs
?   ?   ?   ??? ItemInventory.cs
?   ?   ?   ??? Location.cs
?   ?   ?   ??? Order.cs
?   ?   ?   ??? OrderItem.cs
?   ?   ?   ??? Permission.cs
?   ?   ?   ??? RefreshToken.cs
?   ?   ?   ??? Role.cs
?   ?   ?   ??? RolePermission.cs
?   ?   ?   ??? User.cs
?   ?   ?   ??? UserRole.cs
?   ?   ??? Enums/
?   ?       ??? OrderItemStatus.cs
?   ?       ??? OrderStatus.cs
?   ?       ??? PermissionType.cs
?   ?
?   ??? Application/MobileBackend.Application/
?   ?   ??? MobileBackend.Application.csproj
?   ?   ??? DependencyInjection.cs
?   ?   ??? Common/
?   ?   ?   ??? Behaviors/
?   ?   ?   ?   ??? LoggingBehavior.cs
?   ?   ?   ?   ??? PerformanceBehavior.cs
?   ?   ?   ?   ??? TransactionBehavior.cs
?   ?   ?   ?   ??? ValidationBehavior.cs
?   ?   ?   ??? Constants/
?   ?   ?   ?   ??? AuditConstants.cs
?   ?   ?   ?   ??? ErrorMessages.cs
?   ?   ?   ??? Extensions/
?   ?   ?   ?   ??? ResultExtensions.cs
?   ?   ?   ??? Handlers/
?   ?   ?   ?   ??? BaseCreateHandler.cs
?   ?   ?   ?   ??? BaseGetAllHandler.cs
?   ?   ?   ?   ??? BaseGetByIdHandler.cs
?   ?   ?   ?   ??? BaseSearchHandler.cs
?   ?   ?   ?   ??? BaseSoftDeleteHandler.cs
?   ?   ?   ?   ??? BaseUpdateHandler.cs
?   ?   ?   ??? Helpers/
?   ?   ?   ?   ??? AuditHelper.cs
?   ?   ?   ??? Interfaces/
?   ?   ?   ?   ??? IAuditService.cs
?   ?   ?   ?   ??? ICurrentUserService.cs
?   ?   ?   ?   ??? IDateTimeService.cs
?   ?   ?   ??? Mappings/
?   ?   ?   ?   ??? AuthMappingProfile.cs
?   ?   ?   ??? Queries/
?   ?   ?       ??? BaseGetByIdQuery.cs
?   ?   ?       ??? BasePagedQuery.cs
?   ?   ?       ??? BaseSearchQuery.cs
?   ?   ??? DTOs/
?   ?   ?   ??? Auth/ (LoginRequestDto, LoginResponseDto, RegisterRequestDto, RefreshTokenRequestDto, ChangePasswordDto)
?   ?   ?   ??? Colors/ (ColorDto)
?   ?   ?   ??? Common/ (Result, PagedResult, ValidationError)
?   ?   ?   ??? Inventories/ (CreateInventoryDto, UpdateInventoryDto, InventoryDto, ItemInventoryDto, SetItemInventoryDto, UpdateItemQuantityDto, IncrementInventoryDto, DecrementInventoryDto, AdjustInventoryDto, TransferInventoryDto)
?   ?   ?   ??? Items/ (ItemDto)
?   ?   ?   ??? Locations/ (LocationDto)
?   ?   ?   ??? Orders/ (OrderDto, OrderItemDto)
?   ?   ?   ??? Roles/ (RoleDto, RoleDtoConsolidated, AssignPermissionsDto)
?   ?   ?   ??? Users/ (UserDto, UserDtoConsolidated, UserApprovalDto, ToggleUserStatusDto, UpdateUserRoleDto)
?   ?   ??? Features/
?   ?   ?   ??? Auth/Commands/ (Login, Logout, RefreshToken, Register)
?   ?   ?   ??? Colors/Commands/ (CreateColor, DeleteColor, UpdateColor)
?   ?   ?   ??? Colors/Queries/ (GetAllColors, GetColorById, SearchColors)
?   ?   ?   ??? Inventories/Commands/ (AdjustInventory, CreateInventory, DeleteInventory, SetItemInventory, TransferInventory, UpdateInventory)
?   ?   ?   ??? Inventories/Queries/ (GetActiveInventories, GetAllInventories, GetInventoryById, GetItemInventory, GetItemsInInventory, GetLowStockItems)
?   ?   ?   ??? Items/Commands/ (CreateItem, DeleteItem, UpdateItem)
?   ?   ?   ??? Items/Queries/ (GetAllItems, GetItemById, SearchItems)
?   ?   ?   ??? Locations/Commands/ (CreateLocation, DeleteLocation, UpdateLocation)
?   ?   ?   ??? Locations/Queries/ (GetAllLocations, GetLocationById, SearchLocations)
?   ?   ?   ??? Orders/Commands/ (CancelOrder, ConfirmOrder, CreateOrder, RefundOrderItem)
?   ?   ?   ??? Orders/Queries/ (GetAllOrders, GetOrderById, SearchOrders)
?   ?   ?   ??? Roles/Commands/ (AssignPermissions, CreateRole, DeleteRole, UpdateRole)
?   ?   ?   ??? Roles/Queries/ (GetAllRoles, GetRoleById, SearchRoles)
?   ?   ?   ??? Users/Commands/ (ApproveUser, CreateUser, ToggleUserStatus, UpdateUserRole)
?   ?   ?   ??? Users/Queries/ (GetAllUsers, GetUserById, SearchUsers)
?   ?
?   ??? Infrastructure/MobileBackend.Infrastructure/
?   ?   ??? MobileBackend.Infrastructure.csproj
?   ?   ??? DependencyInjection.cs
?   ?   ??? Data/
?   ?   ?   ??? ApplicationDbContext.cs
?   ?   ?   ??? ApplicationDbContextFactory.cs
?   ?   ?   ??? DbSeeder.cs
?   ?   ?   ??? Configurations/ (18 entity configuration files)
?   ?   ??? Migrations/ (3 migrations + snapshot)
?   ?   ??? Repositories/ (GenericRepository + 11 specific repos + UnitOfWork)
?   ?   ??? Services/ (AuditService, DateTimeService, JwtServiceWrapper, PasswordServiceWrapper)
?   ?
?   ??? Framework/MobileBackend.Framework/
?       ??? MobileBackend.Framework.csproj
?       ??? DependencyInjection.cs
?       ??? Security/
?           ??? BitManipulationService.cs
?           ??? IBitManipulationService.cs
?           ??? IJwtService.cs
?           ??? IPasswordService.cs
?           ??? JwtService.cs
?           ??? PasswordService.cs
?           ??? Models/TokenModels.cs
?
??? tests/MobileBackend.UnitTests/
?   ??? MobileBackend.UnitTests.csproj
?   ??? TestBase.cs
?   ??? UnitTest1.cs
?   ??? Features/ (Colors + Items + Orders tests)
?
??? docs/ (50+ markdown files, Postman collections, scripts)
```

---

## 18. Potential Refactoring & Improvement Notes

### ?? Issues / Concerns

1. ~~**Deprecated `Item.Quantity` field**~~: ? **RESOLVED in Session 5** — `Quantity` property removed from `Item` entity, commands, DTOs, and all handlers. All quantity tracking now uses `ItemInventory` exclusively.

2. **`IInventoryRepository` and `IItemInventoryRepository` missing from Infrastructure DI**: The `AddRepositories()` in `Infrastructure/DependencyInjection.cs` does NOT register `IInventoryRepository` or `IItemInventoryRepository`, but they are in `IUnitOfWork`. If they're resolved only through `UnitOfWork`, this may work, but standalone injection would fail.

3. **Duplicate JWT/Password interfaces**: Both `Application.Interfaces.IJwtService` and `Framework.Security.IJwtService` exist. Infrastructure provides wrappers (`JwtServiceWrapper`, `PasswordServiceWrapper`) to bridge them — this works but adds complexity.

4. **`AuthController` doesn't extend `BaseApiController`**: `AuthController` extends `ControllerBase` directly while all other controllers extend `BaseApiController`. This means auth endpoints have inconsistent response formatting.

5. ~~**`RefundOrderItemRequest` class defined inside `OrdersController.cs`**~~: ? **RESOLVED in Session 6** — Moved to `Application/DTOs/Orders/RefundOrderItemRequest.cs`.

6. **`ChangePasswordDto` exists but no controller endpoint exposes it**: The feature is defined but not wired.

7. **No `SearchItems` endpoint on `ItemsController`**: The query handler `SearchItemsQuery`/`SearchItemsQueryHandler` exists but there's no controller action to call it.

8. **No `SearchLocations` endpoint on `LocationsController`**: Same issue — handler exists but no route.

9. **No `SearchOrders` endpoint on `OrdersController`**: Handler exists but not exposed.

10. **No `SearchUsers` endpoint on `UsersController`**: Handler exists but not exposed.

11. **No `SearchRoles` endpoint on `RolesController`**: Handler exists but not exposed.

12. **`RoleDtoConsolidated` and `UserDtoConsolidated`**: These DTOs exist but appear unused / leftover from refactoring.

13. **`[RequirePermission]` attribute is defined but NOT applied to any controller actions**: The permission system is built but endpoints are not protected by it. Any authenticated user can access all endpoints.

14. ~~**`ItemsController` has `[ApiController]` attribute AND inherits from `BaseApiController`**~~: ? **RESOLVED in Session 6** — Removed redundant `[ApiController]`.

15. ~~**OrdersController has both `[Authorize]` and inherits `BaseApiController`**~~: ? **RESOLVED in Session 6** — Removed redundant attributes.

### ?? Improvements

1. **Apply `[RequirePermission]` to all CRUD endpoints** to enforce the bitwise permission system.
2. **Expose all Search endpoints** that have handlers but no controller routes.
3. ~~**Remove deprecated `Item.Quantity`**~~: ? Resolved in Session 5.
4. **Make `AuthController` extend `BaseApiController`** for consistent response formatting.
5. ~~**Move `RefundOrderItemRequest` to Application DTOs layer.**~~: ? Resolved in Session 6.
6. **Add `ChangePassword` endpoint to `AuthController`.**
7. **Clean up unused `*Consolidated` DTOs.**
8. **Add response caching** using the `ResponseCacheAttribute` filter that exists but isn't applied.
9. **Add rate limiting** — configured in `appsettings.json` but not implemented.
10. **Consider adding an `IInventoryRepository`/`IItemInventoryRepository` registration** in DI for direct injection scenarios.
