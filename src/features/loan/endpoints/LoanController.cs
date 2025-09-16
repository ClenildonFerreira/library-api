using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Loan.models;
using LibraryApi.Features.Loan.views;

namespace LibraryApi.Features.Loan.endpoints;

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
    public async Task<ActionResult<LoanListViewModel>> GetLoans([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .AsQueryable();

        var loans = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var loanDtos = loans.Items.Select(l => new LoanDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            BookId = l.BookId,
            StudentId = l.StudentId,
            BookTitle = l.Book?.Title,
            StudentName = l.Student?.Name
        }).ToList();

        var pagedResult = new PagedResult<LoanDto>(loanDtos, loans.TotalCount, loans.PageNumber, loans.PageSize);

        return Ok(new LoanListViewModel { Loans = pagedResult });
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
    public async Task<ActionResult<LoanDetailViewModel>> GetLoan(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book!)
            .ThenInclude(b => b.Author)
            .Include(l => l.Book!.Genre)
            .Include(l => l.Student)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound();
        }

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            BookId = loan.BookId,
            StudentId = loan.StudentId,
            BookTitle = loan.Book?.Title,
            StudentName = loan.Student?.Name
        };

        var bookDetailDto = new BookDetailDto
        {
            Id = loan.Book!.Id,
            Title = loan.Book.Title,
            ISBN = loan.Book.ISBN,
            AuthorName = loan.Book.Author?.Name,
            GenreName = loan.Book.Genre?.Name
        };

        var studentDetailDto = new StudentDetailDto
        {
            Id = loan.Student!.Id,
            Name = loan.Student.Name,
            Email = loan.Student.Email,
            RegistrationNumber = loan.Student.RegistrationNumber,
            Phone = loan.Student.Phone
        };

        return Ok(new LoanDetailViewModel
        {
            Loan = loanDto,
            Book = bookDetailDto,
            Student = studentDetailDto
        });
    }

    /// <summary>
    /// Cria um novo empréstimo
    /// </summary>
    /// <param name="createLoanDto">Dados do empréstimo a ser criado</param>
    /// <returns>O novo empréstimo criado</returns>
    /// <response code="201">Retorna o novo empréstimo criado</response>
    /// <response code="400">Se os dados do empréstimo forem inválidos, ou se o livro ou estudante não existirem, ou se o livro já estiver emprestado</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto createLoanDto)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == createLoanDto.BookId);
        if (!bookExists)
        {
            return BadRequest("Livro não encontrado.");
        }

        var studentExists = await _context.Students.AnyAsync(s => s.Id == createLoanDto.StudentId);
        if (!studentExists)
        {
            return BadRequest("Estudante não encontrado.");
        }

        var isBookLoaned = await _context.Loans
            .AnyAsync(l => l.BookId == createLoanDto.BookId && l.ReturnDate == null);
        if (isBookLoaned)
        {
            return BadRequest("Este livro já está emprestado.");
        }

        var loan = new Entities.Loan
        {
            LoanDate = createLoanDto.LoanDate ?? DateTime.Now,
            DueDate = createLoanDto.DueDate,
            BookId = createLoanDto.BookId,
            StudentId = createLoanDto.StudentId
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // Carrega o livro e o estudante para retornar no DTO
        await _context.Entry(loan).Reference(l => l.Book).LoadAsync();
        await _context.Entry(loan).Reference(l => l.Student).LoadAsync();

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            BookId = loan.BookId,
            StudentId = loan.StudentId,
            BookTitle = loan.Book?.Title,
            StudentName = loan.Student?.Name
        };

        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loanDto);
    }

    /// <summary>
    /// Atualiza um empréstimo existente
    /// </summary>
    /// <param name="id">ID do empréstimo a ser atualizado</param>
    /// <param name="updateLoanDto">Novos dados do empréstimo</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o empréstimo foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do empréstimo forem inválidos</response>
    /// <response code="404">Se o empréstimo não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLoan(int id, UpdateLoanDto updateLoanDto)
    {
        var loan = await _context.Loans.FindAsync(id);

        if (loan == null)
        {
            return NotFound();
        }

        if (loan.ReturnDate != null)
        {
            return BadRequest("Não é possível atualizar um empréstimo já finalizado.");
        }

        loan.DueDate = updateLoanDto.DueDate;

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
            return BadRequest("Este empréstimo já foi devolvido.");
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
    public async Task<ActionResult<LoanListViewModel>> GetOverdueLoans([FromQuery] PaginationParameters parameters)
    {
        var today = DateTime.Now;
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .Where(l => l.ReturnDate == null && l.DueDate < today)
            .AsQueryable();

        var loans = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var loanDtos = loans.Items.Select(l => new LoanDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            BookId = l.BookId,
            StudentId = l.StudentId,
            BookTitle = l.Book?.Title,
            StudentName = l.Student?.Name
        }).ToList();

        var pagedResult = new PagedResult<LoanDto>(loanDtos, loans.TotalCount, loans.PageNumber, loans.PageSize);

        return Ok(new LoanListViewModel { Loans = pagedResult });
    }

    /// <summary>
    /// Pesquisa empréstimos por livro, estudante ou status
    /// </summary>
    /// <param name="bookTitle">Título ou parte do título do livro para pesquisa</param>
    /// <param name="studentName">Nome ou parte do nome do estudante para pesquisa</param>
    /// <param name="status">Status do empréstimo: 'active', 'returned', 'overdue'</param>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de empréstimos que correspondem aos critérios de pesquisa</returns>
    /// <response code="200">Retorna a lista paginada de empréstimos encontrados</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanListViewModel>> SearchLoans(
        [FromQuery] string? bookTitle,
        [FromQuery] string? studentName,
        [FromQuery] string? status,
        [FromQuery] PaginationParameters parameters)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Student)
            .AsQueryable();

        if (!string.IsNullOrEmpty(bookTitle))
        {
            query = query.Where(l => l.Book != null && l.Book.Title != null && l.Book.Title.Contains(bookTitle));
        }

        if (!string.IsNullOrEmpty(studentName))
        {
            query = query.Where(l => l.Student != null && l.Student.Name != null && l.Student.Name.Contains(studentName));
        }

        var today = DateTime.Now;
        if (!string.IsNullOrEmpty(status))
        {
            switch (status.ToLower())
            {
                case "active":
                    query = query.Where(l => l.ReturnDate == null && l.DueDate >= today);
                    break;
                case "returned":
                    query = query.Where(l => l.ReturnDate != null);
                    break;
                case "overdue":
                    query = query.Where(l => l.ReturnDate == null && l.DueDate < today);
                    break;
            }
        }

        var loans = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var loanDtos = loans.Items.Select(l => new LoanDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            BookId = l.BookId,
            StudentId = l.StudentId,
            BookTitle = l.Book?.Title,
            StudentName = l.Student?.Name
        }).ToList();

        var pagedResult = new PagedResult<LoanDto>(loanDtos, loans.TotalCount, loans.PageNumber, loans.PageSize);

        return Ok(new LoanListViewModel { Loans = pagedResult });
    }

    /// <summary>
    /// Obtém estatísticas de empréstimos
    /// </summary>
    /// <returns>Estatísticas de empréstimos</returns>
    /// <response code="200">Retorna as estatísticas de empréstimos</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetLoanStats()
    {
        var today = DateTime.Now;

        var totalLoans = await _context.Loans.CountAsync();
        var activeLoans = await _context.Loans.CountAsync(l => l.ReturnDate == null);
        var returnedLoans = await _context.Loans.CountAsync(l => l.ReturnDate != null);
        var overdueLoans = await _context.Loans.CountAsync(l => l.ReturnDate == null && l.DueDate < today);

        var stats = new
        {
            TotalLoans = totalLoans,
            ActiveLoans = activeLoans,
            ReturnedLoans = returnedLoans,
            OverdueLoans = overdueLoans
        };

        return Ok(stats);
    }

    private bool LoanExists(int id)
    {
        return _context.Loans.Any(e => e.Id == id);
    }
}