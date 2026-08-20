namespace Library.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Librarian = "Librarian";
    public const string Client = "Client";

    public static readonly string[] All = { Admin, Librarian, Client };

    public const string LibrarianOrAdmin = $"{Librarian},{Admin}";

    public static bool IsValid(string role) => All.Contains(role);
    public static bool IsModerator(string role) => LibrarianOrAdmin.Contains(role);
}
