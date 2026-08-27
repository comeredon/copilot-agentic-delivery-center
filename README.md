# Ninth Parallel Coffee Roasters

A detailed specialty-coffee storefront built to demonstrate the breadth of GitHub Copilot as an
autonomous software-engineering system.

The distinction matters:

- **Ninth Parallel is the product.** Customers browse traceable coffees, compare processing and
  sensory data, open producer stories, scale recipes, choose bag/grind variants, and build a cart.
- **GitHub is the delivery system.** Projects holds the roadmap; Copilot agents plan and implement
  features; independent agents review them; the deterministic gate verifies them; a human decides
  whether to merge.

The store does not know Copilot exists. That makes the demonstration representative of a real
application rather than a dashboard about the demonstration itself.

## Storefront

The current baseline includes:

- 12 deterministic coffees from 10 origin countries and 10 producer partners
- Structured origin, elevation, variety, process, fermentation, drying, harvest, and roast data
- Ordered tasting notes, SCA cup score, acidity/body/sweetness, and freshness state
- Lot code, import partner, available FOB price, and certifications
- V60, AeroPress, and espresso recipes for every coffee
- 250 g and 1 kg variants with whole-bean and grind options
- Filterable/sortable shop, producer pages, a brew-ratio lab, transparency content, and cart
- PostgreSQL persistence, Redis/HybridCache catalog reads, deterministic seeding, and Aspire OTel
- Domain, architecture, deterministic-gate, and Testcontainers integration tests

Checkout is deliberately left as demo issue `NP-102` rather than hidden partial behavior. It can be
provisioned into a GitHub Project for a connected demo; no card is charged by this baseline.

## Run It

Prerequisites: .NET 10 SDK (pinned by `global.json`) and Docker Desktop or another supported
container runtime.

Configure the persistent PostgreSQL password once. The generated value is written only to the
AppHost user-secrets store:

```powershell
$postgresPassword = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Parameters:postgres-password" $postgresPassword --project src/DeliveryControlCenter.AppHost
Remove-Variable postgresPassword
```

Then start the full topology:

```powershell
dotnet restore
dotnet build DeliveryControlCenter.slnx
dotnet run --project src/DeliveryControlCenter.AppHost
```

Default local endpoints:

| Surface | URL |
|---|---|
| Storefront | `http://localhost:5102` or `https://localhost:7102` |
| API | `http://localhost:5101` or `https://localhost:7101` |
| Aspire dashboard | Dynamic login URL printed by AppHost |
| pgAdmin / Redis Insight | Dynamic links in Aspire |

## Architecture

```text
Domain          Coffee, producer, descriptors, variants, recipes, stock invariants
Contracts       Catalog and producer DTOs shared across HTTP
Application     Catalog filters/queries, store ports, versioned cache keys
Infrastructure  EF Core/Postgres, deterministic seed, HybridCache/Redis
Api             Minimal catalog and producer endpoints, ProblemDetails, rate limiting
Worker          Catalog warming now; freshness and low-stock jobs are demo backlog
Web             Blazor storefront; reaches product data through the API only
ServiceDefaults OpenTelemetry, health, resilience, service discovery
AppHost         PostgreSQL, Redis, migrations, API, Worker, Web, admin tools
```

The assembly names still use `DeliveryControlCenter.*` to avoid mixing a broad mechanical rename
with the behavioral product pivot. The public product, routes, data, documentation, and agent
instructions are Ninth Parallel. A later human-authored cleanup can rename assemblies and the
workflow paths in one isolated commit.

## Verified Commands

| Task | Command |
|---|---|
| Restore | `dotnet restore --locked-mode` |
| Build | `dotnet build DeliveryControlCenter.slnx` |
| Format | `dotnet format DeliveryControlCenter.slnx --verify-no-changes` |
| Unit tests | `dotnet test tests/DeliveryControlCenter.UnitTests` |
| Web tests | `dotnet test tests/DeliveryControlCenter.Web.Tests` |
| Architecture tests | `dotnet test tests/DeliveryControlCenter.ArchitectureTests` |
| Gate tests | `dotnet test tests/DeliveryControlCenter.QualityGate.Tests` |
| Integration tests | `dotnet test tests/DeliveryControlCenter.IntegrationTests` |
| Guardrail fixtures | `pwsh -NoProfile -File eng/tests/Test-Guardrails.ps1` |
| Gate determinism | `pwsh -NoProfile -File eng/tests/Test-GateDeterminism.ps1` |
| Quality gate | `dotnet run --project tools/DeliveryControlCenter.QualityGate --artifacts-path artifacts/gate-host -- --profile local --commit <sha\|unknown>` |

`--artifacts-path` keeps the running gate binary outside the solution build that the gate performs.

## Copilot Harness

| Layer | Location | Purpose |
|---|---|---|
| Repository contract | `AGENTS.md`, `.github/copilot-instructions.md` | Commands, architecture, authority boundaries |
| Scoped conventions | `.github/instructions/` | .NET, Blazor, store infrastructure, tests, reviews |
| Agents | `.github/agents/` | 3 orchestrators and 8 specialists |
| Skills | `.github/skills/` | Planning, implementation, review, gate, evidence, demo flow |
| Prompts | `.github/prompts/` | Repeatable issue delivery and verification entry points |
| Hooks | `.github/hooks/`, `eng/agent-hooks/` | Local deny decisions and sanitized audit trail |
| Quality gate | `tools/DeliveryControlCenter.QualityGate/` | Content-addressed deterministic verdict |
| Schemas | `schemas/` | Report, evidence, and ledger contracts |

An agent may plan, edit within its role, run tests, open a branch, and create a pull request. It may
not merge, push to `main`, force push, read credentials, or edit the guardrails. Agent authority ends
at a green pull request. A human authorizes the merge.

The local repository configures Microsoft Learn MCP. GitHub issue/Project/PR tools, an `origin`
remote, and durable evidence/ledger publication are connected-demo prerequisites, not product
runtime dependencies. Run `preflight-github-delivery` before claiming those scenes are available.

## Suggested Live Demo

1. Run `preflight-github-delivery` and declare local or connected mode.
2. Open the store and choose a bounded issue from the backlog document or connected Project.
3. Produce numbered acceptance criteria, non-goals, and test mapping.
4. Research a .NET API through Microsoft Learn MCP.
5. Let the delivery orchestrator delegate backend, Blazor, and test work.
6. Ask Rubber Duck to challenge the design, then run test and threat reviewers in parallel.
7. Run the deterministic quality gate twice and compare its canonical hash.
8. Demonstrate the ledger independence invariant; publish evidence only when a connected sink exists.
9. Run guardrail decision fixtures and state that they do not prove host wiring or branch protection.
10. In connected mode, stop at the green pull request for a human merge; otherwise show dry-run
  Project/PR deltas and the missing capabilities.

High-value demo issues are listed in [PLAN.md](PLAN.md) and
[docs/demo/coffee-store-backlog.md](docs/demo/coffee-store-backlog.md).

## Documentation

- [PLAN.md](PLAN.md) — product boundaries, requirements, delivery phases, and demo backlog
- [Coffee store specification](docs/specs/spec-005-coffee-store.md)
- [MCP configuration](docs/mcp-configuration.md)
- [Original gate and guardrail specifications](docs/specs/)