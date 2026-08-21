# AGENTS.md — tests/

xUnit v3, Shouldly, NetArchTest, Testcontainers.

## Projects

| Project | Covers | Needs |
|---|---|---|
| `UnitTests` | Domain invariants, application handlers via fakes | nothing |
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
- Reuse `UnitTests/Fakes` rather than writing new fakes.

## What is worth testing here

The guarantees, not the plumbing: domain invariants, determinism of the canonical hash, idempotency
of a repeat sync, that a refused action is both refused *and* audited, and that a cache failure
degrades to a direct read.
