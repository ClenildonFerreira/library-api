using LibraryApi.Features.Student.models;
using LibraryApi.Features.Loan.models;

namespace LibraryApi.Features.Student.views;

public class StudentDetailViewModel
{
    public StudentDto? Student { get; set; }
    public List<LoanSummaryDto>? Loans { get; set; }
}

public class LoanSummaryDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? BookTitle { get; set; }
    public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;
}