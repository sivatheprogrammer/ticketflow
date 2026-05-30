# ADR-007: First-Login User Provisioning Strategy

**Status:** Accepted
**Date:** Phase 2
**Deciders:** Project Author

## Context

TicketFlow's domain model has a `Customer` entity with a `Guid` primary key. Microsoft Entra ID identifies users with an Object ID (`oid` claim) — a string GUID in a different format. When an authenticated user makes their first API call, we need to bridge these two identities.

Three options were considered:

1. **Use Entra `oid` directly as the Customer PK** — make `Customer.Id` a string
2. **Provision on first login** — create a local Customer on the first API call, store the `oid` as `ExternalId`
3. **No local Customer entity** — call Entra's Graph API for user info on every request

## Decision

Use **Option 2: first-login provisioning** via the `ProvisionCustomerCommand`.

On every authenticated request, the API:
1. Extracts the `oid` claim from the JWT
2. Looks up `Customer` by `ExternalId == oid`
3. If found → returns the local `Customer.Id`
4. If not found → creates a new `Customer` and returns the new `Id`

The `ProvisionCustomerCommand` is idempotent — calling it multiple times with the same `oid` is safe.

## Reasoning

**Keeps the domain model stable.** The domain layer uses `Guid` PKs throughout. `Booking`, `Ticket`, and all aggregates reference `Customer.Id` as a `Guid`. Changing this to a string would ripple through the entire domain — a disproportionate change for an infrastructure concern.

**Identity provider portability.** The `ExternalId` field is a string, not a Microsoft-specific type. When the Okta bonus branch swaps the identity provider, local `Customer` records remain intact — only the `ExternalId` values change. Bookings, tickets, and order history are preserved.

**Avoids Graph API dependency.** Calling Microsoft Graph on every request adds latency, an external dependency, and additional permissions to manage. Local provisioning is faster and simpler.

**Standard pattern in production systems.** First-login provisioning is how most enterprise systems bridge external identity providers with local domain models. It's a recognizable pattern in architect interviews.

## Consequences

**Positive:**
- Domain model unchanged — all existing code continues to work
- Provider swap (Entra → Okta) requires only `ExternalId` remapping, not a domain migration
- Fast — single indexed DB lookup per authenticated request
- `Customer` entity enriched with local profile data (name, phone) independently of Entra

**Negative:**
- Slight latency on every authenticated request (one extra DB read)
- Customer records can get out of sync with Entra if the user changes their name/email in Entra (mitigated by updating on each login in a future phase)
- Seed data customers (Alex Rivera, Jordan Lee) have no `ExternalId` — they're Phase 1 test data, not real authenticated users

## Sparse Index

The `ExternalId` column has a filtered unique index (`WHERE ExternalId IS NOT NULL`). This allows:
- Seed data customers with no `ExternalId` (multiple nulls in a unique column)
- Real authenticated customers with unique `ExternalId` values
- Fast lookup by `oid` on every request
