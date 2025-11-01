# ?? API DOCUMENTATION - ScanPet Backend

## ?? **Seeded Users**

| Username | Password | Role | Permissions |
|----------|----------|------|-------------|
| admin | Admin@123 | Administrator | ALL (30+) |
| manager | Manager@123 | Manager | 9 operational |
| user | User@123 | User | 4 view-only |

---

## ?? **AUTHENTICATION**

### **POST /api/auth/login** (Public)
Login to get JWT access token
```json
Request:
{
  "username": "string",
  "password": "string"
}

Example:
{
  "username": "admin",
  "password": "Admin@123"
}

Response:
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1...",
    "refreshToken": "abc123def456...",
    "expiresIn": 900
  }
}
```

---

### **POST /api/auth/register** (Public)
Register new user account (requires admin approval)
```json
Request:
{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string",
  "phoneNumber": "string"
}

Example:
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "NewUser@123",
  "fullName": "New User",
  "phoneNumber": "+1234567890"
}
```

---

### **POST /api/auth/refresh** (Public)
Refresh expired access token
```json
Request:
{
  "refreshToken": "string"
}

Example:
{
  "refreshToken": "abc123def456..."
}
```

---

## ?? **USERS**

### **GET /api/users** (Admin)
Get all users with pagination
```
Query Parameters:
- pageNumber: integer (default: 1)
- pageSize: integer (default: 20)

Example:
GET /api/users?pageNumber=1&pageSize=10
```

---

### **GET /api/users/{id}** (Admin)
Get user by ID
```
Path Parameters:
- id: guid

Example:
GET /api/users/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/users** (Admin)
Create new user
```json
Request:
{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string",
  "phoneNumber": "string"
}

Example:
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test@123",
  "fullName": "Test User",
  "phoneNumber": "+1234567890"
}
```

---

### **PUT /api/users/{id}** (Admin)
Update user information
```json
Request:
{
  "email": "string",
  "fullName": "string",
  "phoneNumber": "string"
}

Example:
{
  "email": "updated@example.com",
  "fullName": "Updated Name",
  "phoneNumber": "+9876543210"
}
```

---

### **PUT /api/users/{id}/approve** (Admin)
Approve user account
```
Path Parameters:
- id: guid

Example:
PUT /api/users/123e4567-e89b-12d3-a456-426614174000/approve
```

---

## ?? **ROLES**

### **GET /api/roles** (Admin, Manager, User)
Get all roles
```
No parameters required

Example:
GET /api/roles
```

---

### **GET /api/roles/{id}** (Admin, Manager, User)
Get role by ID
```
Path Parameters:
- id: guid

Example:
GET /api/roles/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/roles** (Admin)
Create new role
```json
Request:
{
  "name": "string",
  "description": "string"
}

Example:
{
  "name": "Supervisor",
  "description": "Supervisor with elevated permissions"
}
```

---

### **PUT /api/roles/{id}** (Admin)
Update role
```json
Request:
{
  "name": "string",
  "description": "string"
}

Example:
{
  "name": "Senior Supervisor",
  "description": "Updated description"
}
```

---

### **DELETE /api/roles/{id}** (Admin)
Delete role
```
Path Parameters:
- id: guid

Example:
DELETE /api/roles/123e4567-e89b-12d3-a456-426614174000
```

---

### **PUT /api/roles/{id}/permissions** (Admin)
Assign permissions to role
```json
Request:
{
  "permissionIds": ["guid"]
}

Example:
{
  "permissionIds": [
    "123e4567-e89b-12d3-a456-426614174001",
    "123e4567-e89b-12d3-a456-426614174002"
  ]
}
```

---

## ?? **ITEMS**

### **GET /api/items** (Admin, Manager, User)
Get all items with pagination
```
Query Parameters:
- pageNumber: integer (default: 1)
- pageSize: integer (default: 20)

Example:
GET /api/items?pageNumber=1&pageSize=10
```

---

### **GET /api/items/{id}** (Admin, Manager, User)
Get item by ID
```
Path Parameters:
- id: guid

Example:
GET /api/items/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/items** (Admin, Manager)
Create new item
```json
Request:
{
  "name": "string",
  "sku": "string",
  "description": "string",
  "basePrice": number,
  "quantity": integer,
  "colorId": "guid"
}

Example:
{
  "name": "Pet Food - Premium",
  "sku": "PF-001",
  "description": "High-quality premium pet food",
  "basePrice": 29.99,
  "quantity": 100,
  "colorId": "123e4567-e89b-12d3-a456-426614174000"
}
```

---

### **PUT /api/items/{id}** (Admin, Manager)
Update item
```json
Request:
{
  "name": "string",
  "description": "string",
  "basePrice": number,
  "quantity": integer
}

Example:
{
  "name": "Pet Food - Super Premium",
  "description": "Updated description",
  "basePrice": 34.99,
  "quantity": 150
}
```

---

### **DELETE /api/items/{id}** (Admin)
Delete item
```
Path Parameters:
- id: guid

Example:
DELETE /api/items/123e4567-e89b-12d3-a456-426614174000
```

---

## ?? **ORDERS**

### **GET /api/orders** (Admin, Manager, User)
Get all orders with pagination
```
Query Parameters:
- pageNumber: integer (default: 1)
- pageSize: integer (default: 20)

