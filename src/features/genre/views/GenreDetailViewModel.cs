using LibraryApi.Features.Genre.models;
using LibraryApi.Features.Book.models;

namespace LibraryApi.Features.Genre.views;

public class GenreDetailViewModel
{
    public GenreDto? Genre { get; set; }
    public List<BookSummaryDto>? Books { get; set; }
}