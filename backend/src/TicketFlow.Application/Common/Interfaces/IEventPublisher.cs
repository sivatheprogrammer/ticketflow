using TicketFlow.Application.Common.Messages;

namespace TicketFlow.Application.Common.Interfaces;

public interface IEventPublisher
{
    Task PublishBookingCreatedAsync(BookingCreatedEvent @event, CancellationToken ct = default);
}