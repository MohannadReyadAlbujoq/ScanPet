# N+1 Query Problems Analysis

## Executive Summary
Analysis of all command handlers and query handlers for potential N+1 query problems, particularly focusing on `Include`, `ThenInclude`, and lazy loading issues.

---

## ? Already Fixed (No N+1 Issues)

### 1. **GetAllUsersQueryHandler** ?
```csharp
// Uses optimized GetPagedWithRolesAsync - single query with Include
var (items, totalCount) = await _unitOfWork.Users.GetPagedWithRolesAsync(
    request.PageNumber,
    request.PageSize,
    cancellationToken);
```
**Status:** ? **SAFE** - Uses repository method with Include

---

### 2. **GetAllRolesQueryHandler** ?
```csharp
// Uses GetAllWithPermissionsAsync - single query with Include
var roles = await _unitOfWork.Roles.GetAllWithPermissionsAsync(cancellationToken);
```
**Status:** ? **SAFE** - Uses repository method with Include for RolePermissions and UserRoles

---

### 3. **GetAllItemsQueryHandler** ?
```csharp
// Uses GetAllWithColorsAsync - single query with Include
var items = await _unitOfWork.Items.GetAllWithColorsAsync(cancellationToken);
```
**Status:** ? **SAFE** - Uses repository method with Include

---

### 4. **GetAllOrdersQueryHandler** ?
```csharp
// Uses GetAllWithLocationsAsync - single query with Include
var orders = await _unitOfWork.Orders.GetAllWithLocationsAsync(cancellationToken);
```
**Status:** ? **SAFE** - Uses repository method with Include

---

### 5. **GetAllColorsQueryHandler** ?
```csharp
// Uses GetAllWithItemCountsAsync - efficient SQL COUNT aggregation
var colorsWithCounts = await _unitOfWork.Colors.GetAllWithItemCountsAsync(cancellationToken);
```
**Status:** ? **SAFE** - Uses server-side aggregation in repository

---

### 6. **GetAllLocationsQueryHandler** ?
```csharp
// Uses GetAllWithOrderCountsAsync - efficient SQL COUNT aggregation
var locationsWithCounts = await _unitOfWork.Locations.GetAllWithOrderCountsAsync(cancellationToken);
```
**Status:** ? **SAFE** - Uses server-side aggregation in repository

---

## ?? Potential N+1 Issues Found

### 1. **LoginCommandHandler** - CRITICAL N+1 ??

**Location:** `src/Application/MobileBackend.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`

**Problem:**
```csharp
// Line 51-52: Two separate queries
var roles = await _unitOfWork.Roles.GetRolesByUserIdAsync(user.Id, cancellationToken);
var permissionsBitmask = await _unitOfWork.Permissions.GetUserPermissionBitmaskAsync(user.Id, cancellationToken);
```

**Issue:**
- `GetRolesByUserIdAsync` executes a query with Include
- `GetUserPermissionBitmaskAsync` executes another query with Join
- Both queries could be combined into one

**Impact:** Medium (happens on every login)

---

### 2. **RefreshTokenCommandHandler** - CRITICAL N+1 ??

**Location:** `src/Application/MobileBackend.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs`

**Problem:**
```csharp
// Lines 52-53: Two separate queries
var roles = await _unitOfWork.Roles.GetRolesByUserIdAsync(user.Id, cancellationToken);
var permissionsBitmask = await _unitOfWork.Permissions.GetUserPermissionBitmaskAsync(user.Id, cancellationToken);
```

**Issue:**
- Same as Login - executes two separate queries
- Could be optimized into one

**Impact:** Medium (happens on every token refresh)

---

### 3. **GetByIdWithRolesAsync Usage** - POTENTIAL N+1 ??

**Location:** Multiple command handlers

**Problem:**
```csharp
// UserRepository.GetByIdWithRolesAsync
return await _dbSet
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
```

**Issue:**
- Uses `ThenInclude` which creates a second join
- Not necessarily N+1, but could be optimized

