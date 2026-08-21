# Copilot Agentic Delivery Center — Implementation Plan

| Field | Value |
|---|---|
| Status | Approved, not started |
| Plan revision | 3 |
| Date | 2026-08-21 |
| Owner | @comeredon |
| Local folder | `c:\Users\comeredon\source\copdemwebabb` |
| Target repository | `comeredon/copilot-agentic-delivery-center` (personal, private) |
| Research baseline | GitHub Copilot and .NET documentation as of 2026-08-21 |

---

## 1. Objective

Build a single repository that is simultaneously two things:

1. **A real, complex .NET application.** An Agentic Delivery Control Center with a Blazor frontend, an ASP.NET Core API, a background worker, PostgreSQL persistence, and a Redis-backed cache. It synchronizes live GitHub Project items, issues, pull requests, checks, and agent evidence.
2. **A complete, auditable demonstration of the GitHub Copilot agentic harness.** Multiple orchestrating agents, specialist subagents, skills, layered instructions, hooks, MCP servers, plugins, canvases, automations, and agentic workflows — all with enterprise-style guardrails and deterministic, machine-verifiable output.

The two halves reinforce each other: the application's subject matter *is* the agentic delivery process it was built by.

### 1.1 Definition of done

- [ ] `aspire run` starts the whole system locally with one command and every resource reports healthy.
- [ ] The application shows live GitHub Project, issue, pull request, check, and evidence data.
- [ ] The majority of product code was written by Copilot agents through issues and pull requests, visible in the repository history.
- [ ] Two consecutive quality-gate runs on the same commit produce byte-identical canonical reports.
- [ ] Every agent merge attempt is blocked; merges happen only after an explicit human authorization signal.
- [ ] Every Copilot capability reachable by a personal account has been exercised and evidenced.

---

## 2. Confirmed decisions

These come directly from the requirements interview and are settled unless explicitly revisited.

| Topic | Decision |
|---|---|
| Product scope | Repository demo only. The product does **not** embed `GitHub.Copilot.SDK`. Copilot builds and maintains the product. |
| Repository | Personal, private. `comeredon/copilot-agentic-delivery-center`. |
| Autonomy boundary | Agent merge is permitted, but only after an explicit human authorization signal. |
| Live GitHub data | Yes. Runtime reads live repository data via a least-privilege GitHub App. |
| Project authentication | Fine-grained token first, verified by a capability probe. |
| Feature tier | Everything required, within what a personal account can reach. |
| Enterprise controls | Excluded. Never simulated as active enforcement. |
| Deployment | Deployment-neutral. Local Aspire plus GitHub Actions only. No cloud infrastructure. |
| Application domain | Agentic Delivery Control Center. |
| Cloud-agent test profile | Cloud sessions skip Playwright. Profiles: `cloud`, `ci`, `local`. |
| Human/agent build split | Humans build the skeleton, guardrails, gate, and CI. Agents build the rest. |

---

## 3. Conflicts found and how they were resolved

Three answers conflicted with what the GitHub platform actually permits. Each was resolved rather than silently ignored.

### C1 — Agent merge in a solo repository

**Selected:** "Agent merge after human approval."

**Problem:** GitHub does not let the user who initiates a Copilot task approve the resulting pull request, and a pull request opened under the agent's own app identity requires an additional approval. In a single-owner repository there is no second party, so a formal required-approval rule can never be satisfied.

**Resolution:** Keep agent merge. Replace the *formal GitHub approval record* with an *explicit human authorization signal*:

- Agents drive the pull request to fully green, then stop and report.
- The owner reviews the diff, the deterministic report, and the gate ledger.
- The owner personally issues `/pr automerge` in the Copilot app, **or** applies the owner-only `human-approved` label consumed by a merge workflow.
- Hooks, agent tool lists, and the label workflow's actor check make it impossible for an agent to produce that signal.

### C2 — Project token type

**Selected:** "Fine-grained / project PAT."

**Problem:** GitHub's documented flow for user-owned Projects still leans on a classic token with the `project` scope; fine-grained tokens have historically been limited here.

**Resolution:** Honour the choice, but verify rather than assume. `eng/github/verify-project-token.ps1` probes read and write against the Project. The result is recorded in the repository. A classic token is used **only** if the probe fails, and that fallback is documented as a platform limitation, not a design preference.

### C3 — "Everything required" versus "exclude unavailable controls"

**Problem:** These two answers pull in opposite directions.

**Resolution:** Everything reachable by a **personal** Copilot account is in scope and must be demonstrated. Organization-level and enterprise-level governance (managed settings, `.github-private`, enterprise MCP allowlists, org policies) is excluded entirely and is never mocked up to look active. Where an enterprise control would normally provide a guarantee, the plan substitutes a repository-level equivalent and says so plainly.

---

## 4. Research baseline

Facts verified against official documentation on 2026-08-21. These shape the design and should be re-checked before implementation begins if significant time has passed.

