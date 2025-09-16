using LibraryApi.Features.Genre.models;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Genre.views;

public class GenreListViewModel
{
    public PagedResult<GenreDto>? Genres { get; set; }
}