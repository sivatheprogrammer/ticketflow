using FluentAssertions;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Events.Entities;
using Xunit;

namespace TicketFlow.Domain.Tests;

public class EventTests
{
    private static Venue CreateVenue() =>
        new("Toyota Center", "1510 Polk St", "Houston", 18055);

    private static Event CreateDraftEvent(Venue? venue = null)
    {
        var v = venue ?? CreateVenue();
        return new Event(
            "Test Concert", "A great show",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(3),
            EventCategory.Concert, v.Id);
    }

    [Fact]
    public void New_event_starts_as_Draft()
    {
        var @event = CreateDraftEvent();
        @event.Status.Should().Be(EventStatus.Draft);
    }

    [Fact]
    public void Cannot_create_event_with_start_in_the_past()
    {
        var act = () => new Event(
            "Past Event", "Too late",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            EventCategory.Concert, Guid.NewGuid());

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "EVENT_PAST_START");
    }

    [Fact]
    public void Cannot_publish_event_without_tickets()
    {
        var @event = CreateDraftEvent();

        var act = () => @event.Publish();

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "EVENT_NO_TICKETS");
    }

    [Fact]
    public void Publishing_event_with_tickets_succeeds()
    {
        var @event = CreateDraftEvent();
        @event.AddTicket(TicketTier.General, 50m, 100);

        @event.Publish();

        @event.Status.Should().Be(EventStatus.Published);
    }

    [Fact]
    public void Cannot_add_tickets_to_published_event()
    {
        var @event = CreateDraftEvent();
        @event.AddTicket(TicketTier.General, 50m, 10);
        @event.Publish();

        var act = () => @event.AddTicket(TicketTier.VIP, 200m, 5);

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "EVENT_NOT_EDITABLE");
    }

    [Fact]
    public void Cannot_cancel_a_completed_event()
    {
        var @event = CreateDraftEvent();
        @event.AddTicket(TicketTier.General, 50m, 10);
        @event.Publish();

        // Force completed status via reflection for test purposes
        typeof(Event).GetProperty("Status")!
            .SetValue(@event, EventStatus.Completed);

        var act = () => @event.Cancel();

        act.Should().Throw<BusinessRuleViolationException>()
           .Where(e => e.RuleCode == "EVENT_INVALID_STATE");
    }

    [Fact]
    public void AddTicket_creates_correct_number_of_ticket_entities()
    {
        var @event = CreateDraftEvent();
        @event.AddTicket(TicketTier.General, 50m, 100);
        @event.AddTicket(TicketTier.VIP, 200m, 10);

        @event.Tickets.Should().HaveCount(110);
        @event.Tickets.Count(t => t.Tier == TicketTier.General).Should().Be(100);
        @event.Tickets.Count(t => t.Tier == TicketTier.VIP).Should().Be(10);
    }
}
