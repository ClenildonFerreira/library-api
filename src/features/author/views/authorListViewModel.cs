using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Author.models;

namespace LibraryApi.Features.Author.views;

/// <summary>
/// ViewModel para retornar uma lista paginada de autores
/// </summary>
public class AuthorListViewModel
{
    /// <summary>
    /// Lista paginada de autores
    /// </summary>
    public PagedResult<AuthorDto> Authors { get; set; } = new PagedResult<AuthorDto>(new List<AuthorDto>(), 0, 1, 10);
}