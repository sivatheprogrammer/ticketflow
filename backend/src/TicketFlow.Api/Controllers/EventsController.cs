using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.Events.Commands;
using TicketFlow.Application.Events.DTOs;
using TicketFlow.Application.Events.Queries;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventsController(IMediator mediator) => _mediator = mediator;

    // Public endpoints — no auth required for browsing events
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<EventSummaryDto>>> List(
        [FromQuery] string? city,
        [FromQuery] EventCategory? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetEventsQuery(city, category, fromDate, toDate, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);
        return Ok(result);
    }

    // Write endpoints — require Organizer or Admin role
    [HttpPost]
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/ticket-tiers")]
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> AddTicketTier(
        Guid id, [FromBody] AddTicketTierRequest req, CancellationToken ct)
    {
        await _mediator.Send(
            new AddTicketTierCommand(id, req.Tier, req.Price, req.Quantity), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new PublishEventCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelEventCommand(id), ct);
        return NoContent();
    }
}

public record AddTicketTierRequest(TicketTier Tier, decimal Price, int Quantity);
