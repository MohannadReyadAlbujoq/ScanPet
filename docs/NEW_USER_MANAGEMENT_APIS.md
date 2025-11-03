# New User Management APIs

This document describes the 3 new APIs that have been added to the ScanPet Mobile Backend API for enhanced user management.

## Table of Contents
1. [Get All Roles](#1-get-all-roles)
2. [Update User Role](#2-update-user-role)
3. [Toggle User Status](#3-toggle-user-status)
4. [Quick Start Guide](#quick-start-guide)
5. [Common Use Cases](#common-use-cases)

---

## 1. Get All Roles

**Endpoint:** `GET /api/roles`

**Description:** Retrieves a list of all roles in the system with their permissions and metadata.

**Authentication:** Required (Bearer Token)

**Authorization:** All authenticated users can view roles

### Request

```http
GET /api/roles HTTP/1.1
Host: localhost:5000
Authorization: Bearer YOUR_ACCESS_TOKEN
```

### Response (200 OK)

```json
{
    "success": true,
    "data": [
        {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "name": "Admin",
            "description": "Administrator with full system access",
            "permissionsBitmask": 1073741823,
            "permissions": [
                "ColorCreate", "ColorEdit", "ColorDelete", "ColorView",
                "ItemCreate", "ItemEdit", "ItemDelete", "ItemView",
                "OrderCreate", "OrderView", "OrderEdit", "OrderConfirm", "OrderCancel",
                "UserView", "UserCreate", "UserEdit", "UserDelete", "UserApprove",
                "LocationCreate", "LocationEdit", "LocationDelete", "LocationView",
                "RoleCreate", "RoleEdit", "RoleDelete", "RoleView",
                "PermissionManage", "AuditLogView", "AuditLogExport", "SystemSettings"
            ],
            "userCount": 1,
            "createdAt": "2025-11-01T10:05:51.344048Z"
        },
        {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
            "name": "Manager",
            "description": "Manager with operational access",
            "permissionsBitmask": 4095,
            "permissions": [
                "ItemView", "ItemCreate", "ItemEdit",
                "OrderView", "OrderCreate", "OrderEdit", "OrderConfirm",
                "ColorView", "LocationView"
            ],
            "userCount": 1,
            "createdAt": "2025-11-01T10:05:51.344048Z"
        },
        {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
            "name": "User",
            "description": "Regular user with limited access",
            "permissionsBitmask": 272,
            "permissions": [
                "ItemView", "OrderView", "ColorView", "LocationView"
            ],
            "userCount": 1,
            "createdAt": "2025-11-01T10:05:51.344048Z"
        }
    ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | GUID | Unique identifier for the role |
| `name` | string | Role name (e.g., "Admin", "Manager", "User") |
| `description` | string | Description of the role's purpose |
| `permissionsBitmask` | long | Bitmask representing all assigned permissions |
| `permissions` | array | List of permission names in human-readable format |
| `userCount` | int | Number of users currently assigned to this role |
| `createdAt` | datetime | When the role was created |

### Use Cases

- **Role Selection UI:** Display available roles in a dropdown when assigning roles to users
- **Permission Management:** View what permissions each role has
- **User Statistics:** See how many users are assigned to each role
- **Role Comparison:** Compare permissions between different roles

---

## 2. Update User Role

**Endpoint:** `PUT /api/users/{userId}/role`

**Description:** Changes a user's role assignment. Removes all existing roles and assigns the new specified role.

**Authentication:** Required (Bearer Token)

**Authorization:** Admin only

### Request

```http
PUT /api/users/9746f462-94c5-4367-923b-6e9d2ae533cb/role HTTP/1.1
Host: localhost:5000
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
    "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa7"
}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | GUID | Yes | ID of the user whose role you want to update |

### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `roleId` | GUID | Yes | ID of the role to assign to the user |

### Success Response (200 OK)

```json
{
    "success": true,
    "message": "User role updated successfully"
}
```

### Error Responses

**User Not Found (404)**
```json
{
    "success": false,
    "message": "User not found"
}
```

**Role Not Found (404)**
```json
{
    "success": false,
    "message": "Role not found"
}
```

**Unauthorized (401)**
```json
{
    "success": false,
    "message": "You are not authorized to access this resource. Please login."
}
```

### Business Rules

1. **Single Role Assignment:** A user can only have one role at a time. The endpoint removes all existing roles before assigning the new one.
2. **Admin Only:** Only users with Admin role can update user roles.
3. **Role History:** Previous role assignments are removed from the database (consider implementing audit logging for role changes).
4. **Permission Propagation:** The user immediately inherits all permissions from the new role.

### Use Cases

- **Promote User:** Change a regular user to a manager
- **Demote User:** Downgrade a manager to a regular user  
- **Role Migration:** Bulk update users to new role structure
- **User Management:** Admin panel for managing user access levels

---

## 3. Toggle User Status (Enable/Disable)

**Endpoint:** `PUT /api/users/{userId}/status`

**Description:** Enables or disables a user account. When disabling, all active refresh tokens are revoked.

**Authentication:** Required (Bearer Token)

**Authorization:** Admin only

### Request - Disable User

```http
PUT /api/users/9746f462-94c5-4367-923b-6e9d2ae533cb/status HTTP/1.1
Host: localhost:5000
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
    "isEnabled": false
}
```

### Request - Enable User

```http
PUT /api/users/9746f462-94c5-4367-923b-6e9d2ae533cb/status HTTP/1.1
Host: localhost:5000
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
    "isEnabled": true
}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | GUID | Yes | ID of the user whose status you want to change |

### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `isEnabled` | boolean | Yes | `true` to enable, `false` to disable |

### Success Response (200 OK)

**When Disabling:**
```json
{
    "success": true,
    "message": "User disabled successfully"
}
```

**When Enabling:**
```json
{
    "success": true,
    "message": "User enabled successfully"
}
```

### Error Responses

**User Not Found (404)**
```json
{
    "success": false,
    "message": "User not found"
}
```

**Cannot Disable Self (400)**
```json
{
    "success": false,
    "message": "You cannot disable your own account"
}
```

**Unauthorized (401)**
```json
{
    "success": false,
    "message": "You are not authorized to access this resource. Please login."
}
```

### Business Rules

1. **Self-Protection:** Users cannot disable their own account (prevents locking yourself out).
2. **Token Revocation:** When disabling a user, all their active refresh tokens are revoked.
3. **Login Prevention:** Disabled users cannot log in until re-enabled.
4. **Admin Only:** Only users with Admin role can enable/disable users.
5. **Approval Status:** Disabling does not affect the `isApproved` flag.

### Use Cases

- **Suspend Account:** Temporarily disable a user's access without deleting the account
- **Security Response:** Quickly disable compromised accounts
- **Employment Status:** Disable accounts for terminated employees
- **Account Reactivation:** Re-enable previously disabled accounts
- **Batch Operations:** Bulk disable/enable users based on criteria

---

## Quick Start Guide

### Step 1: Login to Get Access Token

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin@scanpet.com",
    "password": "Admin@123"
  }'
```

Save the `accessToken` from the response.

### Step 2: Get All Roles

```bash
curl -X GET http://localhost:5000/api/roles \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

Note down the role IDs for the roles you want to assign.

### Step 3: Update a User's Role

```bash
curl -X PUT http://localhost:5000/api/users/USER_ID/role \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleId": "ROLE_ID"
  }'
```

### Step 4: Disable or Enable a User

```bash
# Disable User
curl -X PUT http://localhost:5000/api/users/USER_ID/status \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "isEnabled": false
  }'

# Enable User
curl -X PUT http://localhost:5000/api/users/USER_ID/status \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "isEnabled": true
  }'
```

---

## Common Use Cases

### 1. Role-Based Access Control (RBAC) Setup

**Scenario:** You want to set up proper access control for your users.

**Steps:**
1. Get all available roles using `GET /api/roles`
2. For each user that needs role assignment:
   - Get user list with `GET /api/users`
   - Update their role with `PUT /api/users/{id}/role`
3. Verify roles are showing correctly in `GET /api/users`

**Example Workflow:**
```
Admin ? Assign "Manager" role to user123
Manager ? View items, create orders, manage inventory
User ? Read-only access to items and orders
```

### 2. Security Incident Response

**Scenario:** A user account is compromised and you need to immediately revoke access.

**Steps:**
1. Disable the user account: `PUT /api/users/{id}/status` with `isEnabled: false`
2. All refresh tokens are automatically revoked
3. User cannot login again until re-enabled
4. Investigate the incident
5. Re-enable account when safe: `PUT /api/users/{id}/status` with `isEnabled: true`

### 3. Employee Lifecycle Management

**Scenario:** Managing user access as employees join, change roles, or leave.

**New Employee:**
```
1. Register ? GET /api/auth/register
2. Admin approves ? PUT /api/users/{id}/approve
3. Assign role ? PUT /api/users/{id}/role
4. Enable account ? PUT /api/users/{id}/status (isEnabled: true)
```

**Promotion:**
```
1. Get current role ? GET /api/roles
2. Update to new role ? PUT /api/users/{id}/role
3. User inherits new permissions immediately
```

**Termination:**
```
1. Disable account ? PUT /api/users/{id}/status (isEnabled: false)
2. All tokens revoked automatically
3. Account preserved for audit purposes
```

### 4. Permission Testing

**Scenario:** You want to test different permission levels.

**Steps:**
1. Create test users with different roles
2. Assign roles: `PUT /api/users/{id}/role`
3. Login as each test user
4. Verify they can only access features allowed by their role
5. Toggle between roles to test different scenarios

### 5. Bulk Role Migration

**Scenario:** You need to migrate all "User" role members to "Manager" role.

**Steps:**
1. Get all users: `GET /api/users`
2. Get role IDs: `GET /api/roles`
3. Filter users with "User" role
4. For each user:
   - Update role: `PUT /api/users/{id}/role`
5. Verify: `GET /api/users` to check all roles updated

---

## Testing with Postman

### Import the Collection

1. Open Postman
2. Click **Import** ? **File** ? Select `New-User-Management-APIs.postman_collection.json`
3. The collection will be imported with all 3 endpoints

### Set Variables

1. Click on the collection name
2. Go to **Variables** tab
3. Set these variables:
   - `baseUrl`: Your API URL (e.g., `http://localhost:5000`)
   - `accessToken`: Your JWT access token (get from login)
   - `userId`: A user ID to test with
   - `roleId`: A role ID to assign

### Run Requests

1. **Get All Roles:** No additional setup needed, just run
2. **Update User Role:** Set `userId` and `roleId` variables, then run
3. **Toggle User Status:** Set `userId` variable, modify `isEnabled` in body, then run

---

## Default Role IDs

After running `DbSeeder`, these are the default roles:

| Role | Description | Use Case |
|------|-------------|----------|
| **Admin** | Full system access | System administrators |
| **Manager** | Operational access | Store managers, supervisors |
| **User** | Read-only access | Regular employees, viewers |

To get the actual GUIDs for your system, call `GET /api/roles`.

---

## Security Considerations

### Authentication
- All endpoints require a valid JWT Bearer token
- Tokens expire after 15 minutes (refresh required)
- Use HTTPS in production

### Authorization
- Only **Admin** users can update roles and toggle user status
- All authenticated users can view roles
- Self-service protection: Cannot disable your own account

### Audit Logging
- All role changes are logged with who made the change and when
- User status changes are logged
- Token revocations are logged

### Token Management
- Disabling a user revokes all their refresh tokens
- User must re-authenticate after being re-enabled
- Old tokens cannot be reused

---

## Error Handling

All endpoints follow the same error response format:

```json
{
    "success": false,
    "message": "Error description"
}
```

### Common HTTP Status Codes

| Code | Meaning | When it Occurs |
|------|---------|----------------|
| 200 | OK | Request successful |
| 400 | Bad Request | Invalid input or self-disable attempt |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | User or role doesn't exist |
| 500 | Server Error | Unexpected server issue |

---

## Integration Examples

### JavaScript/TypeScript

```typescript
// Get all roles
async function getAllRoles(accessToken: string) {
    const response = await fetch('http://localhost:5000/api/roles', {
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    });
    return await response.json();
}

// Update user role
async function updateUserRole(userId: string, roleId: string, accessToken: string) {
    const response = await fetch(`http://localhost:5000/api/users/${userId}/role`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${accessToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ roleId })
    });
    return await response.json();
}

