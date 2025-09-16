using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace LibraryApi.Features.Loan;

/// <summary>
/// Controller para gerenciamento de empréstimos de livros
/// </summary>
[ApiController]
[Route("loans")]
[Produces("application/json")]
[Tags("Empréstimos")]
public class LoanController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoanController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de empréstimos
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de empréstimos</returns>
    /// <response code="200">Retorna a lista paginada de empréstimos</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Loan>>> GetLoans([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    /// <summary>
    /// Obtém um empréstimo específico pelo ID
    /// </summary>
    /// <param name="id">ID do empréstimo</param>
    /// <returns>O empréstimo encontrado</returns>
    /// <response code="200">Retorna o empréstimo encontrado</response>
    /// <response code="404">Se o empréstimo não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entities.Loan>> GetLoan(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound();
        }

        return loan;
    }

    /// <summary>
    /// Cria um novo empréstimo
    /// </summary>
    /// <param name="loan">Dados do empréstimo a ser criado</param>
    /// <returns>O novo empréstimo criado</returns>
    /// <response code="201">Retorna o novo empréstimo criado</response>
    /// <response code="400">Se os dados do empréstimo forem inválidos, ou se o livro ou estudante não existirem, ou se o livro já estiver emprestado</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Entities.Loan>> CreateLoan(Entities.Loan loan)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == loan.BookId);
        if (!bookExists)
        {
            return BadRequest("Livro não encontrado.");
        }

        var studentExists = await _context.Students.AnyAsync(s => s.Id == loan.StudentId);
        if (!studentExists)
        {
            return BadRequest("Estudante não encontrado.");
        }

        var isBookLoaned = await _context.Loans
            .AnyAsync(l => l.BookId == loan.BookId && l.ReturnDate == null);
        if (isBookLoaned)
        {
            return BadRequest("Este livro está emprestado.");
        }

        if (loan.LoanDate == default)
        {
            loan.LoanDate = DateTime.Now;
        }

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    /// <summary>
    /// Atualiza um empréstimo existente
    /// </summary>
    /// <param name="id">ID do empréstimo a ser atualizado</param>
    /// <param name="loan">Novos dados do empréstimo</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o empréstimo foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do empréstimo forem inválidos</response>
    /// <response code="404">Se o empréstimo não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLoan(int id, Entities.Loan loan)
    {
        if (id != loan.Id)
        {
            return BadRequest();
        }

        _context.Entry(loan).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LoanExists(id))
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
    /// Remove um empréstimo existente
    /// </summary>
    /// <param name="id">ID do empréstimo a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o empréstimo foi removido com sucesso</response>
    /// <response code="404">Se o empréstimo não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
        {
            return NotFound();
        }

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Registra a devolução de um livro emprestado
    /// </summary>
    /// <param name="id">ID do empréstimo a ser devolvido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o empréstimo foi devolvido com sucesso</response>
    /// <response code="400">Se o empréstimo já foi devolvido</response>
    /// <response code="404">Se o empréstimo não for encontrado</response>
    [HttpPatch("{id}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnLoan(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
        {
            return NotFound();
        }

        if (loan.ReturnDate != null)
        {
            return BadRequest("Este empréstimo foi devolvido.");
        }

        loan.ReturnDate = DateTime.Now;
        _context.Entry(loan).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtém uma lista paginada de empréstimos em atraso
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de empréstimos em atraso</returns>
    /// <response code="200">Retorna a lista paginada de empréstimos em atraso</response>
    [HttpGet("overdue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Entities.Loan>>> GetOverdueLoans([FromQuery] PaginationParameters parameters)
    {
        var today = DateTime.Now;
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .Where(l => l.ReturnDate == null && l.DueDate < today)
            .AsQueryable();

        var pagedResult = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        return Ok(pagedResult);
    }

    private bool LoanExists(int id)
    {
        return _context.Loans.Any(e => e.Id == id);
    }
}