namespace LibraryApi.Features.Book.models;

public class UpdateBookDto
{
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public int Quantity { get; set; }
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
}