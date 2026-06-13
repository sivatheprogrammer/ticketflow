using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Modules.Events.Entities;

namespace TicketFlow.Application.Customers;

/// <summary>
/// First-login provisioning — maps an Entra ID user to a local Customer entity.
///
/// Pattern: On every authenticated request, we look up the Customer by their
/// Entra Object ID (oid claim). If they don't exist yet, we create them.
/// This is idempotent — calling it multiple times with the same oid is safe.
///
/// Why a local Customer entity instead of using Entra directly?
/// - Our domain model (Bookings, Tickets) needs a stable local CustomerId (Guid)
/// - Entra oid is a string; our domain uses Guid PKs
/// - If we ever swap identity providers (Entra → Okta), local Customer records
///   remain intact — only the ExternalId mapping changes
/// - See ADR-007 for the full decision record
/// </summary>
public record ProvisionCustomerCommand(
    string EntraObjectId,
    string Email,
    string FullName
) : IRequest<Guid>;

public class ProvisionCustomerHandler : IRequestHandler<ProvisionCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public ProvisionCustomerHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(ProvisionCustomerCommand req, CancellationToken ct)
    {
        // Look up existing customer by their Entra Object ID
        var existing = await _db.Customers
            .FirstOrDefaultAsync(c => c.ExternalId == req.EntraObjectId, ct);

        if (existing is not null)
            return existing.Id;

        // First login — provision a new Customer record
        var customer = new Customer(req.FullName, req.Email, externalId: req.EntraObjectId);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        return customer.Id;
    }
}
