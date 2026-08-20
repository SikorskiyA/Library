using Library.Api.Data;
using Library.Api.DTOs;
using Library.Core.Constants;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookService _bookService;

    public ReviewService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBookService bookService
    )
    {
        _context = context;
        _userManager = userManager;
        _bookService = bookService;
    }

    private static ReviewResponse ToResponse(Review review)
    {
        return new ReviewResponse
        {
            BookId = review.BookId,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            Id = review.Id,
            Rating = review.Rating,
            UserId = review.UserId,
        };
    }

    public async Task<ReviewResponse> CreateReviewAsync(string userId, ReviewRequest reviewRequest)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("Пользователь с таким ID не найден");
        var book =
            await _bookService.GetBookByIdAsync(reviewRequest.BookId)
            ?? throw new Exception("Книги с таким ID не найдено");

        var reviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();
        if (reviews.Count > 0) throw new Exception("Уже есть отзыв от этого пользователя"); 

        var loans = await _context.Loans.Where(l => l.UserId == user.Id).ToListAsync();
        if (loans.Count == 0) throw new Exception("Пользователь не брал эту книгу");       

        var review = new Review
        {
            BookId = reviewRequest.BookId,
            Comment = reviewRequest.Comment,
            CreatedAt = DateTime.UtcNow,
            Rating = reviewRequest.Rating,
            UserId = userId,
        };

        _context.Reviews.Add(review);

        await _context.SaveChangesAsync();

        await _bookService.UpdateRatingAsync(review.BookId);

        return ToResponse(review);
    }

    public async Task<DeleteReviewResult> DeleteReviewAsync(
        string userId,
        Guid reviewId,
        bool isModerator
    )
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review is null)
            return DeleteReviewResult.ReviewNotFound;

        if (review.UserId != userId && !isModerator)
            return DeleteReviewResult.NotOwner;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        await _bookService.UpdateRatingAsync(review.BookId);

        return DeleteReviewResult.Success;
    }

    public async Task<List<ReviewResponse>> GetBookReviewsByIdAsync(Guid bookId)
    {
        var reviews = await _context
            .Reviews.Where(r => r.BookId == bookId)
            .OrderByDescending(r => !string.IsNullOrEmpty(r.Comment))
            .ThenByDescending(r => r.Rating)
            .ToListAsync();

        return reviews.Select(ToResponse).ToList();
    }
}
