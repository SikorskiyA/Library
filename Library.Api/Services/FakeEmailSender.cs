using Library.Core.Interfaces;

namespace Library.Api.Services;

public class FakeEmailSender : IEmailSender
{
    private readonly ILogger<FakeEmailSender> _logger;

    public FakeEmailSender(ILogger<FakeEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordAsync(string email, string password)
    {
        _logger.LogInformation("Письмо для {Email}: ваш пароль — {Password}", email, password);
        return Task.CompletedTask;
    }
}
