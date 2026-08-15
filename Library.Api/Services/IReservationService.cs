using Library.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Services;

public interface IReservationService
{
    public Task<List<ReservationResponse>> GetMyReservationsAsync(string id);
    public Task<ReservationResponse> CreateReservationAsync(string userId, Guid bookId);
    public Task<CancelReservationResult> DeleteReservationAsync(string userId, Guid reservationId);
}