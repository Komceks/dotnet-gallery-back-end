namespace Gallery.Bl.Models;

// Internal paged result. The controller layer translates this to the exact Spring `Page<T>`
// JSON shape your Angular frontend expects (see Gallery.App/Dto/SpringPage.cs).
public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, long TotalElements);