**Impact:** Low (only one user loaded at a time)

---

### 4. **GetWithItemsAsync in OrderRepository** - POTENTIAL N+1 ??

**Location:** `src/Infrastructure/MobileBackend.Infrastructure/Repositories/OrderRepository.cs`

**Problem:**
```csharp
public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
{
    return await _dbSet
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)  // ?? ThenInclude - second join
        .Include(o => o.Location)
        .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
}
```

**Issue:**
- Uses `ThenInclude` which creates additional joins
- Could potentially load unnecessary data

**Impact:** Low (only one order at a time, used in GetOrderByIdQueryHandler)

**Used In:**
- `GetOrderByIdQueryHandler` ?
- `CancelOrderCommandHandler` ?
- `ConfirmOrderCommandHandler` ?

---

### 5. **CreateOrderCommandHandler** - LOOP WITH QUERIES ??

**Location:** `src/Application/MobileBackend.Application/Features/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs`

**Problem:**
```csharp
// Lines 57-64: Loop with await inside
foreach (var itemDto in request.OrderItems)
{
    var item = await _unitOfWork.Items.GetByIdAsync(itemDto.ItemId ?? Guid.Empty);
    // ... validation and processing
}
```

**Issue:**
- Classic N+1 problem - one query per item
- Should fetch all items in one query before the loop

**Impact:** HIGH ?? (creates N queries for N items)

---

### 6. **CancelOrderCommandHandler** - LOOP WITH QUERIES ??

**Location:** `src/Application/MobileBackend.Application/Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs`

**Problem:**
```csharp
// Lines 48-55: Loop with await inside
if (order.OrderItems != null)
{
    foreach (var orderItem in order.OrderItems)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(orderItem.ItemId);
        // ... restore quantity
    }
}
```

**Issue:**
- Classic N+1 problem - one query per order item
- Items already have ItemId, should fetch all at once

**Impact:** HIGH ?? (creates N queries for N order items)

---

### 7. **UpdateUserRoleCommandHandler** - Multiple Single Queries ??

**Location:** `src/Application/MobileBackend.Application/Features/Users/Commands/UpdateUserRole/UpdateUserRoleCommandHandler.cs`

**Problem:**
```csharp
// Lines 34-49: Three separate queries and a loop
var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
var existingUserRoles = await _unitOfWork.Users.GetActiveUserRolesAsync(request.UserId, cancellationToken);

foreach (var existingRole in existingUserRoles)
{
    _unitOfWork.Users.RemoveUserRole(existingRole);
}
```

**Issue:**
- Three separate queries
- Could potentially be optimized

**Impact:** Low (rare operation, small number of roles per user)

---

## ?? Critical N+1 Issues to Fix

### Priority 1: CreateOrderCommandHandler ??????
**Impact:** CRITICAL
**Reason:** Called on every order creation, loops through items

**Fix:**
```csharp
// BEFORE (N+1):
foreach (var itemDto in request.OrderItems)
{
    var item = await _unitOfWork.Items.GetByIdAsync(itemDto.ItemId ?? Guid.Empty);
}

// AFTER (Single Query):
var itemIds = request.OrderItems.Select(oi => oi.ItemId ?? Guid.Empty).ToList();
var items = await _unitOfWork.Items.FindAsync(i => itemIds.Contains(i.Id), cancellationToken);
var itemsDict = items.ToDictionary(i => i.Id);

foreach (var itemDto in request.OrderItems)
{
    if (!itemsDict.TryGetValue(itemDto.ItemId ?? Guid.Empty, out var item))
    {
        return Result<Guid>.FailureResult($"Item with ID {itemDto.ItemId} not found", 404);
    }
    // ... continue with item
}
```

---

### Priority 2: CancelOrderCommandHandler ??????
**Impact:** CRITICAL
**Reason:** Called on every order cancellation, loops through items