// Toggle user status
async function toggleUserStatus(userId: string, isEnabled: boolean, accessToken: string) {
    const response = await fetch(`http://localhost:5000/api/users/${userId}/status`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${accessToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ isEnabled })
    });
    return await response.json();
}
```

### C# / .NET

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

public class UserManagementService
{
    private readonly HttpClient _httpClient;
    
    public UserManagementService(string baseUrl, string accessToken)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    public async Task<List<Role>> GetAllRolesAsync()
    {
        var response = await _httpClient.GetAsync("/api/roles");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<Role>>>(content);
        return result.Data;
    }
    
    public async Task<bool> UpdateUserRoleAsync(Guid userId, Guid roleId)
    {
        var payload = new { roleId };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), 
            Encoding.UTF8, 
            "application/json");
            
        var response = await _httpClient.PutAsync($"/api/users/{userId}/role", content);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> ToggleUserStatusAsync(Guid userId, bool isEnabled)
    {
        var payload = new { isEnabled };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), 
            Encoding.UTF8, 
            "application/json");
            
        var response = await _httpClient.PutAsync($"/api/users/{userId}/status", content);
        return response.IsSuccessStatusCode;
    }
}
```

---

## Troubleshooting

### Issue: 401 Unauthorized

**Cause:** Missing or invalid access token

