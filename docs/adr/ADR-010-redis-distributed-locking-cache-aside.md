# ADR-010: Redis for Distributed Locking and Cache-Aside; Ticket Pre-Creation Trade-off

**Date:** May 2026  
**Status:** Accepted  
**Phase:** 3 — Containerization + Distributed Caching

---

## Context

Phase 3 introduced multi-replica API deployment (2 instances behind an nginx load balancer). This created two problems that in-process solutions can no longer solve:

1. **Race conditions on ticket reservations** — two replicas can simultaneously read a ticket as `Available` and both attempt to reserve it, causing double-booking.
2. **Redundant database reads** — every `GET /api/events` call hits SQL Server even though event data changes infrequently. Under load with multiple replicas, this multiplies unnecessarily.

Additionally, a question arose about whether pre-creating ticket rows is the right modeling choice for a general admission tier system.

---

## Decision 1: Redis for Distributed Locking

When a user reserves a ticket, the API acquires a Redis lock keyed on the ticket's ID (`lock:ticket:{ticketId}`) before reading and updating the ticket row. The lock has a TTL matching the reservation hold window (30 seconds for the lock acquisition; the reservation itself holds for 15 minutes in the DB).

**Why Redis over alternatives:**
- **In-process locks** (`SemaphoreSlim`) don't work across replicas — each instance has its own memory
- **SQL Server row-level locking** works but adds latency and couples concurrency control to the persistence layer
- **Redis SETNX** ("set if not exists") is atomic by design — Redis processes commands single-threadedly, so two replicas racing to acquire the same lock will always have a clear winner with no additional synchronization needed

**TTL safety net:** If the lock-holding replica crashes before releasing, Redis automatically expires the lock after the TTL, preventing permanent deadlock.

---

## Decision 2: Redis Cache-Aside for Event Reads

Event listings (`GET /api/events`, `GET /api/events/{id}`) are read-heavy and change infrequently (only when an organizer publishes or cancels an event).

The cache-aside pattern is applied:
1. Check Redis for cached result
2. On cache hit → return immediately (no DB call)
3. On cache miss → query SQL Server → store result in Redis with a TTL → return

**Cache invalidation:** When an event is created, updated, or cancelled, the corresponding cache keys are deleted so the next read repopulates from the DB.

**TTL chosen:** 5 minutes for event listings. Acceptable staleness for a portfolio workload; in production this would be tuned based on update frequency and consistency requirements.

---

## Decision 3: Ticket Pre-Creation Trade-off (Known Limitation)

### Current approach
All tickets are pre-created as individual rows at seed/event-creation time. A VIP tier with 80 tickets = 80 rows in the `Tickets` table, each with its own `Id`, `Status`, and `ReservedUntil`.

### Why this was chosen
- Makes per-entity distributed locking concrete and teachable (`lock:ticket:{id}`)
- Demonstrates a richer domain lifecycle: `Available → Reserved → Booked → Used → Cancelled`
- Consistent with assigned-seating systems (Ticketmaster, airline seats) where each seat is a unique, individually trackable asset

### Known limitation
TicketFlow's current tiers (General / Premium / VIP) are **general admission** — there is no seat A14 vs seat A15. Pre-creating 1000 General admission rows is overkill for this model. A counter-based inventory approach would be more appropriate:

```
EventTicketTiers table
-----------------------------------------------
EventId  | Tier     | TotalCount | Reserved | Sold
event-1  | General  | 1000       | 3        | 247
event-1  | VIP      | 80         | 1        | 12
```

Reservation becomes an atomic counter decrement rather than a row-level lock. This scales better and avoids pre-populating thousands of rows.

### Why not changed now
- Refactoring the domain model mid-project would derail Phase 3's primary goals
- The current model is not wrong — it's the right model for assigned seating, just mismatched for general admission tiers
- The distributed locking demonstration is clearer with row-level locks than with atomic counter decrements

### Future recommendation
If this project were a real product, Phase 5 (modular monolith) would be the right time to revisit the `Tickets` aggregate and introduce a `TicketInventory` concept for general admission tiers, reserving the row-per-ticket model for future assigned seating features.

---

## Consequences

- Redis becomes a required infrastructure dependency from Phase 3 onward
- The `ReservationExpiryJob` background service (currently polling SQL Server) will be simplified — Redis TTL handles lock expiry; the job only needs to handle confirmed booking cleanup
- Cache-aside adds a small code overhead per read endpoint but significantly reduces DB load under multi-replica deployment
- The ticket pre-creation limitation is documented and accepted as a known trade-off for this portfolio context
