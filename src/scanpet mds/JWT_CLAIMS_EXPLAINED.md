# ?? JWT CLAIMS EXPLAINED

**Date:** December 2024  
**Purpose:** Understanding JWT claims in ScanPet backend  
**Token Type:** Access Token (RS256)

---

## ?? WHAT ARE CLAIMS?

**Claims** are key-value pairs embedded in JWT tokens that contain information about the user and their session.

### Why Claims?
- ? **Stateless:** Server doesn't need to query database for every request
- ? **Fast:** Authorization decisions made from token data
- ? **Secure:** Signed with private key, tamper-proof
- ? **Self-contained:** All user info in the token

---

## ?? CLAIMS IN YOUR SYSTEM

### Standard JWT Claims (Registered)

These are official JWT standard claims:

```json
{
  "iss": "ScanPetMobileBackend",     // Issuer - who created the token
  "aud": "ScanPetMobileApp",         // Audience - who can use the token
  "sub": "user-guid-here",           // Subject - user ID
  "iat": 1703001234,                 // Issued At - timestamp
  "exp": 1703002134,                 // Expiration - timestamp (15 min later)
  "nbf": 1703001234                  // Not Before - timestamp
}
```

### Custom Claims (Your System)

```json
{
  "nameid": "user-guid-here",              // User ID (ASP.NET standard)
  "unique_name": "admin@scanpet.com",      // Email
  "email": "admin@scanpet.com",            // Email (duplicate for clarity)
  "given_name": "John",                    // First name
  "family_name": "Doe",                    // Last name
  "role": "Admin",                         // Role name
  "permissions": [                         // Array of permissions
    "Users.Create",
    "Users.View",
    "Orders.Refund",
    // ... all user permissions
  ]
}
```

---

## ?? COMPLETE TOKEN EXAMPLE

### Access Token Payload

```json
{
  // Standard Claims
  "iss": "ScanPetMobileBackend",
  "aud": "ScanPetMobileApp",
  "sub": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "iat": 1703001234,
  "exp": 1703002134,
  "nbf": 1703001234,
  
  // User Identity Claims
  "nameid": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "unique_name": "admin@scanpet.com",
  "email": "admin@scanpet.com",
  "given_name": "John",
  "family_name": "Admin",
  
  // Authorization Claims
  "role": "Admin",
  "permissions": [
    "Users.Create",
    "Users.View",
    "Users.Update",
    "Users.Delete",
    "Users.Approve",
    "Roles.Create",
    "Roles.View",
    "Roles.Update",
    "Roles.Delete",
    "Roles.AssignPermissions",
    "Colors.Create",
    "Colors.View",
    "Colors.Update",
    "Colors.Delete",
    "Locations.Create",
    "Locations.View",
    "Locations.Update",
    "Locations.Delete",
    "Items.Create",
    "Items.View",
    "Items.Update",
    "Items.Delete",
    "Orders.Create",
    "Orders.View",
    "Orders.Update",
    "Orders.Delete",
    "Orders.Confirm",
    "Orders.Cancel",
    "Orders.Refund",
    "Audit.View",
    "Audit.Export"
  ],
  
  // Session Claims
  "jti": "unique-token-id",              // JWT ID (for revocation)
  "device": "iOS App"                    // Device information
}
```

---

## ?? CLAIM TYPES BREAKDOWN

### 1. **Standard Claims (RFC 7519)**

#### iss (Issuer)
- **Type:** String
- **Value:** `"ScanPetMobileBackend"`
- **Purpose:** Identify who created the token
- **Used For:** Token validation, multi-service architecture

#### aud (Audience)
- **Type:** String
- **Value:** `"ScanPetMobileApp"`
- **Purpose:** Identify intended recipient
- **Used For:** Prevent token reuse across services

#### sub (Subject)
- **Type:** String (GUID)
- **Value:** `"f47ac10b-58cc-4372-a567-0e02b2c3d479"`
- **Purpose:** User identifier
- **Used For:** Identify user across all requests

