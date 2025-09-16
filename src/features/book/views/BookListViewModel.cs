using LibraryApi.Features.Book.models;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Book.views;

public class BookListViewModel
{
    public PagedResult<BookDto>? Books { get; set; }
}