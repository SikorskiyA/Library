//контроллер для проверки SignalR

using Library.Api.Hubs;
using Library.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/test")]
[Authorize(Roles = Roles.Admin)]
public class TestController : ControllerBase
{
    private readonly IHubContext<BookAvailabilityHub> _hubContext;

    public TestController(IHubContext<BookAvailabilityHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("notify/{bookId}")]
    public async Task<IActionResult> Notify(Guid bookId)
    {
        await _hubContext.Clients.Group(bookId.ToString())
            .SendAsync("BookAvailable", bookId);
        return Ok($"Уведомление отправлено для книги {bookId}");
    }
}