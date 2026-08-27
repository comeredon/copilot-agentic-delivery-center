---
name: rubber-duck-gate
description: Request an independent critique of a plan or an implementation from a second agent, and act on the result. Use after any non-trivial plan, after a complex implementation, and after adding a new test suite. Do NOT use as a substitute for the quality gate — this is advisory and cannot block on its own.
---

# The rubber duck gate

Explaining a design to something that will push back is how you find the assumption you did not
know you were making. Here that something is a different agent, with a different model where
possible, and no stake in the design being right.

## When to duck

| Trigger | Duck with |
|---|---|
| Plan touches three or more projects, a boundary, or a dependency | `architecture-reviewer` |
| Plan changes the determinism or guardrail model | `architecture-reviewer` **and** `threat-modeler` |
| Implementation is complex or subtle | `architecture-reviewer` |
| New test suite added | `test-engineer` (a different session than the one that wrote it) |
| Customer auth, checkout/payment, reviews/uploads, AI, or the ledger | `threat-modeler` |

Skip it for a one-file change with no new behaviour. Ducking everything is how a critique stops
being read.

## How to ask

A weak prompt gets a weak critique. Give the duck:

1. The **problem**, not just the solution.
2. The **design**, with the trade-off you made and the alternative you rejected.
3. The **assumptions** you are aware of.
4. A direct instruction to disagree: *"Tell me what breaks. If nothing breaks, say so in one line
   and stop."*

Do not ask "does this look good?". That invites agreement, and agreement is worthless here.

## How to act on the answer

| Duck says | You do |
|---|---|
| Critical or high finding | Revise before implementing. Do not proceed. |
| Medium finding | Revise, or record an explicit written justification for not revising. |
| Low or info | Note it in evidence. Proceed. |
| "This is fine" | Proceed, and record that the critique happened and found nothing. |

Record the critique either way. A duck that found nothing is evidence that the design was examined
— that is the whole point of the audit trail.

## Rules

- The duck is never the agent that produced the work.
- The duck cannot edit. It critiques; you revise.
- The duck is **advisory**. It cannot block delivery on its own — only the deterministic quality
  gate blocks. This is deliberate: a non-deterministic judge should not have a veto.
- Disagreeing with the duck is allowed. Ignoring it silently is not. Write down why.