Example:
GET /api/orders?pageNumber=1&pageSize=10
```

---

### **GET /api/orders/{id}** (Admin, Manager, User)
Get order by ID
```
Path Parameters:
- id: guid

Example:
GET /api/orders/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/orders** (Admin, Manager)
Create new order
```json
Request:
{
  "customerName": "string",
  "customerEmail": "string",
  "items": [
    {
      "itemId": "guid",
      "quantity": integer,
      "price": number
    }
  ]
}

Example:
{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "items": [
    {
      "itemId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "price": 29.99
    }
  ]
}
```

---

### **PUT /api/orders/{id}** (Admin, Manager)
Update order
```json
Request:
{
  "status": "string"
}

Example:
{
  "status": "Processing"
}
```

---

### **DELETE /api/orders/{id}** (Admin)
Delete order
```
Path Parameters:
- id: guid

Example:
DELETE /api/orders/123e4567-e89b-12d3-a456-426614174000
```

---

## ?? **COLORS**

### **GET /api/colors** (Admin, Manager, User)
Get all colors
```
No parameters required

Example:
GET /api/colors

Seeded Data:
- Red (255, 0, 0)
- Green (0, 255, 0)
- Blue (0, 0, 255)
- Yellow (255, 255, 0)
- Black (0, 0, 0)
- White (255, 255, 255)
- Orange (255, 165, 0)
- Purple (128, 0, 128)
- Pink (255, 192, 203)
- Brown (165, 42, 42)
```

---

### **GET /api/colors/{id}** (Admin, Manager, User)
Get color by ID
```
Path Parameters:
- id: guid

Example:
GET /api/colors/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/colors** (Admin)
Create new color
```json
Request:
{
  "name": "string",
  "redValue": integer (0-255),
  "greenValue": integer (0-255),
  "blueValue": integer (0-255),
  "description": "string"
}

Example:
{
  "name": "Sky Blue",
  "redValue": 135,
  "greenValue": 206,
  "blueValue": 235,
  "description": "Light sky blue color"
}
```

---

### **PUT /api/colors/{id}** (Admin)
Update color
```json
Request:
{
  "name": "string",
  "redValue": integer,
  "greenValue": integer,
  "blueValue": integer,
  "description": "string"
}

Example:
{
  "name": "Deep Sky Blue",
  "redValue": 0,
  "greenValue": 191,
  "blueValue": 255,
  "description": "Deep sky blue color"
}
```

---

### **DELETE /api/colors/{id}** (Admin)
Delete color
```
Path Parameters:
- id: guid

Example:
DELETE /api/colors/123e4567-e89b-12d3-a456-426614174000
```

---

## ?? **LOCATIONS**

### **GET /api/locations** (Admin, Manager, User)
Get all locations
```
No parameters required

Example:
GET /api/locations

Seeded Data:
- Main Warehouse (New York, 10001)
- Distribution Center (Los Angeles, 90001)
- Storage Unit A (Chicago, 60601)
```

---

### **GET /api/locations/{id}** (Admin, Manager, User)
Get location by ID
```
Path Parameters:
- id: guid

Example:
GET /api/locations/123e4567-e89b-12d3-a456-426614174000
```

---

### **POST /api/locations** (Admin)
Create new location
```json
Request:
{
  "name": "string",
  "address": "string",
  "city": "string",
  "country": "string",
  "postalCode": "string",
  "isActive": boolean
}

Example:
{
  "name": "New Warehouse",
  "address": "456 Storage Ave",
  "city": "Boston",
  "country": "United States",
  "postalCode": "02101",
  "isActive": true
}
```

---

### **PUT /api/locations/{id}** (Admin)
Update location
```json
Request:
{
  "name": "string",
  "address": "string",
  "city": "string",
  "postalCode": "string",
  "isActive": boolean
}

Example:
{
  "name": "Updated Warehouse",
  "address": "789 New Street",
  "city": "Boston",
  "postalCode": "02102",
  "isActive": false
}
```

---

### **DELETE /api/locations/{id}** (Admin)
Delete location
```
Path Parameters:
- id: guid

Example:
DELETE /api/locations/123e4567-e89b-12d3-a456-426614174000
```

---

## ?? **HEALTH**

### **GET /health** (Public)
Check API health status
```
No parameters required

Example:
GET /health

Response:
{
  "status": "Healthy",
  "timestamp": "2025-01-15T12:00:00Z",
  "environment": "Production",
  "database": "PostgreSQL",
  "totalEndpoints": 33
}
```

---

## ?? **RESPONSE FORMATS**

### **Success Response:**
```json
{
  "success": true,
  "data": { ... },
  "message": null,
  "errors": null
}
```

### **Error Response:**
```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "errors": ["Detail 1", "Detail 2"]
}
```

### **Validation Error:**
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "Username": ["Username is required"],
    "Email": ["Invalid email format"]
  }
}
```

---

## ?? **AUTHENTICATION**

All protected endpoints require JWT Bearer token:

```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Expiry:**
- Access Token: 15 minutes
- Refresh Token: 7 days

---

## ?? **PAGINATION**

Endpoints supporting pagination return:

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalCount": 50,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

**Base URL:** `http://localhost:5000`  
**Production URL:** `https://your-app.railway.app` (after deployment)  
**Total Endpoints:** 33+  
**API Version:** 1.0
