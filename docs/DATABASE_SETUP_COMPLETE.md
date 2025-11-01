# ? Database Setup Complete - Quick Reference

## ? **Password & Email Validations**

### **Password Requirements:**
```
? Minimum Length: 8 characters
? Maximum Length: 128 characters
? Must contain: Uppercase letter (A-Z)
? Must contain: Lowercase letter (a-z)
? Must contain: Digit (0-9)
? Must contain: Special character (!@#$%^&*)

Example Valid Password: Admin@123456
```

### **Email Requirements:**
```
? Valid email format (user@example.com)
? Maximum Length: 100 characters
? Must be unique in database
```

---

## ? **Database Configuration**

### **Connection String (Neon PostgreSQL):**
```
Host=ep-soft-recipe-a4u2bjus-pooler.us-east-1.aws.neon.tech
Database=neondb
Username=neondb_owner
Password=npg_9zACPHxX4VuZ
SSL Mode=Require
```

**Already configured in:** `src/API/MobileBackend.API/appsettings.json`

---

## ? **Run Migration & Seeding**

### **Option 1: Automatic (When Running API)**
```powershell
cd src\API\MobileBackend.API
dotnet run
```
? **Database will auto-migrate and seed on startup!**

### **Option 2: Manual (Using Script)**
```powershell
.\migrate-database.ps1
```

### **Option 3: Manual (EF Core Commands)**
```powershell
cd src\API\MobileBackend.API

# Create migration
dotnet ef migrations add InitialCreate --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj

# Apply migration
dotnet ef database update --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

---

## ? **Seeded Accounts**

### **? Administrator Account**
```
Username: admin
Password: Admin@123456
Email: admin@scanpet.com
Role: Administrator (Full Access)
```

### **? Manager Account**
```
Username: manager
Password: Manager@123456
Email: manager@scanpet.com
Role: Manager (Items, Orders, Locations)
```

---

## ? **Seeded Data Summary**

| Entity | Count | Details |
|--------|-------|---------|
| **Permissions** | 26 | All CRUD permissions for all modules |
| **Roles** | 3 | Administrator, Manager, User |
| **Users** | 2 | admin, manager |
| **Colors** | 8 | Red, Blue, Green, Yellow, Black, White, Orange, Purple |
| **Locations** | 4 | Warehouse A, Warehouse B, Store Front, Distribution Center |
| **Items** | 3 | Red T-Shirt, Blue Jeans, Black Shoes |
| **Orders** | 1 | Sample pending order |

---

## ? **Permission Details**

### **Administrator Role (26 Permissions):**
- ? **Colors:** Create, Edit, Delete, View
- ? **Locations:** Create, Edit, Delete, View
- ? **Items:** Create, Edit, Delete, View
- ? **Orders:** Create, Edit, Delete, View
- ? **Users:** Create, Edit, Delete, View, Approve
- ? **Roles:** Create, Edit, Delete, View, Assign Permissions

### **Manager Role (8 Permissions):**
- ? **Items:** View, Create, Edit
- ? **Orders:** View, Create, Edit
- ? **Locations:** View
- ? **Colors:** View

### **User Role (4 Permissions):**
- ? **Items:** View
- ? **Orders:** View
- ? **Locations:** View
- ? **Colors:** View

---

## ? **Testing the API**

### **1. Start the API:**
```powershell
cd src\API\MobileBackend.API
dotnet run
```

### **2. Open Swagger:**
```
http://localhost:5000/swagger
```

### **3. Login (Get JWT Token):**

**Endpoint:** `POST /api/auth/login`

**Request Body (Admin):**
```json
{
  "username": "admin",
  "password": "Admin@123456"
}
```

**Request Body (Manager):**
```json
{
  "username": "manager",
  "password": "Manager@123456"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJS...",
    "refreshToken": "abc123...",
    "expiresIn": 900,
    "user": {
      "id": "guid",
      "username": "admin",
      "email": "admin@scanpet.com",
      "firstName": "System",
      "lastName": "Administrator",
      "role": {
        "id": "guid",
        "name": "Administrator"
      }
    }
  }
}
```

### **4. Use JWT Token:**

In Swagger:
1. Click **"Authorize"** button (top right)
2. Enter: `Bearer {your_access_token}`
3. Click **"Authorize"**
4. Now you can test protected endpoints!

---

## ? **Database Schema**

### **Tables Created:**

1. **Users** - User accounts with authentication
2. **Roles** - User roles (Administrator, Manager, User)
3. **Permissions** - System permissions
4. **RolePermissions** - Many-to-many (Roles ? Permissions)
5. **Colors** - Color reference data
6. **Locations** - Location reference data
7. **Items** - Product/item inventory
8. **Orders** - Customer orders
9. **OrderItems** - Order line items
10. **RefreshTokens** - JWT refresh tokens
11. **AuditLogs** - Audit trail

### **Relationships:**

```
User ?????? Role ?????? RolePermission ?????? Permission
  ?                                              
  ??? Item ?????? Color
  ?         ?????? Location
  ?
  ??? Order ?????? OrderItem ?????? Item
