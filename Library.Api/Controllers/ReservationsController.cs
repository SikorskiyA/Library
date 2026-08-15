using System.Security.Claims;
using Library.Api.Services;
using Library.Core.Constants;
using Library.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReservationService _reservationService;

    public ReservationsController(
        IReservationService reservationService,
        UserManager<ApplicationUser> userManager
    )
    {
        _reservationService = reservationService;
        _userManager = userManager;
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Client)]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetCurrentClientId();
        if (userId is null)
            return BadRequest(new { Message = "Пользователь с таким ID не найден" });
        var reservations = await _reservationService.GetReservationsByUserId(userId);

        return Ok(reservations);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Librarian)]
    public async Task<IActionResult> GetByUserId([FromRoute] string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var reservations = await _reservationService.GetReservationsByUserId(id);

        return Ok(reservations);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Guid bookId)
    {
        var userId = GetCurrentClientId();
        if (userId is null)
            return BadRequest(new { Message = "Пользователь с таким ID не найден" });

        try
        {
            var reservation = await _reservationService.CreateReservationAsync(userId, bookId);
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentClientId();
        if (userId is null)
            return BadRequest(new { Message = "Пользователь с таким ID не найден" });

        var result = await _reservationService.DeleteReservationAsync(userId, id);

        return result switch
        {
            CancelReservationResult.Success => NoContent(),
            CancelReservationResult.NotFound => NotFound(),
            CancelReservationResult.NotOwner => Forbid(),
            CancelReservationResult.NotActive => Conflict(
                new { Message = "Бронирование не находится в статусе \"Активно\"" }
            ),
            _ => StatusCode(500),
        };
    }

    private string? GetCurrentClientId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
