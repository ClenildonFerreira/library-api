namespace LibraryApi.Entities;

public class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }

    public ICollection<Loan>? Loans { get; set; }
}