### 4.1 The harness

- The **GitHub Copilot agentic harness** is a shared, model-independent runtime that orchestrates context, tools, delegation, and workflow. It powers Copilot CLI, the Copilot app, Copilot code review, the SDK, and IDE integrations.
- It supports 20+ models plus bring-your-own-key, with Auto model selection routing on task intent and model health.
- Efficiency features include prompt caching, deferred tool loading (tool search), context compaction, and selective subagent delegation.
- **The repository configures and demonstrates the harness. It does not reimplement it.**

### 4.2 The Copilot app

- Built on Copilot CLI and the Copilot SDK. Available for macOS, Linux, and Windows.
- Parallel isolated sessions, each with its own git worktree, local folder, or cloud sandbox.
- Session modes: Interactive, Plan, Autopilot.
- Supports custom agents, skills, hooks, MCP, plugins, canvases, automations, issue and pull request lifecycle, `/security-review`, `/rubber-duck`, `/chronicle`, and OpenTelemetry.

### 4.3 Rubber Duck

- A **built-in** agent, not something to be recreated. Deliberately runs on a model from a complementary family.
- Read-only. It cannot edit files or run state-changing commands.
- Invoked automatically after planning, after complex implementation, after writing tests, or when the agent is stuck. Also invocable via `/rubber-duck`.
- Categorizes findings as blocking, non-blocking, or suggestion.
- **Advisory only.** It is never an approval identity and cannot turn a failing deterministic check green.

### 4.4 Cloud agent constraints

- Runs in an ephemeral GitHub Actions environment with a **59-minute hard ceiling**.
- One branch and one pull request per task. Cannot push to the default branch.
- Cannot mark its own pull request ready for review, approve it, or merge it.
- Workflows on its pull requests require approval from a user with write access before running.
- Firewall enabled by default with a recommended allowlist; it only covers processes started by the agent's Bash tool, not MCP servers or setup steps.
- Commits are signed and co-authored, with a link back to session logs.

### 4.5 Customization file locations

| Primitive | Location |
|---|---|
| Repository-wide instructions | `.github/copilot-instructions.md` |
| Path-specific instructions | `.github/instructions/**/*.instructions.md` (frontmatter `applyTo`, optional `excludeAgent`) |
| Agent instructions | `AGENTS.md` at any level; nearest file wins |
| Custom agents | `.github/agents/*.agent.md` |
| Skills | `.github/skills/<name>/SKILL.md` |
| Prompts | `.github/prompts/*.prompt.md` |
| Hooks | `.github/hooks/*.json` |
| MCP (harness-agnostic) | `.mcp.json` at repository root, or `.github/mcp.json` |
| Plugin manifest | `plugin.json`, or `.github/plugin/plugin.json` |
| Marketplace manifest | `.github/plugin/marketplace.json` |
| Canvas extensions | `.github/extensions/<name>/` |
| Cloud agent environment | `.github/workflows/copilot-setup-steps.yml` |
| Declarative plugins | `.github/copilot/settings.json` |

### 4.6 Cross-surface caveats

- `handoffs` and `argument-hint` in agent frontmatter are **ignored** by the cloud agent on GitHub.com. Orchestration must not depend on them.
- `.vscode/mcp.json` is **not** read by Copilot CLI or the Agent Host. Use `.mcp.json`.
- Copilot automations are stored outside git and are private to their creator. Only recipes and evidence can be versioned.
- MCP tools execute **autonomously** once granted. Every agent needs an explicit, narrow tool list.

---

## 5. Scope

### 5.1 In scope

**Application**
.NET 10 LTS, C# 14, Aspire 13.x, Blazor Web App, ASP.NET Core API, background worker, PostgreSQL, Redis with `HybridCache`, SignalR, health checks, OpenTelemetry, live GitHub synchronization, deterministic evidence generation, full test pyramid.

**Copilot harness**
Repository instructions, path instructions, layered `AGENTS.md`, three orchestrators, eight specialists, skills, prompts, hooks, GitHub MCP, Microsoft Learn MCP, plugin, marketplace, canvas extension, agentic workflows, local and cloud automations, Copilot code review, Rubber Duck, `/security-review`, Chronicle, OpenTelemetry, worktrees, cloud sandbox, deep links, remote control.

### 5.2 Out of scope

- Copilot SDK inside the running product.
- Autonomous merge without a human signal. Autonomous deployment of any kind.
- Azure or any other cloud infrastructure. No IaC.
- Multi-tenancy, cross-repository writes, organization or enterprise governance.
- GitHub Advanced Security features not available to the account.

### 5.3 Prerequisites

