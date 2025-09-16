using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Genre.models;
using LibraryApi.Features.Genre.views;

namespace LibraryApi.Features.Genre.endpoints;

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
    public async Task<ActionResult<GenreListViewModel>> GetGenres([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Genres
            .Include(g => g.Books)
            .AsQueryable();

        var genres = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var genreDtos = genres.Items.Select(g => new GenreDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            BookCount = g.Books?.Count ?? 0
        }).ToList();

        var pagedResult = new PagedResult<GenreDto>(genreDtos, genres.TotalCount, genres.PageNumber, genres.PageSize);

        return Ok(new GenreListViewModel { Genres = pagedResult });
    }

    /// <summary>
    /// Obtém um gênero específico pelo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenreDetailViewModel>> GetGenre(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.Books!)
            .ThenInclude(b => b.Author)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        var genreDto = new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name,
            Description = genre.Description,
            BookCount = genre.Books?.Count ?? 0
        };

        var bookDtos = genre.Books?.Select(b => new BookSummaryDto
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublicationYear = b.PublicationYear,
            AuthorName = b.Author?.Name
        }).ToList() ?? new List<BookSummaryDto>();

        return Ok(new GenreDetailViewModel
        {
            Genre = genreDto,
            Books = bookDtos
        });
    }

    /// <summary>
    /// Cria um novo gênero literário
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenreDto>> CreateGenre(CreateGenreDto createGenreDto)
    {
        var genre = new Entities.Genre
        {
            Name = createGenreDto.Name,
            Description = createGenreDto.Description
        };

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        var genreDto = new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name,
            Description = genre.Description,
            BookCount = 0
        };

        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genreDto);
    }

    /// <summary>
    /// Atualiza um gênero existente
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGenre(int id, UpdateGenreDto updateGenreDto)
    {
        var genre = await _context.Genres.FindAsync(id);

        if (genre == null)
        {
            return NotFound();
        }

        genre.Name = updateGenreDto.Name;
        genre.Description = updateGenreDto.Description;

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
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        if (genre.Books != null && genre.Books.Any())
        {
            return BadRequest("Não é possível excluir o gênero porque possui livros associados.");
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