# 🎟️ TicketFlow

> A production-style event ticketing platform built to demonstrate the architectural evolution from a clean monolithic API to a cloud-native, event-driven distributed system.

Inspired by real-world challenges at platforms like Ticketmaster and BookMyShow, TicketFlow tackles inventory concurrency, time-bound seat reservations, distributed transactions, and elastic scaling — with each development phase introducing a real architectural concern documented in Architecture Decision Records (ADRs). 

This repository is intentionally built **in public, in phases**, so the commit history reads as an architect's decision journal.

---

## 🏗️ Architecture at a Glance

TicketFlow is composed of two independently deployable applications communicating over a versioned REST contract:

- **Backend** — ASP.NET Core 10 Web API following Clean Architecture
- **Frontend** — Angular 20 LTS SPA with standalone components, signals-based state, feature-based structure, and typed service layer

This separation reflects a real-world architectural pattern where frontend and backend teams ship independently, scale independently, and evolve independently.

📐 See [`docs/diagrams`](./docs/diagrams) for C4 diagrams and [`docs/adr`](./docs/adr) for Architecture Decision Records.

---

## 🛣️ The 6-Phase Roadmap

| Phase | Focus | Skills | Status |
|-------|-------|--------|--------|
| **1** | Monolith Foundation | .NET Core, Web API, EF Core, SQL Server, Angular 20 | ✅ Complete | May 2026 |
| **2** | Authentication & Authorization | OAuth 2.0 / OIDC via Microsoft Entra ID, JWT, PKCE, role-based access (+ Okta bonus) | ⏳ Planned |
| **3** | Containerization + Distributed Caching | Docker, docker-compose, multi-stage builds, Redis (locking + caching), nginx load balancer | ⏳ Planned |
| **4** | Cloud Deployment + IaC | Azure App Service, Azure SQL, Azure DevOps, Terraform | ⏳ Planned |
| **5** | Modular Monolith + Identity Service Extraction | Module boundaries (DDD), API Gateway (YARP), Azure Service Bus, Saga pattern, service-to-service auth | ⏳ Planned |
| **6** | Container Orchestration (ACA + AKS) | Azure Container Apps (primary), AKS + Helm (demonstration), KEDA-based autoscaling | ⏳ Planned |

Each phase ends with a tagged release (`v0.1`, `v0.2`...) and a retrospective ADR.

---

## 🎯 Phase 1 — Monolith Foundation

**Goal:** Build a clean, well-tested monolithic API and a working Angular SPA that consumes it. Focus on domain modeling, business rules, and architectural rigor — not distributed-systems complexity.

### Domain Model

Four core entities with real business rules:

- **Event** — what's being ticketed (concert, conference, sports match)
- **Venue** — where the event happens (with capacity)
- **Ticket** — the unit being sold (tier, price, status)
- **Booking** — a customer's reservation of one or more tickets

### Key Business Rules (enforced in the Domain layer)

- A booking can only be created if all requested tickets are currently `Available`
- Reserving a ticket starts a **15-minute hold** — if not confirmed, it auto-releases
- Booking total is calculated **server-side** from current ticket prices
- An event can't be `Published` without at least one venue and ticket tier
- A customer can't book more than **6 tickets per event** (anti-scalping)
- Cancelling a booking releases tickets back to inventory (only before event start)

### What's NOT in Phase 1 (deliberately deferred)

- ❌ Authentication (Phase 2)
- ❌ Real payment processing (mocked)
- ❌ Email/SMS notifications (Phase 5)
- ❌ Microservices decomposition (Phase 5)

This restraint is itself an architectural choice — *build the monolith well before splitting it*.

---

## 📦 Project Structure

```
ticketflow/
├── backend/
│   ├── src/
│   │   ├── TicketFlow.Domain/          # Entities, value objects, business rules
│   │   ├── TicketFlow.Application/     # CQRS handlers, DTOs, validators
│   │   ├── TicketFlow.Infrastructure/  # EF Core, repositories, background jobs
│   │   └── TicketFlow.Api/             # Controllers, middleware, DI setup
│   └── tests/
│       ├── TicketFlow.Domain.Tests/
│       └── TicketFlow.Application.Tests/
├── frontend/
│   └── src/app/
│       ├── core/                       # Services, interceptors, guards, models
│       ├── shared/                     # Reusable components, pipes
│       ├── features/                   # events, bookings, my-bookings
│       └── layout/                     # Header, footer, nav
├── docs/
│   ├── adr/                            # Architecture Decision Records
│   └── diagrams/                       # C4 model diagrams
└── .github/workflows/                  # CI pipelines
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20.11+ LTS and Angular CLI 20 (`npm install -g @angular/cli@20`)
- SQL Server (LocalDB works) or SQL Server Express

### Run the Backend
```bash
cd backend/src/TicketFlow.Api
dotnet ef database update
dotnet run
```
API will be available at `https://localhost:5001`. Swagger UI at `/swagger`.

### Run the Frontend
```bash
cd frontend
npm install
ng serve
```
App will be available at `http://localhost:4200`.

---

## 📚 Architecture Decision Records

Each significant decision is recorded as a short ADR in [`docs/adr`](./docs/adr).

Phase 1 ADRs:
- [ADR-001: Clean Architecture for the Backend](./docs/adr/0001-clean-architecture.md)
- [ADR-002: CQRS with MediatR](./docs/adr/0002-cqrs-with-mediatr.md)
- [ADR-003: Standalone Components in Angular](./docs/adr/0003-angular-standalone-components.md)
- [ADR-004: 15-Minute Reservation Hold via Background Service](./docs/adr/0004-reservation-hold-strategy.md)
- [ADR-005: Angular 20 LTS Over Angular 21](./docs/adr/0005-angular-version-choice.md)

---

## 👤 About

Built by [Your Name] as a portfolio project for .NET Architect roles.

Connect: [LinkedIn](#) · [Email](#)
