using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Data.Configurations;

public class EntityTranslationConfiguration : IEntityTypeConfiguration<EntityTranslation>
{
    public void Configure(EntityTypeBuilder<EntityTranslation> b)
    {
        b.ToTable("EntityTranslations");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(64);
        b.Property(x => x.EntityId).IsRequired();
        b.Property(x => x.LanguageCode).IsRequired().HasMaxLength(8);
        b.Property(x => x.Field).IsRequired().HasMaxLength(64);
        b.Property(x => x.Value).IsRequired().HasMaxLength(2000);
        b.HasIndex(x => new { x.EntityType, x.EntityId, x.LanguageCode, x.Field }).IsUnique();
        b.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
