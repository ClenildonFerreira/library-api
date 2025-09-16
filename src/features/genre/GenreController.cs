using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Genre;

[ApiController]
[Route("genres")]
public class GenreController : ControllerBase
{
    private readonly AppDbContext _context;

    public GenreController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Entities.Genre>>> GetGenres([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Genres.AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    [HttpGet("{id}")]
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

    [HttpPost]
    public async Task<ActionResult<Entities.Genre>> CreateGenre(Entities.Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
    }

    [HttpPatch("{id}")]
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

    [HttpDelete("{id}")]
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