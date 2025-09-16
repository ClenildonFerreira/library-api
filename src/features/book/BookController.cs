using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace LibraryApi.Features.Book;

/// <summary>
/// Controller para gerenciamento de livros
/// </summary>
[ApiController]
[Route("books")]
[Produces("application/json")]
[Tags("Livros")]
public class BookController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de livros
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de livros</returns>
    /// <response code="200">Retorna a lista paginada de livros</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Book>>> GetBooks([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    /// <summary>
    /// Obtém um livro específico pelo ID
    /// </summary>
    /// <param name="id">ID do livro</param>
    /// <returns>O livro encontrado</returns>
    /// <response code="200">Retorna o livro encontrado</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entities.Book>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return book;
    }

    /// <summary>
    /// Cria um novo livro
    /// </summary>
    /// <param name="book">Dados do livro a ser criado</param>
    /// <returns>O novo livro criado</returns>
    /// <response code="201">Retorna o novo livro criado</response>
    /// <response code="400">Se os dados do livro forem inválidos ou se o autor ou gênero não existirem</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Entities.Book>> CreateBook(Entities.Book book)
    {
        var authorExists = await _context.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
        {
            return BadRequest("Autor não encontrado.");
        }

        var genreExists = await _context.Genres.AnyAsync(g => g.Id == book.GenreId);
        if (!genreExists)
        {
            return BadRequest("Gênero não encontrado.");
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    /// <summary>
    /// Atualiza um livro existente
    /// </summary>
    /// <param name="id">ID do livro a ser atualizado</param>
    /// <param name="book">Novos dados do livro</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o livro foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do livro forem inválidos ou se o autor ou gênero não existirem</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(int id, Entities.Book book)
    {
        if (id != book.Id)
        {
            return BadRequest();
        }

        var authorExists = await _context.Authors.AnyAsync(a => a.Id == book.AuthorId);
        if (!authorExists)
        {
            return BadRequest("Autor não encontrado.");
        }

        var genreExists = await _context.Genres.AnyAsync(g => g.Id == book.GenreId);
        if (!genreExists)
        {
            return BadRequest("Gênero não encontrado.");
        }

        _context.Entry(book).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Remove um livro existente
    /// </summary>
    /// <param name="id">ID do livro a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o livro foi removido com sucesso</response>
    /// <response code="400">Se o livro possuir empréstimos associados</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        var hasLoans = await _context.Loans.AnyAsync(l => l.BookId == id);
        if (hasLoans)
        {
            return BadRequest("Não é possível excluir o livro porque existem empréstimos pendentes.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Pesquisa livros por título, autor ou gênero
    /// </summary>
    /// <param name="title">Título ou parte do título para pesquisa</param>
    /// <param name="author">Nome ou parte do nome do autor para pesquisa</param>
    /// <param name="genre">Nome ou parte do nome do gênero para pesquisa</param>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de livros que correspondem aos critérios de pesquisa</returns>
    /// <response code="200">Retorna a lista paginada de livros encontrados</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Book>>> SearchBooks(
        [FromQuery] string? title,
        [FromQuery] string? author,
        [FromQuery] string? genre,
        [FromQuery] PaginationParameters parameters)
    {
        var query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(b => b.Title != null && b.Title.Contains(title));
        }

        if (!string.IsNullOrEmpty(author))
        {
            query = query.Where(b => b.Author != null && b.Author.Name != null && b.Author.Name.Contains(author));
        }

        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(b => b.Genre != null && b.Genre.Name != null && b.Genre.Name.Contains(genre));
        }

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}