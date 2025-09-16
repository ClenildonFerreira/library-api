using LibraryApi.Features.Loan.models;

namespace LibraryApi.Features.Loan.views;

public class LoanDetailViewModel
{
    public LoanDto? Loan { get; set; }
    public BookDetailDto? Book { get; set; }
    public StudentDetailDto? Student { get; set; }
}

public class BookDetailDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ISBN { get; set; }
    public string? AuthorName { get; set; }
    public string? GenreName { get; set; }
}

public class StudentDetailDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
}