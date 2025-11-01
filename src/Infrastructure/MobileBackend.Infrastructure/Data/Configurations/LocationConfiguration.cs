using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Address)
            .HasMaxLength(500);

        builder.Property(l => l.City)
            .HasMaxLength(100);

        builder.Property(l => l.Country)
            .HasMaxLength(100);

        builder.Property(l => l.PostalCode)
            .HasMaxLength(20);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Index for active locations
        builder.HasIndex(l => l.IsActive)
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
