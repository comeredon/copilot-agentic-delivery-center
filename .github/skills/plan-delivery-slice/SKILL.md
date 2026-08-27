---
name: plan-delivery-slice
description: Produce an implementation plan with numbered acceptance criteria, layer sequencing, and a test-to-requirement map. Use before implementing anything larger than a single-file change. Do NOT use for issue triage (use scope-agent-issue) or for judging finished work (use run-quality-gate).
---

# Plan a delivery slice

The plan is what lets a verifier disagree with an implementer on facts rather than on taste.

## Structure

### 1. Restate the problem
One paragraph in your own words. If you cannot restate it, you have not understood it, and
implementing now is guessing.

### 2. Sequence by layer
Always inward-out, because a boundary mistake found in `Domain` is minutes and the same mistake
found in `Api` is a redesign:

```
Domain → Contracts → Application → Infrastructure → Api/Worker → Web
```

For each layer, name the specific types and methods that change. "Update the application layer" is
not a plan.

### 3. Map every requirement to a test

| Requirement | Test | Layer |
|---|---|---|
| REQ-001 | `CoffeeCatalogEndpointsTests.Catalog_FilterByCountryAndMinimumScore_ReturnsOnlyMatchingCoffees` | Integration |

A requirement with no test is either untestable (reword it) or unowned (assign it).

### 4. Name the risks
What breaks that is not obviously related? Cache invalidation, migration ordering, determinism of
the gate report, and the ledger chain are the recurring ones here.

Before sequencing, compare the issue to `PLAN.md`. Persisted carts, checkout, payment, realtime
stock, accounts, reviews, uploads, and AI guidance are backlog, not helpers that can be assumed.

### 5. Define the checkpoints
Where you will stop and build. At minimum after `Domain` and after `Infrastructure`.

## Rubber duck

If the change is architectural — three or more projects, a new boundary, a new dependency, or any
change to the determinism or guardrail model — the plan goes to a rubber-duck critique before
implementation. See the `rubber-duck-gate` skill. Do not skip it because the plan feels obvious;
the plans that feel most obvious are the ones that hide an assumption.

## What a bad plan looks like

- Steps that are restatements of the issue.
- No non-goals.
- No named tests.
- A step called "refactor as needed" — that is unbounded scope wearing a plan's clothing.
