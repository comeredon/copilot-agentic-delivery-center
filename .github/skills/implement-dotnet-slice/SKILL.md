---
name: implement-dotnet-slice
description: Layered implementation procedure with build checkpoints for changes spanning Domain, Application, Infrastructure, Api, or Worker. Use when writing server-side .NET code in this repository. Do NOT use for Blazor components (Web project) or for test-only changes.
---

# Implement a .NET slice

`TreatWarningsAsErrors` is on. Build often, and let the compiler catch the layering mistake before
the architecture tests do.

## Checkpoint procedure

### 1. Domain
Entities and invariants. No framework types, no `async`, no I/O.

Invariants belong here, not in a handler. `AuthorizeByHuman` throwing when the gate has not passed
is the model refusing an illegal state — a check in the API would be a policy that any other
caller could forget.

```
dotnet build src/DeliveryControlCenter.Domain/DeliveryControlCenter.Domain.csproj
```

### 2. Contracts
Only if the wire shape changed. `record` types, no behaviour.

### 3. Application
The use case, and the port it needs. Ports go in `Abstractions`, expressed in the domain's
language — `IGitHubReader.FetchIssuesAsync`, not `IHttpClientWrapper.GetAsync`.

If you find yourself wanting `DbContext` here, you want a port instead.

```
dotnet build src/DeliveryControlCenter.Application/DeliveryControlCenter.Application.csproj
```

### 4. Infrastructure
The adapter. Plus an EF configuration and a migration if the schema moved:

```
dotnet ef migrations add <Name> --project src/DeliveryControlCenter.Infrastructure --output-dir Persistence/Migrations
```

Generating the migration also validates the whole EF model — if it succeeds, the mapping is
coherent. Treat a migration failure as a modelling bug, not a tooling problem.

### 5. Api / Worker
Wire it into DI and expose it. Endpoints go in `Endpoints/`, one group per concern. A state-changing
endpoint gets the `write` rate-limit policy and an audit event on both the allowed and refused path.

### 6. Full build and tests

```
dotnet build DeliveryControlCenter.slnx
dotnet test tests/DeliveryControlCenter.UnitTests
dotnet test tests/DeliveryControlCenter.ArchitectureTests
```

## Recurring traps in this repository

- **Cache invalidation.** Any write that changes a cached projection must invalidate the key
  explicitly. TTL expiry is not publication.
- **Idempotency.** A sync that finds unchanged content must change nothing and invalidate nothing.
  Return `false` from the mutator rather than touching the entity unconditionally.
- **Determinism.** Nothing time-, duration-, machine-, or enumeration-order-dependent may reach the
  canonical section of the gate report.
- **Analyzer promotion.** A new warning is a build break. Fix it properly; add to `NoWarn` only when
  the rule is genuinely wrong for the whole repository, and say why.

## Report honestly

Paste real command output. If a step failed and you moved on, say that. A confident report that
turns out to be untested is worse than an honest partial one.
