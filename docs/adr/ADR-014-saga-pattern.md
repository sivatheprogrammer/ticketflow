# ADR-014: Choreography-based Saga for Booking Flow

## Status
Accepted

## Date
2026-07-01

## Context
Phase 5 introduced async messaging via Azure Service Bus. The booking flow
involves multiple steps that must be coordinated across services:

1. Create booking (Pending)
2. Confirm booking (Confirmed)
3. Mark tickets as Booked

In a distributed system, these steps can fail independently. A Saga pattern
is needed to ensure consistency without distributed transactions.

Two Saga approaches exist:
- **Choreography** — services react to events; no central coordinator
- **Orchestration** — a central Saga orchestrator directs each step

## Decision
Use **Choreography-based Saga** across two Service Bus queues.

### Flow

```
POST /api/bookings
  → BookingCreatedEvent published to booking-created queue
  → BookingCreatedConsumer: calls ConfirmBookingCommand + publishes BookingConfirmedEvent
  → BookingConfirmedConsumer: logs Saga completion (future: email, payment, analytics)
```

## Consequences

### Positive
- No central coordinator — simpler to implement and deploy
- Each consumer is independently deployable
- Naturally fits the existing Service Bus infrastructure
- Easy to extend — add new consumers to booking-confirmed queue without
  changing existing code

### Negative
- Harder to track overall Saga state — no single place shows the full flow
- Compensation logic (rollback) is spread across consumers
- Debugging requires correlating logs across multiple consumers

### Neutral
- Choreography is the right fit at this scale; Orchestration (e.g. Azure
  Durable Functions) would be considered if the flow grows beyond 4-5 steps

## Alternatives Considered
- **Orchestration via Azure Durable Functions:** More visibility into Saga
  state but adds infrastructure complexity; overkill for current flow
- **Synchronous confirmation:** Simpler but couples services tightly and
  loses async benefits
- **MassTransit Saga:** Feature-rich but adds a heavy dependency;
  raw Service Bus keeps the code portable

## Known Gotchas (Lessons Learned)

### 1. Don't double-confirm tickets
`ConfirmBookingCommand` already calls `booking.Confirm()` which marks all
tickets as `Booked`. The `BookingConfirmedConsumer` must NOT call
`ticket.Confirm()` again — it will throw `BusinessRuleViolationException`
because tickets are already in `Booked` state, not `Reserved`.

### 2. BackgroundService needs IServiceScopeFactory
`IMediator` and `IEventPublisher` are scoped/singleton services. Inside a
`BackgroundService`, you must use `IServiceScopeFactory` to create a scope
per message:

```csharp
using var scope = _scopeFactory.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
```

### 3. Message abandonment on failure
Both consumers use `AutoCompleteMessages = false` and explicitly call
`AbandonMessageAsync` on failure. This returns the message to the queue
for retry rather than losing it silently.
