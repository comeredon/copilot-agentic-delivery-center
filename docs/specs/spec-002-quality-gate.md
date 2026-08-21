# SPEC-002 — Quality gate and ledger

## Problem

"The agent says it works" is not an acceptable basis for a merge decision. We need a verdict that is
reproducible, content-addressed, honest about what it did not check, and independently corroborated
by a second agent.

## Requirements

| ID | Requirement |
|---|---|
| REQ-101 | A single command produces the authoritative verdict for a commit. |
| REQ-102 | The report separates a reproducible `canonical` section from a non-reproducible `envelope`; only `canonical` is hashed. |
| REQ-103 | Two runs of the same commit with the same outcomes produce an identical `canonicalHash`, regardless of timing. |
| REQ-104 | The report validates against `schemas/quality-report.schema.json` before it is written; a validation failure exits `2`. |
| REQ-105 | Profiles are ordered `cloud < ci < local`; a weaker profile cannot satisfy a stronger requirement. |
| REQ-106 | A check that could not run is recorded as skipped and not-required, never as passed. |
| REQ-107 | Ledger entries are hash-chained: each entry's hash covers its predecessor's hash. |
| REQ-108 | A ledger entry whose proposer and verifier are the same agent is refused by the domain. |
| REQ-109 | A disagreement between proposer and verifier is recorded, not discarded. |
| REQ-110 | Check ordering in the report is deterministic and independent of execution order. |

## Non-goals

- Replacing GitHub's required status checks. The gate informs the merge decision; branch protection
  enforces it.
- Auto-remediation. The gate reports; `repair-orchestrator` repairs.
- Cryptographic signing of reports. Hash chaining gives tamper-evidence, which is what is needed
  here; signing would need a key, and a key needs custody.

## Design

### Determinism

The report has two parts:

```jsonc
{
  "canonical": { "commit", "profile", "verdict", "checks" },   // hashed
  "canonicalHash": "sha256:...",
  "envelope":  { "generatedAtUtc", "toolVersion", "durationsMs" }  // not hashed
}
```

`CanonicalJson.Serialize` sorts object keys ordinal, emits no insignificant whitespace, and
preserves array order (arrays are ordered data; reordering them would change meaning). Checks are
sorted by name before serialisation, so the order the runner happened to execute them in cannot
affect the hash.

Failure details are truncated and filtered to error lines, sorted and deduplicated — full output
contains paths and timings that would break reproducibility.

### Profiles

`cloud` has no container runtime. Rather than pretending, the integration-test check is emitted with
`required: false` and `passed: false` and a detail explaining exactly that. The verdict computation
ignores non-required checks, so the run can still pass — but the report makes the gap visible, and
`SatisfiesRequirement` refuses to let a `cloud` pass stand in for a `ci` one.

This is the honest version of "exclude unavailable controls": the control is not silently dropped,
it is recorded as not exercised.

### The ledger

`GateLedgerEntry.Append` computes SHA-256 over sequence, gate id, both canonical verdicts, the
evidence hash, and the previous entry hash. Rewriting any entry invalidates every entry after it,
which is the tamper-evidence property.

The proposer/verifier check lives in the domain and throws. It is not a policy an orchestrator can
forget — there is no code path that produces an entry without it.

## Verification

| Requirement | Test |
|---|---|
| REQ-101 | Manual: `dotnet run --project tools/DeliveryControlCenter.QualityGate -- --dry-run --profile ci --commit abc1234` |
| REQ-102 | `ReportBuilderTests.CanonicalHash_IgnoresTimestampsAndDurations` |
| REQ-103 | `ReportBuilderTests.CanonicalHash_IgnoresTimestampsAndDurations`, `CanonicalJsonTests.Hash_IsStableAndPrefixed` |
| REQ-104 | `ReportBuilderTests.Report_ValidatesAgainstTheCommittedSchema`, `ReportBuilderTests.Validator_RejectsAMalformedReport` |
| REQ-105 | `GateReportTests.SatisfiesRequirement_ComparesProfileStrength` |
| REQ-106 | `ReportBuilderTests.Verdict_IgnoresOptionalChecks` |
| REQ-107 | `GateLedgerEntryTests.Append_ChainsToThePreviousEntry`, `Append_StartsFromGenesis` |
| REQ-108 | `GateLedgerEntryTests.Append_RefusesSelfVerification` |
| REQ-109 | `GateLedgerEntryTests.Agreed_IsFalseWhenTheVerifierDisagrees` |
| REQ-110 | `GateReportTests.Checks_AreSortedOnIngestSoTheOrderIsStable`, `CanonicalJsonTests.Serialize_IsIndependentOfInsertionOrder` |

## Open questions

- The gate shells out to `dotnet` with a fixed command set. That set is hard-coded deliberately —
  nothing takes a command string from an agent or from configuration — but it does mean adding a
  check requires a code change and a human review. That trade-off is intentional.
- `JsonSchema.Net` registers schemas in a process-wide registry that rejects duplicate `$id`
  registration. `ReportValidator` caches per path to work around it. Worth revisiting if more
  schemas are validated in one process.
