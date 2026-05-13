using TicketFlow.Domain.Common;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }

    private Customer() { } // EF Core

    public Customer(string fullName, string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("CUSTOMER_NAME_REQUIRED", "Customer name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BusinessRuleViolationException("CUSTOMER_EMAIL_INVALID", "A valid email is required.");

        FullName = fullName;
        Email = email.ToLowerInvariant();
        PhoneNumber = phoneNumber;
    }

    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        MarkUpdated();
    }
}
