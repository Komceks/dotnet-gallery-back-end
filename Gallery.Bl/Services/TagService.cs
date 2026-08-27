using Gallery.Bl.Data;
using Gallery.Model;
using Microsoft.EntityFrameworkCore;

namespace Gallery.Bl.Services;

public interface ITagService
{
    Task<HashSet<Tag>> FindOrCreateTagsAsync(IEnumerable<string> tagNames, CancellationToken ct = default);
}

public class TagService : ITagService
{
    private readonly GalleryDbContext _db;

    public TagService(GalleryDbContext db) => _db = db;

    public async Task<HashSet<Tag>> FindOrCreateTagsAsync(IEnumerable<string> tagNames, CancellationToken ct = default)
    {
        var names = tagNames?.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList() ?? new();
        if (names.Count == 0) return new HashSet<Tag>();

        // LINQ equivalent of: SELECT * FROM tag WHERE name IN (...)
        var existing = await _db.Tags.Where(t => names.Contains(t.Name)).ToListAsync(ct);
        
        var existingNames = existing.Select(t => t.Name).ToHashSet();

        // Create any tags that don't exist yet.
        var newTags = names
            .Where(n => !existingNames.Contains(n))
            .Select(n => new Tag { Name = n })
            .ToList();

        if (newTags.Count > 0)
        {
            _db.Tags.AddRange(newTags);
            // Note: not saving here. The owning ImageService.SaveChangesAsync covers the transaction.
        }

        return existing.Concat(newTags).ToHashSet();
    }
}
