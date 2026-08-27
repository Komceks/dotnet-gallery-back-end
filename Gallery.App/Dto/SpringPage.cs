using Gallery.Bl.Models;
using Gallery.Bl.Sort;

namespace Gallery.App.Dto;

// Your Angular code already deserializes the full Spring Page<T> shape:
//   { content, pageable: {...}, last, totalPages, totalElements, size, sort, first, numberOfElements, empty }
// We reproduce that shape exactly so the frontend works unchanged.
public class SpringPage<T>
{
    public IReadOnlyList<T> Content { get; init; } = Array.Empty<T>();
    public Pageable Pageable { get; init; } = new();
    public bool Last { get; init; }
    public int TotalPages { get; init; }
    public long TotalElements { get; init; }
    public int Size { get; init; }
    public PageSortFlags Sort { get; init; } = new();
    public bool First { get; init; }
    public int NumberOfElements { get; init; }
    public bool Empty { get; init; }

    public static SpringPage<TOut> From<TIn, TOut>(
        PagedResult<TIn> src,
        Func<TIn, TOut> map,
        ImageSortModel sortModel)
    {
        var items = src.Items.Select(map).ToList();
        var totalPages = src.PageSize == 0
            ? 0
            : (int)Math.Ceiling(src.TotalElements / (double)src.PageSize);

        return new SpringPage<TOut>
        {
            Content = items,
            Pageable = new Pageable
            {
                PageNumber = src.PageNumber,
                PageSize = src.PageSize,
                Offset = (long)src.PageNumber * src.PageSize,
                Paged = true,
                Unpaged = false,
                Sort = new PageSortFlags { Empty = false, Sorted = true, Unsorted = false }
            },
            Last = src.PageNumber >= totalPages - 1,
            TotalPages = totalPages,
            TotalElements = src.TotalElements,
            Size = src.PageSize,
            Sort = new PageSortFlags { Empty = false, Sorted = true, Unsorted = false },
            First = src.PageNumber == 0,
            NumberOfElements = items.Count,
            Empty = items.Count == 0
        };
    }
}

public class Pageable
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public PageSortFlags Sort { get; init; } = new();
    public long Offset { get; init; }
    public bool Paged { get; init; }
    public bool Unpaged { get; init; }
}

public class PageSortFlags
{
    public bool Empty { get; init; } = true;
    public bool Sorted { get; init; }
    public bool Unsorted { get; init; } = true;
}
