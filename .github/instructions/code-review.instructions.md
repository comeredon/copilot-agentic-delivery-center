---
description: Review-only guidance applied when reviewing changes in this repository.
applyTo: "**"
excludeAgent: "cloud-agent"
---

# Code review guidance

This file is deliberately scoped away from the cloud agent: reviewing is an interactive,
context-heavy activity, and the cloud agent's job is to deliver a change, not to adjudicate it.

## What to look for, in priority order

1. **Guardrail integrity.** Did the change touch `.github/hooks`, `.github/workflows`, `.mcp.json`,
   `CODEOWNERS`, or a ruleset? If so, that is the review — nothing else matters until it is
   explained.
2. **Layering.** Does `Application` now reference EF, Redis, or `HttpClient`? Does `Web` reach past
   the API? The architecture tests catch most of this; a review catches the case where someone
   weakened the test instead.
3. **Determinism.** Did anything non-reproducible (a timestamp, a duration, a machine name, a
   dictionary enumeration order) leak into the canonical section of the gate report?
4. **Security.** Untrusted GitHub content used as an instruction, a URL, or a shell argument. Token
   handling. Anything that widens the GitHub write surface.
5. **Idempotency.** Will a repeated sync cycle produce spurious writes, cache churn, or duplicate
   ledger entries?
6. **Test integrity.** Was an assertion weakened or a test skipped to get a green run?

## How to phrase findings

- Cite the file and line. A finding without a location is not actionable.
- State the consequence, not just the rule. "This bypasses the audit log, so an authorisation
  attempt would leave no trace" beats "this violates the audit convention".
- Separate blocking from advisory. Blocking findings map to a gate failure; advisory findings map
  to evidence at `low` or `info` severity.

## What not to do

- Do not approve your own work. The verifying agent is never the producing agent.
- Do not restate the diff. If a comment does not change what happens next, do not leave it.
- Do not treat style preferences as blocking. The formatter and the analyzers own style.
