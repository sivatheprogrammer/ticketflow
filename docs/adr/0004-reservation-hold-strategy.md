# ADR-004: 15-Minute Reservation Hold via Background Service

**Status:** Accepted (Phase 1) — to be revisited in Phase 5
**Date:** Phase 1

## Context

When a customer initiates a booking, the chosen tickets must be held exclusively for them for 15 minutes while they complete payment. If they don't confirm in time, the tickets must return to the `Available` pool so other customers can book them.

This is the same problem Ticketmaster, Eventbrite, and BookMyShow solve. There is no single right answer — each option trades off complexity, latency, and resource cost.

## Options considered

1. **Polling background service** (chosen for Phase 1)
   A `BackgroundService` runs every minute, scans for expired `Pending` bookings, and releases their tickets.
   - ✅ Trivial to implement and reason about
   - ✅ Works in a single-instance monolith without additional infrastructure
   - ❌ Up to 60s delay before tickets are released
   - ❌ Doesn't scale to multiple instances without a leader-election mechanism

2. **Scheduled message on Azure Service Bus** (planned for Phase 5)
   When a booking is created, schedule a message to deliver in 15 minutes. Handler checks if the booking is still `Pending` and expires it.
   - ✅ Precise timing (~15 min, not "next poll")
   - ✅ Scales horizontally — any consumer can process
   - ❌ Requires Service Bus infrastructure
   - ❌ Adds complexity inappropriate for a single-instance Phase 1

3. **SQL Server Agent / Hangfire**
   - ✅ Battle-tested
   - ❌ Hangfire adds another moving part; SQL Agent isn't available on Azure SQL.

## Decision

Use option 1 for Phase 1. Re-evaluate when introducing Service Bus in Phase 5 — at that point the polling job will be replaced with option 2, which becomes natural infrastructure once we have a message broker.

## Consequences

The 60-second worst-case latency is acceptable for Phase 1 because:
- The user-visible behavior is just a slightly delayed seat release
- No correctness issue — a held seat cannot be double-booked because of the `TicketStatus` state machine

## Future-proofing

The `Booking.Expire()` domain method is already idempotent and side-effect-free, so the migration from polling to scheduled messages will not require domain-layer changes — only infrastructure.
