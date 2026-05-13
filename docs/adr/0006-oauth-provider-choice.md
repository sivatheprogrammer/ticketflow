# ADR-006: Microsoft Entra ID as Primary OAuth Provider, with Okta as Bonus Demo

**Status:** Accepted
**Date:** Phase 2 (planning)
**Deciders:** Project Author

## Context

Phase 2 introduces authentication and authorization to TicketFlow. The system needs:

- Customer login/logout from the Angular SPA
- Bearer-token authentication for the .NET API
- Role-based authorization (Customer, Organizer, Admin)
- Refresh token rotation
- A foundation that scales cleanly into Phase 5 microservices (each service must validate the same tokens)
- Production-grade security without operating a self-hosted identity server

Five realistic options were evaluated:

1. **Microsoft Entra ID (formerly Azure AD / Azure AD B2C)** — Microsoft's hosted identity platform
2. **Okta** — Enterprise IDaaS, broadly recognized
3. **Auth0** — Developer-friendly IDaaS (now owned by Okta)
4. **Duende IdentityServer** — Self-hosted .NET OAuth server (commercial license required for production)
5. **Keycloak / OpenIddict** — Self-hosted open-source alternatives

The decision is significant because the OAuth provider shapes Phase 2 work, influences the API security model for all later phases, and is the most-named identity skill in .NET Architect job postings.

## Decision

**Primary:** Use **Microsoft Entra ID** (External ID configuration for customer-facing scenarios) as the OAuth/OIDC provider for TicketFlow.

**Bonus demonstration:** Build a feature branch (`oauth/okta-integration`) that swaps the identity provider to Okta, demonstrating that the application is coded against OIDC standards rather than vendor-specific SDKs.

**Architectural principle (binding for all OAuth code):** Code against the OIDC standard via vendor-neutral libraries (`Microsoft.AspNetCore.Authentication.JwtBearer` on .NET, `angular-auth-oidc-client` on Angular) wherever possible — never directly against Entra-specific or Okta-specific SDKs.

## Reasoning

**Job market alignment.** A scan of current (April 2026) .NET Architect job postings shows Microsoft Entra ID / Azure AD listed as a required or preferred skill in the majority of Azure-focused .NET roles. Okta is also broadly recognized but appears more frequently in dedicated IAM Architect postings than in .NET Architect ones. For a portfolio explicitly targeting .NET Architect roles in Microsoft/Azure shops, Entra ID provides the highest resume keyword density.

**Ecosystem coherence.** TicketFlow is built end-to-end on Azure: App Service deployment, Azure SQL, Azure Service Bus, Azure DevOps pipelines, Terraform against the AzureRM provider. Adding Okta would introduce a non-Azure vendor into an otherwise unified stack — defensible, but architecturally noisier. Entra ID is the natural fit.

**Cost at production scale.** Entra External ID's free tier supports 50,000 monthly active users, which means the portfolio app could be promoted to a real demo without operational cost concerns. Okta's free developer tier is limited to 100 MAU and is explicitly for development use.

**Same OAuth depth as Okta.** Both are managed providers; both teach the same OAuth 2.0 / OIDC patterns from the consumer side (PKCE flow, JWT validation, refresh token rotation, role-based authorization). Neither teaches OAuth server *implementation* — but the .NET Architect job market rewards correct OAuth integration far more than the ability to build an identity server from scratch.

**Microservices-ready.** When Phase 5 splits the monolith, every service can validate the same Entra-issued JWTs against the same JWKS endpoint without additional infrastructure. The OIDC-standard approach means each service's auth code is identical and provider-agnostic.

**The Okta bonus branch is the senior move.** A solo portfolio that ships *only* Entra integration looks like "I picked the obvious Microsoft option." The same portfolio with a clean Okta-swap branch demonstrates: (1) the architecture genuinely is provider-agnostic, (2) the engineer thinks about vendor lock-in, and (3) the engineer can integrate with the second-most-named identity provider in the market. The bonus branch is small (estimated 2-3 days) but the architectural signal is large.

## Consequences

**Positive:**
- Direct alignment with target job postings' keyword expectations
- Single Azure ecosystem, no third-party vendor in production
- Free at portfolio scale and production scale
- The OIDC-standard architecture supports future provider swaps with minimal code changes
- Two recognized identity provider names on the resume (Entra + Okta) instead of one

