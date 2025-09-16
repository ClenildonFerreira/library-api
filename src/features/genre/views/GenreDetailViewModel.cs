using LibraryApi.Features.Genre.models;

namespace LibraryApi.Features.Genre.views;

public class GenreDetailViewModel
{
    public GenreDto? Genre { get; set; }
    public List<BookSummaryDto>? Books { get; set; }
}

public class BookSummaryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? AuthorName { get; set; }
}