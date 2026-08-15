using System.Security.Claims;
using Library.Api.Services;
using Library.Core.Constants;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoansController(ILoanService loanService, UserManager<ApplicationUser> userManager)
    {
        _loanService = loanService;
        _userManager = userManager;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetCurrentUsertId();

        if (userId is null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return NotFound("Пользователь не найден");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        switch (role)
        {
            case Roles.Client:
            {
                var result = await _loanService.GetMyLoansAsClientAsync(userId);
                return Ok(result);
            }
            case Roles.Librarian:
            {
                var result = await _loanService.GetIssuedByLibrarianAsync(userId);
                return Ok(result);
            }
            default:
                return BadRequest($"Неподходящая роль: {role}");
        }
    }

    [HttpPost("issue")]
    [Authorize(Roles = Roles.Librarian)]
    public async Task<IActionResult> Issue(Guid reservationId)
    {
        var librarianId = GetCurrentUsertId();

        if (librarianId is null)
            return Unauthorized();
        try
        {
            var result = await _loanService.IssueBookAsync(reservationId, librarianId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpPost("{id}/return")]
    [Authorize(Roles = Roles.Librarian)]
    public async Task<IActionResult> Return([FromRoute] Guid reservationId)
    {
        var result = await _loanService.ReturnBookAsync(reservationId);

        return result switch
        {
            ReturnBookResult.AlreadyReturned => Conflict(new {Message = "Книгу уже вернули"}),
            ReturnBookResult.NotFound => NotFound(),
            ReturnBookResult.Success => NoContent(),
            _ => StatusCode(500)
        };
    }

    private string? GetCurrentUsertId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
