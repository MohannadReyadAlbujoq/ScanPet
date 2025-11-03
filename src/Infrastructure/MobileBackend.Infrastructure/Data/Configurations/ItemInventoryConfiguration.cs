using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ItemInventory entity
/// </summary>
public class ItemInventoryConfiguration : IEntityTypeConfiguration<ItemInventory>
{
    public void Configure(EntityTypeBuilder<ItemInventory> builder)
    {
        // Table name
        builder.ToTable("ItemInventories");

        // Primary key
        builder.HasKey(ii => ii.Id);

        // Properties
        builder.Property(ii => ii.ItemId)
            .IsRequired();

        builder.Property(ii => ii.InventoryId)
            .IsRequired();

        builder.Property(ii => ii.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ii => ii.MinimumQuantity)
            .IsRequired(false);

        builder.Property(ii => ii.MaximumQuantity)
            .IsRequired(false);

        builder.Property(ii => ii.Notes)
            .HasMaxLength(500);

        // Soft Delete
        builder.Property(ii => ii.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ii => ii.DeletedAt)
            .IsRequired(false);

        builder.Property(ii => ii.DeletedBy)
            .IsRequired(false);

        // Audit fields
        builder.Property(ii => ii.CreatedAt)
            .IsRequired();

        builder.Property(ii => ii.CreatedBy)
            .IsRequired(false);

        builder.Property(ii => ii.UpdatedAt)
            .IsRequired(false);

        builder.Property(ii => ii.UpdatedBy)
            .IsRequired(false);

        // Relationships
        builder.HasOne(ii => ii.Item)
            .WithMany(i => i.ItemInventories)
            .HasForeignKey(ii => ii.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.Inventory)
            .WithMany(inv => inv.ItemInventories)
            .HasForeignKey(ii => ii.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - PostgreSQL syntax
        builder.HasIndex(ii => ii.ItemId)
            .HasDatabaseName("IX_ItemInventories_ItemId");

        builder.HasIndex(ii => ii.InventoryId)
            .HasDatabaseName("IX_ItemInventories_InventoryId");

        // Composite index for efficient lookups - PostgreSQL syntax
        builder.HasIndex(ii => new { ii.ItemId, ii.InventoryId })
            .HasDatabaseName("IX_ItemInventories_ItemId_InventoryId")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(ii => ii.CreatedBy)
            .HasDatabaseName("IX_ItemInventories_CreatedBy");

        // Index for soft delete
        builder.HasIndex(ii => ii.IsDeleted)
            .HasDatabaseName("IX_ItemInventories_IsDeleted");
    }
}
