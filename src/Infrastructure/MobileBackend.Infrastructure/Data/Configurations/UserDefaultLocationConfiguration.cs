using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class UserDefaultLocationConfiguration : IEntityTypeConfiguration<UserDefaultLocation>
{
    public void Configure(EntityTypeBuilder<UserDefaultLocation> builder)
    {
        builder.ToTable("UserDefaultLocations");

        builder.HasKey(udl => udl.Id);

        builder.Property(udl => udl.Id)
            .ValueGeneratedOnAdd();

        builder.Property(udl => udl.UserId)
            .IsRequired();

        builder.Property(udl => udl.LocationId)
            .IsRequired();

        builder.HasIndex(udl => new { udl.UserId, udl.LocationId })
            .IsUnique();

        builder.HasOne(udl => udl.User)
            .WithMany(u => u.DefaultLocations)
            .HasForeignKey(udl => udl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(udl => udl.Location)
            .WithMany()
            .HasForeignKey(udl => udl.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
