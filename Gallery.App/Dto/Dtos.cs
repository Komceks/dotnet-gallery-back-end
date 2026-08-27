using System.ComponentModel.DataAnnotations;
using Gallery.Bl.Models;
using Gallery.Bl.Sort;

namespace Gallery.App.Dto;

// ============================================================
// Request DTOs — what Angular sends
// ============================================================

// Spring used @Valid groups (CreateRequest / UpdateRequest) to share one DTO between create & update.
// In .NET it's cleaner to validate per-action; the controller calls ValidateForCreate/ValidateForUpdate.
public class ImageSaveRequest
{
    public long? Id { get; set; }                         // null on create, non-null on update

    [Required(AllowEmptyStrings = false)]
    public string ImageName { get; set; } = null!;

    public string? Description { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string AuthorName { get; set; } = null!;

    public DateOnly Date { get; set; }                    // @PastOrPresent enforced in controller

    public HashSet<string>? Tags { get; set; }

    public CreateImageModel ToCreateModel(byte[] file) => new(
        ImageFile: file,
        ImageName: ImageName,
        Description: Description ?? "",
        Date: Date,
        AuthorName: AuthorName,
        TagNames: Tags ?? new HashSet<string>(),
        UploadDate: DateTime.UtcNow
    );

    public ImageUpdateModel ToUpdateModel(byte[]? file) => new(
        Id: Id!.Value,
        ImageFile: file,
        ImageName: ImageName,
        Description: Description ?? "",
        Date: Date,
        AuthorName: AuthorName,
        TagNames: Tags ?? new HashSet<string>(),
        UploadDate: DateTime.UtcNow
    );
}

public class ImageSearchRequest
{
    [Range(0, int.MaxValue)] public int PageNumber { get; set; }
    [Range(1, int.MaxValue)] public int PageSize { get; set; }
    public string? Query { get; set; }
    public ImageSearchRequestPart? ImageSearchRequestPart { get; set; }
    public ImageSortDto Sort { get; set; } = new();

    // Spring's @AssertTrue: exactly one of query / part is set.
    public bool IsValid() => (Query is null) ^ (ImageSearchRequestPart is null);

    public ImageSearchModel ToModel() => new(
        PageNumber: PageNumber,
        PageSize: PageSize,
        Query: Query,
        SearchPart: ImageSearchRequestPart?.ToModel(),
        Sort: new ImageSortModel(Sort.Field, Sort.Order)
    );
}

public class ImageSearchRequestPart
{
    public string? ImageName { get; set; }
    public string? Description { get; set; }
    public string? AuthorName { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public HashSet<string>? Tags { get; set; }

    public ImageSearchPartModel ToModel() =>
        new(ImageName, Description, AuthorName, DateFrom, DateTo, Tags);
}

public class ImageSortDto
{
    public ImageSortField Field { get; set; } = ImageSortField.UPLOAD_DATE;
    public SortOrder Order { get; set; } = SortOrder.DESC;
}

// ============================================================
// Response DTOs — what we send back to Angular
// ============================================================

public record ImageViewResponse(byte[] Image, IReadOnlySet<string> Tags)
{
    public static ImageViewResponse Of(ImageViewModel m) => new(m.Image, m.Tags);
}

public record ImageUpdateResponse(
    long Id,
    string ImageName,
    string Description,
    string AuthorName,
    DateOnly Date,
    IReadOnlySet<string> Tags,
    byte[]? Image)
{
    public static ImageUpdateResponse Of(ImageUpdateModel m) =>
        new(m.Id, m.ImageName, m.Description, m.AuthorName, m.Date, m.TagNames, m.ImageFile);
}

public record ThumbnailListDto(
    long Id,
    string ImageName,
    string Description,
    string AuthorName,
    DateOnly Date,
    byte[] Thumbnail,
    DateTime UploadDate,
    IReadOnlySet<string> Tags)
{
    public static ThumbnailListDto Of(ThumbnailListModel m) =>
        new(m.Id, m.ImageName, m.Description, m.AuthorName, m.Date, m.Thumbnail, m.UploadDate, m.Tags);
}
