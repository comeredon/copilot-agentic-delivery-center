# AGENTS.md — eng/

Engineering scripts and the agent hook implementations.

## What is here

| Path | Purpose |
|---|---|
| `agent-hooks/` | PowerShell and Bash hook scripts referenced by `.github/hooks/guardrails.json`; parity is intended but not yet proven |
| `tests/Test-Guardrails.ps1` | Fixture tests asserting each deny and allow decision |
| `github/` | Repository provisioning helpers |

## Agents may not edit `agent-hooks/` or `tests/`

These scripts implement and test the deny decisions that constrain agents. An agent that can edit
its own constraints or weaken their fixtures has no constraints. `eng/agent-hooks/` is explicitly
owned by CODEOWNERS, and the repository-wide CODEOWNERS rule covers `eng/tests/`.

The current pre-tool path patterns do not mechanically deny these two `eng/` paths. They remain
off-limits by policy, and hardening that mechanical gap requires a human-authored guardrail change.
The Bash pre-tool hook also allows when `jq` is unavailable, and either implementation can lose its
deny patterns if policy loading fails. The PowerShell-only fixtures do not prove cross-platform
parity. Treat all three as human-owned blockers, not as settled controls.

If a hook is wrong — a false positive blocking legitimate work, or a gap letting something through
— open an issue describing the exact payload and the observed decision. A human changes it.

## If you are a human changing a hook

1. Change both the PowerShell and the Bash implementation. They must behave identically, because
   the platform an agent happens to run on must not change what it is allowed to do.
2. Add a fixture case to `eng/tests/Test-Guardrails.ps1` — one for the new deny, one for a
   near-miss that must still be allowed.
3. Run it:

   ```
   pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1
   ```

4. Keep every hook under 5 seconds, parsing stdin defensively, failing closed on a malformed
   payload, and never echoing a matched path or a secret into the audit trail.

## The audit trail

Hooks write JSON lines to `.agent-runtime/`, which is git-ignored. It is a local forensic record
for the operator, not a committed artifact, and it deliberately contains no arguments, no outputs,
and no matched paths — only the tool, the decision, and the name of the pattern that matched.
