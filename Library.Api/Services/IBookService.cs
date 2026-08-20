using Library.Api.DTOs;
using Library.Core.Enums;

namespace Library.Api.Services;

public interface IBookService
{
    public Task<List<BookResponse>> GetAllBooksAsync();
    public Task<BookResponse?> GetBookByIdAsync(Guid id);
    public Task<List<BookResponse>> SearchBookAsync(string? author, string? genre, string? publisher);
    public Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    public Task<DeleteResult> DeleteBookAsync(Guid id);
    public Task IncreaseStockAsync(Guid id);
    public Task DecreaseStockAsync(Guid id);
    public Task<UpdateRatingResult> UpdateRatingAsync(Guid bookId);
}