---
description: Run the full end-to-end demonstration of the agentic harness, including the failure paths.
mode: agent
---

# Demonstrate the harness

Run the full demonstration described in the `demo-copilot-harness` skill.

## Setup

```
dotnet build DeliveryControlCenter.slnx
dotnet run --project src/DeliveryControlCenter.AppHost
```

`GitHub:Enabled` is false by default, so this runs with no credentials and a fixed seeded payload —
which is also what makes the demonstration repeatable.

## The happy path

Scope → orchestrate → delegate → rubber duck → parallel review → gate → ledger → pull request.

At each step, show the *constraint*, not just the output:

- The orchestrator's tool list has no `edit` and no merge tool.
- The specialists are scoped by area, and the test engineer can only write under `tests/`.
- The duck is a different agent, and it cannot edit.
- The review fan-out is capped at three workers.

## The two things worth pausing on

**Determinism.** Run the gate twice on the same commit. Identical `canonicalHash`, different
`envelope.generatedAtUtc`. That single contrast explains the whole design: the verdict is
content-addressed and the timing is not part of it.

**Independence.** Try to append a ledger entry where the proposer and the verifier are the same
agent. `GateLedgerEntry.Append` throws. The rule lives in the domain, not in a policy document, so
there is no path around it.

## Show the failure paths

A demonstration where nothing goes wrong proves nothing. Run these live:

```
pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1
```

Then attempt, and show the denial for, each of: `gh pr merge`, `git push --force`, an edit to
`.github/hooks/guardrails.json`, and a read of a `.env` file.

Finally, break a test deliberately. Show the gate failing, the repair orchestrator diagnosing it,
and — after two failed attempts — the flow stopping and labelling `needs-human` rather than looping.

## Close

End on the pull request sitting at a passing gate, waiting. Every control here exists to make that
moment trustworthy. A human still makes the call.
