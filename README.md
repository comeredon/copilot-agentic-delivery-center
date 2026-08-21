# Delivery Control Center

An agentic delivery dashboard for a GitHub repository — and, more to the point, a working
demonstration of autonomous software engineering with enterprise-grade guardrails.

The application is real: it polls GitHub, projects issues and pull requests into a delivery model,
records quality gate results in a hash-chained ledger, and shows the state in a Blazor dashboard.
The reason it exists is the harness around it. Every agent action is bounded, evidenced, and
independently verified before a human is asked to make a merge decision.

## The claim, and how it is made checkable

| Claim | How you can check it |
|---|---|
| Agents cannot merge | `pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1` — 20 fixture cases |
| Agents verify each other | `GateLedgerEntry.Append` throws when proposer and verifier match |
| The verdict is reproducible | Run the gate twice; identical `canonicalHash`, different `generatedAtUtc` |
| Layers are enforced, not suggested | `dotnet test tests/DeliveryControlCenter.ArchitectureTests` |
| Skipped checks are visible | The `cloud` profile records integration tests as skipped, never as passed |

## Getting started

Prerequisites: .NET 10 SDK (pinned in `global.json`) and a container runtime for the integration
tests and the Aspire host.

```powershell
dotnet restore
dotnet build DeliveryControlCenter.slnx
dotnet run --project src/DeliveryControlCenter.AppHost
```

`GitHub:Enabled` is `false` by default, so a fresh clone runs with no credentials against a
deterministic seeded payload. Set it to `true` and supply `GitHub:Token` through user-secrets to
point at a real repository.

## Commands

| Task | Command |
|---|---|
| Build | `dotnet build DeliveryControlCenter.slnx` |
| Unit tests | `dotnet test tests/DeliveryControlCenter.UnitTests` |
| Architecture tests | `dotnet test tests/DeliveryControlCenter.ArchitectureTests` |
| Gate determinism tests | `dotnet test tests/DeliveryControlCenter.QualityGate.Tests` |
| Integration tests | `dotnet test tests/DeliveryControlCenter.IntegrationTests` |
| Guardrail fixtures | `pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1` |
| **The quality gate** | `dotnet run --project tools/DeliveryControlCenter.QualityGate -- --profile local --commit <sha>` |

## Architecture

```
Domain          no dependencies at all — entities, invariants, hash chain
Contracts       DTOs shared by Api and Web
Application     use cases + ports. Never references EF, Redis, or HttpClient
Infrastructure  adapters: EF Core/Postgres, HybridCache/Redis, GitHub read client
Api             minimal API + SignalR hub. The only writer to the database
Worker          polling loop that drives synchronisation
Web             Blazor Web App. Talks to the Api over HTTP only
ServiceDefaults telemetry, health, resilience, service discovery
AppHost         Aspire orchestration
```

Dependencies point inward only, and the architecture tests fail the build if they do not.

## The harness

| Layer | Location |
|---|---|
| Repository instructions | `.github/copilot-instructions.md` |
| Scoped instructions | `.github/instructions/*.instructions.md` |
| Agents (3 orchestrators, 8 specialists) | `.github/agents/*.agent.md` |
| Skills | `.github/skills/*/SKILL.md` |
| Prompts | `.github/prompts/*.prompt.md` |
| Hook policy | `.github/hooks/guardrails.json` |
| Hook implementations | `eng/agent-hooks/` |
| MCP servers | `.mcp.json` |
| Agent contracts | `AGENTS.md`, `src/AGENTS.md`, `tests/AGENTS.md`, `eng/AGENTS.md` |

Start the demonstration with the `demo-copilot-harness` skill or the `demo-harness` prompt.

## Documentation

- [`PLAN.md`](PLAN.md) — the full design and execution plan this was built from
- [`docs/specs/`](docs/specs/) — feature specifications with `REQ-###` traceability
- [`docs/mcp-configuration.md`](docs/mcp-configuration.md) — what is registered, and what is
  deliberately not

## The boundary

An agent's authority ends at a pull request with a passing quality gate. It cannot merge, cannot
push to `main`, cannot edit its own guardrails, and cannot read a credential. A human makes the
merge decision, with the gate report and the ledger in front of them.

That boundary is the product.
