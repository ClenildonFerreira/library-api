namespace LibraryApi.Features.Student.models;

public class StudentDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public int ActiveLoansCount { get; set; }
    public int TotalLoansCount { get; set; }
}