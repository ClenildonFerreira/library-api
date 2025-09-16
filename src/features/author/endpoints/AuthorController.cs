using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Author.models;
using LibraryApi.Features.Author.views;

namespace LibraryApi.Features.Author.endpoints;

/// <summary>
/// Controller para gerenciamento de autores
/// </summary>
[ApiController]
[Route("authors")]
[Produces("application/json")]
[Tags("Autores")]
public class AuthorController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthorController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de autores
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de autores</returns>
    /// <response code="200">Retorna a lista paginada de autores</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorListViewModel>> GetAuthors([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Authors
            .Include(a => a.Books)
            .AsQueryable();

        var authors = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var authorDtos = authors.Items.Select(a => new AuthorDto
        {
            Id = a.Id,
            Name = a.Name,
            Biography = a.Biography,
            BookCount = a.Books.Count
        }).ToList();

        var pagedResult = new PagedResult<AuthorDto>(authorDtos, authors.TotalCount, authors.PageNumber, authors.PageSize);

        return Ok(new AuthorListViewModel { Authors = pagedResult });
    }

    /// <summary>
    /// Obtém um autor específico pelo ID
    /// </summary>
    /// <param name="id">ID do autor</param>
    /// <returns>O autor encontrado</returns>
    /// <response code="200">Retorna o autor encontrado</response>
    /// <response code="404">Se o autor não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorDetailViewModel>> GetAuthor(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        var authorDto = new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Biography = author.Biography,
            BookCount = author.Books.Count
        };

        var bookDtos = author.Books.Select(b => new BookSummaryDto
        {
            Id = b.Id,
            Title = b.Title ?? "",
            ISBN = b.ISBN ?? "",
            PublicationYear = b.PublicationYear
        }).ToList();

        return Ok(new AuthorDetailViewModel
        {
            Author = authorDto,
            Books = bookDtos
        });
    }

    /// <summary>
    /// Cria um novo autor
    /// </summary>
    /// <param name="createAuthorDto">Dados do autor a ser criado</param>
    /// <returns>O novo autor criado</returns>
    /// <response code="201">Retorna o novo autor criado</response>
    /// <response code="400">Se os dados do autor forem inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
    {
        var author = new Entities.Author
        {
            Name = createAuthorDto.Name ?? "",
            Biography = createAuthorDto.Biography ?? ""
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var authorDto = new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Biography = author.Biography,
            BookCount = 0
        };

        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, authorDto);
    }

    /// <summary>
    /// Atualiza um autor existente
    /// </summary>
    /// <param name="id">ID do autor a ser atualizado</param>
    /// <param name="updateAuthorDto">Novos dados do autor</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o autor foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do autor forem inválidos</response>
    /// <response code="404">Se o autor não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuthor(int id, UpdateAuthorDto updateAuthorDto)
    {
        var author = await _context.Authors.FindAsync(id);

        if (author == null)
        {
            return NotFound();
        }

        author.Name = updateAuthorDto.Name ?? "";
        author.Biography = updateAuthorDto.Biography ?? "";

        _context.Entry(author).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AuthorExists(id))
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
    /// Remove um autor existente
    /// </summary>
    /// <param name="id">ID do autor a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o autor foi removido com sucesso</response>
    /// <response code="404">Se o autor não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        if (author.Books.Any())
        {
            return BadRequest("Não é possível excluir o autor porque possui livros pendentes.");
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AuthorExists(int id)
    {
        return _context.Authors.Any(e => e.Id == id);
    }
}