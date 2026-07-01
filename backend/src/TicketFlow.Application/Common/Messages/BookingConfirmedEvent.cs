namespace TicketFlow.Application.Common.Messages;

public record BookingConfirmedEvent(
    Guid BookingId,
    Guid CustomerId,
    Guid EventId,
    string ReferenceCode,
    decimal TotalAmount,
    DateTime ConfirmedAt
);