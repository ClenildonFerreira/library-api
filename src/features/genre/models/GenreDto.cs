namespace LibraryApi.Features.Genre.models;

public class GenreDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int BookCount { get; set; }
}