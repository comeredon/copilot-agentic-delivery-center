# Feature specifications

Each specification is the contract between a scoped issue and a verifier. Requirements are numbered
`REQ-###` and every one maps to a named test.

| Spec | Status | Covers |
|---|---|---|
| [SPEC-005 — Ninth Parallel coffee store](spec-005-coffee-store.md) | Implemented baseline | Catalog, detail, traceability, recipes, cart |
| [SPEC-002 — Quality gate and ledger](spec-002-quality-gate.md) | Gate implemented; ledger proof local only | Deterministic report, hash-chain invariant, independent verdict rule |
| [SPEC-004 — Agent guardrails](spec-004-guardrails.md) | Implemented with human hardening gaps | Hook decision fixtures, local audit trail, protected paths |
| [SPEC-001 — Delivery synchronisation](spec-001-delivery-sync.md) | Superseded product slice | Historical pre-pivot implementation |
| [SPEC-003 — Delivery dashboard](spec-003-dashboard.md) | Superseded product slice | Historical pre-pivot UI |

## How to write one

Use the shape below. The section that gets skipped most often — and matters most — is **non-goals**.

```markdown
# SPEC-00N — Title

## Problem
## Requirements     (REQ-001, REQ-002, ... each independently testable)
## Non-goals
## Design
## Verification     (REQ → test mapping table)
## Open questions
```

A requirement that cannot be mapped to a test is not a requirement; it is a wish. Reword it or drop
it.
