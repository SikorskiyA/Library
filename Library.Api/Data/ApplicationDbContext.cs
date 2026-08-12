using Library.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Review> Reviews { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Loan>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Loan>()
            .HasOne(l => l.Librarian)
            .WithMany()
            .HasForeignKey(l => l.LibrarianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<Book>()
            .Property(b => b.Rating)
            .HasPrecision(3, 2);
    }
}
