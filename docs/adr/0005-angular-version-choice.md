# ADR-005: Angular 20 LTS for the Frontend (Not 21)

**Status:** Accepted
**Date:** Phase 1
**Deciders:** Project Author

## Context

At project start (April 2026), the Angular release landscape was:

- **Angular 21.2** — released April 22, 2026 (latest, ~1 week old)
- **Angular 20** — current LTS, supported through November 2026
- **Angular 19** — End of Support May 19, 2026
- **Angular 18 and earlier** — already End of Life

Angular 21 introduces meaningful changes: **zoneless change detection by default**, **Vitest as the default test runner** (replacing Karma), and experimental **Signal Forms**. These are genuine improvements, but they're also bleeding edge.

The framework choice will affect the project for at least 6 months across all 6 phases.

## Decision

Target **Angular 20** for the duration of the 6-phase build. Defer the migration to Angular 21+ until after Phase 6 ships, treating it as a separate "modernization" milestone.

## Reasoning

**Stability matters more than novelty for a portfolio.** Angular 20 has a mature ecosystem — every Angular Material theme, OAuth library (`angular-auth-oidc-client`), Okta SDK, OpenTelemetry instrumentation, and Stack Overflow answer has had months to stabilize. Angular 21 was released seven days before this project began; library compatibility issues are inevitable.

**The portfolio's value isn't "uses the absolute latest framework version" — it's "demonstrates architectural rigor."** Hiring managers reviewing a .NET Architect portfolio care that the Angular code is *clean and current*, not that it's on the version released last week.

**Angular 20 is current.** It's not legacy. It supports standalone components, signals, the new control flow syntax, deferrable views, and modern SSR. None of the architectural patterns demonstrated in this project would change between 20 and 21.

**The phased plan demands a stable base.** Containerization, Azure deployment, microservices decomposition, and Kubernetes orchestration are each non-trivial undertakings. Adding "fight Angular 21 ecosystem gaps" on top of any of those phases is asking for trouble.

**Support runway is sufficient.** Angular 20 is supported until November 2026 — comfortably past the planned completion of all 6 phases.

## Consequences

**Positive:**
- Wider library compatibility, fewer surprises
- Easier troubleshooting (more documentation, more community answers)
- Phase work stays focused on the architectural concern of each phase

**Negative:**
- We don't get zoneless-by-default or Vitest out of the box
- The README will need to acknowledge this isn't the absolute latest version
- A future migration to Angular 21+ will be a deliberate step, not "free"

## Modernization Path (Post-Phase-6)

Once all 6 phases ship, a planned "Phase 7" modernization sprint can:

1. Run `ng update @angular/core@21 @angular/cli@21`
2. Migrate to zoneless change detection using the official schematics
3. Migrate the test suite from Karma to Vitest
4. Adopt Signal Forms once they exit experimental status (likely Angular 22)
5. Document the migration in a follow-up ADR

This staged approach mirrors how real architecture teams handle major framework upgrades — plan, schedule, test, ship — rather than chasing every release.

## Architect-Level Framing for Interviews

When discussing this in interviews, the talking point is:

> *"I chose Angular 20 LTS over the just-released Angular 21 because portfolio stability and ecosystem maturity outweighed novelty. My ADR documents the trade-off and the planned migration path — which itself demonstrates that I think about framework upgrades as managed projects, not impulse decisions."*

That's a senior-level answer to a question many candidates fumble.
