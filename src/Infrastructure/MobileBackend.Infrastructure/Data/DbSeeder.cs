using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;
using MobileBackend.Framework.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MobileBackend.Infrastructure.Data;

/// <summary>
/// Simplified database seeder matching actual domain model
/// Seeds permissions, roles, admin user, and sample data
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        IPasswordService passwordService,
        ILogger logger)
    {
        logger.LogInformation("Starting database seeding...");

        try
        {
            // 1. Seed Permissions
            await SeedPermissionsAsync(context, logger);

            // 2. Seed Roles
            await SeedRolesAsync(context, logger);

            // 3. Create Admin User
            await CreateAdminUserAsync(context, passwordService, logger);

            // 4. Create Manager User
            await CreateManagerUserAsync(context, passwordService, logger);

            // 5. Create Regular User
            await CreateRegularUserAsync(context, passwordService, logger);

            // 6. Assign Admin Role to Admin User
            await AssignAdminRoleAsync(context, logger);

            // 7. Assign Manager Role to Manager User
            await AssignManagerRoleAsync(context, logger);

            // 8. Assign User Role to Regular User
            await AssignUserRoleAsync(context, logger);

            // 9. Assign Permissions to Admin Role
            await AssignPermissionsToAdminRoleAsync(context, logger);

            // 10. Assign Permissions to Manager Role
            await AssignPermissionsToManagerRoleAsync(context, logger);

            // 11. Assign Permissions to User Role
            await AssignPermissionsToUserRoleAsync(context, logger);

            // 12. Seed Sample Colors
            await SeedSampleColorsAsync(context, logger);

            // 13. Seed Sample Locations
            await SeedSampleLocationsAsync(context, logger);

            // 14. Seed Sample Items
            await SeedSampleItemsAsync(context, logger);

            logger.LogInformation("? Database seeding completed successfully!");
            logger.LogInformation("?? Admin Login: username=admin, password=Admin@123");
            logger.LogInformation("?? Manager Login: username=manager, password=Manager@123");
            logger.LogInformation("?? User Login: username=user, password=User@123");
            logger.LogInformation("?? Database contains: 30+ permissions, 3 roles, 3 users (admin, manager, user), 10 colors, 3 locations, 10 items");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Permissions.AnyAsync())
        {
            logger.LogInformation("?? Permissions already exist. Skipping...");
            return;
        }

        logger.LogInformation("?? Seeding permissions...");

        var permissions = new List<Permission>
        {
            // Color Permissions
            new() { Name = "Color Create", Description = "Create colors", PermissionBit = (long)PermissionType.ColorCreate, Category = "Color" },
            new() { Name = "Color Edit", Description = "Edit colors", PermissionBit = (long)PermissionType.ColorEdit, Category = "Color" },
            new() { Name = "Color Delete", Description = "Delete colors", PermissionBit = (long)PermissionType.ColorDelete, Category = "Color" },
            new() { Name = "Color View", Description = "View colors", PermissionBit = (long)PermissionType.ColorView, Category = "Color" },
            
            // Item Permissions
            new() { Name = "Item Create", Description = "Create items", PermissionBit = (long)PermissionType.ItemCreate, Category = "Item" },
            new() { Name = "Item Edit", Description = "Edit items", PermissionBit = (long)PermissionType.ItemEdit, Category = "Item" },
            new() { Name = "Item Delete", Description = "Delete items", PermissionBit = (long)PermissionType.ItemDelete, Category = "Item" },
            new() { Name = "Item View", Description = "View items", PermissionBit = (long)PermissionType.ItemView, Category = "Item" },
            
            // Order Permissions
            new() { Name = "Order Create", Description = "Create orders", PermissionBit = (long)PermissionType.OrderCreate, Category = "Order" },
            new() { Name = "Order View", Description = "View orders", PermissionBit = (long)PermissionType.OrderView, Category = "Order" },
            new() { Name = "Order Edit", Description = "Edit orders", PermissionBit = (long)PermissionType.OrderEdit, Category = "Order" },
            new() { Name = "Order Confirm", Description = "Confirm orders", PermissionBit = (long)PermissionType.OrderConfirm, Category = "Order" },
            new() { Name = "Order Cancel", Description = "Cancel orders", PermissionBit = (long)PermissionType.OrderCancel, Category = "Order" },
            
            // User Management Permissions
            new() { Name = "User View", Description = "View users", PermissionBit = (long)PermissionType.UserView, Category = "User" },
            new() { Name = "User Create", Description = "Create users", PermissionBit = (long)PermissionType.UserCreate, Category = "User" },
            new() { Name = "User Edit", Description = "Edit users", PermissionBit = (long)PermissionType.UserEdit, Category = "User" },
            new() { Name = "User Delete", Description = "Delete users", PermissionBit = (long)PermissionType.UserDelete, Category = "User" },
            new() { Name = "User Approve", Description = "Approve users", PermissionBit = (long)PermissionType.UserApprove, Category = "User" },
            
            // Location Permissions
            new() { Name = "Location Create", Description = "Create locations", PermissionBit = (long)PermissionType.LocationCreate, Category = "Location" },
            new() { Name = "Location Edit", Description = "Edit locations", PermissionBit = (long)PermissionType.LocationEdit, Category = "Location" },
            new() { Name = "Location Delete", Description = "Delete locations", PermissionBit = (long)PermissionType.LocationDelete, Category = "Location" },
            new() { Name = "Location View", Description = "View locations", PermissionBit = (long)PermissionType.LocationView, Category = "Location" },
            
            // Role Permissions
            new() { Name = "Role Create", Description = "Create roles", PermissionBit = (long)PermissionType.RoleCreate, Category = "Role" },
            new() { Name = "Role Edit", Description = "Edit roles", PermissionBit = (long)PermissionType.RoleEdit, Category = "Role" },
            new() { Name = "Role Delete", Description = "Delete roles", PermissionBit = (long)PermissionType.RoleDelete, Category = "Role" },
            new() { Name = "Role View", Description = "View roles", PermissionBit = (long)PermissionType.RoleView, Category = "Role" },
            new() { Name = "Permission Manage", Description = "Manage permissions", PermissionBit = (long)PermissionType.PermissionManage, Category = "Role" },
            
            // Audit Log Permissions
            new() { Name = "Audit Log View", Description = "View audit logs", PermissionBit = (long)PermissionType.AuditLogView, Category = "System" },
            new() { Name = "Audit Log Export", Description = "Export audit logs", PermissionBit = (long)PermissionType.AuditLogExport, Category = "System" },
            
            // System Permissions
            new() { Name = "System Settings", Description = "Manage system settings", PermissionBit = (long)PermissionType.SystemSettings, Category = "System" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Seeded {permissions.Count} permissions");
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Roles.AnyAsync())
        {
            logger.LogInformation("?? Roles already exist. Skipping...");
            return;
        }

        logger.LogInformation("?? Seeding roles...");

        var roles = new List<Role>
        {
            new() { Name = "Admin", Description = "Administrator with full system access" },
            new() { Name = "Manager", Description = "Manager with operational access" },
            new() { Name = "User", Description = "Regular user with limited access" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Seeded {roles.Count} roles");
    }

    private static async Task CreateAdminUserAsync(ApplicationDbContext context, IPasswordService passwordService, ILogger logger)
    {
        if (await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            logger.LogInformation("?? Admin user already exists. Skipping...");
            return;
        }

        logger.LogInformation("?? Creating admin user...");

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@scanpet.com",
            PasswordHash = passwordService.HashPassword("Admin@123"),
            FullName = "System Administrator",
            PhoneNumber = "+1234567890",
            IsEnabled = true,
            IsApproved = true
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("? Admin user created (admin / Admin@123)");
    }

    private static async Task CreateManagerUserAsync(ApplicationDbContext context, IPasswordService passwordService, ILogger logger)
    {
        if (await context.Users.AnyAsync(u => u.Username == "manager"))
        {
            logger.LogInformation("?? Manager user already exists. Skipping...");
            return;
        }

        logger.LogInformation("?? Creating manager user...");

        var managerUser = new User
        {
            Username = "manager",
            Email = "manager@scanpet.com",
            PasswordHash = passwordService.HashPassword("Manager@123"),
            FullName = "Operations Manager",
            PhoneNumber = "+1234567891",
            IsEnabled = true,
            IsApproved = true
        };

        await context.Users.AddAsync(managerUser);
        await context.SaveChangesAsync();

        logger.LogInformation("? Manager user created (manager / Manager@123)");
    }

    private static async Task CreateRegularUserAsync(ApplicationDbContext context, IPasswordService passwordService, ILogger logger)
    {
        if (await context.Users.AnyAsync(u => u.Username == "user"))
        {
            logger.LogInformation("?? Regular user already exists. Skipping...");
            return;
        }

        logger.LogInformation("?? Creating regular user...");

        var regularUser = new User
        {
            Username = "user",
            Email = "user@scanpet.com",
            PasswordHash = passwordService.HashPassword("User@123"),
            FullName = "Regular User",
            PhoneNumber = "+1234567892",
            IsEnabled = true,
            IsApproved = true
        };

        await context.Users.AddAsync(regularUser);
        await context.SaveChangesAsync();

        logger.LogInformation("? Regular user created (user / User@123)");
    }

    private static async Task AssignAdminRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminUser == null || adminRole == null)
        {
            logger.LogWarning("?? Admin user or role not found. Skipping role assignment.");
            return;
        }

        if (await context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
        {
            logger.LogInformation("?? Admin role already assigned. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning Admin role to admin user...");

        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();

        logger.LogInformation("? Admin role assigned to admin user");
    }

    private static async Task AssignManagerRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var managerUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
        var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");

        if (managerUser == null || managerRole == null)
        {
            logger.LogWarning("?? Manager user or role not found. Skipping role assignment.");
            return;
        }

        if (await context.UserRoles.AnyAsync(ur => ur.UserId == managerUser.Id && ur.RoleId == managerRole.Id))
        {
            logger.LogInformation("?? Manager role already assigned. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning Manager role to manager user...");

        var userRole = new UserRole
        {
            UserId = managerUser.Id,
            RoleId = managerRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();

        logger.LogInformation("? Manager role assigned to manager user");
    }

    private static async Task AssignUserRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var regularUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "user");
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

        if (regularUser == null || userRole == null)
        {
            logger.LogWarning("?? Regular user or User role not found. Skipping role assignment.");
            return;
        }

        if (await context.UserRoles.AnyAsync(ur => ur.UserId == regularUser.Id && ur.RoleId == userRole.Id))
        {
            logger.LogInformation("?? User role already assigned. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning User role to regular user...");

        var userRoleAssignment = new UserRole
        {
            UserId = regularUser.Id,
            RoleId = userRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddAsync(userRoleAssignment);
        await context.SaveChangesAsync();

        logger.LogInformation("? User role assigned to regular user");
    }

    private static async Task AssignPermissionsToAdminRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            logger.LogWarning("?? Admin role not found. Skipping permission assignment.");
            return;
        }

        if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id))
        {
            logger.LogInformation("?? Admin role already has permissions. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning all permissions to Admin role...");

        // Get all permission bits and combine them into a bitmask
        var allPermissions = await context.Permissions.ToListAsync();
        long permissionsBitmask = 0;
        
        foreach (var permission in allPermissions)
        {
            permissionsBitmask |= permission.PermissionBit;
        }

        var rolePermission = new RolePermission
        {
            RoleId = adminRole.Id,
            PermissionsBitmask = permissionsBitmask
        };

        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Assigned {allPermissions.Count} permissions to Admin role (bitmask: {permissionsBitmask})");
    }

    private static async Task AssignPermissionsToManagerRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
        if (managerRole == null)
        {
            logger.LogWarning("?? Manager role not found. Skipping permission assignment.");
            return;
        }

        if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == managerRole.Id))
        {
            logger.LogInformation("?? Manager role already has permissions. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning operational permissions to Manager role...");

        // Get specific permissions for Manager role (operational access only)
        var managerPermissionTypes = new[]
        {
            PermissionType.ItemView,
            PermissionType.ItemCreate,
            PermissionType.ItemEdit,
            PermissionType.OrderView,
            PermissionType.OrderCreate,
            PermissionType.OrderEdit,
            PermissionType.OrderConfirm,
            PermissionType.ColorView,
            PermissionType.LocationView
        };

        var managerPermissions = await context.Permissions
            .Where(p => managerPermissionTypes.Contains((PermissionType)p.PermissionBit))
            .ToListAsync();

        // Combine permission bits into bitmask
        long permissionsBitmask = 0;
        foreach (var permission in managerPermissions)
        {
            permissionsBitmask |= permission.PermissionBit;
        }

        var rolePermission = new RolePermission
        {
            RoleId = managerRole.Id,
            PermissionsBitmask = permissionsBitmask
        };

        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Assigned {managerPermissions.Count} permissions to Manager role (bitmask: {permissionsBitmask})");
        logger.LogInformation("?? Manager permissions: Items (View/Create/Edit), Orders (View/Create/Edit/Confirm), Colors (View), Locations (View)");
    }

    private static async Task AssignPermissionsToUserRoleAsync(ApplicationDbContext context, ILogger logger)
    {
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            logger.LogWarning("?? User role not found. Skipping permission assignment.");
            return;
        }

        if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == userRole.Id))
        {
            logger.LogInformation("?? User role already has permissions. Skipping...");
            return;
        }

        logger.LogInformation("?? Assigning view-only permissions to User role...");

        // Get specific permissions for User role (read-only access)
        var userPermissionTypes = new[]
        {
            PermissionType.ItemView,
            PermissionType.OrderView,
            PermissionType.ColorView,
            PermissionType.LocationView
        };

        var userPermissions = await context.Permissions
            .Where(p => userPermissionTypes.Contains((PermissionType)p.PermissionBit))
            .ToListAsync();

        // Combine permission bits into bitmask
        long permissionsBitmask = 0;
        foreach (var permission in userPermissions)
        {
            permissionsBitmask |= permission.PermissionBit;
        }

        var rolePermission = new RolePermission
        {
            RoleId = userRole.Id,
            PermissionsBitmask = permissionsBitmask
        };

        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Assigned {userPermissions.Count} permissions to User role (bitmask: {permissionsBitmask})");
        logger.LogInformation("?? User permissions: Items (View), Orders (View), Colors (View), Locations (View) - READ ONLY");
    }

    private static async Task SeedSampleColorsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Colors.AnyAsync())
        {
            logger.LogInformation("?? Colors already exist. Skipping...");
            return;
        }

        logger.LogInformation("?? Seeding sample colors...");

        var colors = new List<Color>
        {
            new() { Name = "Red", RedValue = 255, GreenValue = 0, BlueValue = 0, Description = "Primary red" },
            new() { Name = "Green", RedValue = 0, GreenValue = 255, BlueValue = 0, Description = "Primary green" },
            new() { Name = "Blue", RedValue = 0, GreenValue = 0, BlueValue = 255, Description = "Primary blue" },
            new() { Name = "Yellow", RedValue = 255, GreenValue = 255, BlueValue = 0, Description = "Yellow" },
            new() { Name = "Black", RedValue = 0, GreenValue = 0, BlueValue = 0, Description = "Black" },
            new() { Name = "White", RedValue = 255, GreenValue = 255, BlueValue = 255, Description = "White" },
            new() { Name = "Orange", RedValue = 255, GreenValue = 165, BlueValue = 0, Description = "Orange" },
            new() { Name = "Purple", RedValue = 128, GreenValue = 0, BlueValue = 128, Description = "Purple" },
            new() { Name = "Pink", RedValue = 255, GreenValue = 192, BlueValue = 203, Description = "Pink" },
            new() { Name = "Brown", RedValue = 165, GreenValue = 42, BlueValue = 42, Description = "Brown" }
        };

        await context.Colors.AddRangeAsync(colors);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Seeded {colors.Count} colors");
    }

    private static async Task SeedSampleLocationsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Locations.AnyAsync())
        {
            logger.LogInformation("?? Locations already exist. Skipping...");
            return;
        }

        logger.LogInformation("?? Seeding sample locations...");

        var locations = new List<Location>
        {
            new() 
            { 
                Name = "Main Warehouse", 
                Address = "123 Main Street",
                City = "New York",
                Country = "United States",
                PostalCode = "10001",
                IsActive = true
            },
            new() 
            { 
                Name = "Distribution Center", 
                Address = "456 Commerce Ave",
                City = "Los Angeles",
                Country = "United States",
                PostalCode = "90001",
                IsActive = true
            },
            new() 
            { 
                Name = "Storage Unit A", 
                Address = "789 Storage Ln",
                City = "Chicago",
                Country = "United States",
                PostalCode = "60601",
                IsActive = true
            }
        };

        await context.Locations.AddRangeAsync(locations);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Seeded {locations.Count} locations");
    }

    private static async Task SeedSampleItemsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Items.AnyAsync())
        {
            logger.LogInformation("?? Items already exist. Skipping...");
            return;
        }

        logger.LogInformation("?? Seeding sample items...");

        // Get first color for relationships
        var firstColor = await context.Colors.FirstOrDefaultAsync();

        if (firstColor == null)
        {
            logger.LogWarning("?? Colors not found. Skipping item seeding.");
            return;
        }

        var items = new List<Item>
        {
            new() 
            { 
                Name = "Pet Food - Premium",
                SKU = "PF-001",
                Description = "High-quality premium pet food for all breeds",
                BasePrice = 29.99m,
                Quantity = 100,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Toy - Ball",
                SKU = "PT-002",
                Description = "Interactive rubber ball toy for active play",
                BasePrice = 9.99m,
                Quantity = 50,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Collar - Large",
                SKU = "PC-003",
                Description = "Adjustable nylon collar for large pets",
                BasePrice = 15.99m,
                Quantity = 75,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Bed - Comfort",
                SKU = "PB-004",
                Description = "Orthopedic comfort bed for senior pets",
                BasePrice = 49.99m,
                Quantity = 30,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Shampoo",
                SKU = "PS-005",
                Description = "Natural and gentle grooming shampoo",
                BasePrice = 12.99m,
                Quantity = 60,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Leash - Retractable",
                SKU = "PL-006",
                Description = "15ft retractable leash with lock mechanism",
                BasePrice = 19.99m,
                Quantity = 40,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Treats - Organic",
                SKU = "PT-007",
                Description = "Healthy organic training treats",
                BasePrice = 7.99m,
                Quantity = 80,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Bowl Set",
                SKU = "PB-008",
                Description = "Stainless steel food and water bowl set",
                BasePrice = 24.99m,
                Quantity = 45,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Carrier - Travel",
                SKU = "PC-009",
                Description = "Airline-approved travel carrier",
                BasePrice = 59.99m,
                Quantity = 20,
                ColorId = firstColor.Id
            },
            new() 
            { 
                Name = "Pet Grooming Kit",
                SKU = "PG-010",
                Description = "Complete grooming tools set",
                BasePrice = 34.99m,
                Quantity = 35,
                ColorId = firstColor.Id
            }
        };

        await context.Items.AddRangeAsync(items);
        await context.SaveChangesAsync();

        logger.LogInformation($"? Seeded {items.Count} sample items");
    }
}
