# AGENTS.md — tests/

xUnit v3, Shouldly, NetArchTest, Testcontainers.

## Projects

| Project | Covers | Needs |
|---|---|---|
| `UnitTests` | Coffee, cache-key, gate-report, and ledger invariants | nothing |
| `Web.Tests` | Scoped storefront state such as `StoreCart` | nothing |
| `ArchitectureTests` | Layering and conventions | nothing |
| `QualityGate.Tests` | Canonical serialisation, hashing, schema validation | nothing |
| `IntegrationTests` | The real API against real Postgres and Redis | a container runtime |

## The rule that matters most

`test-engineer` may edit files here and nowhere else. That boundary is the point: an agent that can
change both the test and the code under test can always produce green, and green then means
nothing.

If the production code is wrong, report the defect — file, line, the invariant it breaks, the
failing assertion — and hand back so an implementer fixes it.

## Forbidden

Weakening an assertion, widening a tolerance, adding `[Fact(Skip = ...)]`, deleting a test, or
catching an expected exception to get a green run. Each converts a visible failure into an
invisible one.

## Conventions

- `MethodOrBehaviour_ExpectedOutcome`. The name states the rule being protected.
- No `Task.Delay` for synchronisation. If you need to wait, you need a signal.
- Testcontainers images pinned by tag. Never `latest`.
- `TestContext.Current.CancellationToken` for real I/O.
- Search for existing fixtures and helpers before adding a test double. No shared fake library
	exists in the baseline.

## What is worth testing here

The guarantees, not the plumbing: coffee publication invariants, catalog search/filter/facet
behavior, current `StoreCart` rules, deterministic canonical hashes, ledger independence, and cache
degradation. Stock concurrency, checkout idempotency, and order snapshots become targets when those
backlog features are introduced.
