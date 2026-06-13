using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Events.Entities;

namespace TicketFlow.Application.Venues;

// --- DTOs ---
public record VenueDto(Guid Id, string Name, string Address, string City, int Capacity);

// --- Queries ---
public record GetVenuesQuery : IRequest<List<VenueDto>>;

public class GetVenuesHandler : IRequestHandler<GetVenuesQuery, List<VenueDto>>
{
    private readonly IApplicationDbContext _db;
    public GetVenuesHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<VenueDto>> Handle(GetVenuesQuery req, CancellationToken ct) =>
        await _db.Venues
            .OrderBy(v => v.Name)
            .Select(v => new VenueDto(v.Id, v.Name, v.Address, v.City, v.Capacity))
            .ToListAsync(ct);
}

public record GetVenueByIdQuery(Guid Id) : IRequest<VenueDto>;

public class GetVenueByIdHandler : IRequestHandler<GetVenueByIdQuery, VenueDto>
{
    private readonly IApplicationDbContext _db;
    public GetVenueByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<VenueDto> Handle(GetVenueByIdQuery req, CancellationToken ct)
    {
        var venue = await _db.Venues.FindAsync(new object[] { req.Id }, ct)
            ?? throw new EntityNotFoundException(nameof(Venue), req.Id);
        return new VenueDto(venue.Id, venue.Name, venue.Address, venue.City, venue.Capacity);
    }
}

// --- Commands ---
public record CreateVenueCommand(string Name, string Address, string City, int Capacity) : IRequest<Guid>;

public class CreateVenueHandler : IRequestHandler<CreateVenueCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public CreateVenueHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateVenueCommand req, CancellationToken ct)
    {
        var venue = new Venue(req.Name, req.Address, req.City, req.Capacity);
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync(ct);
        return venue.Id;
    }
}

public record UpdateVenueCommand(Guid Id, string Name, string Address, string City, int Capacity) : IRequest<Unit>;

public class UpdateVenueHandler : IRequestHandler<UpdateVenueCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateVenueHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateVenueCommand req, CancellationToken ct)
    {
        var venue = await _db.Venues.FindAsync(new object[] { req.Id }, ct)
            ?? throw new EntityNotFoundException(nameof(Venue), req.Id);
        venue.UpdateDetails(req.Name, req.Address, req.City, req.Capacity);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
