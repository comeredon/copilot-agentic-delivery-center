# AGENTS.md — src/

Server and client application code. Dependencies point inward only, and
`tests/DeliveryControlCenter.ArchitectureTests` fails the build if they do not.

## Layers

| Project | Depends on | Must never reference |
|---|---|---|
| `Domain` | nothing | everything else, including EF, ASP.NET, Npgsql, Redis |
| `Contracts` | nothing | any layer |
| `Application` | `Domain`, `Contracts` | EF, Npgsql, Redis, `HttpClient`, any host |
| `Infrastructure` | `Application` | `Api`, `Web`, `Worker` |
| `Api` | `Infrastructure`, `ServiceDefaults` | `Web` |
| `Worker` | `Infrastructure`, `ServiceDefaults` | `Api`, `Web` |
| `Web` | `Contracts`, `ServiceDefaults` | `Application`, `Infrastructure`, EF, Npgsql |

If you want a database call from `Application`, you want a port in `Application/Abstractions` and an
adapter in `Infrastructure`. If you want data in `Web` that the API does not expose, add an
endpoint — do not add a reference.

## Build order when implementing

`Domain` → `Contracts` → `Application` → `Infrastructure` → `Api`/`Worker` → `Web`.

Build after each step. `TreatWarningsAsErrors` is on, so a layering mistake surfaces immediately —
and a mistake found in `Domain` is minutes, while the same mistake found in `Api` is a redesign.

## Ownership

| Area | Agent |
|---|---|
| `DeliveryControlCenter.Web` | `blazor-specialist` |
| Everything else here | `dotnet-implementer` |

If a change spans both, sequence them. Two editing agents on overlapping files produce conflicts
neither can see.

## Non-negotiables

- No secret in any `appsettings*.json`. User-secrets or environment only.
- No new GitHub write path. `IGitHubReader` is read-only; `GuardedProjectWriter` is the single
  audited exception and it is off by default.
- Schema change means a committed migration. Never `EnsureCreated`.
- A write that changes a cached projection invalidates the key explicitly. TTL expiry is not
  publication.
