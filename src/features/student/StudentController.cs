using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace LibraryApi.Features.Student;

/// <summary>
/// Controller para gerenciamento de estudantes
/// </summary>
[ApiController]
[Route("students")]
[Produces("application/json")]
[Tags("Estudantes")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de estudantes
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de estudantes</returns>
    /// <response code="200">Retorna a lista paginada de estudantes</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Student>>> GetStudents([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Students.AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    /// <summary>
    /// Obtém um estudante específico pelo ID
    /// </summary>
    /// <param name="id">ID do estudante</param>
    /// <returns>O estudante encontrado</returns>
    /// <response code="200">Retorna o estudante encontrado</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Cria um novo estudante
    /// </summary>
    /// <param name="student">Dados do estudante a ser criado</param>
    /// <returns>O novo estudante criado</returns>
    /// <response code="201">Retorna o novo estudante criado</response>
    /// <response code="400">Se os dados do estudante forem inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Entities.Student>> CreateStudent(Entities.Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    /// <summary>
    /// Atualiza um estudante existente
    /// </summary>
    /// <param name="id">ID do estudante a ser atualizado</param>
    /// <param name="student">Novos dados do estudante</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o estudante foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do estudante forem inválidos</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Remove um estudante existente
    /// </summary>
    /// <param name="id">ID do estudante a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o estudante foi removido com sucesso</response>
    /// <response code="400">Se o estudante possuir empréstimos associados</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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