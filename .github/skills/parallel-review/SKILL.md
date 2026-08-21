---
name: parallel-review
description: Fan out independent test and threat review across agents, then synthesize the findings without double-counting. Use after an implementation is complete and before the quality gate. Do NOT use for sequential dependent work, or when more than three workers would be needed.
---

# Parallel review

Test review and threat review are genuinely independent — neither needs the other's output — so
they run at the same time. Everything else in this repository's flow is sequential, and pretending
otherwise just produces conflicts.

## What may run in parallel

| Safe | Reason |
|---|---|
| `test-engineer` + `threat-modeler` | Different concerns, and `threat-modeler` cannot edit |
| Multiple read-only reviewers | No writes, no conflicts |

## What must not

| Unsafe | Reason |
|---|---|
| Two editing agents on overlapping files | Silent conflicts neither can see |
| `dotnet-implementer` + `blazor-specialist` on one feature | Web depends on the API shape |
| Anything alongside `quality-verifier` | The gate must judge a settled tree |

Hard cap: **three** parallel workers. Beyond that the synthesis costs more than the parallelism
saves.

## Procedure

1. Confirm the tree is settled. No in-flight edits.
2. Give each reviewer the *same* context: the change, the plan, the acceptance criteria. Different
   inputs produce findings you cannot compare.
3. Give each an explicit, non-overlapping remit. `test-engineer` covers coverage and test integrity;
   `threat-modeler` covers abuse and exposure. Say so, or you will get two versions of the same
   finding.
4. Collect both reports before judging either. Reading them one at a time anchors you.

## Synthesis

- **Deduplicate.** The same defect found by two reviewers is one finding, recorded at the higher
  severity, with both references.
- **Reconcile conflicts.** If they disagree, do not average. State both positions and escalate to
  the orchestrator; a disagreement between reviewers is signal, not noise.
- **Rank by consequence**, not by reviewer confidence.
- **Route**: `critical` and `high` go back to an implementer before the gate. `medium` is a
  judgement call with written justification. `low` and `info` become evidence.

Normalise every finding through the `publish-agent-evidence` skill so the shapes match. Prose
findings cannot be validated, correlated, or hashed.
