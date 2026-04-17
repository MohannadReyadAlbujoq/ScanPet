using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class UserDefaultInventoryConfiguration : IEntityTypeConfiguration<UserDefaultInventory>
{
    public void Configure(EntityTypeBuilder<UserDefaultInventory> builder)
    {
        builder.ToTable("UserDefaultInventories");

        builder.HasKey(udi => udi.Id);

        builder.Property(udi => udi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(udi => udi.UserId)
            .IsRequired();

        builder.Property(udi => udi.InventoryId)
            .IsRequired();

        // Each user can only have one entry per inventory
        builder.HasIndex(udi => new { udi.UserId, udi.InventoryId })
            .IsUnique();

        builder.HasOne(udi => udi.User)
            .WithMany(u => u.DefaultInventories)
            .HasForeignKey(udi => udi.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(udi => udi.Inventory)
            .WithMany()
            .HasForeignKey(udi => udi.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
