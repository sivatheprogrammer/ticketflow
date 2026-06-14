# ADR-011: Extract Identity as a Separate Service

## Status
Accepted

## Date
2026-06-14

## Context
TicketFlow started with a monolithic approach where customer/identity data
lived in the main TicketFlow database. As the system grows toward a
modular architecture, customer identity concerns (authentication, provisioning,
profile management) need to be independently deployable and scalable.

In Phase 2, Entra ID was integrated for authentication. The local Customer
entity was used to map Entra Object IDs to internal GUIDs. This worked for
a monolith but creates tight coupling as more services are added.

## Decision
Extract identity/customer management into a separate deployable service:
`TicketFlow.Identity.Api` with its own database `TicketFlowIdentityDb`.

The main `TicketFlow.Api` communicates with the Identity service via HTTP
using a typed `HttpClient` (`IdentityServiceClient`) rather than direct
database access.

An `IIdentityService` interface is defined in `TicketFlow.Application` to
keep the application layer decoupled from the HTTP implementation detail.

## Consequences

### Positive
- Identity concerns are independently deployable and scalable
- Clear bounded context separation — identity data owned by one service
- Swapping identity providers (Entra → Okta) requires changes only in
  Identity service, not across the entire codebase
- Foundation for future microservices extraction

### Negative
- Network call overhead on every customer provisioning request
- Two databases to manage and migrate
- More complex local development setup (multiple startup projects)

### Neutral
- `ProvisionCustomerHandler` now delegates to `IIdentityService` instead
  of querying `_db.Customers` directly
- `CreateBookingHandler` no longer validates customer existence locally —
  trusts the CustomerId returned by Identity service

## Alternatives Considered
- **Keep in monolith:** Simpler but doesn't demonstrate service extraction
- **Shared database:** Anti-pattern — defeats the purpose of separation
- **gRPC instead of HTTP:** More efficient but adds complexity; HTTP chosen
  for simplicity at this stage; can be revisited in Phase 6