```

---

## ? **Verify Database Seeding**

### **Check if data exists:**

**Using pgAdmin or psql:**
```sql
-- Check users
SELECT username, email, "FirstName", "LastName" FROM "Users";

-- Check roles
SELECT "Name", "Description" FROM "Roles";

-- Check permissions count
SELECT COUNT(*) FROM "Permissions";

-- Check colors
SELECT "Name", "HexCode" FROM "Colors";

-- Check locations
SELECT "Name", "Description", "Address" FROM "Locations";

-- Check items
SELECT "Name", "SKU", "Price", "Quantity" FROM "Items";
```

**Expected Results:**
- Users: 2 rows (admin, manager)
- Roles: 3 rows (Administrator, Manager, User)
- Permissions: 26 rows
- Colors: 8 rows
- Locations: 4 rows
- Items: 3 rows
- Orders: 1 row

---

## ? **Common Issues & Solutions**

### **Issue 1: Migration Failed - Connection Error**

**Error:** `Could not connect to database`

**Solution:**
```powershell
# Check connection string in appsettings.json
# Verify Neon database is accessible
# Check firewall/network settings
```

### **Issue 2: Duplicate Key Error**

**Error:** `Duplicate key value violates unique constraint`

**Solution:**
```powershell
# Database already seeded, skip seeding or:
# Drop database and recreate
dotnet ef database drop --force
dotnet ef database update
```

### **Issue 3: EF Core Tools Not Found**

**Error:** `dotnet ef command not found`

**Solution:**
```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update existing
dotnet tool update --global dotnet-ef
```

---

## ? **Next Steps**

1. ? **Start the API:** `dotnet run`
2. ? **Test Login:** Use admin credentials in Swagger
3. ? **Explore Endpoints:** Try CRUD operations
4. ? **Check Permissions:** Test different user roles
5. ? **View Audit Logs:** Check audit trail entries

---

## ? **Quick Commands**

```powershell
# Start API
cd src\API\MobileBackend.API
dotnet run

# View logs
start C:\AppLogs\ScanPet\log.html

# Check database
dotnet ef database drop --force  # Drop database
dotnet ef database update        # Recreate and seed

# Create new migration
dotnet ef migrations add MigrationName --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj

# Remove last migration
dotnet ef migrations remove --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

---

## ? **API Endpoints Available**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/login` | POST | No | Login with username/password |
| `/api/auth/register` | POST | No | Register new user |
| `/api/auth/refresh` | POST | No | Refresh JWT token |
| `/api/users` | GET | Yes | Get all users |
| `/api/users/{id}` | GET | Yes | Get user by ID |
| `/api/users/{id}/approve` | PUT | Yes | Approve user |
| `/api/roles` | GET | Yes | Get all roles |
| `/api/roles/{id}` | GET | Yes | Get role by ID |
| `/api/roles/{id}/permissions` | PUT | Yes | Assign permissions |
| `/api/colors` | GET | Yes | Get all colors |
| `/api/locations` | GET | Yes | Get all locations |
| `/api/items` | GET | Yes | Get all items |
| `/api/orders` | GET | Yes | Get all orders |

**Total:** 33+ endpoints

---

## ? **Status**

- ? Connection string configured (Neon PostgreSQL)
- ? Database seeder created
- ? Auto-migration on startup configured
- ? Admin account ready (admin / Admin@123456)
- ? Manager account ready (manager / Manager@123456)
- ? All permissions seeded
- ? Sample data seeded
- ? Password validation: 8+ chars, uppercase, lowercase, digit, special
- ? Email validation: Valid format, max 100 chars, unique

---

**? READY TO GO!**

Run `dotnet run` in `src/API/MobileBackend.API` and start testing! ?
