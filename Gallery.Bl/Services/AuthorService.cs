using Gallery.Bl.Data;
using Gallery.Model;
using Microsoft.EntityFrameworkCore;

namespace Gallery.Bl.Services;

public interface IAuthorService
{
    Task<Author> FindOrCreateAsync(string name, CancellationToken ct = default);
}

// Spring: @Service. In ASP.NET Core, registration happens in DependencyInjection.cs
// (instead of being annotation-driven).
public class AuthorService : IAuthorService
{
    private readonly GalleryDbContext _db;

    // Constructor injection — same pattern as Spring, no [Autowired] needed.
    public AuthorService(GalleryDbContext db) => _db = db;

    public async Task<Author> FindOrCreateAsync(string name, CancellationToken ct = default)
    {
        var existing = await _db.Authors.FirstOrDefaultAsync(a => a.Name == name, ct);
        if (existing is not null) return existing;

        var author = new Author { Name = name };
        _db.Authors.Add(author);
        // SaveChanges is called by the calling service inside a transaction.
        return author;
    }
}
