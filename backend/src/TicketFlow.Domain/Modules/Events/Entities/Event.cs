using TicketFlow.Domain.Common;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Modules.Events.Entities;

public class Event : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public EventCategory Category { get; private set; }
    public EventStatus Status { get; private set; }

    public Guid VenueId { get; private set; }
    public Venue Venue { get; private set; } = null!;

    private readonly List<Ticket> _tickets = new();
    public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

    private Event() { } // EF Core

    public Event(string name, string description, DateTime startsAt, DateTime endsAt,
                 EventCategory category, Guid venueId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("EVENT_NAME_REQUIRED", "Event name is required.");
        if (endsAt <= startsAt)
            throw new BusinessRuleViolationException("EVENT_INVALID_DATES", "End date must be after start date.");
        if (startsAt <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("EVENT_PAST_START", "Event cannot start in the past.");

        Name = name;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Category = category;
        VenueId = venueId;
        Status = EventStatus.Draft;
    }

    public void AddTicket(TicketTier tier, decimal price, int quantity)
    {
        if (Status != EventStatus.Draft)
            throw new BusinessRuleViolationException(
                "EVENT_NOT_EDITABLE", "Tickets can only be added while the event is in Draft status.");
        if (price <= 0)
            throw new BusinessRuleViolationException("TICKET_PRICE_INVALID", "Price must be greater than zero.");
        if (quantity <= 0)
            throw new BusinessRuleViolationException("TICKET_QUANTITY_INVALID", "Quantity must be greater than zero.");

        for (int i = 0; i < quantity; i++)
            _tickets.Add(new Ticket(Id, tier, price));

        MarkUpdated();
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new BusinessRuleViolationException(
                "EVENT_INVALID_STATE", "Only Draft events can be published.");
        if (!_tickets.Any())
            throw new BusinessRuleViolationException(
                "EVENT_NO_TICKETS", "Event must have at least one ticket tier defined before publishing.");

        Status = EventStatus.Published;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status == EventStatus.Completed)
            throw new BusinessRuleViolationException(
                "EVENT_INVALID_STATE", "Completed events cannot be cancelled.");
        if (StartsAt <= DateTime.UtcNow)
            throw new BusinessRuleViolationException(
                "EVENT_ALREADY_STARTED", "Events that have already started cannot be cancelled.");

        Status = EventStatus.Cancelled;
        MarkUpdated();
    }

    public bool HasStarted() => StartsAt <= DateTime.UtcNow;
}
