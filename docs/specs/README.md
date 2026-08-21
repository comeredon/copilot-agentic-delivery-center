# Feature specifications

Each specification is the contract between a scoped issue and a verifier. Requirements are numbered
`REQ-###` and every one maps to a named test.

| Spec | Status | Covers |
|---|---|---|
| [SPEC-001 — Delivery synchronisation](spec-001-delivery-sync.md) | Implemented | Polling, ETags, cursors, idempotency |
| [SPEC-002 — Quality gate and ledger](spec-002-quality-gate.md) | Implemented | Deterministic report, hash chain, independent verification |
| [SPEC-003 — Delivery dashboard](spec-003-dashboard.md) | Implemented | Blazor UI, realtime hints, human authorisation |
| [SPEC-004 — Agent guardrails](spec-004-guardrails.md) | Implemented | Hook policy, audit trail, protected paths |

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
