using System.Security.Claims;
using Library.Api.DTOs;
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
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetAllByBookId([FromRoute] Guid bookId)
    {
        var reviews = await _reviewService.GetBookReviewsByIdAsync(bookId);

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewRequest review)
    {
        string? userId = GetCurrentClientId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var result = await _reviewService.CreateReviewAsync(userId, review);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> Delete([FromRoute] Guid reviewId)
    {
        var role = GetCurrentClientRole();
        if (role is null) return Unauthorized();

        var userId = GetCurrentClientId();
        if (userId is null) return Unauthorized();

        var result = await _reviewService.DeleteReviewAsync(userId, reviewId, Roles.IsModerator(role));

        return result switch
        {
            Core.Enums.DeleteReviewResult.Success => NoContent(),
            Core.Enums.DeleteReviewResult.NotOwner => Forbid(),
            Core.Enums.DeleteReviewResult.ReviewNotFound => NotFound(),
            _ => StatusCode(500)
        };
    }

    private string? GetCurrentClientId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    private string? GetCurrentClientRole()
    {
        return User.FindFirstValue(ClaimTypes.Role);
    }
}
