using TicketFlow.Domain.Common;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Entities;

public class Booking : BaseEntity
{
    public const int MaxTicketsPerBookingPerEvent = 6;
    public static readonly TimeSpan ReservationHold = TimeSpan.FromMinutes(15);

    public string ReferenceCode { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public Guid EventId { get; private set; }
    public BookingStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<Ticket> _tickets = new();
    public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

    private Booking() { } // EF Core

    public Booking(Guid customerId, Guid eventId, IReadOnlyList<Ticket> tickets)
    {
        if (tickets is null || !tickets.Any())
            throw new BusinessRuleViolationException(
                "BOOKING_NO_TICKETS", "Booking must contain at least one ticket.");

        if (tickets.Count > MaxTicketsPerBookingPerEvent)
            throw new BusinessRuleViolationException(
                "BOOKING_TICKET_LIMIT_EXCEEDED",
                $"A customer cannot book more than {MaxTicketsPerBookingPerEvent} tickets per event.");

        if (tickets.Any(t => t.EventId != eventId))
            throw new BusinessRuleViolationException(
                "BOOKING_TICKETS_MISMATCH", "All tickets must belong to the same event.");

        if (tickets.Any(t => t.Status != TicketStatus.Available))
            throw new BusinessRuleViolationException(
                "BOOKING_TICKETS_UNAVAILABLE", "One or more tickets are not available.");

        CustomerId = customerId;
        EventId = eventId;
        Status = BookingStatus.Pending;
        ReferenceCode = $"BKG-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

        // Server-side total calculation — never trust the client
        TotalAmount = tickets.Sum(t => t.Price);

        foreach (var ticket in tickets)
        {
            ticket.Reserve(Id, ReservationHold);
            _tickets.Add(ticket);
        }
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new BusinessRuleViolationException(
                "BOOKING_INVALID_STATE", $"Only Pending bookings can be confirmed. Current: {Status}.");

        foreach (var ticket in _tickets)
            ticket.Confirm();

        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Cancel(bool eventHasStarted)
    {
        if (Status == BookingStatus.Cancelled || Status == BookingStatus.Refunded)
            return; // idempotent

        if (Status == BookingStatus.Confirmed && eventHasStarted)
            throw new BusinessRuleViolationException(
                "BOOKING_EVENT_STARTED", "Cannot cancel a confirmed booking after the event has started.");

        foreach (var ticket in _tickets)
        {
            if (ticket.Status == TicketStatus.Reserved)
                ticket.ReleaseReservation();
            else if (ticket.Status == TicketStatus.Booked)
                ticket.CancelBooking();
        }

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Expire()
    {
        if (Status != BookingStatus.Pending)
            return;

        foreach (var ticket in _tickets)
            ticket.ReleaseReservation();

        Status = BookingStatus.Expired;
        MarkUpdated();
    }
}
