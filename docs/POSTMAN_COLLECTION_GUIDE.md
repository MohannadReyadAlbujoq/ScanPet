# ?? POSTMAN COLLECTION GUIDE & SAMPLES

**Date:** December 2024  
**Collection:** ScanPet Mobile Backend API  
**Base URL:** `https://your-api.com` or `http://localhost:5000`

---

## ?? IMPORT EXISTING COLLECTION

**File:** `ScanPet_API_Collection.postman_collection.json` (already in root)

### Import Steps:
1. Open Postman
2. Click **Import**
3. Select `ScanPet_API_Collection.postman_collection.json`
4. Collection appears in sidebar

---

## ?? TEST SCENARIOS BY ROLE

### 1. **ADMIN ROLE** (Full Access - 30+ permissions)

#### Setup
```
Base URL: {{base_url}}
Token: {{admin_token}}
```

#### A. Login as Admin
```http
POST {{base_url}}/api/auth/login
Content-Type: application/json

{
  "email": "admin@scanpet.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 900,
  "user": {
    "id": "guid",
    "email": "admin@scanpet.com",
    "role": "Admin",
    "permissions": ["Users.Create", "Orders.Refund", ...]
  }
}
```

#### B. Create Color (Admin Only)
```http
POST {{base_url}}/api/colors
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "name": "Royal Blue",
  "description": "Deep blue color",
  "redValue": 0,
  "greenValue": 0,
  "blueValue": 255
}
```

#### C. Create Item (Admin Only)
```http
POST {{base_url}}/api/items
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "name": "Premium Pet Collar",
  "description": "Luxury leather collar",
  "sku": "PET-COL-001",
  "basePrice": 29.99,
  "quantity": 100,
  "colorId": "color-guid-here"
}
```

#### D. Refund Order Item (Admin Only)
```http
POST {{base_url}}/api/orders/refund/SN-PET-COL-001-20241215-ABC
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "refundQuantity": 1,
  "refundReason": "Customer requested, defective product"
}
```

#### E. Create User (Admin Only)
```http
POST {{base_url}}/api/users
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "email": "manager@scanpet.com",
  "password": "Manager@123",
  "firstName": "Jane",
  "lastName": "Manager",
  "roleId": "manager-role-guid"
}
```

#### F. Approve User (Admin Only)
```http
PUT {{base_url}}/api/users/{userId}/approve
Authorization: Bearer {{admin_token}}
```

---

### 2. **MANAGER ROLE** (Operational Access - 15 permissions)

#### A. Login as Manager
```http
POST {{base_url}}/api/auth/login
Content-Type: application/json

{
  "email": "manager@scanpet.com",
  "password": "Manager@123"
}
```

#### B. Create Order (Manager Can)
```http
POST {{base_url}}/api/orders
Authorization: Bearer {{manager_token}}
Content-Type: application/json

{
  "clientName": "John Doe",
  "clientPhone": "+1234567890",
  "locationId": "location-guid",
  "description": "Bulk order for pet store",
  "orderItems": [
    {
      "serialNumber": "CUSTOM-SN-001",
      "itemId": "item-guid",
      "quantity": 5,
      "unitPrice": 29.99
    }
  ]
}
```

#### C. Confirm Order (Manager Can)
```http
PUT {{base_url}}/api/orders/{orderId}/confirm
Authorization: Bearer {{manager_token}}
```

#### D. Cancel Order (Manager Can)
```http
PUT {{base_url}}/api/orders/{orderId}/cancel?reason=Customer cancelled
Authorization: Bearer {{manager_token}}
```

#### E. Cannot Create Color (403 Forbidden)
```http
POST {{base_url}}/api/colors
Authorization: Bearer {{manager_token}}
Content-Type: application/json

{
  "name": "Red",
  "redValue": 255,
  "greenValue": 0,
  "blueValue": 0
}
```

**Response:**
```json
{
  "success": false,
  "message": "Forbidden: Insufficient permissions",
  "statusCode": 403
}
```

---

### 3. **USER ROLE** (Limited Access - 5 permissions)

#### A. Login as User
```http
POST {{base_url}}/api/auth/login
Content-Type: application/json

{
  "email": "user@scanpet.com",
  "password": "User@123"
}
```

#### B. View Orders (User Can - Own Orders Only)
```http
GET {{base_url}}/api/orders
Authorization: Bearer {{user_token}}
```

**Response:** Only returns orders created by this user

#### C. View Order Details (User Can - Own Order)
```http
GET {{base_url}}/api/orders/{orderId}
Authorization: Bearer {{user_token}}
```

