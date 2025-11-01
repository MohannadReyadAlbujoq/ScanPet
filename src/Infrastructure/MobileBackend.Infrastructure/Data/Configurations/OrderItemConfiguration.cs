using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.ItemId)
            .IsRequired();

        builder.Property(oi => oi.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(oi => oi.SerialNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(oi => oi.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(oi => oi.SalePrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oi => oi.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(OrderItemStatus.Successful);

        builder.Property(oi => oi.RefundedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(oi => oi.RefundReason)
            .HasColumnType("text");

        builder.Property(oi => oi.CreatedAt)
            .IsRequired();

        builder.Property(oi => oi.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for efficient queries
        builder.HasIndex(oi => oi.OrderId)
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(oi => oi.ItemId)
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(oi => oi.Status)
            .HasFilter("\"IsDeleted\" = false");

        // Composite index for order-item lookups
        builder.HasIndex(oi => new { oi.OrderId, oi.ItemId })
            .HasFilter("\"IsDeleted\" = false");

        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Item)
            .WithMany(i => i.OrderItems)
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(oi => oi.RefundedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(oi => oi.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
