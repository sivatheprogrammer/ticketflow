using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketFlow.Application.Bookings.Commands;
using TicketFlow.Application.Bookings.Queries;
using TicketFlow.Application.Customers;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All booking endpoints require authentication
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BookingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetBookingByIdQuery(id), ct));

    /// <summary>
    /// Reserve tickets (15-minute hold).
    /// Phase 2: CustomerId is extracted from the JWT 'oid' claim — no longer trusted from request body.
    /// The command still accepts CustomerId in the body for Swagger testing but the controller overrides it.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateBookingResult>> Create(
        [FromBody] CreateBookingRequest req, CancellationToken ct)
    {
        // Extract the authenticated user's Entra ID object ID from the JWT token
        // This replaces the hardcoded DEMO_CUSTOMER_ID from Phase 1
        var entraObjectId = User.FindFirstValue("oid")
            ?? User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? throw new UnauthorizedAccessException("Could not extract user identity from token.");

        // Provision or retrieve the local Customer record for this Entra user
        var customerId = await _mediator.Send(
            new ProvisionCustomerCommand(entraObjectId, GetUserEmail(), GetUserName()), ct);

        var cmd = new CreateBookingCommand(customerId, req.EventId, req.Tier, req.Quantity);
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

    [HttpGet("my")]
    public async Task<ActionResult<List<BookingDto>>> MyBookings(CancellationToken ct)
    {
        var entraObjectId = User.FindFirstValue("oid")
            ?? User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? throw new UnauthorizedAccessException("Could not extract user identity from token.");

        var customerId = await _mediator.Send(
            new ProvisionCustomerCommand(entraObjectId, GetUserEmail(), GetUserName()), ct);

        return Ok(await _mediator.Send(new GetCustomerBookingsQuery(customerId), ct));
    }

    // Helper methods to extract claims from JWT
    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("preferred_username")
        ?? "unknown@example.com";

    private string GetUserName() =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue("name")
        ?? "Unknown User";
}

// Separate request body — CustomerId removed, now comes from JWT
public record CreateBookingRequest(
    Guid EventId,
    int Quantity,
    TicketFlow.Domain.Enums.TicketTier Tier
);