**Negative:**
- Less applicable to non-Microsoft enterprise targets (mitigated by the Okta bonus branch)
- Doesn't demonstrate OAuth server *implementation* (mitigated by deep OIDC-flow documentation in ADRs)
- Entra ID's developer experience has historically been less polished than Okta's (acknowledged; tolerable)
- The "External ID" rebrand of Azure AD B2C is recent and some documentation still uses old naming

## Alternatives Considered

**Okta as primary (rejected for this project; used as bonus demo).** Genuinely strong choice, especially for non-Microsoft enterprise targets. Rejected as primary because (a) the target audience is Azure-heavy .NET shops, (b) Okta would introduce a non-Azure vendor into an otherwise coherent stack, and (c) Okta's free tier doesn't extend to production scale. Reconsidered as bonus demo because Okta's market presence in IAM is undeniable and the swap demonstrates architectural abstraction.

**Duende IdentityServer (rejected).** The deepest learning option — would force genuine OAuth server implementation. Rejected because (a) commercial licensing is required for production use beyond development, (b) the .NET Architect job market more often rewards OAuth integration depth than implementation depth, and (c) the time investment (1-2 weeks for IdentityServer setup alone) competes with later-phase priorities. Worth revisiting in a future project specifically focused on identity infrastructure.

**OpenIddict (rejected).** Free open-source equivalent of IdentityServer. Same depth-vs-time trade-off as Duende, with less name recognition. Reasonable choice if licensing is the dominant concern, but ecosystem coherence with Azure tipped the decision toward Entra.

**Auth0 (rejected).** Solid developer experience but, as an Okta-owned product, doesn't add a meaningfully different name to the portfolio. If Okta is the bonus demo, Auth0 would be redundant.

**Keycloak (rejected).** Strong self-hosted option, particularly in Java/government/regulated industries. Rejected because (a) it's not a recognized .NET-ecosystem signal, and (b) operating a Keycloak server is a meaningful infrastructure burden that competes with portfolio time better spent on later-phase work.

## Implementation Notes

**Phase 2 deliverables in priority order:**

1. **Entra ID tenant setup**
   - Create External ID tenant in Azure portal
   - Register two applications: SPA (Angular, PKCE flow) and API (resource server)
   - Define custom scopes: `tickets:read`, `tickets:write`, `events:manage`, `bookings:manage`
   - Define app roles: Customer, Organizer, Admin

2. **Backend integration (.NET)**
   - Add `Microsoft.AspNetCore.Authentication.JwtBearer` (note: NOT `Microsoft.Identity.Web` — that's vendor-specific and would violate the architectural principle)
   - Configure JWT validation against Entra's discovery endpoint (`/.well-known/openid-configuration`)
   - Add `[Authorize]` attributes with scopes/roles to controllers
   - Replace hardcoded `customerId` in `CreateBookingCommand` with the authenticated user's claim
   - Implement first-login provisioning: create local `Customer` entity on first authenticated request

3. **Frontend integration (Angular)**
   - Add `angular-auth-oidc-client` (vendor-neutral OIDC library)
   - Implement login/logout flow with PKCE
   - Add HTTP interceptor to attach access tokens to API calls (extends the existing error interceptor pattern from Phase 1)
   - Add route guards for protected pages (My Bookings)
   - Implement token refresh handling

4. **Documentation**
   - This ADR (ADR-006)
   - ADR-007: First-login user provisioning strategy (mapping Entra `oid` claim to local `Customer.Id`)
   - Sequence diagram of the OAuth flow in `/docs/diagrams`
   - README updates: how to configure your own Entra tenant for local development

**Phase 2.5 (bonus branch):**

- Create `oauth/okta-integration` branch off `main` after Phase 2 ships
- Replace Entra discovery endpoint with Okta's
- Replace SPA client config with Okta tenant config
- Document changes in a follow-up note in this ADR
- Estimated effort: 2-3 days

## Talking Points for Interviews

> *"For TicketFlow's identity layer, I evaluated five OAuth provider options against the project's specific constraints — Azure-heavy stack, .NET Architect target audience, single-developer time budget, and the need for a production-credible solution. I chose Microsoft Entra ID as primary because it gave the strongest keyword alignment for my target roles, fit cleanly into the Azure ecosystem, and remained free at production scale. To prove the architecture wasn't vendor-locked, I built an Okta integration on a feature branch — the swap took two days because I'd coded against OIDC standards rather than vendor SDKs. The full decision and trade-offs are documented in ADR-006."*

That single paragraph answers about six interview questions at once.
