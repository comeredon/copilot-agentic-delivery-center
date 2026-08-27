---
name: demo-copilot-harness
description: Demonstrate Copilot evolving the Ninth Parallel coffee store through Learn research, bounded agents, independent critique, parallel review, deterministic gates, evidence, and the human merge boundary. Supports honest local and connected GitHub modes.
---

# Demonstrate the harness end to end

Open on the store, not on the harness. The audience should first see a real specialty-coffee product
and then watch bounded engineering agents evolve it. The demonstration must distinguish what is
proved locally from what was actually published to GitHub.

## 1. Choose the mode

Run `preflight-github-delivery` before promising issue, Project, pull-request, evidence, or ledger
publication.

| Mode | What can be claimed |
|---|---|
| Local | Store runtime, Learn MCP, agent boundaries, independent critique, tests, guardrail decision fixtures, deterministic gate, local evidence artifacts, ledger domain invariant |
| Connected | Everything in local mode plus GitHub issue/Project deltas, pull request, durable evidence, and ledger publication when each required tool/sink passes preflight |

If preflight is disconnected, do not fake the connected scenes. Show the intended deltas and name
the missing capability.

## 2. Establish the product baseline

```
dotnet restore --locked-mode
dotnet build DeliveryControlCenter.slnx
dotnet run --project src/DeliveryControlCenter.AppHost
```

Open the home, shop, coffee detail, producer, Brew Lab, transparency, and cart journeys. Point out
that the store contains no GitHub or Copilot UI. PostgreSQL, Redis, migrations, API, Worker, Web,
health, logs, and traces are visible through Aspire. The 12-coffee seed makes this product scene
repeatable without GitHub credentials.

## 3. Deliver one bounded coffee issue

1. Pick one issue from `docs/demo/coffee-store-backlog.md`. Use `scope-agent-issue` to show numbered
  requirements, test mapping, and explicit non-goals.
2. Use `research-microsoft-dotnet` for one real API decision. Show the Learn result and the pinned
  repository version it was checked against.
3. Hand the issue to `delivery-orchestrator`. Show that it delegates production and test edits to
  non-overlapping specialists. It has no `edit` tool, but say plainly that its `execute` restriction
  against shell writes is policy until the hook is hardened.
4. Use `rubber-duck-gate` on a non-trivial plan. Ask it to disagree and record the result even when
  no defect is found.
5. Implement through the narrowest specialist. Keep backend HTTP contract work ahead of dependent
  Web work.
6. Run `parallel-review`: `test-engineer` and `threat-modeler` receive the same requirements and
  return independent findings. Normalize them with `publish-agent-evidence`.

## 4. Prove deterministic verification

Run the canonical gate at the strongest environment profile, then perform the predeclared second
run or use the determinism fixture:

```powershell
pwsh -NoProfile -File eng/tests/Test-GateDeterminism.ps1 -Profile local
```

Pause on identical canonical hashes and different envelope timestamps. The verdict is
content-addressed; runtime timing is deliberately outside the hash.

## 5. Prove independent verdicts without overstating persistence

```powershell
dotnet test tests/DeliveryControlCenter.UnitTests --filter "FullyQualifiedName~GateLedgerEntryTests"
```

Show `GateLedgerEntry.Append` refusing self-verification, chaining two records, retaining a
disagreement, and producing a deterministic entry hash. This proves the domain invariant only.
Use `record-gate-ledger` for durable publication **only** when preflight found a trusted append-only
sink; none ships in the local baseline.

## 6. Show guardrail decisions safely

```powershell
pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1
```

The fixture exercises deny and allow payloads without executing destructive commands. It proves the
PowerShell decision function, not Bash parity, host wiring, or branch protection. Show the configured
hook and workflow, state that distinction plainly, and never ask an agent to attempt a merge, force
push, credential read, or guardrail edit for theatre.

The `eng/agent-hooks/**` and `eng/tests/**` paths are human-owned but are not yet in the local
pre-tool deny patterns. The fixture itself can therefore be weakened by a policy-violating agent;
confirm those scripts match a human-reviewed revision before using them as stage evidence. Call
this out as a hardening item rather than claiming complete mechanical coverage.

## 7. Optional repair scene

Use a human-prepared disposable branch or an existing failing pull request. Do not break the shared
demo worktree merely to manufacture drama. Show the first failure, diagnosis, delegated repair, and
the two-attempt stop. If no prepared failure exists, show the repair procedure without claiming it
was exercised.

## 8. Close on the boundary

In connected mode, sync only changed Project fields, open the pull request with gate/evidence links,
and stop. A human may merge through GitHub after the agent turn. In local mode, close on the green
gate, dry-run GitHub delta, and explicit list of missing connected capabilities.

Never call a local-only run the full connected demonstration. Honest boundaries are part of what
the demo is proving.
