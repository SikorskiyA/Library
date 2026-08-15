using Library.Api.Data;
using Library.Api.Services;
using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.BackgroundServices;

public class ReservationExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationExpirationService> _logger;

    public ReservationExpirationService(
        IServiceProvider serviceProvider,
        ILogger<ReservationExpirationService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();

                var query = context.Reservations.Where(r =>
                    r.Status == ReservationStatus.Active && r.ExpiresAt < DateTime.UtcNow
                );
                var expiredReservations = await query.ToListAsync();
                foreach (Reservation reservation in expiredReservations)
                {
                    reservation.Status = ReservationStatus.Expired;
                }
                if (expiredReservations.Count > 0)
                {
                    await context.SaveChangesAsync();

                    foreach (Reservation reservation in expiredReservations)
                    {
                        await bookService.IncreaseStockAsync(reservation.BookId);
                    }

                    _logger.LogInformation($"Обработано просроченных броней: {expiredReservations.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке истёкших броней");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
