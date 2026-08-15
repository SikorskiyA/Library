namespace Library.Api.DTOs;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}