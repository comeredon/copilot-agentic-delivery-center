---
description: Request an adversarial critique of a plan or implementation from an independent agent.
mode: agent
---

# Rubber duck this

Get an independent critique before committing to this direction.

## What to hand the duck

1. **The problem** — not just your solution. A duck given only the solution can only critique the
   solution's internals, not whether it solves the right thing.
2. **The design** — including the trade-off you made and the alternative you rejected.
3. **Your assumptions** — the ones you are aware of. The valuable finding is usually one you were
   not aware of.
4. **A direct instruction to disagree**: *"Tell me what breaks. If nothing breaks, say so in one
   line and stop."*

Never ask "does this look good?". That invites agreement, and agreement here is worthless.

## Who to ask

| Situation | Duck |
|---|---|
| Boundary, dependency, or three-plus projects | `architecture-reviewer` |
| Determinism or guardrail model | `architecture-reviewer` **and** `threat-modeler` |
| New test suite | `test-engineer`, in a separate session from the one that wrote it |
| Authorisation or the ledger | `threat-modeler` |

The duck is never the agent that produced the work, and the duck cannot edit.

## Acting on the answer

| Finding | Action |
|---|---|
| `critical` / `high` | Revise before implementing |
| `medium` | Revise, or write down why you are not |
| `low` / `info` | Record as evidence, proceed |
| Nothing found | Record that the critique happened and found nothing |

Record it either way. The critique is advisory — only the deterministic gate can block — but an
unrecorded critique is indistinguishable from one that never happened.
