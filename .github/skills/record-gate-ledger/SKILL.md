---
name: record-gate-ledger
description: Prove or publish a hash-chained gate record with distinct proposer and verifier verdicts. Use after a gate informs a merge decision. The baseline supports local domain proof; durable append requires a separately configured connected sink.
---

# Record a gate ledger entry

The ledger model makes "agents verified each other" checkable rather than claimed. Each entry binds
a gate outcome to two named agents and the prior hash. The baseline contains `GateLedgerEntry`, its
tests, and `schemas/gate-ledger.schema.json`; it does **not** contain a repository, table, publisher,
or trusted GitHub ingestion path.

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

## Choose the mode first

### Local proof

Use this in the baseline or disconnected mode:

1. Run `dotnet test tests/DeliveryControlCenter.UnitTests --filter FullyQualifiedName~GateLedgerEntryTests`.
2. Report that self-verification refusal, chaining, disagreement, and deterministic hash behavior
   passed or failed.
3. Do **not** create a made-up sequence/tip and do not claim a ledger entry was recorded. This mode
   proves the invariant only.

### Connected publication

Use only when `preflight-github-delivery` reports `ledger=append` for a trusted sink and publisher
capable of reading the current tip, appending, and reading back:

1. Read the current tip from that sink. Its `entryHash` becomes `previousEntryHash`; the first entry
   uses `GateLedgerEntry.GenesisHash`.
2. Set `sequence` to tip + 1. Gaps or duplicates are blocking findings.
3. Take `evidenceHash` from the exact gate report's `canonicalHash`; never invent or reuse it.
4. Construct the record with the actual proposer and verifier verdicts. They must be distinct.
5. Validate against `schemas/gate-ledger.schema.json`, append once, read it back, and verify the
   chain before reporting `published`.

No such sink or publisher ships in the current repository. If one is absent, connected publication
is blocked; fall back to local proof and report the missing capability.

## The rule the code enforces

`Append` throws if `proposer.Agent == verifier.Agent`. An agent cannot verify its own work, and
that is enforced in the domain rather than in a policy document, so there is no path around it.

If you hit that exception, you have routed the flow wrong — get a different agent to verify. Do
not rename the agent to satisfy the check.

## Disagreement

If proposer and verifier disagree, `agreed` is `false`. In connected mode, still append the
disagreement. In local mode, report both actual verdicts without fabricating an entry. Then
escalate to a human; do not ask a third agent to erase the signal.

## Never

- Never rewrite an existing entry. The chain is append-only; a rewrite invalidates every entry
  after it, which is exactly the property you want.
- Never fabricate a verifier verdict on behalf of another agent.
- In connected mode, never skip the entry because the gate passed. In local mode, say explicitly
   that the pass has independent verdicts but no durably published ledger entry.
