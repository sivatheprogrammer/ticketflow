# ADR-012: YARP API Gateway over Nginx

## Status
Accepted

## Date
2026-06-14

## Context
In Phase 3, nginx was used as a reverse proxy and load balancer in the
Docker Compose setup, routing traffic between two replicas of `TicketFlow.Api`.

With Phase 5 introducing `TicketFlow.Identity.Api` as a separate service,
clients would need to know the URLs of multiple services. This is not
scalable and exposes internal service topology to external clients.

A single entry point (API Gateway) is needed to:
- Route requests to the correct service
- Hide internal service topology from clients
- Provide a single place for cross-cutting concerns (auth, logging, rate limiting)

## Decision
Replace nginx with **YARP (Yet Another Reverse Proxy)** as the API Gateway,
implemented as a new `TicketFlow.Gateway` project.

Routing rules:
- `/api/identity/**` → `TicketFlow.Identity.Api`
- `/api/**` → `TicketFlow.Api`

## Consequences

### Positive
- Single entry point for all external traffic (Angular, Postman, future clients)
- Routing configuration in `appsettings.json` — no separate nginx.conf
- Extensible in C# — custom middleware, auth, rate limiting can be added
- Works in all environments (local, Docker, Azure) unlike nginx which was
  Docker-only in our setup
- Load balancing supported natively by adding multiple destinations

### Negative
- Additional service to deploy and manage
- Adds latency (one extra network hop)
- Port management in local dev (3 services now running)

### Neutral
- nginx config in `docker-compose.yml` from Phase 3 is now superseded
- Angular frontend will point to Gateway URL instead of API directly
- Future services (Payment, Notification) added as new routes in config

## Alternatives Considered
- **Keep nginx:** Docker-only, config in separate file, not extensible in C#
- **Azure API Management:** Powerful but expensive and overkill for this stage
- **Ocelot:** Popular .NET gateway but less actively maintained than YARP
- **No gateway:** Each client knows all service URLs — not scalable