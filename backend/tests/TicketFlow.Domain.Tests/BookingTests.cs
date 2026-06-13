using FluentAssertions;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Bookings.Entities;
using TicketFlow.Domain.Modules.Events.Entities;
using Xunit;

namespace TicketFlow.Domain.Tests;

public class BookingTests
{
    private static List<Ticket> CreateAvailableTickets(int count, decimal price = 50m)
    {
        var eventId = Guid.NewGuid();
        var venue = new Venue("Arena", "1 Main St", "Houston", 1000);
        var @event = new Event("Test Event", "desc",
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(7).AddHours(3),
            EventCategory.Concert, venue.Id);
        @event.AddTicket(TicketTier.General, price, count);
        return @event.Tickets.ToList();
    }

    [Fact]
    public void Creating_a_booking_reserves_all_tickets_for_15_minutes()
    {
        var tickets = CreateAvailableTickets(3);
        var eventId = tickets[0].EventId;

        var booking = new Booking(Guid.NewGuid(), eventId, tickets);

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.Tickets.Should().HaveCount(3);
        booking.Tickets.Should().AllSatisfy(t =>
        {
            t.Status.Should().Be(TicketStatus.Reserved);
            t.ReservedUntil.Should().BeCloseTo(
                DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
        });
    }

    [Fact]
    public void Total_amount_is_calculated_server_side_from_ticket_prices()
    {
        var tickets = CreateAvailableTickets(3, price: 75m);

        var booking = new Booking(Guid.NewGuid(), tickets[0].EventId, tickets);

        booking.TotalAmount.Should().Be(225m);
    }

    [Fact]
    public void Booking_more_than_six_tickets_for_the_same_event_is_rejected()
    {
        var tickets = CreateAvailableTickets(7);

        var act = () => new Booking(Guid.NewGuid(), tickets[0].EventId, tickets);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "BOOKING_TICKET_LIMIT_EXCEEDED");
    }

    [Fact]
    public void Expiring_a_pending_booking_releases_its_tickets()
    {
        var tickets = CreateAvailableTickets(2);
        var booking = new Booking(Guid.NewGuid(), tickets[0].EventId, tickets);

        booking.Expire();

        booking.Status.Should().Be(BookingStatus.Expired);
        booking.Tickets.Should().AllSatisfy(t =>
            t.Status.Should().Be(TicketStatus.Available));
    }

    [Fact]
    public void Confirmed_booking_cannot_be_cancelled_after_event_starts()
    {
        var tickets = CreateAvailableTickets(1);
        var booking = new Booking(Guid.NewGuid(), tickets[0].EventId, tickets);
        booking.Confirm();

        var act = () => booking.Cancel(eventHasStarted: true);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "BOOKING_EVENT_STARTED");
    }
}
