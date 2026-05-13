# ADR-003: Angular Standalone Components and Signals

**Status:** Accepted
**Date:** Phase 1
**Deciders:** Project Author

## Context

When starting an Angular 20 project today, two architectural style choices need to be made early because they affect every component going forward:

1. **Module-based vs standalone components** — the legacy `NgModule` approach vs the newer standalone component model (default since Angular 17)
2. **State management approach** — RxJS-only, NgRx, or the newer Signals API for component-local state

These decisions are practically reversible at the component level but expensive to flip across an entire codebase later.

## Decision

Adopt:

1. **Standalone components everywhere.** No `NgModule` declarations. Each component, directive, and pipe declares its own dependencies via the `imports` array.

2. **Signals for component-local state**, with RxJS retained for asynchronous streams (HTTP responses, route params, time-based events). No NgRx in Phase 1 — defer until complexity actually demands it.

3. **Functional providers and interceptors.** `provideRouter()`, `provideHttpClient(withInterceptors([...]))`, etc. — no class-based providers.

## Reasoning

**Standalone is the official Angular direction.** As of Angular 20, standalone is the default for new components, and `NgModule` is being phased out across the framework. Building on `NgModule` today would mean writing legacy code from day one.

**Signals are simpler than RxJS for component state.** A `signal<EventSummary[]>([])` is more readable than a `BehaviorSubject<EventSummary[]>` plus an `async` pipe. For state that lives inside a single component, signals win on clarity and ergonomics. RxJS remains essential for stream-based concerns — we use both, intentionally.

**No NgRx until it earns its place.** NgRx is powerful but adds significant ceremony — actions, reducers, effects, selectors, store devtools setup. For a single-customer view of events and bookings, it's overkill. The principle here mirrors the backend: don't introduce infrastructure complexity until the domain complexity demands it. A future ADR will revisit this if Phase 2+ introduces multi-tab synchronization, optimistic updates with rollback, or other state-heavy scenarios.

**Functional APIs match modern Angular idioms.** `provideHttpClient(withInterceptors([errorInterceptor]))` is the current best practice. Class-based interceptors and `HTTP_INTERCEPTORS` providers still work but are considered legacy.

**Lazy-loaded routes via `loadComponent`.** Each feature route uses `loadComponent: () => import(...)` for proper code-splitting. Standalone components make this dramatically simpler than the old `loadChildren` + module approach.

## Consequences

**Positive:**
- Each component file is self-contained and immediately understandable
- Smaller bundle sizes via natural code-splitting
- Less boilerplate per feature
- Aligns with where Angular is heading (zoneless + signals + standalone is the trajectory)
- Easier to onboard contributors familiar with modern Angular

**Negative:**
- Imports must be declared on every component (vs once per module)
- Some older Angular tutorials and Stack Overflow answers use `NgModule` patterns and need translation
- Signals are still relatively new — fewer battle-tested patterns and less third-party tooling than RxJS
- Mixing signals and RxJS requires understanding both paradigms and when each applies

## Alternatives Considered

**`NgModule`-based architecture (rejected).** The legacy approach. Fully supported but actively being deprecated across the framework. Building new code on `NgModule` today is choosing tomorrow's tech debt.

**RxJS-only state (rejected).** Familiar and powerful, but unnecessarily verbose for component-local state. `BehaviorSubject` + `async` pipe is more ceremony than `signal()` for the same outcome.

**NgRx from day one (rejected).** Premature for the Phase 1 scope. Introduces significant indirection (actions, reducers, effects, selectors) for a project that today only needs to display events and create bookings. Will be revisited if state complexity grows.

**SignalStore (Angular Signals' answer to NgRx) (deferred).** Promising lightweight alternative to NgRx, built on signals. Worth evaluating in a later phase if shared state across components emerges as a real need.

## Notes

- **Angular 21 will make zoneless change detection the default.** This decision is forward-compatible — a future ADR will document the migration to `provideZonelessChangeDetection()` once we move off Angular 20. Components written today using signals will require minimal changes during that migration. See ADR-005.
- **The `@if` / `@for` / `@switch` control flow syntax** (introduced in Angular 17) is preferred over the older `*ngIf` / `*ngFor` structural directives throughout the codebase.
