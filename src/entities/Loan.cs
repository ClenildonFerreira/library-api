using System;

namespace LibraryApi.Entities;

public class Loan
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned => ReturnDate.HasValue;
    public bool IsLate => !IsReturned && DateTime.Now > DueDate;

    public int BookId { get; set; }
    public int StudentId { get; set; }
    public Book? Book { get; set; }
    public Student? Student { get; set; }
}