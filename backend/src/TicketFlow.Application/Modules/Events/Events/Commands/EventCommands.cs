using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Events.Entities;

namespace TicketFlow.Application.Events.Commands;

// --- Create Event ---
public record CreateEventCommand(
    string Name,
    string Description,
    DateTime StartsAt,
    DateTime EndsAt,
    EventCategory Category,
    Guid VenueId
) : IRequest<Guid>;

public class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public CreateEventHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateEventCommand req, CancellationToken ct)
    {
        var venue = await _db.Venues.FindAsync(new object[] { req.VenueId }, ct)
            ?? throw new EntityNotFoundException(nameof(Venue), req.VenueId);

        var @event = new Event(
            req.Name, req.Description, req.StartsAt,
            req.EndsAt, req.Category, venue.Id);

        _db.Events.Add(@event);
        await _db.SaveChangesAsync(ct);
        return @event.Id;
    }
}

// --- Add Ticket Tier to Event ---
public record AddTicketTierCommand(
    Guid EventId,
    TicketTier Tier,
    decimal Price,
    int Quantity
) : IRequest<Unit>;

public class AddTicketTierHandler : IRequestHandler<AddTicketTierCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public AddTicketTierHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(AddTicketTierCommand req, CancellationToken ct)
    {
        var @event = await _db.Events.FindAsync(new object[] { req.EventId }, ct)
            ?? throw new EntityNotFoundException(nameof(Event), req.EventId);

        @event.AddTicket(req.Tier, req.Price, req.Quantity);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// --- Publish Event ---
public record PublishEventCommand(Guid EventId) : IRequest<Unit>;

public class PublishEventHandler : IRequestHandler<PublishEventCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public PublishEventHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(PublishEventCommand req, CancellationToken ct)
    {
        var @event = await _db.Events
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == req.EventId, ct)
            ?? throw new EntityNotFoundException(nameof(Event), req.EventId);

        @event.Publish();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// --- Cancel Event ---
public record CancelEventCommand(Guid EventId) : IRequest<Unit>;

public class CancelEventHandler : IRequestHandler<CancelEventCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public CancelEventHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(CancelEventCommand req, CancellationToken ct)
    {
        var @event = await _db.Events.FindAsync(new object[] { req.EventId }, ct)
            ?? throw new EntityNotFoundException(nameof(Event), req.EventId);

        @event.Cancel();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
