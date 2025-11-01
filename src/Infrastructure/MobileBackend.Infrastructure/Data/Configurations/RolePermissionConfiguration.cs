using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.Property(rp => rp.PermissionsBitmask)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(rp => rp.CreatedAt)
            .IsRequired();

        builder.Property(rp => rp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship with Role
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for quick role permission lookups
        builder.HasIndex(rp => rp.RoleId)
            .HasFilter("\"IsDeleted\" = false");
    }
}
