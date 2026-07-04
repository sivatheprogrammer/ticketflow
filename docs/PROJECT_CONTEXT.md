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
| **5** | Modular Monolith + Identity Service Extraction | Module boundaries (DDD), API Gateway (YARP), Azure Service Bus, Saga pattern, service-to-service auth | ✅ Complete | July 2026 |
| **6** | Container Orchestration (ACA + AKS) | Azure Container Apps (primary), AKS + Helm (demonstration), health probe / networking troubleshooting | ✅ Complete | July 2026 |

> **How to update:** When a phase ships, change status to ✅ Complete, fill in the date, and tag a Git release (`v0.1` for Phase 1, etc.).

> **All 6 phases now complete.** Next up: comprehensive six-phase portfolio document.

---

## 📍 Where I Am Right Now

**Current Phase:** 6 — Container Orchestration (ACA + AKS) — ✅ Complete
**Current Sub-task:** Building comprehensive six-phase portfolio PDF
**Last commit:** feat: Phase 6 Step 2 - AKS + Helm deployment track (PR #9, merged)
**Latest tag:** `v0.6`
**Branch:** main
**GitHub:** https://github.com/sivatheprogrammer/ticketflow
**Blocking issues:** None — Phase 6 fully shipped, both tracks verified end-to-end

| Step | Description | Status |
|------|-------------|--------|
| 1 | Track 1: Azure Container Apps (ACA) via Terraform | ✅ Done — PR #8 merged |
| 2 | Track 1: Verify end-to-end (Gateway → API/Identity → SQL) | ✅ Done |
| 3 | Track 2: AKS cluster via Terraform (`Standard_D2s_v3` node pool) | ✅ Done |
| 4 | Track 2: Helm charts for all 3 services | ✅ Done |
| 5 | Track 2: nginx ingress controller + Azure LB health probe fix | ✅ Done |
| 6 | Track 2: Dedicated Identity database (service data ownership) | ✅ Done |
| 7 | Track 2: Verify end-to-end (public IP → Gateway → API/Identity → SQL) | ✅ Done |
| 8 | ADR-017: AKS deployment challenges write-up | ✅ Done |
| 9 | Infrastructure destroyed post-verification (both tracks) | ✅ Done |
| 10 | Tagged `v0.6` | ✅ Done |
| 11 | Comprehensive 6-phase portfolio PDF | ⏳ In progress |

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
| ADR-011 | Modular Monolith + one extracted Identity service (not full microservices) | Demonstrates architectural restraint AND decomposition skill | 5 |
| ADR-012 | YARP API Gateway over nginx | Native .NET, programmatic config, better for service-to-service routing | 5 |
| ADR-013 | Azure Service Bus Emulator for local development | Zero cost, offline, mirrors production; AmqpTcp on port 5672 | 5 |
| ADR-014 | Choreography-based Saga for booking flow | No central coordinator; fits existing Service Bus infrastructure | 5 |
| ADR-015 | Azure Container Apps as primary deployment target | ACA's built-in scaling, ingress, and secrets management fit this workload with far less operational overhead than AKS | 6 |
| ADR-016 | ACA (primary) vs AKS (demonstration) comparison | Documents trade-offs so the choice reads as deliberate, not default | 6 |
| ADR-017 | AKS deployment challenges and lessons learned | 8 distinct issues (quota/SKU mismatch, Helm naming collisions, .NET config key mismatches, shared-database migration collision, YARP path mismatch, Azure LB health probe defaulting to `/`) documented with root cause and fix | 6 |

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
│   │   ├── TicketFlow.Identity.Api/    # Extracted identity service (own DbContext, own DB)
│   │   └── TicketFlow.Api/             # Controllers, middleware, DI
│   ├── TicketFlow.Gateway/             # YARP API Gateway
│   ├── Dockerfile / Dockerfile.Identity / Dockerfile.Gateway
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
├── helm/                               # Phase 6 Track 2 — Helm charts for AKS
│   ├── ticketflow-api/
│   ├── ticketflow-identity/
│   └── ticketflow-gateway/
├── terraform/                          # Phase 4 — Azure App Service IaC
├── terraform-phase6/                   # Phase 6 — ACA + AKS IaC (separate state)
├── docker-compose.yml                  # Full stack: API×2, Angular, SQL Server, Redis, nginx LB
├── .env                                # SA_PASSWORD (gitignored)
├── docs/
│   ├── adr/                            # Architecture Decision Records (ADR-001 to ADR-017)
│   ├── diagrams/                       # C4 diagrams (planned)
│   ├── track2-aks-helm-guide.pdf       # Step-by-step AKS + Helm deployment guide
│   └── PROJECT_CONTEXT.md              # 👈 You are here
└── .github/workflows/                  # CI pipelines (planned)
```

---

## 🔗 Important Links

- **GitHub Repo:** https://github.com/sivatheprogrammer/ticketflow
- **Live Demo:** Infrastructure is destroyed between demo sessions to avoid cost — redeploy via `terraform apply` in `terraform-phase6/` to bring either track back up
- **Swagger:** [paste URL after next deploy]

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

---

### Phase 5 — Modular Monolith + Identity Service Extraction ✅
- **What I built:** `TicketFlow.Identity.Api` (extracted identity service), `IdentityServiceClient` (typed HttpClient), `TicketFlow.Gateway` (YARP on port 7153), `BookingCreatedEvent` message contract, `IEventPublisher` + `ServiceBusEventPublisher`, `BookingCreatedConsumer` background worker, Service Bus emulator infrastructure. ADR-011, ADR-012, ADR-013, ADR-014 committed.
- **What surprised me:** Service Bus emulator requires `AmqpTcp` (port 5672) not `AmqpWebSockets` — WebSockets causes silent connection failure. `SAS_KEY_VALUE` in connection string is the literal key, not a placeholder. Namespace must be exactly `sbemulatorns`.
- **Known limitations:** None outstanding.
- **Time spent:** ~4 sessions

---

### Phase 6 — Container Orchestration (ACA primary + AKS demonstration) ✅

- **What I built:** Two parallel deployment tracks from a shared Terraform config
  (`terraform-phase6/`) provisioning a common Resource Group, ACR, Azure SQL Server,
  and Azure Service Bus (real, not emulator).
  - **Track 1 (ACA, primary):** Three Azure Container Apps (API, Identity, Gateway)
    behind a Container Apps Environment, with the Gateway as the only public ingress.
    Verified end-to-end: Angular/curl → Gateway → API/Identity → Azure SQL.
  - **Track 2 (AKS, demonstration):** AKS cluster (`Standard_D2s_v3`, 1 node), Helm
    charts for all three services, nginx ingress controller, Kubernetes Secrets for
    connection strings, and a dedicated `sqldb-ticketflow-identity-dev` database so
    Identity no longer shares a database (and migration history) with the main API.
    Verified end-to-end through the public Load Balancer IP after fixing an Azure LB
    health probe misconfiguration.
  - ADR-015 (ACA as primary), ADR-016 (ACA vs AKS comparison), ADR-017 (AKS deployment
    challenges — 8 issues documented with root cause and fix) all committed.
    A standalone step-by-step guide (`docs/track2-aks-helm-guide.pdf`) covers the full
    AKS + Helm deployment process for anyone picking this track back up later.

- **What surprised me:**
  - VM size selection for AKS needs two independent checks, not one: region
    availability (`az vm list-skus`) and subscription quota
    (`az vm list-usage`) can each fail independently. `Standard_B2s` failed
    on availability, `Standard_B2s_v2` failed on quota; `Standard_D2s_v3`
    cleared both.
  - `terraform destroy` removes the ACR along with all pushed images —
    re-`apply` recreates an empty registry, so images must be rebuilt and
    re-pushed before dependent Container Apps/Deployments can start.
  - A failed `terraform apply` can leave real Azure resources behind without
    corresponding Terraform state; `terraform import` doesn't work if the
    resource itself is stuck in a `Failed` provisioning state. Deleting the
    broken resource directly and letting Terraform recreate it was faster
    than fighting the state file.
  - Helm chart templates that build resource names as
    `{{ .Release.Name }}-api-secrets` produce doubled names
    (`ticketflow-api-api-secrets`) when the release name already contains
    the service name — easy to miss until a pod fails to start.
  - .NET's configuration binder silently ignores environment variables that
    don't exactly match an existing `appsettings.json` key path — it doesn't
    error, it just falls back to the default (LocalDB, in this case). This
    caused two separate incidents: a wrong `ConnectionStrings` key name, and
    a wrong YARP `ReverseProxy:Clusters:...:Destinations:...:Address` path
    that caused the Gateway to silently keep proxying to a hardcoded
    `localhost` dev port.
  - Sharing one Azure SQL database between API and Identity caused an EF Core
    migration collision (`There is already an object named 'Customers'`) —
    fixed by giving Identity its own dedicated database, reinforcing the
    "each service owns its data" principle from Phase 5's extraction.
  - The single most disruptive issue: Azure's Load Balancer health probe
    defaults to `GET /` against the ingress controller's NodePort. Since the
    Gateway has no route at `/` (only `/api/*`), nginx returned `404`, Azure
    marked the backend unhealthy, and silently dropped all external traffic
    — while internal cluster-to-cluster traffic worked perfectly the whole
    time. Fixed via the
    `service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path`
    annotation pointed at nginx's `/healthz` endpoint.

- **What I'd do differently:** Copy connection string and YARP env var names
  directly from the already-working ACA Terraform config instead of
  re-deriving them by hand for the Helm charts — every naming-mismatch bug
  in this phase would have been avoided by doing that from the start.
  Provision separate databases per service from the very first Terraform
  apply, rather than discovering the collision at migration time.

- **Known limitations:** Both tracks' infrastructure is destroyed between
  demo sessions to avoid ongoing Azure cost — there is no persistent live
  URL; redeploying either track takes a few minutes via
  `terraform apply` (+ `helm install` for Track 2).

- **Time spent:** ~3 sessions (Track 1) + ~4 sessions (Track 2, most of it
  debugging the issues above)

---

## 🤖 Instructions for Future AI Conversations

If you're an AI assistant reading this to help me continue the project, here's what you need to know:

1. **Read this entire file first** before suggesting anything. It contains the roadmap, current state, and key decisions already made.
2. **All 6 phases are complete as of `v0.6`.** Current work is the comprehensive portfolio document, not new application features — don't suggest a "Phase 7" unless I explicitly ask for one.
3. **Respect existing ADRs.** If you disagree with one, say so explicitly and propose a new ADR rather than silently changing direction.
4. **Runtime:** The project runs on **.NET 10** (not .NET 8). All csproj files target `net10.0`. EF Core packages are version `10.0.0`.
5. **Code style:** C# with nullable enabled, file-scoped namespaces, primary constructors where they help. Angular standalone components, signals over RxJS for component state, RxJS for HTTP streams. `@if` / `@for` control flow syntax.
6. **Docker:** Full stack runs via `docker compose up` from repo root. API image is `ticketflow-api`, Angular image is `ticketflow-web`. SA password in `.env` file (gitignored).
7. **Deployment:** Two working, documented tracks exist — ACA (primary, `terraform-phase6/`) and AKS (demonstration, same Terraform + `helm/`). Both are destroyed between sessions; redeploy either on request rather than assuming something is live.
8. **My current goal:** Build a comprehensive PDF covering all six phases — architecture, decisions, screenshots, lessons learned — as the primary portfolio artifact for my .NET Architect job search.
9. **My experience level:** Comfortable with .NET and Angular, learning architect-level patterns (DDD, distributed systems, cloud-native deployment). Prefers complete file contents over partial diffs, uses VS Code Source Control for commits/PRs, pastes terminal output as screenshots.

---

## 📝 Open Questions / Parking Lot

- [ ] Add OpenTelemetry — future consideration, not required for portfolio
- [ ] C4 architecture diagrams in `/docs/diagrams` — needed for the portfolio PDF
- [ ] Cache invalidation on booking confirmation — invalidate `events:detail:{id}` when tickets are reserved
- [ ] RedLock algorithm for production-grade distributed locking — future consideration
- [ ] KEDA autoscaling demo on AKS — deferred, not required for portfolio completeness

---

*Last updated: July 2026 — Phase 6 complete (`v0.6` tagged, both ACA and AKS tracks verified end-to-end), building the six-phase portfolio PDF next.*
