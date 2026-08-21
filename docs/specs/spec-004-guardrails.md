# SPEC-004 — Agent guardrails

## Problem

Instructions tell an agent what not to do. That is necessary and insufficient: a misaligned,
confused, or prompt-injected agent will do it anyway. We need controls that do not depend on the
agent's cooperation, plus a record of every time one of them bit.

## Requirements

| ID | Requirement |
|---|---|
| REQ-301 | Merge, automerge, and branch-protection commands are denied. |
| REQ-302 | Pushes to `main` and force pushes are denied. |
| REQ-303 | Destructive filesystem and git commands are denied. |
| REQ-304 | Verification bypass (`--no-verify`) is denied. |
| REQ-305 | Piping remote content into a shell is denied. |
| REQ-306 | Edits to `.github/hooks/**`, `.github/workflows/**`, `.mcp.json`, and `CODEOWNERS` are denied. |
| REQ-307 | Reads of credential-shaped paths are denied. |
| REQ-308 | A payload that cannot be parsed is denied — the hook fails closed. |
| REQ-309 | Ordinary builds, tests, gate runs, source edits, and branch pushes are allowed. |
| REQ-310 | Every decision is recorded to a git-ignored local audit trail containing no arguments, no outputs, and no matched paths. |
| REQ-311 | The PowerShell and Bash implementations behave identically. |
| REQ-312 | Every hook completes within 5 seconds. |

## Non-goals

- Being the authoritative control. Hooks run inside the agent's own environment. Branch protection,
  required status checks, least-privilege tokens, and the human merge signal are authoritative;
  hooks are a strong deterrent and a complete audit trail.
- Sandboxing the process. Hooks intercept tool calls, not syscalls.
- Preventing a determined human. `CODEOWNERS` and branch protection cover that.

## Design

### Policy as data

`.github/hooks/guardrails.json` holds two regex lists — `deniedCommandPatterns` and
`deniedPathPatterns` — plus the hook registrations. Keeping the patterns as committed data rather
than embedded in the scripts means the policy is reviewable in a diff, and the two platform
implementations cannot drift on *what* is denied even if they differ in *how*.

### Fail closed

`pre-tool-use` denies on an unparseable payload. If we cannot tell what is being attempted, we do
not approve it. That choice costs an occasional false denial and prevents a whole class of evasion
by malformed input.

### The audit trail records shape, not content

Records contain: timestamp, event, tool, and the *name of the pattern that matched*. They contain
no command arguments, no file contents, no outputs, and — critically — no matched path. A matched
path may itself be a credential location, so echoing it into a log would leak the thing the rule
exists to protect.

`.agent-runtime/` is git-ignored. It is a local forensic record, not an artifact.

### Auditing never breaks a session

Every audit write is wrapped and swallows its own failures. A lost audit line is preferable to a
blocked agent, because a hook that can fail a session is a hook people will want to disable.

### Two implementations, one behaviour

PowerShell for Windows, Bash for Linux and macOS. They read the same policy file and produce the
same decision, because the platform an agent happens to run on must not change what it is allowed
to do.

## Verification

`eng/tests/Test-Guardrails.ps1` — 20 fixture cases, run in CI by `.github/workflows/agent-guardrails.yml`.

| Requirement | Fixture case |
|---|---|
| REQ-301 | `merge command`, `automerge` |
| REQ-302 | `force push`, `push to main` |
| REQ-303 | `hard reset`, `recursive delete` |
| REQ-304 | `no-verify commit` |
| REQ-305 | `curl pipe shell` |
| REQ-306 | `edit hooks`, `edit workflows`, `edit mcp config`, `edit CODEOWNERS` |
| REQ-307 | `read dotenv`, `read private key` |
| REQ-308 | `malformed payload` |
| REQ-309 | `ordinary build`, `ordinary test`, `quality gate`, `ordinary source edit`, `branch push` |
| REQ-310 | Inspection of `common.ps1` / `common.sh` — only tool, event, and pattern name are written |
| REQ-311 | Currently verified by inspection; see open questions |
| REQ-312 | `timeoutSeconds: 5` on every registration |

Run it:

```
pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1
```

## Open questions

- REQ-311 is asserted by inspection, not by test. The fixture harness runs the PowerShell
  implementation only. A parity harness that runs both against the same cases on a Linux runner
  would close this properly, and is the single most valuable addition to this spec.
- The Bash implementation degrades when `jq` is unavailable: payload fields cannot be extracted, so
  the pattern checks have nothing to match and the decision defaults to allow. `jq` is present on
  GitHub-hosted runners; a self-hosted runner without it would silently weaken the guardrails.
  Recorded rather than hidden — a preflight check for `jq` is the obvious fix.