- [ ] Paid Copilot plan with the Copilot app and cloud agent enabled
- [ ] GitHub plan supporting rulesets and required checks on private repositories
- [ ] Docker Desktop running
- [ ] .NET 10 SDK
- [ ] Aspire CLI
- [ ] GitHub CLI (`gh`) 2.0+, authenticated
- [ ] `gh extension install github/gh-aw`
- [ ] GitHub Copilot CLI
- [ ] PowerShell 7+
- [ ] Node.js (only for canvas and plugin tooling)

### 5.4 Credentials — four distinct identities

Never reuse one token across roles.

| Identity | Purpose | Storage |
|---|---|---|
| GitHub App (read-only install) | Runtime reads of repository, issues, pull requests, checks | App ID + private key in user-secrets locally, Actions secrets remotely |
| Project token (fine-grained, probe-verified) | Project field reads and explicit writes | Actions secret, never exposed to implementation agents |
| `COPILOT_GITHUB_TOKEN` (fine-grained, Copilot Requests) | Agentic Workflows inference | Actions secret |
| OAuth / built-in GitHub MCP | Interactive Copilot app sessions | Managed by the Copilot client |

---

## 6. Application architecture

### 6.1 Project topology

```
DeliveryControlCenter.slnx
├── src/
│   ├── DeliveryControlCenter.AppHost           Aspire orchestration
│   ├── DeliveryControlCenter.ServiceDefaults   OTel, health, resilience, discovery
│   ├── DeliveryControlCenter.Web               Blazor Web App
│   ├── DeliveryControlCenter.Api               REST + SignalR hub
│   ├── DeliveryControlCenter.Worker            Polling sync + report ingestion
│   ├── DeliveryControlCenter.MigrationService  One-shot EF migrations
│   ├── DeliveryControlCenter.Domain            Entities, value objects, rules
│   ├── DeliveryControlCenter.Application       Use cases, ports, validation
│   ├── DeliveryControlCenter.Infrastructure    EF Core, GitHub adapters, cache
│   └── DeliveryControlCenter.Contracts         DTOs, SignalR envelopes, reports
├── tests/
│   ├── DeliveryControlCenter.UnitTests
│   ├── DeliveryControlCenter.ArchitectureTests
│   ├── DeliveryControlCenter.IntegrationTests
│   ├── DeliveryControlCenter.Web.Tests         bUnit
│   ├── DeliveryControlCenter.E2E.Tests         Playwright .NET
│   └── DeliveryControlCenter.QualityGate.Tests
└── tools/
    └── DeliveryControlCenter.QualityGate       Canonical gate runner
```

### 6.2 Data model

One PostgreSQL database, logically partitioned. No CQRS, no event sourcing, no message broker until there is measured need.

| Entity | Purpose |
|---|---|
| `RepositoryConnection` | Which repository, which install, sync configuration |
| `ProjectItem` | Project v2 item with custom field values |
| `IssueSnapshot` | Issue state, labels, assignees, timestamps |
| `PullRequestSnapshot` | PR state, draft status, mergeability, review state |
| `CheckSnapshot` | Check run and status results per commit |
| `GateReport` | Canonical deterministic report, hash, profile |
| `AgentEvidence` | Rubber Duck, reviewer, and security findings |
| `GateLedgerEntry` | Proposer, verifier, verdicts, hash chain |
| `SyncCursor` | Durable per-stream cursor and ETag |
| `AuditEvent` | Actor, action, input, result for every write |

Rules: optimistic concurrency, idempotent external IDs, UTC everywhere, explicit retention windows, EF Core migrations.

### 6.3 Cache strategy

`HybridCache` with in-memory L1 and Redis L2.

| Cached item | TTL (L1 / L2) | Invalidated by |
|---|---|---|
| Project field schema | 5 min / 60 min | Manual refresh, schema change detection |
| Repository summary | 1 min / 10 min | Sync cycle completion |
| Issue detail | 30 s / 5 min | Issue sync, explicit write |
| Pull request readiness | 15 s / 2 min | PR sync, check update |
| Check summary | 15 s / 2 min | Check sync |
| Dashboard read model | 10 s / 60 s | Any upstream invalidation |

Requirements: versioned key prefixes, stampede protection, ETag-aware refresh, explicit invalidation after every committed write, no secrets ever cached, and hit / miss / stale / error metrics surfaced in the UI.

### 6.4 GitHub integration boundaries

- **Reads:** GitHub App installation token. REST for repository, issues, pull requests, checks, workflow runs. GraphQL for Projects v2.
- **Writes:** Project fields only. Explicit operator command, separate token, audited, idempotent, and **disabled by default** behind `GitHub:EnableProjectWrites=false`.
- **Transport:** Polling-first with durable cursors. An optional HMAC-validated webhook acts purely as a wake-up optimization, never as the source of truth. This keeps local `aspire run` working with no public endpoint.
- **Resilience:** ETag conditional requests, rate-limit header handling, retry with jitter, circuit breaking, token redaction in all logs.
- **Never:** arbitrary URL fetching, cross-repository writes, or merge API calls.

