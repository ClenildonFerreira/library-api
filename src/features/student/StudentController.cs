using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Student;

[ApiController]
[Route("students")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Entities.Student>>> GetStudents([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Students.AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Entities.Student>> GetStudent(int id)
    {
        var student = await _context.Students
            .Include(s => s.Loans!)
                .ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound();
        }

        return student;
    }

    [HttpPost]
    public async Task<ActionResult<Entities.Student>> CreateStudent(Entities.Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, Entities.Student student)
    {
        if (id != student.Id)
        {
            return BadRequest();
        }

        _context.Entry(student).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StudentExists(id))
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
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        var hasLoans = await _context.Loans.AnyAsync(l => l.StudentId == id);
        if (hasLoans)
        {
            return BadRequest("Não é possível excluir o estudante porque existem empréstimos pendentes.");
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool StudentExists(int id)
    {
        return _context.Students.Any(e => e.Id == id);
    }
}