#### iat (Issued At)
- **Type:** Number (Unix timestamp)
- **Value:** `1703001234`
- **Purpose:** When token was created
- **Used For:** Age verification, debugging

#### exp (Expiration)
- **Type:** Number (Unix timestamp)
- **Value:** `1703002134` (iat + 900 seconds = 15 min)
- **Purpose:** When token becomes invalid
- **Used For:** Security - force re-authentication

#### nbf (Not Before)
- **Type:** Number (Unix timestamp)
- **Value:** `1703001234` (same as iat)
- **Purpose:** Token not valid before this time
- **Used For:** Clock skew tolerance

---

### 2. **Identity Claims**

#### nameid (Name Identifier)
- **Type:** String (GUID)
- **Purpose:** ASP.NET standard for user ID
- **Used In:** `User.Identity.Name`
- **Access:** `HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)`

#### unique_name (Unique Name)
- **Type:** String (Email)
- **Purpose:** Unique user identifier (human-readable)
- **Used For:** Display in UI, logging

#### email
- **Type:** String
- **Purpose:** User's email address
- **Used For:** Communication, identification

#### given_name (First Name)
- **Type:** String
- **Purpose:** User's first name
- **Used For:** Personalization, display

#### family_name (Last Name)
- **Type:** String
- **Purpose:** User's last name
- **Used For:** Full name display

---

### 3. **Authorization Claims**

#### role
- **Type:** String
- **Values:** `"Admin"`, `"Manager"`, `"User"`
- **Purpose:** User's primary role
- **Used For:** High-level authorization
- **Access:** `User.IsInRole("Admin")`

#### permissions
- **Type:** Array of strings
- **Values:** `["Users.Create", "Orders.Refund", ...]`
- **Purpose:** Fine-grained authorization
- **Used For:** Permission checks in `[RequirePermission]`
- **Count:** 30+ permissions available

---

### 4. **Session Claims**

#### jti (JWT ID)
- **Type:** String (GUID)
- **Purpose:** Unique token identifier
- **Used For:** Token revocation, blacklisting

#### device
- **Type:** String
- **Purpose:** Device/app information
- **Used For:** Security monitoring, debugging

---

## ?? CLAIMS BY ROLE

### Admin Claims
```json
{
  "role": "Admin",
  "permissions": [
    // All 30+ permissions
    "Users.Create", "Users.View", "Users.Update", "Users.Delete", "Users.Approve",
    "Roles.Create", "Roles.View", "Roles.Update", "Roles.Delete", "Roles.AssignPermissions",
    "Colors.Create", "Colors.View", "Colors.Update", "Colors.Delete",
    "Locations.Create", "Locations.View", "Locations.Update", "Locations.Delete",
    "Items.Create", "Items.View", "Items.Update", "Items.Delete",
    "Orders.Create", "Orders.View", "Orders.Update", "Orders.Delete", 
    "Orders.Confirm", "Orders.Cancel", "Orders.Refund",
    "Audit.View", "Audit.Export"
  ]
}
```

### Manager Claims
```json
{
  "role": "Manager",
  "permissions": [
    "Colors.View",
    "Locations.View",
    "Items.View",
    "Orders.Create", "Orders.View", "Orders.Update", "Orders.Confirm", "Orders.Cancel",
    "Users.View"
  ]
}
```

### User Claims
```json
{
  "role": "User",
  "permissions": [
    "Colors.View",
    "Locations.View",
    "Items.View",
    "Orders.View",
    "Users.View"
  ]
}
```

---

## ?? HOW CLAIMS ARE CREATED

### In JwtService.cs

