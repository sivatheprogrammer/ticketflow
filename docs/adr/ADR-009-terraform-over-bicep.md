# ADR-009: Terraform over Bicep for Infrastructure as Code

## Status
Accepted

## Context
Phase 4 requires provisioning Azure infrastructure (Resource Group, ACR, App Service Plan,
2x App Services, Azure SQL Server + Database) in a repeatable, version-controlled way.

Two main IaC options were considered for Azure:

- **Bicep** — Microsoft's native, Azure-only DSL. Compiles to ARM templates.
- **Terraform** — HashiCorp's multi-cloud IaC tool using HCL, with a mature
  `azurerm` provider.

## Decision
Use **Terraform** with the `azurerm` provider.

## Rationale

1. **Job market relevance** — Terraform is the most commonly requested IaC skill
   across cloud job postings (AWS, Azure, GCP), not just Azure-specific roles.
   Bicep knowledge transfers only within the Azure ecosystem.

2. **Multi-cloud transferability** — demonstrates skills applicable beyond Azure,
   valuable for architect roles where cloud-agnostic infrastructure design is
   often a stated goal.

3. **State management** — Terraform's state file model (`terraform plan` / `apply`
   / `destroy`) gives a clear, auditable view of infrastructure drift —
   useful both for cost control (destroy between sessions) and for
   demonstrating IaC discipline.

4. **Ecosystem maturity** — large module registry, strong community docs,
   well-understood patterns for resource naming, tagging, and dependency
   graphs.

5. **Provider stability trade-off (known)** — the `azurerm` v4 provider had
   several breaking changes from v3 during this project (e.g.
   `enable_non_ssl_port` removed from `azurerm_redis_cache`,
   `DOCKER_REGISTRY_SERVER_*` settings no longer valid in `app_settings`,
   `prevent_deletion_if_contains_resources` now required for clean destroys).
   Bicep, being natively maintained by Microsoft, would track Azure API
   changes faster — but the job-market and multi-cloud arguments outweigh
   this for a portfolio project.

## Consequences

- All infrastructure lives in `/terraform`, version-controlled alongside
  application code.
- `terraform destroy` / `terraform apply` is used between work sessions to
  control Azure costs on a personal subscription.
- A **fixed suffix** (`var.suffix = "siva04"`) is used instead of
  `random_string`, so resource URLs (App Service hostnames, ACR login server)
  remain stable across destroy/apply cycles — avoiding manual updates to
  hardcoded URLs in `nginx.conf` (API proxy) and `Program.cs` (CORS origins).
- Azure Managed Redis was descoped from Phase 4 due to cost (~$100/mo minimum)
  and the retirement of the classic `azurerm_redis_cache` resource. The API
  is configured with `Redis__ConnectionString = "localhost:6379,abortConnect=false"`
  so it runs without Redis in the cloud, gracefully degrading caching —
  Redis remains fully demonstrated via Docker in Phase 3 (ADR-010).