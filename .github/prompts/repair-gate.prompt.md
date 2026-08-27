---
description: Diagnose and repair a failing check or quality gate, with a hard two-attempt limit.
mode: agent
---

# Repair a failing gate

Delegate the failure to `repair-orchestrator`. Its role definition is the single source for
diagnosis classes, routing, forbidden shortcuts, and the two-attempt limit. Provide the exact
failing command and raw first error; do not preselect a repair hypothesis.

After a delegated repair, require `quality-verifier` to follow `run-quality-gate`. If two repair
attempts fail, stop with the orchestrator's `needs-human` report.
