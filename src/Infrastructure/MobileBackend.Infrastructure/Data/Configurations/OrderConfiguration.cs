using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(o => o.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.ClientPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(o => o.ClientPhone)
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(o => o.LocationId)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasColumnType("text");

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.OrderStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(OrderStatus.Pending);

        builder.HasIndex(o => o.OrderStatus)
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.HasIndex(o => o.OrderDate)
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship with Location
        builder.HasOne(o => o.Location)
            .WithMany(l => l.Orders)
            .HasForeignKey(o => o.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
