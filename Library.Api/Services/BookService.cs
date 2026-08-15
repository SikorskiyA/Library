using Library.Api.Data;
using Library.Api.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

public class BookService : IBookService
{
    private readonly ApplicationDbContext _context;

    public BookService(ApplicationDbContext context)
    {
        _context = context;
    }

    public static BookResponse ToResponse(Book book)
    {
        return new BookResponse
        {
            Id = book.Id,
            Name = book.Name,
            Author = book.Author,
            Genre = book.Genre,
            Publisher = book.Publisher,
            About = book.About,
            Pages = book.Pages,
            Quantity = book.Quantity,
            InStock = book.InStock,
            Rating = book.Rating,
        };
    }

    public async Task DecreaseStockAsync(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);

        if (book is null)
            throw new InvalidOperationException("Книга не найдена");

        if (book.InStock <= 0)
            throw new InvalidOperationException("Нет доступных экземпляров книги");

        book.InStock--;
        await _context.SaveChangesAsync();
    }

    public async Task IncreaseStockAsync(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);

        if (book is null)
            throw new InvalidOperationException("Книга не найдена");

        if (book.InStock >= book.Quantity)
            throw new InvalidOperationException(
                "Количество экземпляров не может превышать общее число"
            );

        book.InStock++;
        await _context.SaveChangesAsync();
    }

    public async Task<List<BookResponse>> GetAllBooksAsync()
    {
        var query = _context.Books.Where(q => q.InStock != 0);
        var books = await query.OrderBy(r => r.Rating).ToListAsync();

        var booksResponse = new List<BookResponse>();
        foreach (Book book in books)
        {
            booksResponse.Add(ToResponse(book));
        }

        return booksResponse;
    }

    public async Task<BookResponse?> GetBookById(Guid id)
    {
        var book = await _context.Books.FindAsync(id);

        return book is null ? null : ToResponse(book);
    }

    public async Task<List<BookResponse>> SearchBookAsync(
        string? author,
        string? genre,
        string? publisher
    )
    {
        var query =
            author is not null && author.Length != 0
                ? _context.Books.Where(a => EF.Functions.ILike(a.Author, $"%{author}%"))
                : _context.Books;

        if (genre is not null && genre.Length != 0)
            query = query.Where(g => EF.Functions.ILike(g.Genre, $"%{genre}%"));

        if (publisher is not null && publisher.Length != 0)
            query = query.Where(p => EF.Functions.ILike(p.Publisher, $"%{publisher}%"));

        var books = await query.OrderBy(r => r.Rating).ToListAsync();
        var booksResponse = new List<BookResponse>();

        foreach (Book book in books)
        {
            booksResponse.Add(ToResponse(book));
        }

        return booksResponse;
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
    {
        Book book = new Book
        {
            About = request.About,
            Author = request.Author,
            Genre = request.Genre,
            Quantity = request.Quantity,
            InStock = request.Quantity,
            Name = request.Name,
            Pages = request.Pages,
            Publisher = request.Publisher,
            Rating = 0,
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return ToResponse(book);
    }

    public async Task<DeleteResult> DeleteBookAsync(Guid id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book is null)
            return DeleteResult.NotFound;

        var hasActiveReservations = await _context.Reservations.AnyAsync(r =>
            r.BookId == id && r.Status == ReservationStatus.Active
        );
        if (hasActiveReservations)
            return DeleteResult.HasActiveOperations;

        var hasActiveLoans = await _context.Loans.AnyAsync(l =>
            l.BookId == id && l.ReturnedAt == null
        );
        if (hasActiveLoans)
            return DeleteResult.HasActiveOperations;
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return DeleteResult.Success;
    }
}
