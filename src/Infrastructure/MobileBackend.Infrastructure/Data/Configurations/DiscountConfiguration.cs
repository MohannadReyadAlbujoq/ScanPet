using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Scope).IsRequired();
        builder.Property(d => d.ItemId).IsRequired();
        builder.Property(d => d.Amount).HasPrecision(18, 4);
        builder.Property(d => d.Label).HasMaxLength(200);
        builder.Property(d => d.IsStackable).HasDefaultValue(true);
        builder.Property(d => d.IsRevertable).HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasOne(d => d.Item)
            .WithMany(i => i.Discounts)
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Inventory)
            .WithMany()
            .HasForeignKey(d => d.InventoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Location)
            .WithMany()
            .HasForeignKey(d => d.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => new { d.ItemId, d.Scope });
        builder.HasIndex(d => new { d.ItemId, d.InventoryId });
        builder.HasIndex(d => new { d.ItemId, d.LocationId });
    }
}
