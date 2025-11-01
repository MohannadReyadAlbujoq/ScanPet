using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        builder.ToTable("Colors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(c => c.RedValue)
            .IsRequired();

        builder.Property(c => c.GreenValue)
            .IsRequired();

        builder.Property(c => c.BlueValue)
            .IsRequired();

        // Unique constraint on RGB values
        builder.HasIndex(c => new { c.RedValue, c.GreenValue, c.BlueValue })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(c => c.Description)
            .HasColumnType("text");

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Ignore computed HexCode property - it's calculated in entity
        builder.Ignore(c => c.HexCode);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
