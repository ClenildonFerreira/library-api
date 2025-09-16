namespace LibraryApi.Features.Book.models;

public class BookDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public int Quantity { get; set; }
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
    public string? AuthorName { get; set; }
    public string? GenreName { get; set; }
    public int AvailableQuantity { get; set; }
}