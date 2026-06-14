using System.Net.Http.Json;
using TicketFlow.Api.Infrastructure;

namespace TicketFlow.Api.Infrastructure;

public record CustomerResponse(
    Guid Id,
    string ExternalId,
    string Email,
    string FullName);

public record ProvisionCustomerRequest(
    string ExternalId,
    string Email,
    string FullName);

public class IdentityServiceClient
{
    private readonly HttpClient _httpClient;

    public IdentityServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerResponse?> ProvisionCustomerAsync(
        string externalId, string email, string fullName)
    {
        var request = new ProvisionCustomerRequest(externalId, email, fullName);
        var response = await _httpClient.PostAsJsonAsync(
            "/api/identity/customers/provision", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>();
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<CustomerResponse>(
            $"/api/identity/customers/{id}");
    }

    public async Task<CustomerResponse?> GetCustomerByExternalIdAsync(string externalId)
    {
        return await _httpClient.GetFromJsonAsync<CustomerResponse>(
            $"/api/identity/customers/by-external/{externalId}");
    }
}