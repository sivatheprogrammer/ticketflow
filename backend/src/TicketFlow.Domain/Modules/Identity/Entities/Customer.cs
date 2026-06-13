using TicketFlow.Domain.Common;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Modules.Events.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// The external identity provider's user ID.
    /// Phase 2: Entra ID Object ID (oid claim).
    /// If we swap to Okta in the bonus branch, this stores the Okta sub claim.
    /// Keeping this as a string (not Guid) because different IDPs use different formats.
    /// See ADR-007.
    /// </summary>
    public string? ExternalId { get; private set; }

    private Customer() { } // EF Core

    public Customer(string fullName, string email,
        string? phoneNumber = null, string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException(
                "CUSTOMER_NAME_REQUIRED", "Customer name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BusinessRuleViolationException(
                "CUSTOMER_EMAIL_INVALID", "A valid email is required.");

        FullName = fullName;
        Email = email.ToLowerInvariant();
        PhoneNumber = phoneNumber;
        ExternalId = externalId;
    }

    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        MarkUpdated();
    }

    public void LinkExternalId(string externalId)
    {
        ExternalId = externalId;
        MarkUpdated();
    }
}
