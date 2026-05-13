using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Events.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Events.Queries;

public record GetEventsQuery(
    string? City = null,
    EventCategory? Category = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 12
) : IRequest<List<EventSummaryDto>>;

public class GetEventsHandler : IRequestHandler<GetEventsQuery, List<EventSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    public GetEventsHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<EventSummaryDto>> Handle(GetEventsQuery req, CancellationToken ct)
    {
        var query = _db.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .Where(e => e.Status == EventStatus.Published);

        if (!string.IsNullOrWhiteSpace(req.City))
            query = query.Where(e => e.Venue.City.Contains(req.City));

        if (req.Category.HasValue)
            query = query.Where(e => e.Category == req.Category.Value);

        if (req.FromDate.HasValue)
            query = query.Where(e => e.StartsAt >= req.FromDate.Value);

        if (req.ToDate.HasValue)
            query = query.Where(e => e.StartsAt <= req.ToDate.Value);

        return await query
            .OrderBy(e => e.StartsAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(e => new EventSummaryDto(
                e.Id,
                e.Name,
                e.StartsAt,
                e.Category,
                e.Venue.Name,
                e.Venue.City,
                e.Tickets.Where(t => t.Status == TicketStatus.Available).Any()
                    ? e.Tickets.Where(t => t.Status == TicketStatus.Available).Min(t => t.Price)
                    : 0,
                e.Tickets.Count(t => t.Status == TicketStatus.Available)
            ))
            .ToListAsync(ct);
    }
}
