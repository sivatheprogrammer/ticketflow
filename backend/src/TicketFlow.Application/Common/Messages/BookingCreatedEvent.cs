namespace TicketFlow.Application.Common.Messages;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid CustomerId,
    Guid EventId,
    string ReferenceCode,
    decimal TotalAmount,
    DateTime CreatedAt
);