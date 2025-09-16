namespace LibraryApi.Entities;

public class Book
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public int Quantity { get; set; }
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
    public Author? Author { get; set; }
    public Genre? Genre { get; set; }
    public ICollection<Loan>? Loans { get; set; }
}