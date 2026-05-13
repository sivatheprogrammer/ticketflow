using TicketFlow.Domain.Common;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public int Capacity { get; private set; }

    private Venue() { } // EF Core

    public Venue(string name, string address, string city, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("VENUE_NAME_REQUIRED", "Venue name is required.");
        if (capacity <= 0)
            throw new BusinessRuleViolationException("VENUE_CAPACITY_INVALID", "Capacity must be greater than zero.");

        Name = name;
        Address = address;
        City = city;
        Capacity = capacity;
    }

    public void UpdateDetails(string name, string address, string city, int capacity)
    {
        if (capacity <= 0)
            throw new BusinessRuleViolationException("VENUE_CAPACITY_INVALID", "Capacity must be greater than zero.");

        Name = name;
        Address = address;
        City = city;
        Capacity = capacity;
        MarkUpdated();
    }
}
