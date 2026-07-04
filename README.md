# 🎟️ TicketFlow

> A production-style event ticketing platform built to demonstrate the architectural evolution from a clean monolithic API to a cloud-native, event-driven distributed system.

Inspired by real-world challenges at platforms like Ticketmaster and BookMyShow, TicketFlow tackles inventory concurrency, time-bound seat reservations, distributed transactions, and elastic scaling — with each development phase introducing a real architectural concern documented in Architecture Decision Records (ADRs).

This repository is intentionally built **in public, in phases**, so the commit history reads as an architect's decision journal.

**📄 [Project Summary (PDF)](./docs/TicketFlow-Recruiter-Summary.pdf)** — a 2-page overview of the full project, tech stack, and engineering highlights.

---

## 🏗️ Architecture at a Glance

TicketFlow is composed of several independently deployable applications communicating over versioned REST contracts and Azure Service Bus:

- **`TicketFlow.Api`** — ASP.NET Core 10 Web API, Clean Architecture (Domain → Application → Infrastructure → API), CQRS via MediatR
- **`TicketFlow.Identity.Api`** — an extracted identity service with its own database and its own EF Core migration history
- **`TicketFlow.Gateway`** — a YARP-based API Gateway routing to both backend services
- **Frontend** — Angular 20 LTS SPA with standalone components, signals-based state, feature-based structure, and a typed service layer

This separation reflects a real-world architectural pattern where independently deployable services communicate over well-defined boundaries — each able to scale and evolve on its own.

📐 See [`docs/diagrams`](./docs/diagrams) for C4 diagrams and [`docs/adr`](./docs/adr) for all 17 Architecture Decision Records.

---

## 🛣️ The 6-Phase Roadmap — All Complete ✅

| Phase | Focus | Skills | Status | Tag |
|-------|-------|--------|--------|-----|
| **1** | Monolith Foundation | .NET 10, Web API, EF Core 10, SQL Server, Angular 20 | ✅ Complete | `v0.1` |
| **2** | Authentication & Authorization | OAuth 2.0 / OIDC via Microsoft Entra ID, JWT, PKCE | ✅ Complete | `v0.2` |
| **3** | Containerization + Distributed Caching | Docker, docker-compose, multi-stage builds, Redis (locking + caching), nginx load balancer | ✅ Complete | `v0.3` |
| **4** | Cloud Deployment + IaC | Azure App Service, Azure SQL, Terraform | ✅ Complete | `v0.4` |
| **5** | Modular Monolith + Identity Service Extraction | Module boundaries (DDD), API Gateway (YARP), Azure Service Bus, Saga pattern | ✅ Complete | `v0.5` |
| **6** | Container Orchestration (ACA + AKS) | Azure Container Apps (primary), AKS + Helm (demonstration), cloud networking troubleshooting | ✅ Complete | `v0.6` |

Each phase shipped with a tagged release and a retrospective ADR. Full narrative, decision rationale, and lessons learned for every phase live in [`docs/PROJECT_CONTEXT.md`](./docs/PROJECT_CONTEXT.md) and the [recruiter summary PDF](./docs/TicketFlow-Recruiter-Summary.pdf) linked above.

---

## 🚢 Deployment — Two Independent Tracks

Both tracks deploy the same three container images (API, Identity, Gateway) from a shared Terraform configuration (`terraform-phase6/`), and are torn down between demo sessions to avoid ongoing Azure cost.

- **Track 1 — Azure Container Apps (primary):** the production deployment target, chosen for its lower operational overhead at this workload's scale. See ADR-015 and ADR-016.
- **Track 2 — Azure Kubernetes Service + Helm (demonstration):** a full AKS deployment with Helm charts, an nginx Ingress Controller, and Kubernetes Secrets — built specifically to demonstrate hands-on Kubernetes operational skill. See [`docs/track2-aks-helm-guide.pdf`](./docs/track2-aks-helm-guide.pdf) for the complete step-by-step deployment guide, and ADR-017 for 8 real debugging incidents encountered while building it (VM quota mismatches, Helm naming collisions, silent .NET config fallbacks, and an Azure Load Balancer health probe misconfiguration that silently dropped all external traffic).

---

## 🎯 Domain Model

Five core entities with real business rules enforced in the Domain layer:

