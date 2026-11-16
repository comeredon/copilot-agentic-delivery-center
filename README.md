# Ninth Parallel Coffee Roasters

A detailed specialty-coffee storefront built to demonstrate the breadth of GitHub Copilot as an
autonomous software-engineering system.

The distinction matters:

- **Ninth Parallel is the product.** Customers browse traceable coffees, compare processing and
  sensory data, open producer stories, scale recipes, choose bag/grind variants, and build a cart.
- **GitHub is the delivery system.** Projects holds the roadmap; GitHub Copilot agents plan and implement
  features; independent agents review them; the deterministic gate verifies them; a human decides
  whether to merge.

The store does not know GitHub Copilot exists. That makes the demonstration representative of a real
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

Prerequisites: the .NET 10 SDK pinned by `global.json`, Docker Desktop or another supported
container runtime, and PowerShell 7+ (`pwsh`) for the demo fixtures. Bash and `jq` are also required
for the Linux/macOS hook scripts. Verify `dotnet --version`, `docker info`, `pwsh --version`, and
`jq --version` before the demo; an installed Docker client alone is not sufficient.

Committed AppHost lockfiles cover SDK hosts `linux-x64` and `win-x64`. The Windows dependency graph
was checked with a Linux SDK RID override, not on Windows; run locked restore on the actual Windows
demo machine before presenting. Other SDK hosts need the reviewed lockfile procedure below first.

Configure the persistent PostgreSQL password once. The generated value is written only to the
AppHost user-secrets store:

```powershell
$postgresPassword = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Parameters:postgres-password" $postgresPassword --project src/DeliveryControlCenter.AppHost
Remove-Variable postgresPassword
```

Then start the full topology:

```powershell
dotnet restore --locked-mode
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

## GitHub Copilot Harness

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

Agent profiles inherit the host's selected/default model rather than requiring a fixed model name.
Learn tool grants use explicit `microsoft-learn/<tool>` names and do not grant any GitHub writes.
See [MCP configuration](docs/mcp-configuration.md) for host setup and capability checks.
Confirm all 11 custom agents appear in the host's agent picker and invoke `requirements-planner`
before the demo. If names are unavailable, reload/restart the host after applying the profiles;
using a built-in reviewer instead does not demonstrate custom-agent routing.

AppHost has separate `packages.linux-x64.lock.json` and `packages.win-x64.lock.json` files because
Aspire selects its Dashboard and orchestration packages for the SDK host's runtime identifier.
The host SDK and hosting integrations use Aspire 13.6.5, the fixed release for
[the Dashboard advisory](https://github.com/advisories/GHSA-4g86-54fm-h9h8) and
[the hosting advisory](https://github.com/advisories/GHSA-f7vh-h5g3-7jrh); NuGet auditing remains enabled.
Maintain the SDK pin in AppHost's project and the hosting pins in `Directory.Packages.props` together.
AppHost explicitly retains the supported package-based `dotnet run` workflow, without CLI-only
features. Only its optional CLI-bundle migration warning (`ASPIRE010`) is suppressed; the gate's
warnings-as-errors policy and security diagnostics are unchanged.
Other SDK host platforms require a reviewed lockfile generated with
`dotnet restore src/DeliveryControlCenter.AppHost --force-evaluate` before locked restore can be
used there. Do not regenerate locks during a live demo or disable locked mode to hide a failure.

### Before calling the demo ready

The checked-in workflows filter `main`. If the demo baseline/default branch has another name, a
human must align both workflows' push/pull-request filters before relying on automatic checks.
There is no `.github/workflows/copilot-setup-steps.yml`; a human must configure the pinned SDK,
PowerShell, and hook dependencies for repeatable cloud-agent startup. Agents cannot edit workflows.
The human-owned follow-up is [#4](https://github.com/comeredon/copilot-agentic-delivery-center/issues/4).

The hook registration and decision payloads also require human review against the
[GitHub Copilot hooks reference](https://docs.github.com/en/copilot/reference/hooks-reference).
Passing PowerShell decision fixtures does not prove that the host loads the hooks or enforces
their decisions. The determinism fixture likewise needs human hardening: it does not check gate
exit codes/verdicts or distinguish a dirty tree from `HEAD`. Until fixed, use two explicit gate
runs with `--commit unknown` for uncommitted changes, require both to pass, and compare their
canonical hashes and envelope timestamps.
Hook integration is tracked in [#5](https://github.com/comeredon/copilot-agentic-delivery-center/issues/5);
fixture hardening is tracked in [#6](https://github.com/comeredon/copilot-agentic-delivery-center/issues/6).

Verify repository visibility and branch protection in GitHub settings separately; neither a local
gate nor the checked-in files establish those settings. Projects and durable evidence/ledger
publication remain unavailable unless their exact tools and sinks pass preflight.

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