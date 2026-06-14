namespace TicketFlow.Application.Common.Interfaces;

public record ProvisionedCustomer(Guid Id, string ExternalId, string Email, string FullName);

public interface IIdentityService
{
    Task<ProvisionedCustomer> ProvisionCustomerAsync(
        string externalId, string email, string fullName, CancellationToken ct = default);
}