# ?? Quick Reference - 3 New User Management APIs

## ?? API Endpoints Summary

| # | Method | Endpoint | Description | Auth Required |
|---|--------|----------|-------------|---------------|
| 1 | GET | `/api/roles` | Get all roles with permissions | ? Yes (Any User) |
| 2 | PUT | `/api/users/{userId}/role` | Update user's role | ? Yes (Admin Only) |
| 3 | PUT | `/api/users/{userId}/status` | Enable/Disable user | ? Yes (Admin Only) |

---

## ?? Quick Start

### 1?? Login First
```bash
POST /api/auth/login
{
  "usernameOrEmail": "admin@scanpet.com",
  "password": "Admin@123"
}
```
**Save the `accessToken` from response!**

---

## ?? API Quick Examples

### 1. Get All Roles
```bash
GET /api/roles
Authorization: Bearer YOUR_ACCESS_TOKEN
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid-here",
      "name": "Admin",
      "permissions": ["ColorCreate", "UserEdit", ...],
      "userCount": 1
    }
  ]
}
```

---

### 2. Update User Role
```bash
PUT /api/users/9746f462-94c5-4367-923b-6e9d2ae533cb/role
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa7"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User role updated successfully"
}
```

---

### 3. Disable User
```bash
PUT /api/users/9746f462-94c5-4367-923b-6e9d2ae533cb/status
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
  "isEnabled": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "User disabled successfully"
}
```

---

## ?? Postman Collection

**File:** `New-User-Management-APIs.postman_collection.json`

### Import Steps:
1. Open Postman
2. Click **Import**
3. Select the JSON file
4. Done! ?

### Set Variables:
- `baseUrl` ? `http://localhost:5000`
- `accessToken` ? Your JWT token
- `userId` ? User ID to test
- `roleId` ? Role ID to assign

---

## ? Common Tasks

### Task 1: Change User from "User" to "Manager"
```
1. GET /api/roles ? Get Manager role ID
2. PUT /api/users/{userId}/role ? Assign Manager role
3. Done! User now has Manager permissions
```

### Task 2: Suspend a User Account
```
1. PUT /api/users/{userId}/status ? Set isEnabled: false
2. All tokens revoked automatically
3. User cannot login until re-enabled
```

### Task 3: View All Available Roles
```
1. GET /api/roles
2. Shows: Admin, Manager, User roles
3. Includes permissions and user counts
```

---

## ??? Security Notes

| Feature | Details |
|---------|---------|
| **Authentication** | Required for all endpoints |
| **Admin Only** | Update role & toggle status |
| **Self-Protection** | Can't disable your own account |
| **Token Revocation** | Automatic when disabling user |
| **Audit Logging** | All changes logged |

---

## ? Common Errors

| Status | Error | Solution |
|--------|-------|----------|
| 401 | Unauthorized | Add Bearer token |
| 403 | Forbidden | Use Admin account |
| 404 | Not Found | Check user/role ID |
| 400 | Bad Request | Can't disable yourself |

---

## ?? Files to Import

1. **Postman Collection:** `New-User-Management-APIs.postman_collection.json`
2. **Documentation:** `NEW_USER_MANAGEMENT_APIS.md`
3. **Summary:** `NEW_APIS_IMPLEMENTATION_SUMMARY.md`

---

## ?? Default Test Data

### Users (from DbSeeder)
- **admin@scanpet.com** ? Password: `Admin@123` ? Role: Admin
- **manager@scanpet.com** ? Password: `Manager@123` ? Role: Manager
- **user@scanpet.com** ? Password: `User@123` ? Role: User

### Roles
- **Admin** ? Full access (30 permissions)
- **Manager** ? Operational access (9 permissions)
- **User** ? Read-only (4 permissions)

---

## ?? Related Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/login` | POST | Get access token |
| `/api/users` | GET | List all users |
| `/api/users/{id}` | GET | Get user details |
| `/api/roles/{id}` | GET | Get role details |

---

## ?? Pro Tips

1. **Save your token** in Postman variables
2. **Use the collection** for consistent testing
3. **Check user roles** with `GET /api/users`
4. **Get role IDs** from `GET /api/roles`
5. **Test with non-admin** to verify authorization

---

## ? Verification Checklist

- [ ] API is running (`dotnet run`)
- [ ] Logged in as admin
- [ ] Access token saved
- [ ] Postman collection imported
- [ ] Variables configured
- [ ] Can get all roles
- [ ] Can update user role
- [ ] Can toggle user status

---

## ?? Need Help?

1. **Full Documentation:** See `NEW_USER_MANAGEMENT_APIS.md`
2. **API Collection:** Import `New-User-Management-APIs.postman_collection.json`
3. **Implementation Details:** See `NEW_APIS_IMPLEMENTATION_SUMMARY.md`
4. **Main API Docs:** See `API_DOCUMENTATION.md`

---

**Quick Start Time:** < 5 minutes  
**APIs Ready:** ? All 3  
**Postman Ready:** ? Import and go!  
