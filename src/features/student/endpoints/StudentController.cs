using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Student.models;
using LibraryApi.Features.Student.views;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace LibraryApi.Features.Student.endpoints;

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
    public async Task<ActionResult<StudentListViewModel>> GetStudents([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Students
            .Include(s => s.Loans)
            .AsQueryable();

        var students = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var studentDtos = students.Items.Select(s => new StudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Email = s.Email,
            RegistrationNumber = s.RegistrationNumber,
            Phone = s.Phone,
            ActiveLoansCount = s.Loans?.Count(l => l.ReturnDate == null) ?? 0,
            TotalLoansCount = s.Loans?.Count ?? 0
        }).ToList();

        var pagedResult = new PagedResult<StudentDto>(studentDtos, students.TotalCount, students.PageNumber, students.PageSize);

        return Ok(new StudentListViewModel { Students = pagedResult });
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
    public async Task<ActionResult<StudentDetailViewModel>> GetStudent(int id)
    {
        var student = await _context.Students
            .Include(s => s.Loans!)
                .ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound();
        }

        var studentDto = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            RegistrationNumber = student.RegistrationNumber,
            Phone = student.Phone,
            ActiveLoansCount = student.Loans?.Count(l => l.ReturnDate == null) ?? 0,
            TotalLoansCount = student.Loans?.Count ?? 0
        };

        var loanDtos = student.Loans?.Select(l => new LoanSummaryDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            BookTitle = l.Book?.Title
        }).ToList() ?? new List<LoanSummaryDto>();

        return Ok(new StudentDetailViewModel
        {
            Student = studentDto,
            Loans = loanDtos
        });
    }

    /// <summary>
    /// Cria um novo estudante
    /// </summary>
    /// <param name="createStudentDto">Dados do estudante a ser criado</param>
    /// <returns>O novo estudante criado</returns>
    /// <response code="201">Retorna o novo estudante criado</response>
    /// <response code="400">Se os dados do estudante forem inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentDto createStudentDto)
    {
        var student = new Entities.Student
        {
            Name = createStudentDto.Name ?? "",
            Email = createStudentDto.Email ?? "",
            RegistrationNumber = createStudentDto.RegistrationNumber ?? "",
            Phone = createStudentDto.Phone ?? ""
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var studentDto = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            RegistrationNumber = student.RegistrationNumber,
            Phone = student.Phone,
            ActiveLoansCount = 0,
            TotalLoansCount = 0
        };

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
    }

    /// <summary>
    /// Atualiza um estudante existente
    /// </summary>
    /// <param name="id">ID do estudante a ser atualizado</param>
    /// <param name="updateStudentDto">Novos dados do estudante</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o estudante foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do estudante forem inválidos</response>
    /// <response code="404">Se o estudante não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto updateStudentDto)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound();
        }

        student.Name = updateStudentDto.Name ?? student.Name;
        student.Email = updateStudentDto.Email ?? student.Email;
        student.RegistrationNumber = updateStudentDto.RegistrationNumber ?? student.RegistrationNumber;
        student.Phone = updateStudentDto.Phone ?? student.Phone;

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
        var student = await _context.Students
            .Include(s => s.Loans)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound();
        }

        if (student.Loans != null && student.Loans.Any(l => l.ReturnDate == null))
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

    /// <summary>
    /// Pesquisa estudantes por nome, email ou número de matrícula
    /// </summary>
    /// <param name="name">Nome ou parte do nome para pesquisa</param>
    /// <param name="email">Email ou parte do email para pesquisa</param>
    /// <param name="registrationNumber">Número de matrícula ou parte dele para pesquisa</param>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de estudantes que correspondem aos critérios de pesquisa</returns>
    /// <response code="200">Retorna a lista paginada de estudantes encontrados</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentListViewModel>> SearchStudents(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] string? registrationNumber,
        [FromQuery] PaginationParameters parameters)
    {
        var query = _context.Students
            .Include(s => s.Loans)
            .AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(s => s.Name != null && s.Name.Contains(name));
        }

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(s => s.Email != null && s.Email.Contains(email));
        }

        if (!string.IsNullOrEmpty(registrationNumber))
        {
            query = query.Where(s => s.RegistrationNumber != null && s.RegistrationNumber.Contains(registrationNumber));
        }

        var students = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var studentDtos = students.Items.Select(s => new StudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Email = s.Email,
            RegistrationNumber = s.RegistrationNumber,
            Phone = s.Phone,
            ActiveLoansCount = s.Loans?.Count(l => l.ReturnDate == null) ?? 0,
            TotalLoansCount = s.Loans?.Count ?? 0
        }).ToList();

        var pagedResult = new PagedResult<StudentDto>(studentDtos, students.TotalCount, students.PageNumber, students.PageSize);

        return Ok(new StudentListViewModel { Students = pagedResult });
    }
}