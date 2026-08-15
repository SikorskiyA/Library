namespace Library.Core.Interfaces;

public interface IEmailSender
{
    Task SendPasswordAsync(string email, string password);
}
