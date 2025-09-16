namespace LibraryApi.Infrastructure.Pagination;

public static class PaginationExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        return PagedResult<T>.Create(source, pageNumber, pageSize);
    }
}