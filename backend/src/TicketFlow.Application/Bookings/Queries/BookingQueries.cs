using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Application.Bookings.Queries;

// --- DTOs ---
public record BookingDto(
    Guid Id,
    string ReferenceCode,
    Guid EventId,
    string EventName,
    BookingStatus Status,
    decimal TotalAmount,
    int TicketCount,
    DateTime CreatedAt,
    DateTime? ReservedUntil,
    List<BookingTicketDto> Tickets
);

public record BookingTicketDto(
    Guid Id,
    string ReferenceCode,
    TicketTier Tier,
    decimal Price,
    TicketStatus Status
);

// --- Get single booking ---
public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IApplicationDbContext _db;
    public GetBookingByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<BookingDto> Handle(GetBookingByIdQuery req, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == req.Id, ct)
            ?? throw new EntityNotFoundException(nameof(Booking), req.Id);

        var @event = await _db.Events.FindAsync(new object[] { booking.EventId }, ct)
            ?? throw new EntityNotFoundException(nameof(Event), booking.EventId);

        return BookingMapper.MapToDto(booking, @event.Name);
    }
}

// --- Get all bookings for a customer ---
public record GetCustomerBookingsQuery(Guid CustomerId) : IRequest<List<BookingDto>>;

public class GetCustomerBookingsHandler : IRequestHandler<GetCustomerBookingsQuery, List<BookingDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCustomerBookingsHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<BookingDto>> Handle(GetCustomerBookingsQuery req, CancellationToken ct)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.CustomerId == req.CustomerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var eventIds = bookings.Select(b => b.EventId).Distinct().ToList();
        var events = await _db.Events
            .Where(e => eventIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, ct);

        return bookings
            .Select(b => BookingMapper.MapToDto(b, events.GetValueOrDefault(b.EventId, "Unknown Event")))
            .ToList();
    }
}

// --- Cancel booking ---
public record CancelBookingCommand(Guid BookingId) : IRequest<Unit>;

public class CancelBookingHandler : IRequestHandler<CancelBookingCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public CancelBookingHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(CancelBookingCommand req, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == req.BookingId, ct)
            ?? throw new EntityNotFoundException(nameof(Booking), req.BookingId);

        var @event = await _db.Events.FindAsync(new object[] { booking.EventId }, ct)
            ?? throw new EntityNotFoundException(nameof(Event), booking.EventId);

        booking.Cancel(@event.HasStarted());
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// --- Shared mapper ---
file static class BookingMapper
{
    public static BookingDto MapToDto(Booking booking, string eventName) => new(
        booking.Id,
        booking.ReferenceCode,
        booking.EventId,
        eventName,
        booking.Status,
        booking.TotalAmount,
        booking.Tickets.Count,
        booking.CreatedAt,
        booking.Tickets.FirstOrDefault(t => t.ReservedUntil.HasValue)?.ReservedUntil,
        booking.Tickets.Select(t => new BookingTicketDto(
            t.Id, t.ReferenceCode, t.Tier, t.Price, t.Status)).ToList()
    );
}
