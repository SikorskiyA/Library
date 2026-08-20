using Library.Api.DTOs;
using Library.Core.Enums;

namespace Library.Api.Services;

public interface IReviewService
{
    public Task<ReviewResponse> CreateReviewAsync(string userId, ReviewRequest reviewRequest);
    public Task<List<ReviewResponse>> GetBookReviewsByIdAsync(Guid bookId);
    public Task<DeleteReviewResult> DeleteReviewAsync(string userId, Guid reviewId, bool isModerator);
}