**Solution:**
1. Verify you're sending the Authorization header
2. Check token hasn't expired (15-minute lifetime)
3. Get a new token via `/api/auth/login`
4. Ensure Bearer prefix: `Bearer YOUR_TOKEN`

### Issue: 403 Forbidden

**Cause:** User doesn't have Admin role

**Solution:**
1. Login with an admin account
2. Or assign Admin role to your user first

### Issue: 404 Not Found (User or Role)

**Cause:** Invalid GUID or entity doesn't exist

**Solution:**
1. Verify the GUID format is correct
2. Check the entity exists via GET endpoints
3. Ensure no typos in the ID

### Issue: 400 Bad Request (Cannot disable self)

**Cause:** Trying to disable your own account

**Solution:**
1. Use a different admin account
2. Or have another admin disable your account

### Issue: Roles array is empty in user list

**Cause:** Users don't have roles assigned yet

**Solution:**
1. Run the database seeder
2. Or manually assign roles using `PUT /api/users/{id}/role`

---

## Summary

These 3 new APIs provide essential user management capabilities:

1. **GET /api/roles** - View all available roles and their permissions
2. **PUT /api/users/{id}/role** - Assign roles to users
3. **PUT /api/users/{id}/status** - Enable or disable user accounts

Together, they enable complete Role-Based Access Control (RBAC) and user lifecycle management for the ScanPet Mobile Backend API.

For more information, see the main [API_DOCUMENTATION.md](API_DOCUMENTATION.md).
