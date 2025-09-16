namespace LibraryApi.Features.Loan.models;

public class CreateLoanDto
{
    public DateTime? LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public int BookId { get; set; }
    public int StudentId { get; set; }
}