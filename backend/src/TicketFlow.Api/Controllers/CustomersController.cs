using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.Bookings.Queries;
using TicketFlow.Application.Customers;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetCustomerByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateCustomerCommand(id, req.FullName, req.PhoneNumber), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/bookings")]
    public async Task<ActionResult<List<BookingDto>>> GetBookings(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetCustomerBookingsQuery(id), ct));
}

public record UpdateCustomerRequest(string FullName, string? PhoneNumber);
