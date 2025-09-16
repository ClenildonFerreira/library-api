using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace LibraryApi.Features.Genre;

/// <summary>
/// Controller para gerenciamento de gêneros literários
/// </summary>
[ApiController]
[Route("genres")]
[Produces("application/json")]
[Tags("Gêneros")]
public class GenreController : ControllerBase
{
    private readonly AppDbContext _context;

    public GenreController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de gêneros literários
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de gêneros</returns>
    /// <response code="200">Retorna a lista paginada de gêneros</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Genre>>> GetGenres([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Genres.AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    /// <summary>
    /// Obtém um gênero específico pelo ID
    /// </summary>
    /// <param name="id">ID do gênero</param>
    /// <returns>O gênero encontrado</returns>
    /// <response code="200">Retorna o gênero encontrado</response>
    /// <response code="404">Se o gênero não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entities.Genre>> GetGenre(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        return genre;
    }

    /// <summary>
    /// Cria um novo gênero literário
    /// </summary>
    /// <param name="genre">Dados do gênero a ser criado</param>
    /// <returns>O novo gênero criado</returns>
    /// <response code="201">Retorna o novo gênero criado</response>
    /// <response code="400">Se os dados do gênero forem inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Entities.Genre>> CreateGenre(Entities.Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
    }

    /// <summary>
    /// Atualiza um gênero existente
    /// </summary>
    /// <param name="id">ID do gênero a ser atualizado</param>
    /// <param name="genre">Novos dados do gênero</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o gênero foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do gênero forem inválidos</response>
    /// <response code="404">Se o gênero não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGenre(int id, Entities.Genre genre)
    {
        if (id != genre.Id)
        {
            return BadRequest();
        }

        _context.Entry(genre).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GenreExists(id))
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
    /// Remove um gênero existente
    /// </summary>
    /// <param name="id">ID do gênero a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o gênero foi removido com sucesso</response>
    /// <response code="404">Se o gênero não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
        {
            return NotFound();
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool GenreExists(int id)
    {
        return _context.Genres.Any(e => e.Id == id);
    }
}