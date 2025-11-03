using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Inventory entity
/// </summary>
public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        // Table name
        builder.ToTable("Inventories");

        // Primary key
        builder.HasKey(i => i.Id);

        // Properties
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Location)
            .HasMaxLength(500);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Soft Delete
        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.DeletedAt)
            .IsRequired(false);

        builder.Property(i => i.DeletedBy)
            .IsRequired(false);

        // Audit fields
        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.CreatedBy)
            .IsRequired(false);

        builder.Property(i => i.UpdatedAt)
            .IsRequired(false);

        builder.Property(i => i.UpdatedBy)
            .IsRequired(false);

        // Indexes - PostgreSQL syntax
        builder.HasIndex(i => i.Name)
            .HasDatabaseName("IX_Inventories_Name")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(i => i.IsActive)
            .HasDatabaseName("IX_Inventories_IsActive")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(i => i.CreatedBy)
            .HasDatabaseName("IX_Inventories_CreatedBy");

        builder.HasIndex(i => i.IsDeleted)
            .HasDatabaseName("IX_Inventories_IsDeleted");
    }
}
