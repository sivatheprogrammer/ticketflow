# ADR-001: Clean Architecture for the Backend

**Status:** Accepted
**Date:** Phase 1
**Deciders:** Project Author

## Context

TicketFlow is intended to evolve from a monolith (Phase 1) into a set of microservices (Phase 5). Whatever architecture I choose for the monolith must support that future split without requiring a rewrite. It also needs to make business rules testable in isolation, since the domain logic (reservation expiry, anti-scalping, ticket state transitions) is the most complex and error-prone part of the system.

## Decision

Adopt Clean Architecture (Onion / Hexagonal) with four layers:

- **Domain** — Entities, value objects, domain exceptions. No external dependencies.
- **Application** — Use cases as CQRS handlers (MediatR), DTOs, validators, interface abstractions.
- **Infrastructure** — EF Core, background services, external integrations.
- **API** — Controllers, middleware, DI composition root.

Dependencies flow inward only: API → Infrastructure → Application → Domain.

## Consequences

**Positive:**
- Domain is fully unit-testable without a database or web host.
- Swapping SQL Server for another store in Phase 6 (e.g. Cosmos for the read model) requires changes only in Infrastructure.
- When services are split in Phase 5, each can keep this same internal structure — the slicing happens at the Application/feature boundary.

**Negative:**
- More projects and ceremony than a single-project Web API. Justified here by the complexity of the domain rules.
- New contributors need a brief orientation to the dependency rule.

## Alternatives considered

- **Single-project Web API.** Faster initial setup but blurs business rules into controllers. Rejected because the reservation/booking logic deserves its own home.
- **Vertical Slice Architecture.** Strong contender. Ultimately deferred — Clean Architecture pairs better with the explicit Domain/Application/Infrastructure split that hiring reviewers expect to see in a .NET Architect portfolio.
