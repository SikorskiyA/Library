using Library.Core.Enums;

namespace Library.Core.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
}
