namespace Library.Core.Entities;

public class Loan
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public string LibrarianId { get; set; } = string.Empty;
    public ApplicationUser Librarian { get; set; } = null!;
    public string UserId {get; set;} = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
}