#### D. View Colors (User Can - Read Only)
```http
GET {{base_url}}/api/colors
Authorization: Bearer {{user_token}}
```

#### E. Cannot Create Order (403 Forbidden)
```http
POST {{base_url}}/api/orders
Authorization: Bearer {{user_token}}
Content-Type: application/json

{
  "clientName": "Test",
  "clientPhone": "123",
  "locationId": "guid"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Forbidden: Insufficient permissions",
  "statusCode": 403
}
```

---

## ?? COMPLETE DATA TYPE SAMPLES

### Authentication DTOs

```typescript
// LoginDto
{
  email: string;          // "admin@scanpet.com"
  password: string;       // "Admin@123"
}

// RegisterDto
{
  email: string;          // "newuser@example.com"
  password: string;       // "Password@123"
  firstName: string;      // "John"
  lastName: string;       // "Doe"
  phoneNumber?: string;   // "+1234567890" (optional)
}

// RefreshTokenDto
{
  accessToken: string;    // "eyJhbGc..."
  refreshToken: string;   // "eyJhbGc..."
}
```

### Color DTOs

```typescript
// ColorDto (Create/Update)
{
  name: string;           // "Royal Blue" (required, max 100 chars)
  description?: string;   // "Deep blue color" (optional, max 500 chars)
  redValue: number;       // 0-255 (required)
  greenValue: number;     // 0-255 (required)
  blueValue: number;      // 0-255 (required)
}

// ColorDto (Response)
{
  id: string;             // "guid"
  name: string;
  description?: string;
  redValue: number;
  greenValue: number;
  blueValue: number;
  hexCode: string;        // "#0000FF" (auto-generated)
  createdAt: string;      // ISO 8601 date
  updatedAt?: string;
}
```

### Location DTOs

```typescript
// LocationDto (Create/Update)
{
  name: string;           // "Main Warehouse" (required, max 200)
  address?: string;       // "123 Main St" (optional, max 500)
  city?: string;          // "New York" (optional, max 100)
  state?: string;         // "NY" (optional, max 50)
  zipCode?: string;       // "10001" (optional, max 20)
  country?: string;       // "USA" (optional, max 100)
  isActive: boolean;      // true/false
}

// LocationDto (Response)
{
  id: string;
  name: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}
```

### Item DTOs

```typescript
// ItemDto (Create/Update)
{
  name: string;           // "Premium Pet Collar" (required, max 200)
  description?: string;   // "Leather collar" (optional, max 1000)
  sku?: string;           // "PET-COL-001" (optional, max 50, unique)
  basePrice: number;      // 29.99 (required, decimal(18,2))
  quantity: number;       // 100 (required, integer)
  colorId?: string;       // "guid" (optional)
  locationId?: string;    // "guid" (optional)
}

// ItemDto (Response)
{
  id: string;
  name: string;
  description?: string;
  sku?: string;
  basePrice: number;
  quantity: number;
  colorId?: string;
  colorName?: string;     // Populated from Color
  locationId?: string;
  locationName?: string;  // Populated from Location
  createdAt: string;
  updatedAt?: string;
}
```

### Order DTOs

```typescript
// OrderDto (Create)
{
  clientName: string;           // "John Doe" (required, max 200)
  clientPhone: string;          // "+1234567890" (required, max 20)
  locationId: string;           // "guid" (required)
  description?: string;         // "Bulk order" (optional, max 1000)
  orderItems: OrderItemDto[];   // At least 1 item required
}

// OrderItemDto (within OrderDto)
{
  serialNumber?: string;        // "CUSTOM-SN-001" (optional, auto-gen if empty)
  itemId: string;               // "guid" (required)
  quantity: number;             // 1-999 (required)
  unitPrice?: number;           // 29.99 (optional, uses basePrice if empty)
}

// OrderDto (Response)
{
  id: string;
  orderNumber: string;          // "ORD-20241215-ABCD"
  clientName: string;
  clientPhone: string;
  locationId: string;
  locationName: string;
  description?: string;
  orderDate: string;            // ISO 8601
  totalAmount: number;          // Auto-calculated
  orderStatus: string;          // "Pending" | "Confirmed" | "Cancelled"
  orderItems: OrderItemDto[];
  createdAt: string;
  updatedAt?: string;
}

// OrderItemDto (Response)
{
  id: string;
  orderId: string;
  itemId: string;
  itemName: string;
  serialNumber: string;         // "SN-PET-COL-001-20241215-ABC"
  quantity: number;
  salePrice: number;
  status: string;               // "Successful" | "Refunded"
  refundedQuantity: number;
  refundedAt?: string;
  refundReason?: string;
}
```

