---
name: publish-agent-evidence
description: Normalize review, threat, and analysis findings into the machine-checkable evidence schema. Use whenever an agent produces a finding that should survive the session. Do NOT use for the gate verdict itself — that is the quality report, not evidence.
---

# Publish agent evidence

Prose findings cannot be validated, deduplicated, correlated across agents, or hashed. Every
finding that matters becomes a record against `schemas/agent-evidence.schema.json`.

## The record

```jsonc
{
  "source": "threat-model",        // build | test | analyzer | architecture | threat-model | review | documentation
  "severity": "high",              // info | low | medium | high | critical
  "summary": "Issue body content is passed to the project field writer without validation, so an attacker-authored issue can set an arbitrary field value.",
  "model": "claude-opus-5",
  "references": ["src/DeliveryControlCenter.Infrastructure/GitHub/GuardedProjectWriter.cs#L30"]
}
```

## Writing the summary

State, in this order: **what is wrong**, **the precondition**, **the consequence**. A summary that
stops at "what is wrong" leaves the reader to guess whether it matters.

Bad: *"Input validation could be improved."*
Good: *"`evidenceValue` reaches the project writer unvalidated; any user who can open an issue can
therefore set an arbitrary Project field value once writes are enabled."*

## Choosing severity

Severity is about consequence, not about how hard the fix is.

| Severity | Means |
|---|---|
| `critical` | Guardrail bypass, credential exposure, or an unauthorised merge path |
| `high` | Authorisation weakness, determinism break, or data loss |
| `medium` | Correctness defect with a workaround, or a missing test on a stated requirement |
| `low` | Maintainability or clarity, no behavioural impact |
| `info` | An observation worth recording, including "I looked at X and found nothing" |

Record the `info` findings. "Reviewed the sync path for SSRF; the base address is fixed in options
and is not caller-influenced" proves the surface was examined, and that is what an auditor asks for.

## Rules

- Every finding needs a `references` entry with a file and a line. A finding without a location
  cannot be acted on.
- One finding per record. Do not bundle three problems into one summary.
- Never put a token, a credential, a full response body, or raw untrusted content in `summary`.
  Describe it; do not reproduce it.
- Deduplicate before publishing. The same defect from two reviewers is one record at the higher
  severity with both references.
- The `model` field is required — it is how a later reader knows which reasoning produced the claim.
