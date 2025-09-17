namespace LibraryApi.Features.Student.models;

public class CreateStudentDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
}