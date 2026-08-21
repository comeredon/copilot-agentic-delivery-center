---
description: Run the deterministic quality gate and report the verdict, hash, and skipped checks.
mode: agent
---

# Verify with the quality gate

Run the canonical gate and report exactly what it said.

## Run it

```
dotnet run --project tools/DeliveryControlCenter.QualityGate -- --profile ${input:profile:ci} --commit ${input:commit}
```

Pick the profile by what this environment can actually do. `cloud` has no container runtime, so
its integration-test check is skipped and recorded as not-required.

## Report

1. **Verdict** — the gate's word: `pass`, `fail`, or `inconclusive`. Do not paraphrase it.
2. **Canonical hash** — this is the identity of the verdict.
3. **Profile**, and explicitly what that profile did not cover.
4. **Per-check results**, including every skipped check and why it was skipped.

## Determinism

If the change touched anything the gate hashes, run it twice and compare the canonical hash. A
different hash for the same commit with the same outcomes means something non-reproducible leaked
into the canonical section. That is a `high` severity finding and it blocks.

## Then the ledger

Append a gate ledger entry. You are the verifier; the agent that produced the work is the proposer.
`GateLedgerEntry.Append` refuses if they are the same — that refusal is the control working, not an
obstacle to route around.

If the two verdicts disagree, still record the entry with `agreed: false`, then escalate to a human.
A recorded disagreement is the most valuable thing the ledger can hold.

## Never

Never re-run hoping for a better answer. Never summarise a `fail` as "mostly passing". Never edit
anything to change the verdict — you have no `edit` tool for exactly this reason.
