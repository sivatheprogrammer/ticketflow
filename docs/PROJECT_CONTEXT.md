# 📋 Project Context — TicketFlow

> **Purpose of this document:** A single source of truth that brings any new collaborator (human or AI) up to speed in under 5 minutes. Update this at the end of every phase. When starting a new chat with an AI assistant, paste this entire file as your opening message.

---

## 🎯 The Project in One Paragraph

TicketFlow is a portfolio project I'm building to demonstrate .NET Architect-level skills for my job search. It's a production-style event ticketing platform (think Ticketmaster / BookMyShow) built **incrementally across 6 phases**, where each phase introduces one major architectural concern. The goal is not just working software — it's a Git history and ADR trail that reads as an architect's decision journal.

**Tech stack:** ASP.NET Core 10 (Clean Architecture + CQRS) backend, Angular 20 LTS (standalone components + signals) frontend, SQL Server, Docker + Redis → Azure + Terraform → modular monolith with one extracted service → Azure Container Apps (primary) + AKS (demonstration).

---

## 🛣️ The 6-Phase Roadmap

| Phase | Focus | Skills Demonstrated | Status | Completed |
|-------|-------|---------------------|--------|-----------|
| **1** | Monolith Foundation | .NET 10, Web API, EF Core 10, SQL Server, Angular 20 | ✅ Complete | May 2026 |
| **2** | Authentication & Authorization | OAuth 2.0 / OIDC via Microsoft Entra ID | ✅ Complete | May 2026 |
| **3** | Containerization + Distributed Caching | Docker, docker-compose, multi-stage builds, Redis (distributed locking + cache-aside), nginx load balancer | ✅ Complete | May 2026 |
| **4** | Cloud Deployment + IaC | Azure App Service, Azure SQL, Terraform | ✅ Complete | June 2026 |
| **5** | Modular Monolith + Identity Service Extraction | Module boundaries (DDD), API Gateway (YARP), Azure Service Bus, Saga pattern, service-to-service auth | ⏳ Planned | — |
| **6** | Container Orchestration (ACA + AKS) | Azure Container Apps (primary), AKS + Helm (demonstration), KEDA autoscaling | ⏳ Planned | — |

> **How to update:** When a phase ships, change status to ✅ Complete, fill in the date, and tag a Git release (`v0.1` for Phase 1, etc.).

---

## 📍 Where I Am Right Now

**Current Phase:** 5 — Modular Monolith + Identity Service Extraction
**Current Sub-task:** Not started
**Last commit:** docs: add ADR-009 - Terraform over Bicep
**GitHub:** https://github.com/sivatheprogrammer/ticketflow
**Blocking issues:** None — Phase 4 fully shipped

