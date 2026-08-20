namespace Library.Api.DTOs;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Rating { get; set; }
}
