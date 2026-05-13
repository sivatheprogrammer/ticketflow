using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Events.DTOs;

public record EventSummaryDto(
    Guid Id,
    string Name,
    DateTime StartsAt,
    EventCategory Category,
    string VenueName,
    string City,
    decimal MinPrice,
    int AvailableTickets
);

public record EventDetailsDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartsAt,
    DateTime EndsAt,
    EventCategory Category,
    EventStatus Status,
    string VenueName,
    string City,
    int VenueCapacity,
    List<TicketTierAvailabilityDto> TicketTiers
);

public record TicketTierAvailabilityDto(
    TicketTier Tier,
    decimal Price,
    int AvailableCount
);
