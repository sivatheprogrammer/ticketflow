using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.Venues;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IMediator _mediator;
    public VenuesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<VenueDto>>> List(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetVenuesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VenueDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetVenueByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVenueCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVenueRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateVenueCommand(id, req.Name, req.Address, req.City, req.Capacity), ct);
        return NoContent();
    }
}

public record UpdateVenueRequest(string Name, string Address, string City, int Capacity);
