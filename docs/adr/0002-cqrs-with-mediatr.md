# ADR-002: CQRS with MediatR for the Application Layer

**Status:** Accepted
**Date:** Phase 1
**Deciders:** Project Author

## Context

The Application layer needs a way to organize use cases — "create a booking," "publish an event," "list events for a city." There are several mainstream patterns in the .NET ecosystem:

1. **Service classes** — `BookingService`, `EventService`, etc. with one method per use case
2. **CQRS via MediatR** — each use case is its own command/query class with a dedicated handler
3. **Vertical Slice Architecture** — each feature owns everything it needs in a single folder
4. **Minimal APIs with handler delegates** — simple request → handler functions

The choice affects how testable, discoverable, and evolvable the use cases will be — and how easily they can be split across microservices in Phase 5.

## Decision

Adopt **CQRS (Command Query Responsibility Segregation) with MediatR** as the application layer pattern.

Each use case is a single class — e.g., `CreateBookingCommand` and `CreateBookingHandler` — co-located in a feature folder under `Application/`. Controllers are thin: they construct the command from the HTTP request and dispatch it via `IMediator.Send()`.

## Reasoning

**Each use case is independently visible and testable.** Open `Application/Bookings/Commands/CreateBookingCommand.cs` and you see exactly what the booking creation flow does — no hunting through a 600-line `BookingService` class.

**Cross-cutting concerns become trivial.** MediatR's pipeline behaviors let us add validation (FluentValidation), logging, transactions, and authorization as separate concerns wrapped around every handler. We get this for free in later phases without touching individual handlers.

**Controllers stay thin.** Controllers don't contain business logic — they delegate. This makes the API layer almost trivially testable and means the same handler can later be invoked from a background job, a gRPC endpoint, or a message consumer with zero changes.

**Maps cleanly to microservices later.** When Phase 5 splits the monolith, each handler is already a self-contained unit of work. We can move `Application/Events/*` into a new `EventService` project with minimal refactoring — the dependency surface is already small.

**Strong .NET community signal.** CQRS + MediatR is the dominant pattern in clean-architecture .NET projects. Hiring managers reviewing the repo will recognize it immediately and know what questions to ask.

## Consequences

**Positive:**
- One file per use case → easy navigation, easy code review
- Pipeline behaviors give us validation, logging, transactions for free
- Each handler is a unit-testable boundary
- Future microservice extraction is mechanical, not architectural

**Negative:**
- More files than a service-class approach (one command + one handler per use case)
- Some boilerplate per command — but it's predictable boilerplate
- Slight indirection at the controller layer (mediator dispatch vs direct service call)
- New contributors need to understand the MediatR convention

## Alternatives Considered

**Service classes (rejected).** Familiar to most .NET developers, but tend to grow into "god services" with 30+ methods. Cross-cutting concerns end up duplicated across services. Microservice extraction becomes a major refactor instead of a move operation.

**Vertical Slice Architecture (deferred).** Genuinely strong contender — arguably better fit for a small project. Rejected here because Clean Architecture + CQRS is a more recognizable pattern in .NET Architect interviews, and the explicit Domain/Application/Infrastructure split is what hiring reviewers expect to see. Worth revisiting in a future project.

**Minimal APIs with delegate handlers (rejected).** Great for trivial APIs, but the booking/reservation domain is complex enough to deserve named command/handler types with explicit dependencies. Minimal APIs would push too much logic into Program.cs.

## Notes

- MediatR moved to a paid commercial license in late 2024 for new projects — the pre-license version (12.x) remains free for use, which is what we're targeting. **A future ADR will revisit this if we hit the commercial threshold or want to migrate to alternatives like FastEndpoints, Wolverine, or hand-rolled mediator.**
- This decision pairs naturally with FluentValidation for command validation (added in the Application project).
