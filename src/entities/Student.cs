namespace LibraryApi.Entities;

using System.ComponentModel.DataAnnotations;

public class Student
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do estudante é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email fornecido não é válido")]
    [StringLength(100, ErrorMessage = "O email deve ter no máximo {1} caracteres")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "O número de matrícula é obrigatório")]
    [StringLength(20, ErrorMessage = "O número de matrícula deve ter no máximo {1} caracteres")]
    public string? RegistrationNumber { get; set; }

    [Phone(ErrorMessage = "O número de telefone fornecido não é válido")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo {1} caracteres")]
    public string? Phone { get; set; }

    public ICollection<Loan>? Loans { get; set; }
}