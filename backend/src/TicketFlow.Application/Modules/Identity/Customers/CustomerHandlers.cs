using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Modules.Events.Entities;
using TicketFlow.Domain.Modules.Identity;

namespace TicketFlow.Application.Customers;

// --- DTOs ---
public record CustomerDto(Guid Id, string FullName, string Email, string? PhoneNumber);

// --- Queries ---
public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IApplicationDbContext _db;
    public GetCustomerByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery req, CancellationToken ct)
    {
        var customer = await _db.Customers.FindAsync(new object[] { req.Id }, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), req.Id);
        return new CustomerDto(customer.Id, customer.FullName, customer.Email, customer.PhoneNumber);
    }
}

// --- Commands ---
public record CreateCustomerCommand(string FullName, string Email, string? PhoneNumber) : IRequest<Guid>;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    public CreateCustomerHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateCustomerCommand req, CancellationToken ct)
    {
        // In Phase 1, check email uniqueness manually.
        // In Phase 2, this is replaced by Entra ID provisioning.
        var exists = await _db.Customers.AnyAsync(c => c.Email == req.Email.ToLowerInvariant(), ct);
        if (exists)
            throw new Domain.Exceptions.BusinessRuleViolationException(
                "CUSTOMER_EMAIL_EXISTS", "A customer with this email already exists.");

        var customer = new Customer(req.FullName, req.Email, req.PhoneNumber);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer.Id;
    }
}

public record UpdateCustomerCommand(Guid Id, string FullName, string? PhoneNumber) : IRequest<Unit>;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    public UpdateCustomerHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateCustomerCommand req, CancellationToken ct)
    {
        var customer = await _db.Customers.FindAsync(new object[] { req.Id }, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), req.Id);
        customer.UpdateProfile(req.FullName, req.PhoneNumber);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
