using Library.Api.DTOs;
using Library.Api.Services;
using Library.Core.Constants;
using Library.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _bookService.GetAllBooksAsync();

        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _bookService.GetBookById(id);

        return book is not null ? Ok(book) : NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? author,
        [FromQuery] string? genre,
        [FromQuery] string? publisher
    )
    {
        var books = await _bookService.SearchBookAsync(author, genre, publisher);

        return Ok(books);
    }

    [HttpPost("create")]
    [Authorize(Roles = Roles.Librarian)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request)
    {
        var book = await _bookService.CreateBookAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Librarian)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bookService.DeleteBookAsync(id);

        switch (result)
        {
            case DeleteBookResult.HasActiveOperations:
            {
                return Conflict(
                    new { Message = "Нельзя удалить книгу с активными бронями или выдачами" }
                );
            }
            case DeleteBookResult.NotFound:
            {
                return NotFound();
            }
            case DeleteBookResult.Success:
            {
                return Ok();
            }
            default:
                return StatusCode(500);
        }
    }
}