### Refund DTOs

```typescript
// RefundOrderItemRequest
{
  refundQuantity: number;       // 1-999 (required, must not exceed ordered qty)
  refundReason?: string;        // "Defective product" (optional, max 500)
}
```

### Role DTOs

```typescript
// RoleDto (Create/Update)
{
  name: string;                 // "Manager" (required, max 100, unique)
  description?: string;         // "Operational role" (optional, max 500)
  permissions?: string[];       // ["Orders.Create", "Orders.View"]
}

// RoleDto (Response)
{
  id: string;
  name: string;
  description?: string;
  isSystemRole: boolean;        // true for Admin, false for custom
  permissions: string[];
  createdAt: string;
  updatedAt?: string;
}
```

### User DTOs

```typescript
// UserDto (Create)
{
  email: string;                // "user@example.com" (required, unique)
  password: string;             // "Password@123" (required, min 8 chars)
  firstName: string;            // "John" (required, max 100)
  lastName: string;             // "Doe" (required, max 100)
  phoneNumber?: string;         // "+1234567890" (optional)
  roleId: string;               // "guid" (required)
}

// UserDto (Response)
{
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  roleId: string;
  roleName: string;
  isEnabled: boolean;
  isApproved: boolean;
  createdAt: string;
  updatedAt?: string;
}
```

---

## ?? PERMISSIONS BY ROLE

### Admin (30+ permissions)
```json
[
  "Users.Create", "Users.View", "Users.Update", "Users.Delete", "Users.Approve",
  "Roles.Create", "Roles.View", "Roles.Update", "Roles.Delete", "Roles.AssignPermissions",
  "Colors.Create", "Colors.View", "Colors.Update", "Colors.Delete",
  "Locations.Create", "Locations.View", "Locations.Update", "Locations.Delete",
  "Items.Create", "Items.View", "Items.Update", "Items.Delete",
  "Orders.Create", "Orders.View", "Orders.Update", "Orders.Delete", "Orders.Confirm", "Orders.Cancel", "Orders.Refund",
  "Audit.View", "Audit.Export"
]
```

### Manager (15 permissions)
```json
[
  "Colors.View",
  "Locations.View",
  "Items.View",
  "Orders.Create", "Orders.View", "Orders.Update", "Orders.Confirm", "Orders.Cancel",
  "Users.View"
]
```

### User (5 permissions)
```json
[
  "Colors.View",
  "Locations.View",
  "Items.View",
  "Orders.View",  // Own orders only
  "Users.View"    // Own profile only
]
```

---

## ?? TESTING WORKFLOW

### Complete Test Sequence

1. **Register New User**
```http
POST /api/auth/register
{ "email": "test@example.com", "password": "Test@123" }
```

2. **Admin Approves User**
```http
PUT /api/users/{userId}/approve
Authorization: Bearer {{admin_token}}
```

3. **User Logs In**
```http
POST /api/auth/login
{ "email": "test@example.com", "password": "Test@123" }
```

4. **Create Color** (Admin only)
5. **Create Location** (Admin only)
6. **Create Item** (Admin only)
7. **Create Order** (Manager/Admin)
8. **Confirm Order** (Manager/Admin)
9. **Refund Item** (Admin only)
10. **View Audit Logs** (Admin only)

---

## ?? POSTMAN ENVIRONMENT VARIABLES

Create environment in Postman:

```json
{
  "name": "ScanPet Development",
  "values": [
    { "key": "base_url", "value": "http://localhost:5000", "enabled": true },
    { "key": "admin_token", "value": "", "enabled": true },
    { "key": "manager_token", "value": "", "enabled": true },
    { "key": "user_token", "value": "", "enabled": true }
  ]
}
```

**Auto-set token script** (in Tests tab of login request):
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.environment.set("admin_token", response.accessToken);
}
```

---

## ?? SUMMARY

**Total Endpoints:** 36  
**Roles:** 3 (Admin, Manager, User)  
**Permissions:** 30+  
**Data Types:** Strongly typed DTOs  
**Authentication:** JWT Bearer tokens  

**Postman Collection:** ? Complete and ready to use!

---

**Status:** ? **POSTMAN GUIDE COMPLETE**  
**Collection File:** ScanPet_API_Collection.postman_collection.json  
**Ready:** Import and test!

---

**END OF POSTMAN GUIDE**
