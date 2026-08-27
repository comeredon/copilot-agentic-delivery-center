---
description: Run the deterministic quality gate and report the verdict, hash, and skipped checks.
mode: agent
---

# Verify with the quality gate

Delegate to `quality-verifier` and require it to follow `run-quality-gate` with profile
`${input:profile:ci}` and commit `${input:commit}`. The skill is the single source for command,
profile, determinism, report, and exit-code behavior.

After the verdict, invoke `record-gate-ledger` in local-proof or connected-publication mode as
reported by `preflight-github-delivery`. Do not duplicate those procedures here and do not claim a
durable entry unless it was read back from a configured ledger sink.
