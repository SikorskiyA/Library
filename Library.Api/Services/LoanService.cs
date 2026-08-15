using Library.Api.Data;
using Library.Api.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookService _bookService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoanService(
        ApplicationDbContext context,
        IBookService bookService,
        UserManager<ApplicationUser> userManager
    )
    {
        _context = context;
        _bookService = bookService;
        _userManager = userManager;
    }

    private static LoanResponse ToResponse(Loan loan)
    {
        return new LoanResponse
        {
            BookId = loan.BookId,
            BookName = loan.Book.Name,
            Id = loan.Id,
            IssuedAt = loan.IssuedAt,
            DueDate = loan.DueDate,
            LibrarianId = loan.LibrarianId,
            UserId = loan.UserId,
        };
    }

    public async Task<List<LoanResponse>> GetMyLoansAsClientAsync(string userId)
    {
        var loans = await _context
            .Loans.Include(l => l.Book)
            .Where(l => l.UserId == userId && l.ReturnedAt == null)
            .ToListAsync();

        return loans.Select(ToResponse).ToList();
    }

    public async Task<List<LoanResponse>> GetIssuedByLibrarianAsync(string librarianId)
    {
        var loans = await _context
            .Loans.Include(l => l.Book)
            .Where(l => l.LibrarianId == librarianId && l.ReturnedAt == null)
            .ToListAsync();

        return loans.Select(ToResponse).ToList();
    }

    public async Task<LoanResponse> IssueBookAsync(Guid reservationId, string librarianId)
    {
        var reservation =
            await _context.Reservations.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == reservationId)
            ?? throw new Exception("Бронирования с таким ID не существует");
        var librarian =
            await _userManager.FindByIdAsync(librarianId)
            ?? throw new Exception("Пользователя с таким ID не существует");

        if (reservation.Status != ReservationStatus.Active)
            throw new Exception("Бронирование не активно");

        var book = await _context.Books.FindAsync(reservation.BookId) ?? throw new Exception("Книга не найдена");;

        var loan = new Loan
        {
            BookId = reservation.BookId,
            Book = book,
            DueDate = DateTime.UtcNow.AddDays(31),
            IssuedAt = DateTime.UtcNow,
            Librarian = librarian,
            LibrarianId = librarianId,
            User = reservation.User,
            UserId = reservation.UserId,
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        _context.Loans.Add(loan);
        reservation.Status = ReservationStatus.Converted;
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return ToResponse(loan);
    }

    public async Task<ReturnBookResult> ReturnBookAsync(Guid loanId)
    {
        var loan = await _context.Loans.FindAsync(loanId);

        if (loan is null)
            return ReturnBookResult.NotFound;

        if (loan.ReturnedAt is not null)
        {
            return ReturnBookResult.AlreadyReturned;
        }

        loan.ReturnedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _bookService.IncreaseStockAsync(loan.BookId);

        return ReturnBookResult.Success;
    }
}
