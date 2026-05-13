using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketFlow.Domain.Enums;
using TicketFlow.Infrastructure.Persistence;

namespace TicketFlow.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically releases tickets whose 15-minute reservation hold has expired.
/// In Phase 1 this is a simple in-process BackgroundService.
/// In Phase 5+ this is replaced by an Azure Service Bus scheduled message
/// per reservation, eliminating the polling overhead. See ADR-004.
/// </summary>
public class ReservationExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpiryJob> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    public ReservationExpiryJob(IServiceScopeFactory scopeFactory, ILogger<ReservationExpiryJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReleaseExpiredAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release expired reservations");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ReleaseExpiredAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;

        var expiredBookings = await db.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.Status == BookingStatus.Pending
                     && b.Tickets.Any(t => t.ReservedUntil < now))
            .ToListAsync(ct);

        if (expiredBookings.Count == 0) return;

        foreach (var booking in expiredBookings)
            booking.Expire();

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Expired {Count} bookings and released their tickets.", expiredBookings.Count);
    }
}
