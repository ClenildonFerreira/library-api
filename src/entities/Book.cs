namespace LibraryApi.Entities;

using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O título do livro é obrigatório")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo {1} caracteres")]
    public string? Title { get; set; }

    [StringLength(20, ErrorMessage = "O ISBN deve ter no máximo {1} caracteres")]
    public string? ISBN { get; set; }

    [Range(1000, 2100, ErrorMessage = "O ano de publicação deve estar entre {1} e {2}")]
    public int PublicationYear { get; set; }

    [Range(0, 1000, ErrorMessage = "A quantidade deve estar entre {1} e {2}")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "O autor é obrigatório")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "O gênero é obrigatório")]
    public int GenreId { get; set; }

    public Author? Author { get; set; }
    public Genre? Genre { get; set; }
    public ICollection<Loan>? Loans { get; set; }
}