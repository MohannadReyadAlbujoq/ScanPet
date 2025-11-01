using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id)
            .ValueGeneratedOnAdd();

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(al => al.Action);

        builder.Property(al => al.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        // Composite index for entity lookups
        builder.HasIndex(al => new { al.EntityName, al.EntityId });

        builder.Property(al => al.OldValues)
            .HasColumnType("jsonb"); // PostgreSQL JSONB type

        builder.Property(al => al.NewValues)
            .HasColumnType("jsonb"); // PostgreSQL JSONB type

        builder.Property(al => al.IpAddress)
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.Timestamp)
            .IsRequired();

        builder.HasIndex(al => al.Timestamp);

        builder.Property(al => al.AdditionalInfo)
            .HasColumnType("text");

        // Index for user-specific audit logs
        builder.HasIndex(al => al.UserId);

        // Relationship with User
        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