### 6.5 User interface

A dense operational tool, not a marketing page.

| Screen | Contents |
|---|---|
| Overview | Sync health, gate pass rate, open PRs, cache health, alerts |
| Project board | Project items grouped by `Agent Stage`, filterable |
| Work item detail | Issue, linked PR, checks, evidence, ledger entries |
| PR readiness | Matrix of PRs against required checks and gate verdict |
| Evidence explorer | Canonical reports, hashes, Rubber Duck and reviewer findings |
| Audit and sync | Timeline of every write, cursor positions, failures |
| Connections | Repository, Project, token status, write toggle |

Requirements: responsive at desktop and mobile widths, semantic and keyboard-navigable, explicit loading / empty / error / stale states, live updates via SignalR, no overlapping or clipped layout at any breakpoint.

---

## 7. Copilot harness design

### 7.1 Instruction layers

| File | Contents |
|---|---|
| `.github/copilot-instructions.md` | Verified bootstrap, build, test, and run commands. Architecture map. The single quality-gate command. Safety rules and trust boundaries. Kept under two pages. |
| `.github/instructions/dotnet.instructions.md` | `applyTo: "**/*.cs"` — nullable, analyzers, async, DI, error handling |
| `.github/instructions/blazor.instructions.md` | `applyTo: "src/**/Web/**"` — component structure, accessibility, state |
| `.github/instructions/tests.instructions.md` | `applyTo: "tests/**"` — naming, isolation, Testcontainers, no flaky waits |
| `.github/instructions/infrastructure.instructions.md` | `applyTo: "src/**/Infrastructure/**"` — EF, cache keys, GitHub adapters |
| `.github/instructions/customizations.instructions.md` | `applyTo: ".github/**"` — frontmatter rules, tool-list discipline |
| `.github/instructions/code-review.instructions.md` | `applyTo: "**"`, `excludeAgent: "cloud-agent"` — demonstrates review-only scoping |
| `AGENTS.md` (root) | Repository purpose, agent contract, escalation rules |
| `src/**/AGENTS.md`, `tests/AGENTS.md`, `eng/AGENTS.md` | Nearest-scope guidance |

### 7.2 Agents

Three orchestrators. None may edit files or call merge tools.

| Agent | Tools | Responsibility |
|---|---|---|
| `delivery-orchestrator` | `read`, `search`, `execute`, `agent` | Issue to green pull request |
| `repair-orchestrator` | `read`, `search`, `execute`, `agent` | Failing checks, flaky tests, gate repair |
| `release-orchestrator` | `read`, `search`, `agent`, Project reads | Readiness roll-up, evidence, release notes |

Eight specialists.

| Agent | Tools | May edit | Notes |
|---|---|---|---|
| `requirements-planner` | `read`, `search`, Learn MCP | No | Produces scoped acceptance criteria |
| `architecture-reviewer` | `read`, `search`, Learn MCP | No | Challenges boundaries and reuse |
| `dotnet-implementer` | `read`, `search`, `edit`, `execute`, Learn MCP | Yes | Domain, Application, Infrastructure, Api |
| `blazor-specialist` | `read`, `search`, `edit`, `execute` | Yes | `Web` and its tests only |
| `test-engineer` | `read`, `search`, `edit`, `execute` | Tests only | Cannot change production behaviour |
| `threat-modeler` | `read`, `search`, GitHub security reads | No | Threat model and findings |
| `quality-verifier` | `read`, `search`, `execute` | No | Runs the canonical gate, reports evidence |
| `github-project-curator` | Enumerated issue and Project MCP tools only | No | No shell, no file edits, no merge tools |

### 7.3 Orchestration policy

Selective delegation, not an unconditional swarm.

| Change size | Flow |
|---|---|
| Small (one file, no new behaviour) | implementer → quality-verifier |
| Standard | planner → implementer → parallel(test-engineer, threat-modeler) → quality-verifier |
| Architectural, or 3+ projects touched | planner → architecture-reviewer → **Rubber Duck** → implementer → parallel review → quality-verifier |

Hard limits:

- Maximum **3** parallel workers.
- Maximum **2** repair loops per gate. The third failure sets `needs-human` and stops.
- Reviewers and verifiers never review their own edits.
- Rubber Duck is consulted after any non-trivial plan and after complex implementations or new test suites.

### 7.4 Skills

| Skill | Purpose |
|---|---|
| `research-microsoft-dotnet` | Learn MCP lookups before choosing an API |
| `scope-agent-issue` | Turn a rough idea into a bounded agent-ready issue |
| `plan-delivery-slice` | Produce an implementation plan with acceptance criteria |
| `implement-dotnet-slice` | Layered implementation procedure with checkpoints |
| `rubber-duck-gate` | When and how to request a critique, and how to act on it |
| `parallel-review` | Fan out independent test and threat review, then synthesize |
| `run-quality-gate` | Invoke the gate with the correct profile |
| `record-gate-ledger` | Append a verified ledger entry |
| `publish-agent-evidence` | Normalize advisory findings into the evidence schema |
| `sync-github-project` | Idempotent Project field updates |
| `demo-copilot-harness` | Drive the full end-to-end demonstration |

