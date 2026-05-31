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
    private readonly IRedisService _redis;

    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public GetEventByIdHandler(IApplicationDbContext db, IRedisService redis)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<EventDetailsDto> Handle(GetEventByIdQuery req, CancellationToken ct)
    {
        var cacheKey = $"events:detail:{req.Id}";

        // 1. Check cache
        var cached = await _redis.GetAsync<EventDetailsDto>(cacheKey, ct);
        if (cached is not null)
            return cached;

        // 2. Cache miss — query DB
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

        var result = new EventDetailsDto(
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

        // 3. Store in cache
        await _redis.SetAsync(cacheKey, result, CacheExpiry, ct);

        return result;
    }
}
