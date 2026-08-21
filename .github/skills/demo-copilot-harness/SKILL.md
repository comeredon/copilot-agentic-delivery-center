---
name: demo-copilot-harness
description: Drive the full end-to-end demonstration of the agentic delivery flow — orchestration, delegation, rubber duck, parallel review, deterministic gate, ledger, and the human merge boundary. Use when showing the harness to an audience or validating that every control still works after a change.
---

# Demonstrate the harness end to end

The demonstration has to prove three things that are easy to claim and hard to show: the agents are
genuinely bounded, they genuinely check each other, and the verdict is genuinely reproducible.

## Before you start

```
dotnet build DeliveryControlCenter.slnx
dotnet run --project src/DeliveryControlCenter.AppHost
```

The app runs with `GitHub:Enabled=false` by default, so it works with no credentials — the seeded
reader supplies a fixed payload, which also makes the demo repeatable.

## The run

### 1. Scope an issue
`scope-agent-issue`. Show the acceptance criteria and, more importantly, the **non-goals**. Point
out that the scoping step is what prevents a runaway session.

### 2. Orchestrate
Hand it to `delivery-orchestrator`. Show its tool list: no `edit`, no merge tool. Say why — an
orchestrator that can edit will edit, and the change then skips review.

### 3. Delegate
Show the routing table picking the smallest sufficient flow. Show `blazor-specialist` and
`dotnet-implementer` being chosen by area rather than run together.

### 4. Rubber duck
`rubber-duck-gate`. Ask the duck to disagree, explicitly. Show what it found — and if it found
nothing, show that the critique is still recorded. The record is the point.

### 5. Parallel review
`test-engineer` and `threat-modeler` at the same time, capped at three workers. Show the
synthesis deduplicating a finding both of them raised.

### 6. The deterministic gate
Run it **twice**:

```
dotnet run --project tools/DeliveryControlCenter.QualityGate -- --profile local --commit <sha>
dotnet run --project tools/DeliveryControlCenter.QualityGate -- --profile local --commit <sha>
```

Identical canonical hash, different `envelope.generatedAtUtc`. That contrast is the clearest single
demonstration in the whole run: the verdict is content-addressed, the timing is not part of it.

### 7. The ledger
Show `GateLedgerEntry.Append` refusing when proposer and verifier are the same agent. That refusal
lives in the domain, not in a policy document — try it live.

### 8. The wall
Have the orchestrator attempt to merge. The hook denies it. Then authorise as the repository owner
through the UI and show the audit event for both the refused and the allowed attempt.

## Show the failure paths too

A demo where nothing goes wrong proves nothing:

- Attempt an edit to `.github/hooks/guardrails.json` → denied.
- Attempt `git push --force` → denied.
- Attempt to read a `.env` file → denied.
- Break a test deliberately → gate fails, `repair-orchestrator` runs, and after two failed attempts
  it stops and labels `needs-human` rather than looping.

## Close on the boundary

The last slide is the pull request sitting at a passing gate, waiting. Every control in this
repository exists to make that moment trustworthy — and a human still makes the call.