- **Event** — what's being ticketed (concert, conference, sports match)
- **Venue** — where the event happens (with capacity)
- **Ticket** — the unit being sold (tier, price, status)
- **Booking** — a customer's reservation of one or more tickets
- **Customer** — a real OAuth-backed identity (Microsoft Entra ID), managed by the extracted Identity service

### Key Business Rules

- A booking can only be created if all requested tickets are currently `Available`
- Reserving a ticket starts a **15-minute hold** — if not confirmed, it auto-releases
- Booking total is calculated **server-side** from current ticket prices
- An event can't be `Published` without at least one venue and ticket tier
- A customer can't book more than **6 tickets per event** (anti-scalping)
- Confirmed bookings can't be cancelled once the event has started

---

## 📦 Project Structure

```
ticketflow/
├── backend/
│   ├── src/
│   │   ├── TicketFlow.Domain/          # Entities, business rules, no external deps
│   │   ├── TicketFlow.Application/     # CQRS handlers, DTOs, validators, interfaces
│   │   ├── TicketFlow.Infrastructure/  # EF Core, Redis, background jobs, seed data
│   │   ├── TicketFlow.Identity.Api/    # Extracted identity service (own DbContext, own DB)
│   │   └── TicketFlow.Api/             # Controllers, middleware, DI
│   ├── TicketFlow.Gateway/             # YARP API Gateway
│   ├── Dockerfile / Dockerfile.Identity / Dockerfile.Gateway
│   └── tests/
│       └── TicketFlow.Domain.Tests/    # Pure domain unit tests
├── frontend/                           # Angular 20 SPA
│   └── src/app/
│       ├── core/                       # Services, interceptors, guards, models
│       ├── features/                   # events, bookings, my-bookings
│       └── layout/                     # Header, footer, nav
├── nginx/                              # Load balancer config (Phase 3)
├── helm/                               # Helm charts for AKS (Phase 6, Track 2)
│   ├── ticketflow-api/
│   ├── ticketflow-identity/
│   └── ticketflow-gateway/
├── terraform/                          # Azure App Service IaC (Phase 4)
├── terraform-phase6/                   # ACA + AKS IaC (Phase 6)
├── docker-compose.yml                  # Full local stack: API×2, Angular, SQL Server, Redis, nginx LB
├── docs/
│   ├── adr/                            # Architecture Decision Records (ADR-001 to ADR-017)
│   ├── diagrams/                       # C4 diagrams
│   ├── PROJECT_CONTEXT.md              # Full project state, decisions, and phase retrospectives
│   ├── TicketFlow-Recruiter-Summary.pdf
│   └── track2-aks-helm-guide.pdf
└── .github/workflows/                  # CI pipelines
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20.11+ LTS and Angular CLI 20 (`npm install -g @angular/cli@20`)
- SQL Server (LocalDB works) or SQL Server Express
- Docker Desktop (for the full containerized stack)

### Run the Full Stack Locally (Docker Compose)
```bash
docker compose up
```
This starts SQL Server, Redis, two API replicas behind an nginx load balancer, and the Angular SPA.

### Run the Backend Standalone
```bash
cd backend/src/TicketFlow.Api
dotnet ef database update
dotnet run
```
API available at `https://localhost:5001`. Swagger UI at `/swagger`.

### Run the Frontend Standalone
```bash
cd frontend
npm install
ng serve
```
App available at `http://localhost:4200`.

### Deploy to Azure
See `terraform-phase6/` for both deployment tracks (ACA and AKS). Full AKS walkthrough in [`docs/track2-aks-helm-guide.pdf`](./docs/track2-aks-helm-guide.pdf).

---

## 📚 Architecture Decision Records

All 17 ADRs live in [`docs/adr`](./docs/adr), spanning every phase — architecture style, auth provider choice, caching strategy, IaC tooling, service extraction, messaging, and the full AKS deployment debugging log (ADR-017). See [`docs/PROJECT_CONTEXT.md`](./docs/PROJECT_CONTEXT.md) for the fast-scan summary table of all 17 decisions with rationale.

---

## 👤 About

Built by Siva as a portfolio project for .NET Architect roles in the Houston/Texas market.

**📄 [Project Summary (PDF)](./docs/TicketFlow-Recruiter-Summary.pdf)** · **[Full Project History](./docs/PROJECT_CONTEXT.md)**

Connect: [LinkedIn](#) · [Email](#)
