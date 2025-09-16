namespace LibraryApi.Features.Book.models;

/// <summary>
/// DTO para exibição resumida de informações de livros em listagens
/// </summary>
public class BookSummaryDto
{
    /// <summary>
    /// ID do livro
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Título do livro
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// ISBN do livro
    /// </summary>
    public string? ISBN { get; set; }
    
    /// <summary>
    /// Ano de publicação do livro
    /// </summary>
    public int PublicationYear { get; set; }
    
    /// <summary>
    /// Nome do autor do livro
    /// </summary>
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// Nome do gênero do livro
    /// </summary>
    public string? GenreName { get; set; }
}