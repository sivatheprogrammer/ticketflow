# 📋 Project Context — TicketFlow

> **Purpose of this document:** A single source of truth that brings any new collaborator (human or AI) up to speed in under 5 minutes. Update this at the end of every phase. When starting a new chat with an AI assistant, paste this entire file as your opening message.

---

## 🎯 The Project in One Paragraph

TicketFlow is a portfolio project I'm building to demonstrate .NET Architect-level skills for my job search. It's a production-style event ticketing platform (think Ticketmaster / BookMyShow) built **incrementally across 6 phases**, where each phase introduces one major architectural concern. The goal is not just working software — it's a Git history and ADR trail that reads as an architect's decision journal.

**Tech stack:** ASP.NET Core 8 (Clean Architecture + CQRS) backend, Angular 20 LTS (standalone components + signals) frontend, SQL Server, evolving toward Docker → Azure → modular monolith with one extracted service → Azure Container Apps (primary) + AKS (demonstration).

---

## 🛣️ The 6-Phase Roadmap

| Phase | Focus | Skills Demonstrated | Status | Completed |
|-------|-------|---------------------|--------|-----------|
| **1** | Monolith Foundation | .NET Core, Web API, EF Core, SQL Server, Angular | 🚧 In Progress | — |
| **2** | Authentication & Authorization | OAuth 2.0 / OIDC via Microsoft Entra ID, JWT, PKCE, role-based access (+ Okta bonus branch) | ⏳ Planned | — |
| **3** | Containerization + Distributed Caching | Docker, docker-compose, multi-stage builds, Redis (distributed locking + cache-aside), nginx load balancer | ⏳ Planned | — |
| **4** | Cloud Deployment | Azure App Service, Azure SQL, Azure DevOps, Bicep | ⏳ Planned | — |
| **5** | Modular Monolith + Identity Service Extraction | Module boundaries (DDD), API Gateway (YARP), Azure Service Bus, Saga pattern, service-to-service auth | ⏳ Planned | — |
| **6** | Container Orchestration (ACA + AKS) | Azure Container Apps (primary), AKS + Helm (demonstration), KEDA autoscaling | ⏳ Planned | — |

> **How to update:** When a phase ships, change status to ✅ Complete, fill in the date, and tag a Git release (`v0.1` for Phase 1, etc.).

---

## 📍 Where I Am Right Now

**Current Phase:** 1 — Monolith Foundation
**Current Sub-task:** [e.g., "Building out the EventsController and corresponding Angular event-list page"]
**Last commit:** [paste latest commit hash + message here when updating]
**Blocking issues:** [anything stuck — e.g., "EF Core migration failing on Booking → Tickets relationship"]

---

## 🧠 Key Architectural Decisions (So Far)

Each decision links to a full ADR in `/docs/adr`. This list is the TL;DR.

| # | Decision | Why | Phase |
|---|----------|-----|-------|
| ADR-001 | Clean Architecture (4 projects: Domain → Application → Infrastructure → API) | Domain testability + clean future split into microservices | 1 |
| ADR-002 | CQRS with MediatR | Clear command/query separation, easy to unit test handlers | 1 |
| ADR-003 | Angular standalone components + signals | Modern Angular, no NgModule baggage | 1 |
| ADR-004 | 15-min reservation hold via in-process BackgroundService | Simple in Phase 1; will migrate to Azure Service Bus scheduled messages in Phase 5 | 1 |
| ADR-005 | Angular 20 LTS (not 21) | Stable ecosystem, mature library compatibility, planned future migration to 21+ | 1 |
| ADR-006 | Microsoft Entra ID (primary) + Okta (bonus demo) for OAuth | Best resume keyword density for .NET Architect roles, Azure ecosystem coherence, free at production scale | 2 |
| ADR-010 *(planned)* | Redis for distributed locking and read-through caching | Multi-instance API deployment requires distributed coordination; fulfills the future-need flagged in ADR-004 | 3 |
| ADR-011 *(planned)* | Modular Monolith + one extracted Identity service (not full microservices) | Demonstrates architectural restraint AND decomposition skill; 90% of microservices learning at 50% of the time investment | 5 |
| ADR-013 *(planned)* | Azure Container Apps (primary) + AKS (demonstration track) | ACA fits the workload; AKS demonstrates Kubernetes operational skill; comparison ADR shows architectural judgment | 6 |

---

## 🏗️ Domain Model Cheat Sheet

Four core entities. Business rules live in the Domain layer:

