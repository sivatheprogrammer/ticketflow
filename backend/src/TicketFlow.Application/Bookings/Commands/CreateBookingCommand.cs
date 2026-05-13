using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Application.Bookings.Commands;

public record CreateBookingCommand(
    Guid CustomerId,
    Guid EventId,
    int Quantity,
    TicketTier Tier
) : IRequest<CreateBookingResult>;

public record CreateBookingResult(Guid BookingId, string ReferenceCode, decimal TotalAmount);

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IApplicationDbContext _db;

    public CreateBookingHandler(IApplicationDbContext db) => _db = db;

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

        // Reserve the first N available tickets of the requested tier.
        // Note: In Phase 1 this is single-instance safe via SaveChanges transaction.
        // In later phases we'll evolve to row-version-based optimistic concurrency
        // and ultimately distributed locking via Redis. See ADR-004.
        var available = @event.Tickets
            .Where(t => t.Tier == request.Tier && t.Status == TicketStatus.Available)
            .Take(request.Quantity)
            .ToList();

        if (available.Count < request.Quantity)
            throw new BusinessRuleViolationException(
                "TICKETS_INSUFFICIENT",
                $"Only {available.Count} tickets available for tier {request.Tier}, requested {request.Quantity}.");

        var booking = new Booking(customer.Id, @event.Id, available);
        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync(ct);

        return new CreateBookingResult(booking.Id, booking.ReferenceCode, booking.TotalAmount);
    }
}
