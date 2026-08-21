---
description: Rules for authoring Copilot customization files in this repository.
applyTo: ".github/**"
---

# Customization authoring rules

## Frontmatter

- `*.instructions.md` requires `description` and `applyTo`. `applyTo` is a glob relative to the
  repository root; use `"**"` only when the guidance is genuinely universal.
- `*.agent.md` requires `name`, `description`, and `tools`. `model` is optional but recommended for
  agents where the reasoning trade-off matters.
- `*.prompt.md` requires `description`; `mode` and `tools` are optional.
- `SKILL.md` requires `name` and `description`. The description is what the model matches against,
  so it must name the triggering situation, not just the topic.

## Tool-list discipline

- Enumerate tools explicitly. Never use a wildcard.
- Grant the narrowest set that lets the agent finish its job. An agent that does not edit files
  must not have `edit`. An agent that does not run commands must not have `execute`.
- No agent may be given a merge tool, a push tool, or a branch-protection tool. There is no
  exception to this.
- If an agent needs a capability it does not have, the correct response is to hand off to an agent
  that does — not to widen the tool list.

## Content rules

- Instructions describe *rules*, not tasks. If it reads like a checklist for one piece of work, it
  belongs in a prompt or a skill.
- Keep `copilot-instructions.md` under two pages. Everything longer belongs in a scoped
  `.instructions.md` file or a skill.
- Never put a secret, a token, an internal hostname, or a personal access token pattern in any of
  these files. They are committed and world-readable.
- Do not restate guidance that already exists in another layer. Duplication drifts.

## Changing the guardrails

`.github/hooks/**`, `.github/workflows/**`, `.mcp.json`, `CODEOWNERS`, and rulesets are excluded
from agent edits by hook policy. If one of them is wrong, open an issue describing the problem and
the proposed change. Do not attempt to edit them and do not attempt to route around the hook.
