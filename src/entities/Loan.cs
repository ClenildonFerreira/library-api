using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Entities;

public class Loan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A data do empréstimo é obrigatória")]
    [DataType(DataType.Date)]
    public DateTime LoanDate { get; set; }

    [Required(ErrorMessage = "A data de devolução é obrigatória")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    public bool IsReturned => ReturnDate.HasValue;
    public bool IsLate => !IsReturned && DateTime.Now > DueDate;

    [Required(ErrorMessage = "O livro é obrigatório")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "O estudante é obrigatório")]
    public int StudentId { get; set; }

    public Book? Book { get; set; }
    public Student? Student { get; set; }
}