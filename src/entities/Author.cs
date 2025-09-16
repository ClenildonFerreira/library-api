namespace LibraryApi.Entities;

using System.ComponentModel.DataAnnotations;

public class Author
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do autor é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
    public string? Name { get; set; }

    [StringLength(2000, ErrorMessage = "A biografia deve ter no máximo {1} caracteres")]
    public string? Biography { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}