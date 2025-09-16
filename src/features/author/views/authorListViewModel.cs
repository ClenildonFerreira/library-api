using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Author.models;

namespace LibraryApi.Features.Author.views;

public class AuthorListViewModel
{
    public PagedResult<AuthorDto>? Authors { get; set; }
}