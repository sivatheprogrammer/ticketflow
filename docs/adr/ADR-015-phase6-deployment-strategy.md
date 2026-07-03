# ADR-015: Phase 6 Deployment Strategy — Move from App Service to Container Orchestration

## Status
Accepted

## Date
2026-07-03

## Context
Phase 4 deployed TicketFlow to Azure App Service using Terraform (`terraform/`).
While functional, App Service has limitations for a cloud-native, multi-service
architecture:

- No native container orchestration
- Manual scaling — no event-driven autoscaling
- Not representative of how production microservices are deployed
- Doesn't demonstrate Kubernetes operational knowledge

Phase 5 extracted two additional services (Identity, Gateway), making the app
a distributed system with 3 independently deployable services. Phase 6 moves
these to proper container orchestration.

## Decision

### Keep Phase 4 Terraform intact — create a new `terraform-phase6/` folder

| Folder | Phase | Deployment Target | Status |
|--------|-------|-------------------|--------|
| `terraform/` | 4 | Azure App Service | Preserved as historical reference |
| `terraform-phase6/` | 6 | ACA + AKS | Active |

The Phase 4 folder is kept intact (not modified or deleted) to:
- Show the evolution from App Service to container orchestration
- Allow side-by-side comparison for portfolio reviewers
- Serve as a reference for App Service deployment patterns

### What each Terraform folder provisions

**`terraform/` (Phase 4 — App Service):**
- Resource Group
- Azure Container Registry (ACR)
- App Service Plan (B2 Linux)
- 2x Linux Web Apps (API + Angular frontend)
- Azure SQL Server + Database

**`terraform-phase6/` (Phase 6 — ACA + AKS):**
- Resource Group
- Azure Container Registry (ACR)
- Azure SQL Server + Database
- Azure Service Bus (real, not emulator)
- ACA Environment (shared runtime for all container apps)
- ACA Container App — TicketFlow.Gateway (public ingress)
- ACA Container App — TicketFlow.Api
- ACA Container App — TicketFlow.Identity.Api
- AKS Cluster (demonstration track)

### Why NOT stay on App Service?
- Doesn't support independent scaling per service
- No event-driven autoscaling (KEDA)
- Not the industry standard for container orchestration
- Doesn't demonstrate cloud-native deployment skills expected at architect level

### Why NOT update the existing `terraform/` folder?
Updating it would lose Phase 4 history and make the architectural evolution
invisible to portfolio reviewers. The two-folder approach tells a story:
App Service (Phase 4) → Container Orchestration (Phase 6).

## Consequences

### Positive
- Portfolio shows clear architectural evolution across phases
- Phase 4 Terraform preserved as a working reference
- Real Azure Service Bus replaces local emulator — production-ready
- 3 services deploy and scale independently

### Negative
- Two Terraform folders to maintain
- Phase 4 infra must be destroyed before Phase 6 apply to avoid
  resource name conflicts on ACR and SQL
- Higher Azure cost while infra is running — destroy after demo

### Neutral
- Fixed suffix (`siva04`) carried forward for stable resource URLs
- Same ACR used — images tagged differently per phase

## Alternatives Considered
- **Update existing `terraform/` folder:** Loses Phase 4 history
- **Azure Container Instances (ACI):** Simpler than ACA but no
  autoscaling, not production-grade for multi-service apps
- **Docker Compose on a VM:** Not cloud-native; misses the
  orchestration demonstration entirely
