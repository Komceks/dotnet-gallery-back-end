using Gallery.Bl.Sort;

namespace Gallery.Bl.Models;

// These are the BL-layer DTOs (input/output of the service methods),
// equivalent to your `bl/.../model/*.java` classes (CreateImageModel, ImageSearchModel, etc).
// Keeping them separate from controller DTOs lets you change the API surface
// without rewriting business logic.
//
// `record` is C#'s answer to Lombok @Builder + @Data: immutable, value-equality, concise.

public record CreateImageModel(
    byte[] ImageFile,
    string ImageName,
    string Description,
    DateOnly Date,
    string AuthorName,
    IReadOnlySet<string> TagNames,
    DateTime UploadDate
);

public record ImageUpdateModel(
    long Id,
    byte[]? ImageFile,         // null means "keep existing"
    string ImageName,
    string Description,
    DateOnly Date,
    string AuthorName,
    IReadOnlySet<string> TagNames,
    DateTime UploadDate
);

public record ImageSearchPartModel(
    string? ImageName,
    string? Description,
    string? AuthorName,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    IReadOnlySet<string>? TagNames
);

public record ImageSortModel(ImageSortField Field, SortOrder Order);

public record ImageSearchModel(
    int PageNumber,
    int PageSize,
    string? Query,
    ImageSearchPartModel? SearchPart,
    ImageSortModel Sort
);

public record ThumbnailListModel(
    long Id,
    string ImageName,
    string Description,
    string AuthorName,
    DateOnly Date,
    byte[] Thumbnail,
    DateTime UploadDate,
    IReadOnlySet<string> Tags
);

public record ImageViewModel(byte[] Image, IReadOnlySet<string> Tags);
