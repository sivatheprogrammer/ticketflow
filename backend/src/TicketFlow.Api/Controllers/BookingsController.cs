using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.Bookings.Commands;
using TicketFlow.Application.Bookings.Queries;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BookingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetBookingByIdQuery(id), ct));

    /// <summary>Reserve tickets (15-minute hold). Phase 1: customerId supplied in body.
    /// Phase 2: will be extracted from the JWT claims.</summary>
    [HttpPost]
    public async Task<ActionResult<CreateBookingResult>> Create(
        [FromBody] CreateBookingCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, result);
    }

    /// <summary>Confirm after payment. Phase 1: payment mocked. Phase 5: Saga step.</summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmBookingCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelBookingCommand(id), ct);
        return NoContent();
    }
}
