namespace LibraryApi.Entities;

using System.ComponentModel.DataAnnotations;

public class Genre
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do gênero é obrigatório")]
    [StringLength(50, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres")]
    public string? Description { get; set; }

    public ICollection<Book>? Books { get; set; } = new List<Book>();
}