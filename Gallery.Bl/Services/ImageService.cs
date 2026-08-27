using Gallery.Bl.Data;
using Gallery.Bl.Models;
using Gallery.Bl.Sort;
using Gallery.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gallery.Bl.Services;

public interface IImageService
{
    Task UploadAsync(CreateImageModel model, CancellationToken ct = default);
    Task<PagedResult<ThumbnailListModel>> SearchAsync(ImageSearchModel model, CancellationToken ct = default);
    Task<ImageViewModel?> ViewAsync(long id, CancellationToken ct = default);
    Task<ImageUpdateModel> UpdateAsync(ImageUpdateModel model, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public class ImageService : IImageService
{
    private readonly GalleryDbContext _db;
    private readonly IAuthorService _authors;
    private readonly ITagService _tags;

    public ImageService(
        GalleryDbContext db,
        IAuthorService authors,
        ITagService tags)
    {
        _db = db;
        _authors = authors;
        _tags = tags;
    }

    // ---- Upload (Spring: @Transactional save) ----
    public async Task UploadAsync(CreateImageModel m, CancellationToken ct = default)
    {
        var author = await _authors.FindOrCreateAsync(m.AuthorName, ct);
        var tags = await _tags.FindOrCreateTagsAsync(m.TagNames, ct);
        var thumbnail = ThumbnailGenerator.CreateThumbnail(m.ImageFile);

        var image = new Image
        {
            ImageBlob = m.ImageFile,
            Name = m.ImageName,
            Description = m.Description,
            Date = m.Date,
            Author = author,
            Tags = tags,
            UploadDate = m.UploadDate,
            Thumbnail = thumbnail
        };

        _db.Images.Add(image);
        await _db.SaveChangesAsync(ct);
    }

    // ---- Search ----
    // Compare this to your CustomImageRepositoryImpl + ImageSpecification (~250 lines of JPA Criteria).
    // LINQ + EF Core does the same job in ~40 lines; the expression tree gets translated to SQL.
    public async Task<PagedResult<ThumbnailListModel>> SearchAsync(ImageSearchModel m, CancellationToken ct = default)
    {
        // Start with all images. AsNoTracking is the EF Core equivalent of @Transactional(readOnly = true).
        IQueryable<Image> query = _db.Images
            .AsNoTracking()
            .Include(i => i.Author)
            .Include(i => i.Tags);

        // Apply the search predicate. XOR is validated at the controller level.
        query = m.Query is not null
            ? ApplyTextQuery(query, m.Query)
            : ApplyStructuredSearch(query, m.SearchPart!);

        // Count BEFORE pagination.
        var total = await query.LongCountAsync(ct);

        // Sorting.
        query = ApplySort(query, m.Sort);

        // Pagination.
        var page = await query
            .Skip(m.PageNumber * m.PageSize)
            .Take(m.PageSize)
            .Select(i => new ThumbnailListModel(
                i.Id,
                i.Name,
                i.Description,
                i.Author.Name,
                i.Date,
                i.Thumbnail,
                i.UploadDate,
                i.Tags.Select(t => t.Name).ToHashSet()))
            .ToListAsync(ct);

        return new PagedResult<ThumbnailListModel>(page, m.PageNumber, m.PageSize, total);
    }

    // Spring's ImageSpecification: a free-text "query" string is split on whitespace
    // and each term ORs across name/description/author/tag (and equals date if it looks like one).
    // Then all term groups are AND-ed together.
    private static IQueryable<Image> ApplyTextQuery(IQueryable<Image> q, string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString)) return q;

        foreach (var term in queryString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = term; // capture for closure
            var pattern = $"%{t.ToLower()}%";
            var hasDate = DateOnly.TryParse(t, out var parsedDate);

            // EF.Functions.ILike is Npgsql's case-insensitive LIKE.
            // If you switch to another provider, use .ToLower().Contains(...) instead.
            q = q.Where(i =>
                EF.Functions.ILike(i.Name, pattern) ||
                EF.Functions.ILike(i.Description, pattern) ||
                EF.Functions.ILike(i.Author.Name, pattern) ||
                i.Tags.Any(tag => tag.Name.ToLower() == t.ToLower()) ||
                (hasDate && i.Date == parsedDate));
        }

        return q;
    }

    private static IQueryable<Image> ApplyStructuredSearch(IQueryable<Image> q, ImageSearchPartModel s)
    {
        if (!string.IsNullOrWhiteSpace(s.ImageName))
            q = q.Where(i => EF.Functions.ILike(i.Name, $"%{s.ImageName}%"));

        if (!string.IsNullOrWhiteSpace(s.Description))
            q = q.Where(i => EF.Functions.ILike(i.Description, $"%{s.Description}%"));

        if (!string.IsNullOrWhiteSpace(s.AuthorName))
            q = q.Where(i => EF.Functions.ILike(i.Author.Name, $"%{s.AuthorName}%"));

        if (s.DateFrom is { } from) q = q.Where(i => i.Date >= from);
        if (s.DateTo is { } to) q = q.Where(i => i.Date <= to);

        if (s.TagNames is { Count: > 0 } tagNames)
            q = q.Where(i => i.Tags.Any(t => tagNames.Contains(t.Name)));

        return q;
    }

    private static IQueryable<Image> ApplySort(IQueryable<Image> q, ImageSortModel sort)
    {
        // Replaces the Java metamodel-Supplier dance with simple lambdas.
        Expression<Func<Image, object>> keySelector = sort.Field switch
        {
            ImageSortField.UPLOAD_DATE => i => i.UploadDate,
            ImageSortField.NAME => i => i.Name,
            ImageSortField.DATE => i => i.Date,
            _ => i => i.UploadDate
        };

        return sort.Order == SortOrder.DESC
            ? q.OrderByDescending(keySelector)
            : q.OrderBy(keySelector);
    }

    // ---- View single image ----
    public async Task<ImageViewModel?> ViewAsync(long id, CancellationToken ct = default)
    {
        var img = await _db.Images
            .AsNoTracking()
            .Include(i => i.Tags)
            .Where(i => i.Id == id)
            .Select(i => new ImageViewModel(i.ImageBlob, i.Tags.Select(t => t.Name).ToHashSet()))
            .FirstOrDefaultAsync(ct);

        return img;
    }

    // ---- Update ----
    public async Task<ImageUpdateModel> UpdateAsync(ImageUpdateModel m, CancellationToken ct = default)
    {
        var image = await _db.Images
            .Include(i => i.Tags)
            .Include(i => i.Author)
            .FirstOrDefaultAsync(i => i.Id == m.Id, ct);

        if (image is null)
            throw new KeyNotFoundException($"Image with ID: {m.Id} not found");

        var author = await _authors.FindOrCreateAsync(m.AuthorName, ct);
        var tags = await _tags.FindOrCreateTagsAsync(m.TagNames, ct);

        image.Name = m.ImageName;
        image.Description = m.Description;
        image.Date = m.Date;
        image.Author = author;
        image.Tags = tags;
        image.UploadDate = m.UploadDate;

        if (m.ImageFile is not null)
        {
            image.ImageBlob = m.ImageFile;
            image.Thumbnail = ThumbnailGenerator.CreateThumbnail(m.ImageFile);
        }

        await _db.SaveChangesAsync(ct);

        return m with { ImageFile = image.ImageBlob };
    }

    // ---- Delete ----
    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        // ExecuteDeleteAsync = bulk delete without loading the entity first.
        // Equivalent to Spring Data's deleteById but generates a single DELETE statement.
        await _db.Images.Where(i => i.Id == id).ExecuteDeleteAsync(ct);
    }
}