- **Event** — name, dates, venue, category, status (Draft/Published/Cancelled/Completed)
- **Venue** — name, address, city, capacity
- **Ticket** — tier (General/Premium/VIP), price, status (Available/Reserved/Booked/Used/Cancelled), 15-min `ReservedUntil`
- **Booking** — customer, list of tickets, total amount (server-calculated), status (Pending/Confirmed/Cancelled/Refunded/Expired)
- **Customer** — minimal in Phase 1; becomes a real OAuth user in Phase 2

**Critical business rules to remember:**
- Booking can't be created if any requested ticket isn't `Available`
- Reserving starts a 15-minute hold; auto-released if not confirmed
- Total amount calculated server-side, never trusted from client
- Event can't be `Published` without ≥1 venue and ≥1 ticket tier
- Max 6 tickets per booking per event (anti-scalping)
- Confirmed bookings can't be cancelled after event starts

---

## 📁 Repository Structure

```
ticketflow/
├── backend/
│   ├── src/
│   │   ├── TicketFlow.Domain/          # Entities, business rules, NO external deps
│   │   ├── TicketFlow.Application/     # CQRS handlers, DTOs, validators
│   │   ├── TicketFlow.Infrastructure/  # EF Core, background jobs
│   │   └── TicketFlow.Api/             # Controllers, middleware, DI
│   └── tests/
├── frontend/                           # Angular 18 SPA
│   └── src/app/
│       ├── core/                       # Services, interceptors, models
│       ├── shared/                     # Reusable components
│       └── features/                   # events, bookings, my-bookings
├── docs/
│   ├── adr/                            # Architecture Decision Records
│   ├── diagrams/                       # C4 diagrams
│   └── PROJECT_CONTEXT.md              # 👈 You are here
└── .github/workflows/                  # CI pipelines
```

---

## 🔗 Important Links

- **GitHub Repo:** [paste URL once pushed]
- **Live Demo:** [paste URL after Phase 4 deploys]
- **Swagger:** [paste URL after Phase 4]
- **My LinkedIn:** [for future commits where I share progress]

---

## 📜 Phase Retrospectives

> Add a short retrospective at the end of each phase. Hiring managers love seeing reflection — it signals seniority.

### Phase 1 — Monolith Foundation
- **What I built:** [fill in when complete]
- **What surprised me:** [e.g., "Reservation expiry was harder than expected — leader-election would matter even at small scale"]
- **What I'd do differently:** [e.g., "Would consider Vertical Slice Architecture for solo work"]
- **Time spent:** [vs. planned 1-2 weeks]

### Phase 2 — OAuth via Microsoft Entra ID
*To be completed.*

### Phase 3 — Containerization + Redis (Distributed Locking & Caching)
*To be completed.*

### Phase 4 — Azure + DevOps
*To be completed.*

### Phase 5 — Modular Monolith + Identity Service Extraction
*To be completed.*

### Phase 6 — Container Orchestration (ACA primary + AKS demonstration)
*To be completed.*

---

## 🤖 Instructions for Future AI Conversations

If you're an AI assistant reading this to help me continue the project, here's what you need to know:

1. **Read this entire file first** before suggesting anything. It contains the roadmap, current state, and key decisions.
2. **Don't suggest skipping ahead.** This project is intentionally incremental — Phase 5 things shouldn't appear in Phase 2.
3. **Respect existing ADRs.** If you disagree with one, say so explicitly and propose a new ADR rather than silently changing direction.
4. **Code style:** C# 12 with nullable enabled, file-scoped namespaces, primary constructors where they help. Angular standalone components, signals over RxJS for component state, RxJS for HTTP streams.
5. **My current goal:** [update this line each session, e.g., "Finish Phase 1 EventsController + corresponding Angular event-details page"]
6. **My experience level:** Comfortable with .NET and Angular, learning the architect-level patterns (DDD tactics, distributed systems, cloud-native deployment).

---

## 📝 Open Questions / Parking Lot

> Things I want to think about but aren't urgent. Useful for future architectural conversations.

- [ ] Should the Booking aggregate emit domain events for future Saga refactor in Phase 5?
- [ ] Read model vs write model split — introduce in Phase 1 or wait until microservices?
- [ ] Move from Mapster to plain mapping methods? (less magic, more explicit)
- [ ] At what phase do I introduce a feature flag system?
- [ ] Should I add OpenTelemetry from Phase 1, or wait until microservices?

---

*Last updated: [DATE] — after completing [PHASE/MILESTONE]*
