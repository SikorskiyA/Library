using Library.Core.Entities;

namespace Library.Api.DTOs;

public class BookResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public int Pages { get; set; }
    public int Quantity { get; set; }
    public int InStock { get; set; }
    public double Rating { get; set; }
}