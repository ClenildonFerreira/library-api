using LibraryApi.Features.Author.models;

namespace LibraryApi.Features.Author.views;

public class AuthorDetailViewModel
{
    public AuthorDto Author { get; set; } = new AuthorDto();
    public List<BookSummaryDto> Books { get; set; } = new List<BookSummaryDto>();
}

public class BookSummaryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
}