using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;

namespace LibraryApi.Features.Loan;

[ApiController]
[Route("loans")]
public class LoanController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoanController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entities.Loan>>> GetLoans()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .ToListAsync();
    }

    [HttpGet("{id}")]
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

    [HttpPost]
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

    [HttpPatch("{id}")]
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

    [HttpDelete("{id}")]
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

    [HttpPatch("{id}/return")]
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

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<Entities.Loan>>> GetOverdueLoans()
    {
        var today = DateTime.Now;
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .Where(l => l.ReturnDate == null && l.DueDate < today)
            .ToListAsync();
    }

    private bool LoanExists(int id)
    {
        return _context.Loans.Any(e => e.Id == id);
    }
}