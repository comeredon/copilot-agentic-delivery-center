# AGENTS.md — eng/

Engineering scripts and the agent hook implementations.

## What is here

| Path | Purpose |
|---|---|
| `agent-hooks/` | The hook scripts referenced by `.github/hooks/guardrails.json` — PowerShell and Bash, behaviourally identical |
| `tests/Test-Guardrails.ps1` | Fixture tests asserting each deny and allow decision |
| `github/` | Repository provisioning helpers |

## Agents may not edit `agent-hooks/`

These scripts implement the deny decisions that constrain agents. An agent that can edit its own
constraints has no constraints. `.github/hooks/**` is in the deny list, and `eng/agent-hooks/` is
owned by CODEOWNERS for the same reason.

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
