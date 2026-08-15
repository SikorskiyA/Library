using Library.Api.Data;
using Library.Api.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookService _bookService;

    public ReservationService(ApplicationDbContext context, IBookService bookService)
    {
        _context = context;
        _bookService = bookService;
    }

    private static ReservationResponse ToResponse(Reservation reservation)
    {
        return new ReservationResponse
        {
            Id = reservation.Id,
            BookId = reservation.BookId,
            ReservedAt = reservation.ReservedAt,
            BookName = reservation.Book.Name,
            ExpiresAt = reservation.ExpiresAt,
            Status = reservation.Status.ToString(),
            UserId = reservation.UserId,
        };
    }

    public async Task<ReservationResponse> CreateReservationAsync(string userId, Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book is null)
            throw new Exception("Книги с таким ID не найдено");
        if (book.InStock == 0)
            throw new Exception("Не осталось свободных экземпляров этой книги в наличии");

        var reservation = new Reservation
        {
            BookId = bookId,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            UserId = userId,
            Book = book,
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        await _bookService.DecreaseStockAsync(bookId);

        return ToResponse(reservation);
    }

    public async Task<CancelReservationResult> DeleteReservationAsync(
        string userId,
        Guid reservationId
    )
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation is null)
            return CancelReservationResult.NotFound;

        if (reservation.UserId != userId)
            return CancelReservationResult.NotOwner;

        if (reservation.Status != ReservationStatus.Active)
            return CancelReservationResult.NotActive;

        reservation.Status = ReservationStatus.Cancelled;

        using var transaction = await _context.Database.BeginTransactionAsync();

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();
        await _bookService.IncreaseStockAsync(reservation.BookId);

        await transaction.CommitAsync();

        return CancelReservationResult.Success;
    }

    public async Task<List<ReservationResponse>> GetMyReservationsAsync(string id)
    {
        var query = _context.Reservations.Include(r => r.Book).Where(i => i.UserId == id);
        var reservations = await query.OrderBy(d => d.ReservedAt).ToListAsync();

        var reservationsResponse = new List<ReservationResponse>();

        foreach (Reservation r in reservations)
        {
            reservationsResponse.Add(ToResponse(r));
        }

        return reservationsResponse;
    }
}
