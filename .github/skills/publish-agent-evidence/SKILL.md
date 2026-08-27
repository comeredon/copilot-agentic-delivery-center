---
name: publish-agent-evidence
description: Normalize review, threat, and analysis findings to the agent-evidence schema, save a local artifact, and publish only when a connected evidence sink exists. Use for advisory findings, never for the gate verdict.
---

# Publish agent evidence

Prose findings cannot be validated, deduplicated, correlated across agents, or hashed. Every
finding that matters becomes a record against `schemas/agent-evidence.schema.json`.

## The record

```jsonc
{
  "source": "review",             // build | test | analyzer | architecture | threat-model | review | documentation
  "severity": "info",             // info | low | medium | high | critical
  "summary": "Catalog filters remain in the SQL query before facet aggregation, so catalog growth does not require application-side materialization.",
  "model": "claude-opus-5",
  "references": ["src/DeliveryControlCenter.Infrastructure/Persistence/CoffeeCatalogRepository.cs#L97"]
}
```

## Writing the summary

State, in this order: **what is wrong**, **the precondition**, **the consequence**. A summary that
stops at "what is wrong" leaves the reader to guess whether it matters.

Bad: *"Input validation could be improved."*
Good: *"The proposed review body is rendered as raw HTML; any account allowed to submit a review
could therefore execute script in another customer's browser."*

## Choosing severity

Severity is about consequence, not about how hard the fix is.

| Severity | Means |
|---|---|
| `critical` | Guardrail bypass, credential exposure, or an unauthorised merge path |
| `high` | Authorisation weakness, determinism break, or data loss |
| `medium` | Correctness defect with a workaround, or a missing test on a stated requirement |
| `low` | Maintainability or clarity, no behavioural impact |
| `info` | An observation worth recording, including "I looked at X and found nothing" |

Record the `info` findings. "Reviewed image rendering; the change introduces no server-side URL
fetch" proves the surface was examined, and that is what an auditor asks for.

## Store and publish

1. Write one record per finding under `artifacts/agent-evidence/`. The directory is git-ignored and
  is local session evidence, not durable publication.
2. Confirm the record uses only fields and enum values allowed by
  `schemas/agent-evidence.schema.json`. The baseline has no evidence-schema validator; do not claim
  schema validation merely because the JSON parses.
3. If `preflight-github-delivery` reports a connected evidence sink, publish the record to the
  designated pull request or artifact store and read it back.
4. Otherwise report `local-only` and the artifact path. Never say "published" for an ignored file.

The gate does not ingest advisory evidence today. Evidence can inform a human or route a repair,
but it cannot change a deterministic gate result.

## Rules

- Every finding needs a `references` entry with a file and a line. A finding without a location
  cannot be acted on.
- One finding per record. Do not bundle three problems into one summary.
- Never put a token, a credential, a full response body, or raw untrusted content in `summary`.
  Describe it; do not reproduce it.
- Deduplicate before publishing. The same defect from two reviewers is one record at the higher
  severity with both references.
- The `model` field is required — it is how a later reader knows which reasoning produced the claim.