### 7.5 Hooks — the enforcement layer

`.github/hooks/guardrails.json`, with cross-platform scripts in `eng/agent-hooks/`.

| Event | Behaviour |
|---|---|
| `preToolUse` | **Deny:** destructive filesystem or git commands; push to `main`; any merge or automerge command; reads of credential files; arbitrary network egress; edits to `.github/hooks`, `.github/workflows`, `.mcp.json`, rulesets, or `CODEOWNERS` |
| `postToolUse` | Append sanitized tool record to `.agent-runtime/*.jsonl` |
| `subagentStop` | Record subagent verdict for the ledger |
| `errorOccurred` | Record error class and context, no payloads |
| `sessionEnd` | Emit schema-checked session summary |

Constraints: under 5 seconds, parse stdin JSON defensively, never log secrets, fixture-tested on both Windows (PowerShell) and Linux (Bash).

> Hooks are repository code. They are a strong deterrent and a complete audit trail, but the authoritative controls remain branch rules, required checks, least-privilege tokens, and the human merge signal.

### 7.6 MCP configuration

```jsonc
// .mcp.json  — read-only, no authentication required
{
  "mcpServers": {
    "microsoft-learn": {
      "type": "http",
      "url": "https://learn.microsoft.com/api/mcp",
      "tools": [
        "microsoft_docs_search",
        "microsoft_docs_fetch",
        "microsoft_code_sample_search"
      ]
    }
  }
}
```

- GitHub context uses the **built-in** GitHub MCP server — no extra configuration, correctly scoped by default.
- A separately named Projects MCP profile is added **only after** live tool discovery, with every tool enumerated explicitly. No wildcards, no literal tokens.
- `config/github/copilot-mcp.json` mirrors the cloud-agent repository-settings JSON for manual entry.

---

## 8. Assurance model

Four layers, in order. Only the last three can block delivery.

| Layer | Mechanism | Deterministic? | Can block? |
|---|---|---|---|
| 1. Cross-model critique | Built-in Rubber Duck | No | No — advisory |
| 2. Independent review | Read-only specialists with matched verdicts | No | Yes — via ledger disagreement |
| 3. Deterministic gate | QualityGate tool + CI required checks | Yes | Yes |
| 4. Human authorization | Owner reviews, then signals | Human | Yes |

### 8.1 The gate ledger

Each gate produces one hash-chained entry:

```jsonc
{
  "gateId": "implement-cache-strategy",
  "proposer":  { "agent": "dotnet-implementer", "model": "…", "verdict": "pass" },
  "verifier":  { "agent": "quality-verifier",   "model": "…", "verdict": "pass" },
  "evidenceHash": "sha256:…",
  "previousEntryHash": "sha256:…",
  "commit": "…"
}
```

The orchestrator advances **only** when both verdicts agree. Disagreement escalates to `needs-human`. This is the concrete implementation of "agents verify each other".

### 8.2 Determinism rules

Canonical report **includes:** sorted check list, normalized relative paths, invariant culture formatting, schema version, source commit, evidence hashes, profile name, verdict.

Canonical report **excludes:** timestamps, durations, machine names, absolute paths, environment details. Those go to a separate non-canonical `run-metadata.json`.

**Proof:** run the gate twice on the same commit; the two canonical reports must have identical SHA-256 hashes.

### 8.3 Profiles

| Profile | Runs | Used by |
|---|---|---|
| `cloud` | restore, build, unit, architecture, integration | Copilot cloud agent (59-min budget) |
| `ci` | everything, including Playwright E2E and accessibility | GitHub Actions |
| `local` | everything, plus container smoke | Developer machine |

A `cloud` report can never satisfy a required check that demands `ci`. The profile is recorded in the canonical report.

---

## 9. Execution plan

### Phase 1 — Control plane and credentials

- [ ] 1.1 `git init` in `c:\Users\comeredon\source\copdemwebabb`; create private `comeredon/copilot-agentic-delivery-center`; push `main`; confirm the Copilot app can see the repository.
- [ ] 1.2 Create the user Project with fields `Status`, `Priority`, `Workstream`, `Risk`, `Agent Stage`, `Gate`, `Evidence`, `Target Date`, and views Backlog, Agentic Flow, PR Readiness, Gate Risk, Roadmap. Enable built-in auto-add and auto-close workflows.
- [ ] 1.3 Add labels, agent-ready issue forms (bounded problem, acceptance criteria, allowed scope, validation commands, risk, out-of-scope), the pull request template, and the owner-only `human-approved` label.
- [ ] 1.4 **(C2)** Create the fine-grained Project token. Run `eng/github/verify-project-token.ps1`. Record the probe result. Fall back to classic only on documented failure.
- [ ] 1.5 Install the read-only GitHub App; store App ID and private key as secrets.
- [ ] 1.6 Apply the ruleset: pull-request-only changes to `main`, no force push, no deletion, strict required checks, conversation resolution, linear history. Do **not** set an unsatisfiable approval count.
- [ ] 1.7 Configure cloud-agent internet access: firewall on, recommended allowlist on, add only genuinely required hosts.
- [ ] 1.8 Write `eng/github/provision.ps1` plus declarative JSON desired state. Unsupported features must emit a typed gap report.

