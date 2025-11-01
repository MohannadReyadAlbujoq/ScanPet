using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasColumnType("text");

        builder.Property(i => i.SKU)
            .HasMaxLength(100);

        builder.HasIndex(i => i.SKU)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"SKU\" IS NOT NULL");

        builder.Property(i => i.BasePrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Index for color lookup
        builder.HasIndex(i => i.ColorId)
            .HasFilter("\"IsDeleted\" = false");

        // Index for items with available quantity
        builder.HasIndex(i => i.Quantity)
            .HasFilter("\"IsDeleted\" = false AND \"Quantity\" > 0");

        // Relationship with Color
        builder.HasOne(i => i.Color)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.ColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