```csharp
public string GenerateAccessToken(User user, List<string> permissions)
{
    var claims = new List<Claim>
    {
        // Standard claims
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        
        // Identity claims
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        
        // Authorization claims
        new Claim(ClaimTypes.Role, user.Role.Name)
    };
    
    // Add permissions
    foreach (var permission in permissions)
    {
        claims.Add(new Claim("permissions", permission));
    }
    
    // Create token
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(15),
        Issuer = "ScanPetMobileBackend",
        Audience = "ScanPetMobileApp",
        SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256)
    };
    
    return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
}
```

---

## ?? HOW CLAIMS ARE USED

### 1. In Authorization Filters

```csharp
[RequirePermission("Orders.Refund")]
public async Task<IActionResult> RefundOrderItem(string serialNumber)
{
    // Filter checks if user has "Orders.Refund" in permissions claim
    // If not, returns 403 Forbidden
    
    // Your code here
}
```

### 2. In CurrentUserService

```csharp
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId => Guid.Parse(
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString()
    );
    
    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.Email);
        
    public string? Role => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.Role);
        
    public List<string> Permissions => _httpContextAccessor.HttpContext?.User
        .FindAll("permissions")
        .Select(c => c.Value)
        .ToList() ?? new List<string>();
}
```

### 3. In Controllers

```csharp
public class OrdersController : ControllerBase
{
    public async Task<IActionResult> GetMyOrders()
    {
        // Access current user ID from claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Get orders for this user
        var orders = await _mediator.Send(new GetOrdersByUserQuery 
        { 
            UserId = Guid.Parse(userId) 
        });
        
        return Ok(orders);
    }
}
```

---

## ?? SECURITY BENEFITS OF CLAIMS

### 1. **Stateless Authentication**
- No database lookup per request
- Server doesn't store session
- Scales horizontally easily

### 2. **Tamper-Proof**
- Signed with private key
- Any modification invalidates signature
- Frontend cannot fake claims

### 3. **Self-Contained**
- All auth info in token
- No session storage needed
- Works across distributed systems

### 4. **Fine-Grained Authorization**
- Check specific permissions
- Not just role-based
- Flexible security model

---

## ?? CLAIM SIZE CONSIDERATIONS

### Token Size Impact

**Typical Token Size:**
- Admin (30+ permissions): ~2-3 KB
- Manager (15 permissions): ~1.5 KB
- User (5 permissions): ~1 KB

**Considerations:**
- ? Stored in HTTP headers (not cookies)
- ? Gzipped by browsers automatically
- ?? Keep permission names short
- ?? Don't add unnecessary claims

---

## ?? VIEWING YOUR CLAIMS

### Option 1: JWT.io
1. Go to https://jwt.io
2. Paste your token
3. View decoded payload (claims)

### Option 2: Postman
1. Send authenticated request
2. In Tests tab, add:
```javascript
const token = pm.response.json().accessToken;
const decoded = pm.response.json();
console.log('Claims:', decoded);
```

### Option 3: C# Code
```csharp
var token = "your-token-here";
var handler = new JwtSecurityTokenHandler();
var jsonToken = handler.ReadJwtToken(token);
var claims = jsonToken.Claims;

foreach (var claim in claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}
```

---

## ?? SUMMARY

**Total Claim Types:** 3 categories
1. **Standard (6):** iss, aud, sub, iat, exp, nbf
2. **Identity (5):** nameid, unique_name, email, given_name, family_name
3. **Authorization (2):** role, permissions

**Why Create Claims:**
- ? Stateless authentication
- ? Fast authorization
- ? Self-contained tokens
- ? Fine-grained permissions
- ? Scalability

**Best Practices:**
- ? Keep tokens short-lived (15 min)
- ? Use refresh tokens for long sessions
- ? Include only necessary claims
- ? Validate claims on every request
- ? Sign with strong algorithm (RS256)

---

**Status:** ? **CLAIMS DOCUMENTATION COMPLETE**  
**Token Type:** JWT (RS256)  
**Claims Count:** 13+ per token  
**Security:** ?? **Enterprise Grade**

---

**END OF CLAIMS GUIDE**