**Fix:**
```csharp
// BEFORE (N+1):
foreach (var orderItem in order.OrderItems)
{
    var item = await _unitOfWork.Items.GetByIdAsync(orderItem.ItemId);
}

// AFTER (Single Query):
var itemIds = order.OrderItems.Select(oi => oi.ItemId).ToList();
var items = await _unitOfWork.Items.FindAsync(i => itemIds.Contains(i.Id), cancellationToken);
var itemsDict = items.ToDictionary(i => i.Id);

foreach (var orderItem in order.OrderItems)
{
    if (itemsDict.TryGetValue(orderItem.ItemId, out var item))
    {
        item.Quantity += orderItem.Quantity;
        _unitOfWork.Items.Update(item);
    }
}
```

---

### Priority 3: LoginCommandHandler & RefreshTokenCommandHandler ????
**Impact:** HIGH
**Reason:** Called on every login/refresh, happens frequently

**Fix - Option 1 (Repository Method):**
```csharp
// New interface method in IUserRepository:
Task<(IEnumerable<string> Roles, long PermissionsBitmask)> GetUserRolesAndPermissionsAsync(
    Guid userId, 
    CancellationToken cancellationToken = default);

// Implementation:
public async Task<(IEnumerable<string> Roles, long PermissionsBitmask)> GetUserRolesAndPermissionsAsync(
    Guid userId, 
    CancellationToken cancellationToken = default)
{
    var query = await _context.Set<UserRole>()
        .Where(ur => ur.UserId == userId)
        .Join(_context.Set<Role>(),
            ur => ur.RoleId,
            r => r.Id,
            (ur, r) => new { ur.RoleId, r.Name })
        .Join(_context.Set<RolePermission>(),
            r => r.RoleId,
            rp => rp.RoleId,
            (r, rp) => new { r.Name, rp.PermissionsBitmask })
        .ToListAsync(cancellationToken);

    var roles = query.Select(q => q.Name).Distinct().ToList();
    var permissionsBitmask = query.Select(q => q.PermissionsBitmask)
        .Aggregate(0L, (acc, val) => acc | val);

    return (roles, permissionsBitmask);
}

// Usage in handlers:
var (roles, permissionsBitmask) = await _unitOfWork.Users.GetUserRolesAndPermissionsAsync(
    user.Id, 
    cancellationToken);
```

---

## ?? Summary

### By Priority:
1. **CRITICAL (Fix Immediately)**
   - CreateOrderCommandHandler (N+1 in loop)
   - CancelOrderCommandHandler (N+1 in loop)

2. **HIGH (Fix Soon)**
   - LoginCommandHandler (2 separate queries)
   - RefreshTokenCommandHandler (2 separate queries)

3. **LOW (Monitor)**
   - UpdateUserRoleCommandHandler (multiple queries, rare operation)
   - GetWithItemsAsync (ThenInclude, single entity load)
   - GetByIdWithRolesAsync (ThenInclude, single entity load)

### Total Issues Found:
- **Critical:** 2
- **High:** 2
- **Low:** 3

### Already Optimized:
- ? GetAllUsersQueryHandler
- ? GetAllRolesQueryHandler
- ? GetAllItemsQueryHandler
- ? GetAllOrdersQueryHandler
- ? GetAllColorsQueryHandler
- ? GetAllLocationsQueryHandler

---

## ?? Recommended Action Plan

1. **Immediate (Today)**
   - Fix CreateOrderCommandHandler
   - Fix CancelOrderCommandHandler

2. **This Week**
   - Fix LoginCommandHandler
   - Fix RefreshTokenCommandHandler
   - Add new repository method for combined roles/permissions query

3. **Monitor**
   - Track performance of ThenInclude queries
   - Consider optimization if performance degrades

4. **Testing**
   - Write unit tests for new optimized methods
   - Performance test with large datasets
   - Verify SQL queries generated by EF Core

---

## ?? Notes

- All query handlers using specific `GetAllWith*Async` methods are already optimized
- Color and Location repositories use efficient SQL COUNT aggregation
- Main issues are in command handlers with loops
- Auth handlers could benefit from a single combined query

