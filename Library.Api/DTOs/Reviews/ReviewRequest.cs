namespace Library.Api.DTOs;

public class ReviewRequest
{
    public Guid BookId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
}
