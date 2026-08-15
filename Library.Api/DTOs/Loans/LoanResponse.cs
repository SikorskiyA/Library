namespace Library.Api.DTOs;

public class LoanResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string LibrarianId { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
}