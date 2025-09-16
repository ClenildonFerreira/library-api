namespace LibraryApi.Features.Author.models;

public class AuthorDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Biography { get; set; }
    public int BookCount { get; set; }
}