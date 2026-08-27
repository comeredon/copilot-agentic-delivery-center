---
description: Run the honest local or connected Ninth Parallel Copilot demonstration, including capability preflight and safe failure-path evidence.
mode: agent
---

# Demonstrate the harness

Run the full demonstration described in the `demo-copilot-harness` skill.

Treat the skill as the single procedure; do not duplicate or improvise a second script here.

Start with `preflight-github-delivery` and report the selected mode. In disconnected mode, complete
the local store/Learn/agent/review/gate/guardrail/ledger-proof scenes and emit dry-run GitHub deltas.
In connected mode, add only the operations whose tools and sinks passed preflight.

Do not execute forbidden commands, read credential-shaped files, edit guardrails, or deliberately
break the shared worktree. Use the guardrail fixtures and a human-prepared failing branch when those
scenes are available. End at the human merge boundary.
