using MediatR;
using TicketFlow.Application.Common.Interfaces;

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
///
/// Phase 5 update: DB lookup replaced with IdentityServiceClient HTTP call.
/// Customer data now lives in TicketFlow.Identity.Api (separate service + DB).
/// </summary>
public record ProvisionCustomerCommand(
    string EntraObjectId,
    string Email,
    string FullName
) : IRequest<Guid>;

public class ProvisionCustomerHandler : IRequestHandler<ProvisionCustomerCommand, Guid>
{
    private readonly IIdentityService _identityService;

    public ProvisionCustomerHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Guid> Handle(ProvisionCustomerCommand req, CancellationToken ct)
    {
        var customer = await _identityService.ProvisionCustomerAsync(
            req.EntraObjectId, req.Email, req.FullName, ct);

        return customer.Id;
    }
}