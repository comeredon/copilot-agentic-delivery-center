---
description: Diagnose and repair a failing check or quality gate, with a hard two-attempt limit.
mode: agent
---

# Repair a failing gate

Something is red. Diagnose it properly before you change anything.

## Diagnose first

1. Reproduce the failure with the exact failing command. If you cannot reproduce it, say so —
   that finding points at flakiness or environment drift, not a code defect.
2. Read the actual output and identify the **first** failure. Later failures are usually
   consequences of the first.
3. Classify it: compilation, assertion, flake, layering, determinism, or environment. The class
   determines who fixes it and whether it is a code defect at all.

## Repair

- **Attempt 1** — fix the diagnosed cause.
- **Attempt 2** — if attempt 1 failed, re-diagnose from scratch. Do not tweak the failed fix; if
  the hypothesis was wrong, a variation on it is probably also wrong.
- **There is no attempt 3.** Apply `needs-human` and write the report.

## Forbidden, without exception

- Weakening a test, widening a tolerance, adding `Skip`, or deleting an assertion.
- `--no-verify`, `git reset --hard`, force push.
- Editing `.github/hooks/**` or `.github/workflows/**` to stop a check running.

Any of these converts a visible failure into a hidden one, which is strictly worse than leaving it
red.

## Verify

Re-run the quality gate and report the verdict, the canonical hash, and the per-check results.
"Should be fixed now" is not a status.
