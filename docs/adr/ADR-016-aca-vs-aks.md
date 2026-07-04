# ADR-016: ACA (Primary) vs AKS (Demonstration) — Container Orchestration Choice

## Status
Accepted

## Date
2026-07-03

## Context
Phase 6 moves TicketFlow to container orchestration. Two Azure options exist
for running containers at scale:

- **Azure Container Apps (ACA)** — managed container platform built on Kubernetes
- **Azure Kubernetes Service (AKS)** — managed Kubernetes cluster

Both are valid production choices. The decision is which to use as the primary
deployment target and how to demonstrate both for portfolio purposes.

## Decision
Use **ACA as the primary deployment track** and **AKS as a demonstration track**.

Both are implemented in `terraform-phase6/` and documented with Helm charts.

## ACA vs AKS — Detailed Comparison

| Dimension | ACA | AKS |
|-----------|-----|-----|
| Cluster management | None — fully managed | You manage the cluster |
| Scaling | Built-in KEDA — scale to zero | Manual KEDA setup required |
| Ingress | Built-in HTTP ingress | Need ingress controller (nginx) |
| Networking | Managed VNet | Full VNet control |
| Cost | Pay per request — scale to zero | Always-on node pool |
| Complexity | Low | High |
| Kubernetes knowledge needed | Low | High |
| Helm support | Limited | Full |
| Best for | Microservices, event-driven apps | Complex workloads, full K8s control |

## Why ACA is the right primary choice for TicketFlow

1. **Scale to zero** — TicketFlow is a portfolio project; ACA costs nothing
   when idle. AKS always-on node pool would cost ~$150/month minimum.

2. **Built-in KEDA** — ACA natively supports scaling based on Service Bus
   queue depth. No additional setup required.

3. **Fits the workload** — 3 services with HTTP communication and Service Bus
   messaging is exactly what ACA is designed for.

4. **Simpler operations** — No cluster upgrades, node pool management, or
   networking configuration. Focus is on the application, not the platform.

5. **Production-ready** — ACA is used in real production workloads at scale.
   It's not a simplification for the demo — it's the right tool.

## Why AKS is included as a demonstration track

1. **Resume keyword density** — Many .NET Architect job descriptions explicitly
   require Kubernetes experience. AKS demonstrates this directly.

2. **Helm charts** — Writing Helm charts shows deployment packaging skills
   that ACA doesn't require.

3. **Architectural judgment** — Implementing both and documenting the trade-offs
   in this ADR demonstrates the ability to choose the right tool, not just
   use the most complex one.

4. **Future-proofing** — If TicketFlow were to grow to 10+ services with
   complex networking requirements, AKS would become the right choice.

## When to choose ACA vs AKS in real projects

**Choose ACA when:**
- 1-10 microservices
- Event-driven or HTTP workloads
- Team doesn't want to manage Kubernetes
- Cost efficiency is important (scale to zero)
- Getting to production quickly matters

**Choose AKS when:**
- 10+ services with complex inter-service networking
- Need full Kubernetes API access (custom operators, CRDs)
- Stateful workloads requiring persistent volumes
- Organization already has Kubernetes expertise
- Need multi-cloud portability (EKS, GKE compatibility)

## Consequences

### Positive
- ACA primary track is cost-efficient for portfolio demo
- AKS demonstration track satisfies Kubernetes skill requirement
- Both tracks documented — shows architectural judgment
- KEDA autoscaling works out of the box with ACA

### Negative
- AKS cluster adds cost and complexity during demonstration
- Two deployment targets to maintain and document
- Helm charts add scope to Phase 6

### Neutral
- Both tracks use the same Docker images from ACR
- AKS cluster destroyed after demonstration to stop billing

## Alternatives Considered
- **AKS only:** Overkill for this workload; higher cost; misses the
  "right tool for the job" narrative
- **ACA only:** Misses the Kubernetes demonstration; weaker for roles
  requiring explicit K8s experience
- **Docker Compose on Azure VM:** Not cloud-native; no orchestration
  benefits; not relevant to architect-level roles
