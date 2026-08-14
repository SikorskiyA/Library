namespace Library.Api.DTOs;

public class CreateBookRequest
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public int Pages { get; set; }
    public int Quantity { get; set; }
}