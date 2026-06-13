using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Bookings.Entities;
using TicketFlow.Domain.Modules.Events.Entities;
using TicketFlow.Domain.Modules.Identity;

namespace TicketFlow.Application.Bookings.Commands;

public record CreateBookingCommand(
    Guid CustomerId,
    Guid EventId,
    TicketTier Tier,
    int Quantity) : IRequest<CreateBookingResult>;

public record CreateBookingResult(Guid BookingId, string ReferenceCode, decimal TotalAmount);

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IRedisService _redis;

    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);

    public CreateBookingHandler(IApplicationDbContext db, IRedisService redis)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        // Verify customer and event exist
        var customer = await _db.Customers.FindAsync(new object[] { request.CustomerId }, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), request.CustomerId);

        var @event = await _db.Events
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct)
            ?? throw new EntityNotFoundException(nameof(Event), request.EventId);

        if (@event.Status != EventStatus.Published)
            throw new BusinessRuleViolationException(
                "EVENT_NOT_PUBLISHED", "Tickets can only be booked for published events.");

        // Find available tickets for the requested tier
        var available = @event.Tickets
            .Where(t => t.Tier == request.Tier && t.Status == TicketStatus.Available)
            .Take(request.Quantity)
            .ToList();

        if (available.Count < request.Quantity)
            throw new BusinessRuleViolationException(
                "TICKETS_INSUFFICIENT",
                $"Only {available.Count} tickets available for tier {request.Tier}, requested {request.Quantity}.");

        // --- Distributed locking (Phase 3: Redis) ---
        // Acquire a lock per ticket to prevent double-booking across API replicas.
        // Each lock key is unique to the ticket row: lock:ticket:{id}
        var lockKeys = available.Select(t => $"lock:ticket:{t.Id}").ToList();
        var acquiredLocks = new List<string>();

        try
        {
            foreach (var lockKey in lockKeys)
            {
                var acquired = await _redis.AcquireLockAsync(lockKey, LockExpiry, ct);
                if (!acquired)
                {
                    // Another replica is processing one of these tickets — abort
                    throw new BusinessRuleViolationException(
                        "TICKETS_UNAVAILABLE",
                        "One or more tickets are temporarily unavailable. Please try again.");
                }
                acquiredLocks.Add(lockKey);
            }

            // All locks acquired — safe to create the booking
            var booking = new Booking(customer.Id, @event.Id, available);
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            return new CreateBookingResult(booking.Id, booking.ReferenceCode, booking.TotalAmount);
        }
        finally
        {
            // Always release locks — even if an exception was thrown
            foreach (var lockKey in acquiredLocks)
                await _redis.ReleaseLockAsync(lockKey, ct);
        }
    }
}
