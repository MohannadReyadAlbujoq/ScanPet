using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Services;

public class TranslationService : ITranslationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TranslationService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task ReplaceTranslationsAsync(string entityType, Guid entityId,
        IEnumerable<TranslationInput> translations, CancellationToken ct = default)
    {
        var existing = await _db.EntityTranslations
            .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDeleted)
            .ToListAsync(ct);

        foreach (var row in existing)
        {
            row.IsDeleted = true;
            row.DeletedAt = DateTime.UtcNow;
            row.DeletedBy = _currentUser.UserId;
        }

        foreach (var input in translations)
        {
            if (string.IsNullOrWhiteSpace(input.LanguageCode)) continue;
            foreach (var (field, value) in input.Fields)
            {
                if (string.IsNullOrWhiteSpace(value)) continue;
                _db.EntityTranslations.Add(new EntityTranslation
                {
                    Id = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    LanguageCode = input.LanguageCode.Trim().ToLowerInvariant(),
                    Field = field,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                });
            }
        }
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> GetTranslationsAsync(
        string entityType, Guid entityId, CancellationToken ct = default)
    {
        var rows = await _db.EntityTranslations
            .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDeleted)
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.LanguageCode)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.Field, r => r.Value));
    }
}
