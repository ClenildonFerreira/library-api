using LibraryApi.Features.Author.models;
using LibraryApi.Features.Book.models;

namespace LibraryApi.Features.Author.views;

public class AuthorDetailViewModel
{
    public AuthorDto? Author { get; set; }
    public List<BookSummaryDto>? Books { get; set; }
}