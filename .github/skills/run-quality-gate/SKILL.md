---
name: run-quality-gate
description: Invoke the deterministic quality gate with the correct profile and interpret its verdict, canonical hash, and skipped checks. Use whenever you need an authoritative pass/fail judgement on a change. Do NOT assemble a verdict yourself from individual build or test output.
---

# Run the quality gate

The gate is the only thing that decides whether a change is acceptable. Individual command output
tells you what happened; the gate tells you what it means.

## The command

```
dotnet run --project tools/DeliveryControlCenter.QualityGate -- --profile <cloud|ci|local> --commit <sha>
```

Optional: `--output <path>` (default `artifacts/quality-report.json`), `--dry-run` (records the
check set without executing — use it to inspect the report shape, never to claim a verdict).

## Choosing the profile

| Profile | Environment | Integration tests |
|---|---|---|
| `cloud` | Cloud agent — no container runtime | Skipped, recorded as not-required |
| `ci` | GitHub Actions with containers | Required |
| `local` | Developer machine | Required |

Ordered `cloud < ci < local`. **A `cloud` pass does not satisfy a `ci` requirement.** The cloud
profile genuinely did not run the integration tests, and the report says so — do not let the word
"pass" imply coverage that did not happen.

## Reading the report

```jsonc
{
  "schemaVersion": "1.0.0",
  "canonical": { "commit": ..., "profile": ..., "verdict": ..., "checks": [...] },
  "canonicalHash": "sha256:...",
  "envelope": { "generatedAtUtc": ..., "toolVersion": ..., "durationsMs": {...} }
}
```

Only `canonical` is hashed. `envelope` holds everything non-reproducible, which is why two runs of
the same outcome produce the same hash. If you ever see the hash change between two runs of the
same commit with the same results, something non-deterministic has leaked into `canonical` — that
is a `high` severity finding and it blocks.

## Exit codes

| Code | Meaning | Do |
|---|---|---|
| `0` | Verdict `pass` | Proceed to the ledger entry |
| `1` | Verdict `fail` or `inconclusive` | Route to `repair-orchestrator` |
| `2` | Report failed schema validation | Treat as a defect in the gate tool itself; escalate |

Exit code `2` is not a soft failure. A report that does not validate cannot be trusted to mean
anything, including its own verdict.

## Reporting

Always report: verdict, canonical hash, profile, per-check results, and which checks were skipped
and why. Never paraphrase a `fail`. Never re-run hoping for a different answer — the first honest
run is the answer.
