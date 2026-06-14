using TicketFlow.Application.Common.Interfaces;

namespace TicketFlow.Api.Infrastructure;

public class IdentityServiceAdapter : IIdentityService
{
    private readonly IdentityServiceClient _client;

    public IdentityServiceAdapter(IdentityServiceClient client)
    {
        _client = client;
    }

    public async Task<ProvisionedCustomer> ProvisionCustomerAsync(
        string externalId, string email, string fullName, CancellationToken ct = default)
    {
        var result = await _client.ProvisionCustomerAsync(externalId, email, fullName);
        return new ProvisionedCustomer(result!.Id, result.ExternalId, result.Email, result.FullName);
    }
}