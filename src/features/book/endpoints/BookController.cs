using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;
using LibraryApi.Infrastructure.Pagination;
using LibraryApi.Features.Book.models;
using LibraryApi.Features.Book.views;

namespace LibraryApi.Features.Book.endpoints;

/// <summary>
/// Controller para gerenciamento de livros
/// </summary>
[ApiController]
[Route("books")]
[Produces("application/json")]
[Tags("Livros")]
public class BookController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém uma lista paginada de livros
    /// </summary>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de livros</returns>
    /// <response code="200">Retorna a lista paginada de livros</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BookListViewModel>> GetBooks([FromQuery] PaginationParameters parameters)
    {
        var query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Loans)
            .AsQueryable();

        var books = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var bookDtos = books.Items.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublicationYear = b.PublicationYear,
            Quantity = b.Quantity,
            AuthorId = b.AuthorId,
            GenreId = b.GenreId,
            AuthorName = b.Author?.Name,
            GenreName = b.Genre?.Name,
            AvailableQuantity = b.Quantity - (b.Loans?.Count(l => l.ReturnDate == null) ?? 0)
        }).ToList();

        var pagedResult = new PagedResult<BookDto>(bookDtos, books.TotalCount, books.PageNumber, books.PageSize);

        return Ok(new BookListViewModel { Books = pagedResult });
    }

    /// <summary>
    /// Obtém um livro específico pelo ID
    /// </summary>
    /// <param name="id">ID do livro</param>
    /// <returns>O livro encontrado</returns>
    /// <response code="200">Retorna o livro encontrado</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetailViewModel>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Loans!)
            .ThenInclude(l => l.Student)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            Quantity = book.Quantity,
            AuthorId = book.AuthorId,
            GenreId = book.GenreId,
            AuthorName = book.Author?.Name,
            GenreName = book.Genre?.Name,
            AvailableQuantity = book.Quantity - (book.Loans?.Count(l => l.ReturnDate == null) ?? 0)
        };

        var loanDtos = book.Loans?.Select(l => new LoanSummaryDto
        {
            Id = l.Id,
            LoanDate = l.LoanDate,
            ReturnDate = l.ReturnDate,
            DueDate = l.DueDate,
            StudentName = l.Student?.Name
        }).ToList() ?? new List<LoanSummaryDto>();

        return Ok(new BookDetailViewModel
        {
            Book = bookDto,
            Loans = loanDtos
        });
    }

    /// <summary>
    /// Cria um novo livro
    /// </summary>
    /// <param name="createBookDto">Dados do livro a ser criado</param>
    /// <returns>O novo livro criado</returns>
    /// <response code="201">Retorna o novo livro criado</response>
    /// <response code="400">Se os dados do livro forem inválidos ou se o autor ou gênero não existirem</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        var authorExists = await _context.Authors.AnyAsync(a => a.Id == createBookDto.AuthorId);
        if (!authorExists)
        {
            return BadRequest("Autor não encontrado.");
        }

        var genreExists = await _context.Genres.AnyAsync(g => g.Id == createBookDto.GenreId);
        if (!genreExists)
        {
            return BadRequest("Gênero não encontrado.");
        }

        var book = new Entities.Book
        {
            Title = createBookDto.Title,
            ISBN = createBookDto.ISBN,
            PublicationYear = createBookDto.PublicationYear,
            Quantity = createBookDto.Quantity,
            AuthorId = createBookDto.AuthorId,
            GenreId = createBookDto.GenreId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var author = await _context.Authors.FindAsync(book.AuthorId);
        var genre = await _context.Genres.FindAsync(book.GenreId);

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            Quantity = book.Quantity,
            AuthorId = book.AuthorId,
            GenreId = book.GenreId,
            AuthorName = author?.Name,
            GenreName = genre?.Name,
            AvailableQuantity = book.Quantity
        };

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
    }

    /// <summary>
    /// Atualiza um livro existente
    /// </summary>
    /// <param name="id">ID do livro a ser atualizado</param>
    /// <param name="updateBookDto">Novos dados do livro</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o livro foi atualizado com sucesso</response>
    /// <response code="400">Se os dados do livro forem inválidos ou se o autor ou gênero não existirem</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        var authorExists = await _context.Authors.AnyAsync(a => a.Id == updateBookDto.AuthorId);
        if (!authorExists)
        {
            return BadRequest("Autor não encontrado.");
        }

        var genreExists = await _context.Genres.AnyAsync(g => g.Id == updateBookDto.GenreId);
        if (!genreExists)
        {
            return BadRequest("Gênero não encontrado.");
        }

        book.Title = updateBookDto.Title;
        book.ISBN = updateBookDto.ISBN;
        book.PublicationYear = updateBookDto.PublicationYear;
        book.Quantity = updateBookDto.Quantity;
        book.AuthorId = updateBookDto.AuthorId;
        book.GenreId = updateBookDto.GenreId;

        _context.Entry(book).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
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
    /// Remove um livro existente
    /// </summary>
    /// <param name="id">ID do livro a ser removido</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Se o livro foi removido com sucesso</response>
    /// <response code="400">Se o livro possuir empréstimos associados</response>
    /// <response code="404">Se o livro não for encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        if (book.Loans != null && book.Loans.Any(l => l.ReturnDate == null))
        {
            return BadRequest("Não é possível excluir o livro porque existem empréstimos pendentes.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Pesquisa livros por título, autor ou gênero
    /// </summary>
    /// <param name="title">Título ou parte do título para pesquisa</param>
    /// <param name="author">Nome ou parte do nome do autor para pesquisa</param>
    /// <param name="genre">Nome ou parte do nome do gênero para pesquisa</param>
    /// <param name="parameters">Parâmetros de paginação</param>
    /// <returns>Uma lista paginada de livros que correspondem aos critérios de pesquisa</returns>
    /// <response code="200">Retorna a lista paginada de livros encontrados</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BookListViewModel>> SearchBooks(
        [FromQuery] string? title,
        [FromQuery] string? author,
        [FromQuery] string? genre,
        [FromQuery] PaginationParameters parameters)
    {
        var query = _context.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Loans)
            .AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(b => b.Title != null && b.Title.Contains(title));
        }

        if (!string.IsNullOrEmpty(author))
        {
            query = query.Where(b => b.Author != null && b.Author.Name != null && b.Author.Name.Contains(author));
        }

        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(b => b.Genre != null && b.Genre.Name != null && b.Genre.Name.Contains(genre));
        }

        var books = await Task.FromResult(query.ToPagedResult(parameters.PageNumber, parameters.PageSize));

        var bookDtos = books.Items.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            ISBN = b.ISBN,
            PublicationYear = b.PublicationYear,
            Quantity = b.Quantity,
            AuthorId = b.AuthorId,
            GenreId = b.GenreId,
            AuthorName = b.Author?.Name,
            GenreName = b.Genre?.Name,
            AvailableQuantity = b.Quantity - (b.Loans?.Count(l => l.ReturnDate == null) ?? 0)
        }).ToList();

        var pagedResult = new PagedResult<BookDto>(bookDtos, books.TotalCount, books.PageNumber, books.PageSize);

        return Ok(new BookListViewModel { Books = pagedResult });
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}