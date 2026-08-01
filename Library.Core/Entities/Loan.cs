using System.Dynamic;

namespace Library.Core.Entities;

public class Loan
{
    public Guid Id {get; set;}
    public Guid BookId {get; set;}
    public Guid LibrarianId {get; set;}
    public DateTime IssuedAt {get; set;} = DateTime.Now;
    public DateTime DueDate {get; set;}
    public DateTime ReturnedAt {get; set;}
}