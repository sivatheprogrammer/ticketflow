using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Modules.Identity;
using TicketFlow.Identity.Api.Persistence;

namespace TicketFlow.Identity.Api.Controllers;

[ApiController]
[Route("api/identity/customers")]
public class CustomersController : ControllerBase
{
    private readonly IdentityDbContext _db;

    public CustomersController(IdentityDbContext db)
    {
        _db = db;
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] ProvisionCustomerRequest request)
    {
        var existing = await _db.Customers
            .FirstOrDefaultAsync(c => c.ExternalId == request.ExternalId);

        if (existing is not null)
            return Ok(new { existing.Id, existing.ExternalId, existing.Email, existing.FullName });

        var customer = new Customer(
            fullName: request.FullName,
            email: request.Email,
            externalId: request.ExternalId);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            new { customer.Id, customer.ExternalId, customer.Email, customer.FullName });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return Ok(new { customer.Id, customer.ExternalId, customer.Email, customer.FullName });
    }

    [HttpGet("by-external/{externalId}")]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.ExternalId == externalId);
        if (customer is null) return NotFound();
        return Ok(new { customer.Id, customer.ExternalId, customer.Email, customer.FullName });
    }
}

public record ProvisionCustomerRequest(string ExternalId, string Email, string FullName);
