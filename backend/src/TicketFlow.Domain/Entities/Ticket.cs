using TicketFlow.Domain.Common;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Entities;

public class Ticket : BaseEntity
{
    public string ReferenceCode { get; private set; } = null!;
    public Guid EventId { get; private set; }
    public TicketTier Tier { get; private set; }
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTime? ReservedUntil { get; private set; }
    public Guid? BookingId { get; private set; }

    private Ticket() { } // EF Core

    internal Ticket(Guid eventId, TicketTier tier, decimal price)
    {
        EventId = eventId;
        Tier = tier;
        Price = price;
        Status = TicketStatus.Available;
        ReferenceCode = GenerateReferenceCode();
    }

    public void Reserve(Guid bookingId, TimeSpan holdDuration)
    {
        if (Status != TicketStatus.Available)
            throw new BusinessRuleViolationException(
                "TICKET_NOT_AVAILABLE", $"Ticket {ReferenceCode} is not available for reservation.");

        Status = TicketStatus.Reserved;
        BookingId = bookingId;
        ReservedUntil = DateTime.UtcNow.Add(holdDuration);
        MarkUpdated();
    }

    public void Confirm()
    {
        if (Status != TicketStatus.Reserved)
            throw new BusinessRuleViolationException(
                "TICKET_NOT_RESERVED", $"Only reserved tickets can be confirmed. Ticket {ReferenceCode} is {Status}.");
        if (ReservedUntil < DateTime.UtcNow)
            throw new BusinessRuleViolationException(
                "TICKET_RESERVATION_EXPIRED", $"Reservation for ticket {ReferenceCode} has expired.");

        Status = TicketStatus.Booked;
        ReservedUntil = null;
        MarkUpdated();
    }

    public void ReleaseReservation()
    {
        if (Status != TicketStatus.Reserved)
            return; // idempotent

        Status = TicketStatus.Available;
        BookingId = null;
        ReservedUntil = null;
        MarkUpdated();
    }

    public void CancelBooking()
    {
        if (Status != TicketStatus.Booked)
            throw new BusinessRuleViolationException(
                "TICKET_NOT_BOOKED", $"Only booked tickets can be cancelled. Ticket {ReferenceCode} is {Status}.");

        Status = TicketStatus.Available;
        BookingId = null;
        MarkUpdated();
    }

    public bool IsReservationExpired() =>
        Status == TicketStatus.Reserved && ReservedUntil < DateTime.UtcNow;

    private static string GenerateReferenceCode() =>
        $"TKT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
}
