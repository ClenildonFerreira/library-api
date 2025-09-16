namespace LibraryApi.Features.Loan.models;

public class LoanDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int BookId { get; set; }
    public int StudentId { get; set; }
    public string? BookTitle { get; set; }
    public string? StudentName { get; set; }
    public bool IsReturned => ReturnDate.HasValue;
    public bool IsLate => !IsReturned && DateTime.Now > DueDate;
}