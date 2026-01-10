namespace Adorika.Application.Common.Wrapper;

public class PagedResult<T>(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
{
    public IEnumerable<T> Items { get; init; } = items;
    public PaginationMetadata Metadata { get; init; } = new PaginationMetadata(pageNumber, pageSize, totalCount);

    // Static Factory Methods
    public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var items = source as IList<T> ?? source.ToList();
        var totalCount = items.Count;
        var pagedItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        return new PagedResult<T>(pagedItems, totalCount, pageNumber, pageSize);
    }

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
        => new(Enumerable.Empty<T>(), 0, pageNumber, pageSize);

    // Mapping logic
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new PagedResult<TResult>(
            Items.Select(mapper),
            Metadata.TotalCount,
            Metadata.PageNumber,
            Metadata.PageSize
        );
    }
}
