using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Application.Bookings.Commands;

public record ConfirmBookingCommand(Guid BookingId) : IRequest<Unit>;

public class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public ConfirmBookingHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(ConfirmBookingCommand request, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, ct)
            ?? throw new EntityNotFoundException(nameof(Booking), request.BookingId);

        // In Phase 1: payment is mocked as always-successful.
        // In Phase 5: this becomes a Saga step that runs after Payment service confirms.
        booking.Confirm();

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
