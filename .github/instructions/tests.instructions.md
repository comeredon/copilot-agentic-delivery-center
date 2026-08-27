---
description: Test conventions — naming, isolation, and what counts as evidence.
applyTo: "tests/**"
---

# Test conventions

Stack: xUnit v3, Shouldly, NetArchTest, Testcontainers.

## Naming

`MethodOrBehaviour_ExpectedOutcome`. The test name should read as a sentence describing the rule
being protected, for example `Append_RefusesSelfVerification`.

## Isolation

- No test depends on the execution order of another test.
- No test writes to a shared path outside its own temp directory.
- No `Task.Delay` as a synchronisation mechanism. If you are tempted to sleep, you have a design
  problem: expose a completion signal instead.
- Integration tests use Testcontainers with a pinned image tag. Never `latest`.
- Use `TestContext.Current.CancellationToken` rather than `CancellationToken.None` in tests that
  perform real I/O, so a hung test is cancelled rather than blocking the run.

## What to assert

Assert on behaviour and invariants, not on implementation detail. Good targets in this repository:

- Coffee invariants (cup score, publish readiness, recipe bounds, stock never negative).
- Catalog behavior (search escaping, filters, facets, sorting, paging, stable slugs).
- Current Web state (`StoreCart` quantities, stock cap, removal, subtotal).
- Determinism (identical input produces an identical canonical hash).
- Layering (the architecture tests).
- Caching (equivalent filters create stable keys; Redis failure degrades).

Checkout idempotency, order price snapshots, concurrent reservation, and write invalidation become
required targets only when the scoped issue introduces those behaviors.

## What not to do

- Do not weaken an assertion to make a test pass. If the test is right and the code is wrong, fix
  the code. If the test is wrong, say so explicitly and explain why.
- Do not add `[Fact(Skip = ...)]` to get a green run. A skipped test is a failing test with extra
  steps.
- Search for an existing fixture or helper before adding one. There is no baseline shared `Fakes`
  directory; add only the smallest test double required by the issue.
