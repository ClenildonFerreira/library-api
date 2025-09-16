namespace LibraryApi.Infrastructure.Pagination;

public class PagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public List<T> Items { get; set; } = new List<T>();

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        Items = items;
    }

    public static PagedResult<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}