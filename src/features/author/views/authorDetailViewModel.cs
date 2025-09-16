using LibraryApi.Features.Author.models;

namespace LibraryApi.Features.Author.views;

public class AuthorDetailViewModel
{
    public AuthorDto? Author { get; set; }
    public List<BookSummaryDto>? Books { get; set; }
}

public class BookSummaryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
}