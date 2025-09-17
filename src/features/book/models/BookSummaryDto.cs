namespace LibraryApi.Features.Book.models;

public class BookSummaryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? AuthorName { get; set; }
    public string? GenreName { get; set; }
}