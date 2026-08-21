---
name: record-gate-ledger
description: Append a hash-chained ledger entry capturing a proposer's and an independent verifier's verdict on a gate. Use after every quality gate run that is going to inform a merge decision. Do NOT use to record a single agent's opinion — an entry requires two distinct agents.
---

# Record a gate ledger entry

The ledger is what makes "agents verified each other" checkable rather than claimed. Each entry
binds a gate outcome to two named agents and chains to the entry before it, so an entry cannot be
inserted, reordered, or quietly rewritten later.

## The entry

```jsonc
{
  "sequence": 12,
  "gateId": "workitem-<guid>-<commit>",
  "evidenceHash": "sha256:...",     // the gate report's canonicalHash
  "proposer": { "agent": "dotnet-implementer", "model": "...", "verdict": "pass" },
  "verifier": { "agent": "quality-verifier",   "model": "...", "verdict": "pass" },
  "agreed": true,
  "previousEntryHash": "sha256:...",
  "entryHash": "sha256:..."
}
```

Schema: `schemas/gate-ledger.schema.json`. Domain type: `GateLedgerEntry`.

## Procedure

1. Read the current tip with `FindTipAsync`. Its `EntryHash` becomes your `previousEntryHash`. The
   first entry uses `GateLedgerEntry.GenesisHash`.
2. `sequence` is tip + 1. Sequences are unique and contiguous — a gap means an entry was lost.
3. `evidenceHash` is the gate report's `canonicalHash`. Do not invent a hash and do not reuse one
   from a different run; the whole point is that the entry points at a specific verdict.
4. Call `GateLedgerEntry.Append`. It computes `entryHash` over the whole payload including the
   previous hash.
5. Persist it. Verify with `VerifyIntegrity()` before you report success.

## The rule the code enforces

`Append` throws if `proposer.Agent == verifier.Agent`. An agent cannot verify its own work, and
that is enforced in the domain rather than in a policy document, so there is no path around it.

If you hit that exception, you have routed the flow wrong — get a different agent to verify. Do
not rename the agent to satisfy the check.

## Disagreement

If proposer and verifier disagree, `agreed` is `false`. **Still record the entry.** A recorded
disagreement is the most valuable thing in the ledger: it is proof that the two agents were
genuinely independent. Then escalate to a human — a disagreement is not something to resolve by
asking a third agent to break the tie.

## Never

- Never rewrite an existing entry. The chain is append-only; a rewrite invalidates every entry
  after it, which is exactly the property you want.
- Never fabricate a verifier verdict on behalf of another agent.
- Never skip the entry because the gate passed. A pass with no ledger entry is unverified.