**Live Azure URLs (when infra is up):**
- API: https://app-ticketflow-api-dev-siva04.azurewebsites.net
- Web: https://app-ticketflow-web-dev-siva04.azurewebsites.net
- Run `terraform apply -auto-approve` from `/terraform` to recreate; `terraform destroy -auto-approve` to tear down and stop billing.

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
| ADR-005b | Upgraded to .NET 10 (not .NET 8) | Machine had .NET 10 installed; dotnet-ef tool v10 installed automatically; all packages aligned to net10.0 | 1 |
| ADR-006 | Microsoft Entra ID (primary) + Okta (bonus demo) for OAuth | Best resume keyword density for .NET Architect roles, Azure ecosystem coherence, free at production scale | 2 |
| ADR-009 | Terraform over Bicep for IaC | Job market relevance, multi-cloud transferability, state management | 4 |
| ADR-010 | Redis for distributed locking and cache-aside; ticket pre-creation trade-off documented | Multi-instance API deployment requires distributed coordination; cache-aside reduces DB load under load | 3 |
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
│   │   ├── TicketFlow.Application/     # CQRS handlers, DTOs, validators, interfaces
│   │   ├── TicketFlow.Infrastructure/  # EF Core, Redis, background jobs, seed data
│   │   └── TicketFlow.Api/             # Controllers, middleware, DI
│   └── tests/
│       └── TicketFlow.Domain.Tests/    # Pure domain unit tests (no DB, no web host)
├── frontend/                           # Angular 20 SPA
│   ├── src/app/
│   │   ├── core/                       # Services, interceptors, models
│   │   ├── features/                   # events, bookings, my-bookings
│   │   └── layout/                     # Header, navigation
│   ├── Dockerfile                      # Multi-stage: Node build → nginx runtime
│   └── nginx.conf                      # SPA routing + /api/ proxy pass
├── nginx/
│   └── nginx-lb.conf                   # Round-robin load balancer across 2 API replicas
├── docker-compose.yml                  # Full stack: API×2, Angular, SQL Server, Redis, nginx LB
├── .env                                # SA_PASSWORD (gitignored)
├── docs/
│   ├── adr/                            # Architecture Decision Records (ADR-001 to ADR-010)
│   ├── diagrams/                       # C4 diagrams (planned)
│   └── PROJECT_CONTEXT.md              # 👈 You are here
└── .github/workflows/                  # CI pipelines (planned Phase 4)
```

---

## 🔗 Important Links

- **GitHub Repo:** https://github.com/sivatheprogrammer/ticketflow
- **Live Demo:** [paste URL after Phase 4 deploys]
- **Swagger:** [paste URL after Phase 4]

---

## 📜 Phase Retrospectives

### Phase 1 — Monolith Foundation ✅
- **What I built:** ASP.NET Core 10 Web API with Clean Architecture (Domain / Application / Infrastructure / API layers). CQRS via MediatR with handlers for Events, Venues, Customers, Bookings. EF Core 10 + SQL Server with entity configurations and seed data (3 venues, 4 events across Houston / Dallas / Austin). 15-minute ticket reservation hold via BackgroundService. Angular 20 SPA with standalone components, signals, lazy-loaded routes, typed HTTP services, error interceptor, and 4 feature pages (Events list, Event details, Booking detail with countdown timer, My Bookings).
- **What surprised me:** Machine had .NET 10 installed (not .NET 8 as scaffolded) — required updating all csproj TargetFramework and package versions. Angular Material theming API changed — `mat.$indigo-palette` no longer valid in newer versions; replaced with `mat.define-theme()`.
- **What I'd do differently:** Verify target .NET version on the machine before scaffolding. Add `.gitattributes` from day one. Pin `dotnet-ef` tool version explicitly.
- **Known limitations:** `DEMO_CUSTOMER_ID` was hardcoded — Phase 2 replaced this with the authenticated user's JWT `oid` claim.
- **Time spent:** ~1 full day

---

### Phase 2 — OAuth via Microsoft Entra ID ✅
- **What I built:** Microsoft Entra ID OAuth integration with PKCE flow. JWT validation in .NET API. First-login customer provisioning. Angular OIDC client with auth interceptor and route guards. DEMO_CUSTOMER_ID replaced with real JWT oid claim.
- **What surprised me:** `angular-auth-oidc-client` property name changed between versions — `clockSkewInSeconds` not valid, correct property is `maxIdTokenIatOffsetAllowedInSeconds`. Azure portal My APIs showing "No results" required manifest JSON edit.
- **Known limitations:** Header simplified — MatMenuModule removed to fix compilation issues.
- **Time spent:** ~2 sessions

---

### Phase 3 — Containerization + Redis ✅
- **What I built:** Multi-stage Dockerfile for .NET 10 API (SDK → aspnet runtime, non-root `app` user). Multi-stage Dockerfile for Angular 20 (Node 22 build → nginx 1.27 runtime). `docker-compose.yml` wiring 6 services: SQL Server 2022, Redis 7, 2× API replicas, nginx load balancer, Angular SPA. Redis cache-aside for `GET /api/events` and `GET /api/events/{id}` with 5-minute TTL. Redis distributed locking on ticket reservations (`lock:ticket:{id}`) protecting against double-booking across replicas. `appsettings.Docker.json` for container-specific config. `MigrateAsync()` moved outside `IsDevelopment()` guard so DB initializes in all environments.

- **What surprised me:** `.dockerignore` is critical — Windows-generated `obj/` folders with LocalDB paths in `project.assets.json` break Linux container builds entirely. The `aspnet:10.0` runtime image doesn't have `addgroup`/`adduser` — use the built-in `app` user instead. `appsettings.Docker.json` won't override the base file if the connection string key names don't match exactly (`"Default"` not `"DefaultConnection"`). `MigrateAsync()` was inside `IsDevelopment()` guard — had to move it out for Docker environment.

- **What I'd do differently:** Add `.dockerignore` from day one of any project. Verify connection string key names match across all `appsettings.*.json` files before containerizing.

- **Known limitations:** Redis distributed locking uses a simple SETNX pattern. For production, RedLock algorithm (across multiple Redis nodes) provides stronger guarantees. Cache invalidation on booking confirmation not yet implemented — event availability counts can be stale for up to 5 minutes. Ticket pre-creation model documented as a known trade-off in ADR-010.

- **Time spent:** ~1 full day (including Docker debugging)

---

### Phase 4 — Cloud Deployment + Terraform IaC ✅

- **What I built:** Terraform IaC provisioning Resource Group, ACR, App Service Plan (B2),
  2x Linux App Services (API + Angular, Docker-based), Azure SQL Server + Database (Basic, 2GB).
  Fixed suffix (`siva04`) for stable resource URLs across destroy/apply cycles.
  Docker images pushed to ACR. Full end-to-end flow verified: Angular → nginx →
  Azure App Service API → Azure SQL.

- **What surprised me:**
  - Azure SQL provisioning is region-restricted on free/trial subscriptions —
    `eastus`, `eastus2`, `westus2` all failed with `ProvisioningDisabled`;
    `centralus` worked.
  - Classic `azurerm_redis_cache` is being retired by Microsoft — descoped
    Redis from Azure entirely for Phase 4 (documented in ADR-009).
  - `azurerm` v4 provider breaking changes from v3: `enable_non_ssl_port`
    removed from Redis resource, `DOCKER_REGISTRY_SERVER_*` moved from
    `app_settings` into `site_config.application_stack`,
    `prevent_deletion_if_contains_resources = false` required for clean
    `terraform destroy`.
  - `ConnectionMultiplexer.Connect()` throws synchronously even with
    `abortConnect=false` in the connection string if Redis is unreachable —
    app crashed on every request until Redis config key name was fixed
    (`Redis__ConnectionString`, was previously mismatched/had trailing space).
  - Azure App Service routes by `Host` header — nginx reverse proxy to the
    API needed `proxy_ssl_server_name on;` and an explicit `Host` header
    override, or Azure returned 400 Bad Request before the API code ran.
  - `random_string.suffix` regenerates on every destroy/apply, breaking
    hardcoded URLs in `nginx.conf` and CORS config — switched to a fixed
    `var.suffix` ("siva04").

- **What I'd do differently:** Pick the fixed-suffix approach from the start.
  Verify Azure SQL region availability before writing Terraform.

- **Known limitations:** Entra ID OAuth redirect URI is still configured for
  `localhost:4200` — login on the Azure-hosted Angular app redirects back to
  localhost. Would need to add the Azure Web App URL as an additional redirect
  URI in the Entra ID App Registration to fix.

- **Time spent:** ~2 sessions

### Phase 5 — Modular Monolith + Identity Service Extraction
*To be completed.*

### Phase 6 — Container Orchestration (ACA primary + AKS demonstration)
*To be completed.*

---

## 🤖 Instructions for Future AI Conversations

If you're an AI assistant reading this to help me continue the project, here's what you need to know:

1. **Read this entire file first** before suggesting anything. It contains the roadmap, current state, and key decisions already made.
2. **Don't suggest skipping ahead.** This project is intentionally incremental — Phase 5 things shouldn't appear in Phase 3.
3. **Respect existing ADRs.** If you disagree with one, say so explicitly and propose a new ADR rather than silently changing direction.
4. **Runtime:** The project runs on **.NET 10** (not .NET 8). All csproj files target `net10.0`. EF Core packages are version `10.0.0`.
5. **Code style:** C# with nullable enabled, file-scoped namespaces, primary constructors where they help. Angular standalone components, signals over RxJS for component state, RxJS for HTTP streams. `@if` / `@for` control flow syntax.
6. **Docker:** Full stack runs via `docker compose up` from repo root. API image is `ticketflow-api`, Angular image is `ticketflow-web`. SA password in `.env` file (gitignored).
7. **My current goal:** Start Phase 4 — Azure cloud deployment with Terraform IaC.
8. **My experience level:** Comfortable with .NET and Angular, learning architect-level patterns (DDD, distributed systems, cloud-native deployment).

---

## 📝 Open Questions / Parking Lot

- [ ] Should the Booking aggregate emit domain events for future Saga refactor in Phase 5?
- [ ] Add `.gitattributes` for LF/CRLF handling on Windows — do in cleanup commit
- [ ] Add OpenTelemetry from Phase 3 (Docker) or wait until Phase 5 (microservices)?
- [ ] Should I add OpenAPI versioning (`/api/v1/`) before Phase 4 cloud deployment?
- [ ] C4 architecture diagrams in `/docs/diagrams` — generate before or after Phase 5?
- [ ] Cache invalidation on booking confirmation — invalidate `events:detail:{id}` when tickets are reserved
- [ ] RedLock algorithm for production-grade distributed locking (Phase 6 consideration)

---

*Last updated: May 2026 — Phase 3 complete, Phase 4 planning started*
