using Microsoft.EntityFrameworkCore;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        // This will automatically filter out soft-deleted records in all queries
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if entity implements ISoftDelete
            if (typeof(Domain.Common.ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                // Create expression: entity => !entity.IsDeleted
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "entity");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(Domain.Common.ISoftDelete.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var equals = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // This will be enhanced by interceptors for audit logging
        return base.SaveChangesAsync(cancellationToken);
    }
}