**Blocks:** every later phase.

### Phase 2 — Walking skeleton (human-built, deliberately minimal)

- [ ] 2.1 `DeliveryControlCenter.slnx`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`. Nullable on, warnings as errors, deterministic builds, NuGet lock files, central package management.
- [ ] 2.2 Scaffold all ten projects with **one** working slice each: one entity, one endpoint, one page, one cached read, one test per layer.
- [ ] 2.3 Aspire composition: PostgreSQL with persistent volume, Redis with health check and Redis Insight, MigrationService, Api, Worker, Web. Api and Worker wait for migrations.
- [ ] 2.4 Verify `aspire run` brings everything up healthy.

### Phase 3 — Guardrails and deterministic gate (before any agent delivery)

- [ ] 3.1 Build `tools/DeliveryControlCenter.QualityGate` with the three profiles.
- [ ] 3.2 Author `schemas/quality-report.schema.json` and `schemas/agent-evidence.schema.json`.
- [ ] 3.3 **(R3)** Author `schemas/gate-ledger.schema.json` and the hash-chain logic.
- [ ] 3.4 Implement canonical report generation and `run-metadata.json` separation.
- [ ] 3.5 Prove determinism: two runs, identical SHA-256.
- [ ] 3.6 Author `.github/hooks/guardrails.json` and `eng/agent-hooks/` scripts (Bash + PowerShell).
- [ ] 3.7 Fixture-test every allow and deny case on Windows and Linux.
- [ ] 3.8 Author `.github/workflows/copilot-setup-steps.yml`; validate it runs standalone.
- [ ] 3.9 Author CI workflows, SHA-pinned, least privilege: customization lint, build+unit matrix (Windows/Linux), integration, E2E, security, quality report, container smoke, agentic-workflow lock drift.
- [ ] 3.10 Add Dependabot for NuGet, Actions, and container references.
- [ ] 3.11 After checks pass on `main`, promote them to required in the ruleset.

### Phase 4 — Copilot harness definition

- [ ] 4.1 Write `.github/copilot-instructions.md` with commands verified by actually running them.
- [ ] 4.2 Write the seven path-specific instruction files, including the `excludeAgent` demonstration.
- [ ] 4.3 Write root and nested `AGENTS.md` files.
- [ ] 4.4 **(R2)** Author the three orchestrators.
- [ ] 4.5 Author the eight specialists with exact tool lists.
- [ ] 4.6 Encode the orchestration policy, parallel cap, and loop limits in orchestrator bodies.
- [ ] 4.7 Author the eleven skills with progressive disclosure and bundled assets.
- [ ] 4.8 Author the five prompt files.
- [ ] 4.9 Write `.mcp.json`, `config/github/copilot-mcp.json`, and `.github/copilot/settings.json`.
- [ ] 4.10 Verify discovery: `/env`, `/agent`, `/skills list`, `copilot mcp list`, `copilot plugins list --json`. Zero frontmatter warnings.

### Phase 5 — Agent-delivered product increments

> **(R1)** From here, agents write the code. Humans review and authorize.

- [ ] 5.1 File agent-ready issues: GitHub adapters, sync engine, cache strategy, API slices, SignalR, Worker durability, Blazor screens, evidence explorer, audit and cache telemetry.
- [ ] 5.2 For each issue, run the harness end to end: Plan mode → Learn MCP research → orchestrated implementation in an isolated worktree → parallel review → `/security-review` → deterministic gate → pull request → `/pr auto` repair → ledger entry.
- [ ] 5.3 Enforce the write constraint: repository data read-only; Project writes explicit, audited, and off by default.
- [ ] 5.4 Complete the test pyramid as part of each increment.
- [ ] 5.5 Verify the repository history shows agent authorship with human co-authoring and session links.

### Phase 6 — Full capability demonstrations

- [ ] 6.1 Enable Copilot code review; add review-specific instructions; configure the MCP-in-review toggle.
- [ ] 6.2 Build `plugins/delivery-governance/` (manifest, demo agent, evidence skill, non-invasive hook, MCP reference). Publish `.github/plugin/marketplace.json`. Install from local path **and** from the remote repository.
- [ ] 6.3 Build the `delivery-gates-canvas` extension — read-only, renders ledger and readiness. No merge, token, shell, or network capability.
- [ ] 6.4 `gh aw init`; author three agentic workflows (issue triage, CI-failure investigation, scheduled delivery report); commit source **and** lock files; set per-run AI credit caps.
- [ ] 6.5 Configure local and cloud automations from `docs/copilot/automations/` recipes. Untrusted-user triggers stay disabled.
- [ ] 6.6 Exercise and document every remaining surface — see the coverage matrix in section 11.

### Phase 7 — Human-authorized merge path

- [ ] 7.1 **(C1)** Implement the authorization protocol and the owner-only label workflow with an actor check.
- [ ] 7.2 Prove agents cannot self-authorize: hooks block merge commands, tool lists exclude merge MCP tools, the workflow rejects non-owner actors. Log every attempt.
- [ ] 7.3 **(R5)** Publish each report four ways: check run, pull request comment, workflow artifact, Project `Evidence` field.
- [ ] 7.4 Write architecture, threat model, cache strategy, GitHub auth, agent security model, ADRs, contributor guide, and troubleshooting docs.

### Phase 8 — Acceptance

- [ ] 8.1 Full scenario from a Project issue through to human-authorized agent merge and Project update.
- [ ] 8.2 Two additional parallel sessions on independent issues; verify worktree isolation and no file contention.
- [ ] 8.3 Adversarial suite — all must fail closed:
  - Prompt injection from an untrusted issue attempting a privileged write
  - An agent attempting `gh pr merge` / `/pr automerge`
  - An agent attempting to edit `.github/hooks/`
  - A tampered evidence hash breaking the ledger chain
- [ ] 8.4 Run every agentic workflow and automation; inspect safe outputs and `gh aw audit`.
- [ ] 8.5 Record `docs/demo/acceptance.md` with all evidence listed in section 10.

---

## 10. Repository file map

```
copdemwebabb/
├── PLAN.md                                  ← this file
├── README.md
├── AGENTS.md
├── .mcp.json
├── DeliveryControlCenter.slnx
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── .editorconfig
├── .github/
│   ├── copilot-instructions.md
│   ├── CODEOWNERS
│   ├── copilot/settings.json
│   ├── instructions/          7 × *.instructions.md
│   ├── agents/                3 orchestrators + 8 specialists
│   ├── skills/                11 skill folders
│   ├── prompts/               5 × *.prompt.md
│   ├── hooks/guardrails.json
│   ├── plugin/marketplace.json
│   ├── extensions/delivery-gates-canvas/
│   ├── ISSUE_TEMPLATE/
│   ├── PULL_REQUEST_TEMPLATE.md
│   └── workflows/             CI + copilot-setup-steps + 3 agentic (.md + .lock.yml)
├── config/github/copilot-mcp.json
├── schemas/
│   ├── quality-report.schema.json
│   ├── agent-evidence.schema.json
│   └── gate-ledger.schema.json
├── eng/
│   ├── AGENTS.md
│   ├── agent-hooks/           *.sh + *.ps1 + fixtures
│   ├── github/                provision.ps1, verify-project-token.ps1, desired-state/
│   └── quality/
├── src/                       10 projects (section 6.1)
├── tests/                     6 projects (section 6.1)
├── tools/DeliveryControlCenter.QualityGate/
├── plugins/delivery-governance/
├── docs/
│   ├── architecture/
│   ├── security/
│   ├── copilot/               capability-matrix.md, demo-runbook.md, automations/
│   ├── adr/
│   └── demo/acceptance.md
└── .agent-runtime/            gitignored — hook audit output
```

---

## 11. Capability coverage matrix

Fill the Evidence column during Phase 6 and 8.

| # | Capability | Where demonstrated | Evidence |
|---|---|---|---|
| 1 | Chats (no branch) | Copilot app | |
| 2 | Interactive mode | App session | |
| 3 | Plan mode | Feature planning | |
| 4 | Autopilot mode | Implementation | |
| 5 | Auto model selection | Session config | |
| 6 | Explicit model + reasoning effort | Session config | |
| 7 | BYOK | App settings — *prerequisite dependent* | |
| 8 | Voice dictation | App settings — *prerequisite dependent* | |
| 9 | Parallel worktree sessions | Phase 8.2 | |
| 10 | Cloud sandbox session | Phase 6.6 | |
| 11 | Remote control / GitHub Mobile | Phase 6.6 | |
| 12 | Deep links | `docs/copilot/demo-runbook.md` | |
| 13 | Custom agents | `.github/agents/` | |
| 14 | Subagent delegation | Orchestrator flows | |
| 15 | `/fleet` parallel execution | Phase 6.6 | |
| 16 | `/research` | Phase 6.6 | |
| 17 | `/delegate` to cloud agent | Phase 6.6 | |
| 18 | Sidekick agents | Phase 6.6 | |
| 19 | Session fork / rewind | Phase 6.6 | |
| 20 | `/sandbox` + permission modes | Phase 6.6 | |
| 21 | Copilot Memory | Phase 6.6 — *plan dependent* | |
| 22 | Rubber Duck | Phase 5 flows | |
| 23 | `/security-review` | Phase 5 flows | |
| 24 | Copilot code review | Phase 6.1 | |
| 25 | `/chronicle` | Phase 8 | |
| 26 | Skills | `.github/skills/` | |
| 27 | Prompts | `.github/prompts/` | |
| 28 | Hooks | Phase 3.6 | |
| 29 | Microsoft Learn MCP | `.mcp.json` | |
| 30 | GitHub MCP | Built-in | |
| 31 | Plugin + marketplace | Phase 6.2 | |
| 32 | Canvas extension | Phase 6.3 | |
| 33 | Agentic Workflows | Phase 6.4 | |
| 34 | Local automations | Phase 6.5 | |
| 35 | Cloud automations | Phase 6.5 | |
| 36 | OpenTelemetry | Phase 8 | |
| 37 | Issue → PR lifecycle | Phase 5 | |
| 38 | `/pr auto` repair | Phase 5 | |
| 39 | Human-authorized agent merge | Phase 7 | |
| 40 | Project synchronization | Phase 5, 7 | |

---

## 12. Verification checklist

- [ ] **Provisioning** — `provision.ps1` is idempotent; token probe result recorded; ruleset contains only enforceable rules.
- [ ] **Build** — locked restore, `dotnet format --verify-no-changes`, Release build with warnings as errors, all tests green, coverage thresholds met, no vulnerable packages, container builds and smoke-tests.
- [ ] **Runtime** — `aspire run` all healthy; migrations precede Api and Worker; live sync observed; ETag and rate-limit handling verified; explicit Project write audited; SignalR refresh confirmed.
- [ ] **Cache** — hit, miss, and invalidation observable; Redis outage degrades gracefully.
- [ ] **UI** — Playwright screenshots at desktop and mobile; keyboard navigation; no overlaps; loading / error / stale states; accessibility scan clean.
- [ ] **Determinism** — two gate runs, identical canonical SHA-256; both schemas validate; ledger chain verifies.
- [ ] **Guardrails** — every hook fixture passes on Windows and Linux; merge, `main` push, secret read, destructive command, and governance-file edit all blocked; normal build/test/edit unaffected.
- [ ] **Customizations** — `/env`, `/agent`, `/skills list`, `copilot mcp list`, `copilot plugins list --json` all clean; no wildcard GitHub write tools anywhere.
- [ ] **Telemetry** — OTel shows nested agent, model, tool, hook, skill, and MCP spans; content capture off; Rubber Duck confirmed on a complementary model.
- [ ] **Workflows** — `gh aw compile` produces no diff; `gh aw audit` clean; automations emit only declared safe outputs.
- [ ] **Merge control** — merge occurs only after the human signal; every agent attempt denied and logged.

---

## 13. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Cloud agent 59-min ceiling | Task times out mid-implementation | Issue-sized scope; `cloud` profile skips Playwright; `copilot-setup-steps.yml` pre-installs everything |
| Fine-grained token cannot reach the Project | Phase 1 blocked | Capability probe in 1.4 with a documented classic fallback |
| Preview features change under us | Rework | Research baseline dated in section 4; re-verify before Phase 6 |
| Prompt injection via untrusted issue | Unauthorized action | Read-only defaults, narrow tool lists, hooks, safe outputs, adversarial test 8.3 |
| Agent loops burning credits | Cost | Repair loop cap of 2, parallel cap of 3, `--max-ai-credits`, `max-ai-credits` in workflows |
| Non-determinism leaks into the report | Gate proof fails | Strict canonical/non-canonical split; two-run hash comparison in CI |
| Solo repository weakens review | Reduced assurance | Cross-verification ledger + deterministic gate + explicit human signal compensate; limitation stated openly |
| Over-engineering the product | Slower, harder to demo | Explicit exclusions in 6.2; no CQRS, event sourcing, sagas, or message broker |

---

## 14. Open items

| # | Item | Default if unanswered |
|---|---|---|
| 1 | Confirm repository name `copilot-agentic-delivery-center` | Proceed with this name |
| 2 | Confirm the Copilot plan tier (affects Memory, subagent concurrency) | Detect at Phase 1 and record in the capability matrix |
| 3 | Decide whether to enable the optional webhook path | Polling only |

---

## 15. Change log

| Rev | Date | Change |
|---|---|---|
| 1 | 2026-08-21 | Initial plan from requirements interview |
| 2 | 2026-08-21 | Resolved C1–C3; added bootstrap-then-agent-built sequencing, three orchestrators, gate ledger, expanded capability coverage |
| 3 | 2026-08-21 | Resolved cloud test profile and human/agent split; written to `PLAN.md` |
