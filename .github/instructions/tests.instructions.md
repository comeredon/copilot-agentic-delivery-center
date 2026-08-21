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

- Domain invariants (a work item cannot be authorised before the gate passed).
- Determinism (identical input produces an identical canonical hash).
- Idempotency (a second sync with unchanged content changes nothing and invalidates nothing).
- Layering (the architecture tests).
- Guardrails (a non-owner authorisation attempt is refused *and* audited).

## What not to do

- Do not weaken an assertion to make a test pass. If the test is right and the code is wrong, fix
  the code. If the test is wrong, say so explicitly and explain why.
- Do not add `[Fact(Skip = ...)]` to get a green run. A skipped test is a failing test with extra
  steps.
- Do not mock what you own the port for — use the fakes in `tests/DeliveryControlCenter.UnitTests/Fakes`.
