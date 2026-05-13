using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Events.DTOs;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Application.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDetailsDto>;

public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventDetailsDto>
{
    private readonly IApplicationDbContext _db;
    public GetEventByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<EventDetailsDto> Handle(GetEventByIdQuery req, CancellationToken ct)
    {
        var @event = await _db.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == req.Id, ct)
            ?? throw new EntityNotFoundException(nameof(Event), req.Id);

        var tiers = @event.Tickets
            .GroupBy(t => new { t.Tier, t.Price })
            .Select(g => new TicketTierAvailabilityDto(
                g.Key.Tier,
                g.Key.Price,
                g.Count(t => t.Status == TicketStatus.Available)
            ))
            .OrderBy(t => t.Price)
            .ToList();

        return new EventDetailsDto(
            @event.Id,
            @event.Name,
            @event.Description,
            @event.StartsAt,
            @event.EndsAt,
            @event.Category,
            @event.Status,
            @event.Venue.Name,
            @event.Venue.City,
            @event.Venue.Capacity,
            tiers
        );
    }
}
