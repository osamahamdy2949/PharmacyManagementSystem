namespace PharmacyManagement.PL.Helpers;

public interface IPagedList
{
    int PageNumber { get; }
    int PageSize { get; }
    int TotalItems { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public class PagedList<T> : List<T>, IPagedList
{
    public const int DefaultPageSize = 10;

    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    private PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int totalItems)
        : base(items)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize = DefaultPageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var items = source as IList<T> ?? source.ToList();
        var totalItems = items.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        pageNumber = Math.Min(pageNumber, totalPages);

        return new PagedList<T>(
            items.Skip((pageNumber - 1) * pageSize).Take(pageSize),
            pageNumber,
            pageSize,
            totalItems);
    }
}
