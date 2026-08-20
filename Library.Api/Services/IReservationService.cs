using Library.Api.DTOs;
using Library.Core.Enums;

namespace Library.Api.Services;

public interface IReservationService
{
    public Task<ReservationResponse> CreateReservationAsync(string userId, Guid bookId);
    public Task<CancelReservationResult> DeleteReservationAsync(string userId, Guid reservationId);
    public Task<List<ReservationResponse>> GetReservationsByUserId(string id);
}