using LibraryApi.Features.Book.models;

namespace LibraryApi.Features.Book.views;

public class BookDetailViewModel
{
    public BookDto? Book { get; set; }
    public List<LoanSummaryDto>? Loans { get; set; }
}

public class LoanSummaryDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? StudentName { get; set; }
    public bool IsOverdue => ReturnDate == null && DateTime.Now > DueDate;
}