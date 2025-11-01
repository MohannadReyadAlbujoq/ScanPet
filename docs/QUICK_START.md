# ?? QUICK START GUIDE

## ? **Everything Is Ready - Just Run It!**

---

## **One Command to Rule Them All:**

```powershell
cd C:\Users\malbujoq\source\repos\ScanPet\src\API\MobileBackend.API
dotnet run
```

**That's it!** The application will:
1. ? Connect to your Neon PostgreSQL database
2. ? Apply any pending migrations
3. ? Automatically seed the database (first run only)
4. ? Start the API on http://localhost:5000

---

## **What Happens on First Run:**

### **Console Output:**
```
=== ScanPet API Starting Up ===
JWT authentication configured successfully
Starting database migration and seeding...
? Database migrations applied successfully
? Database seeded successfully
? Admin credentials: admin / Admin@123
=== ScanPet API Started Successfully ===
Now listening on: http://localhost:5000
```

### **Database Seeding (First Run Only):**
? **30+ Permissions** created  
? **3 Roles** created (Admin, Manager, User)  
? **1 Admin User** created (admin / Admin@123)  
? **10 Colors** seeded  
? **3 Locations** seeded  
? **10 Sample Items** seeded  

---

## **Test It Immediately:**

### **1. Health Check:**
```powershell
curl http://localhost:5000/health
```

**Expected:** `{"status":"Healthy",...}`

### **2. Open Swagger UI:**
```
http://localhost:5000/swagger
```

### **3. Login:**

**In Swagger:**
1. Find `POST /api/auth/login`
2. Click "Try it out"
3. Enter:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
4. Click "Execute"
5. Copy the `accessToken` from response

**Or via PowerShell:**
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method Post `
    -Body (@{username="admin"; password="Admin@123"} | ConvertTo-Json) `
    -ContentType "application/json"

$response.data.accessToken
```

### **4. Use JWT Token in Swagger:**
1. Click **"Authorize"** button (top right)
2. Enter: `Bearer {your_access_token}`
3. Click **"Authorize"**
4. Now you can test all protected endpoints!

---

## **Admin Account:**

```
Username: admin
Password: Admin@123
Email: admin@scanpet.com
Role: Administrator (Full Access)
```

---

## **Database Info:**

```
Host: ep-soft-recipe-a4u2bjus-pooler.us-east-1.aws.neon.tech
Database: neondb
Provider: PostgreSQL 16
Status: ? Connected & Seeded
```

---

## **View Logs:**

### **HTML Log (Interactive):**
```powershell
start C:\AppLogs\ScanPet\log.html
```

### **Text Log (Real-time):**
```powershell
Get-Content C:\AppLogs\ScanPet\log-*.txt -Tail 50 -Wait
```

### **Error Log:**
```powershell
Get-Content C:\AppLogs\ScanPet\errors-*.log
```

---

## **All Available Endpoints (33+):**

### **Authentication (3):**
- POST `/api/auth/login` - Login with username/password
- POST `/api/auth/register` - Register new user
- POST `/api/auth/refresh` - Refresh JWT token

### **Users (4):**
- GET `/api/users` - Get all users (paginated)
- GET `/api/users/{id}` - Get user by ID
- PUT `/api/users/{id}` - Update user
- PUT `/api/users/{id}/approve` - Approve user

### **Roles (6):**
- GET `/api/roles` - Get all roles
- GET `/api/roles/{id}` - Get role by ID
- POST `/api/roles` - Create role
- PUT `/api/roles/{id}` - Update role
- DELETE `/api/roles/{id}` - Delete role
- PUT `/api/roles/{id}/permissions` - Assign permissions

### **Colors (5):**
- GET `/api/colors` - Get all colors
- GET `/api/colors/{id}` - Get color by ID
- POST `/api/colors` - Create color
- PUT `/api/colors/{id}` - Update color
- DELETE `/api/colors/{id}` - Delete color

### **Locations (5):**
- GET `/api/locations` - Get all locations
- GET `/api/locations/{id}` - Get location by ID
- POST `/api/locations` - Create location
- PUT `/api/locations/{id}` - Update location
- DELETE `/api/locations/{id}` - Delete location

### **Items (5):**
- GET `/api/items` - Get all items
- GET `/api/items/{id}` - Get item by ID
- POST `/api/items` - Create item
- PUT `/api/items/{id}` - Update item
- DELETE `/api/items/{id}` - Delete item

### **Orders (5+):**
- GET `/api/orders` - Get all orders
- GET `/api/orders/{id}` - Get order by ID
- POST `/api/orders` - Create order
- PUT `/api/orders/{id}` - Update order
- DELETE `/api/orders/{id}` - Delete order

---

## **Common Commands:**

### **Start API:**
```powershell
cd src\API\MobileBackend.API
dotnet run
```

### **Build Solution:**
```powershell
dotnet build
```

### **Run Tests:**
```powershell
dotnet test
```

### **Create Migration (if needed):**
```powershell
dotnet ef migrations add MigrationName --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

### **Apply Migration:**
```powershell
dotnet ef database update --project ..\..\Infrastructure\MobileBackend.Infrastructure\MobileBackend.Infrastructure.csproj
```

### **Regenerate JWT Keys:**
```powershell
.\generate-jwt-keys.ps1
```

---

## **Troubleshooting:**

### **Issue: JWT keys not configured**
**Solution:**
```powershell
.\generate-jwt-keys.ps1
```

### **Issue: Database connection failed**
**Solution:** Check Neon database is accessible:
```powershell
Test-NetConnection -ComputerName ep-soft-recipe-a4u2bjus-pooler.us-east-1.aws.neon.tech -Port 5432
```

### **Issue: NLog errors**
**Solution:** Create log directory:
```powershell
New-Item -ItemType Directory -Force -Path "C:\AppLogs\ScanPet"
```

### **Issue: Port 5000 already in use**
**Solution:** Use different port:
```powershell
dotnet run --urls="http://localhost:5555"
```

---

## **Production Deployment:**

### **Before deploying to production:**

1. **Move JWT keys to environment variables:**
   ```bash
   JwtSettings__PrivateKey="your_key_here"
   JwtSettings__PublicKey="your_key_here"
   ```

2. **Update CORS origins:**
   ```json
   "AllowedOrigins": ["https://your-domain.com"]
   ```

3. **Disable Swagger:**
   ```json
   "ApiSettings": { "EnableSwagger": false }
   ```

4. **Use production connection string:**
   - Store in environment variable or secrets manager
   - Don't commit to source control

---

## **?? READY TO GO!**

**Everything is configured and ready for production:**

? Database: Neon PostgreSQL (migrated)  
? Authentication: JWT RS256 (configured)  
? Logging: NLog HTML (enabled)  
? Seeding: Automatic (on first run)  
? Admin Account: admin / Admin@123  
? Documentation: Complete  

**Just run:**
```powershell
cd src\API\MobileBackend.API
dotnet run
```

**Then open:** http://localhost:5000/swagger

**Login with:** admin / Admin@123

**?? START BUILDING! ??**
