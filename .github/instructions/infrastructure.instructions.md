---
description: Persistence, caching, and GitHub adapter conventions.
applyTo: "src/DeliveryControlCenter.Infrastructure/**"
---

# Infrastructure conventions

## Entity Framework

- Configuration lives in `IEntityTypeConfiguration<T>` classes, never in `OnModelCreating` directly.
- Every entity goes through `ApplyEntityConventions()` so identity and the concurrency token are
  mapped consistently.
- `Version` is a domain-owned optimistic concurrency token converted to `long`. Do not switch to a
  provider system column — the integration tests and the domain tests both depend on it being a
  real, incrementable property.
- Enums persist as strings with an explicit `HasMaxLength`. An integer column would silently
  reinterpret if the enum is reordered.
- Every schema change needs a migration committed alongside it. `EnsureCreated` is never acceptable.
- Repository implementations are `internal`. Callers depend on the port.

## Caching

- Keys come from `CacheKeys` and always carry the `dcc:v1:` prefix. Bump the version segment when
  a cached shape changes; never hand-roll a key at a call site.
- Reads are cache-aside via `IDeliveryCache`. Writes invalidate explicitly — never rely on TTL
  expiry to publish a change.
- A cache failure must degrade to a direct read and increment the error counter. It must never
  fail the request.

## GitHub adapters

- `IGitHubReader` is read-only by construction. Do not add a method that mutates.
- Always send the stored ETag as `If-None-Match` and honour `304 Not Modified` by returning
  `RemoteSyncResult.Unchanged`. Burning rate limit on unchanged data is a defect.
- On `403`/`429`, keep the previous cursor and log the fact of throttling — never the response body.
- Never log, echo, or embed a token. The reader gets its token from `IGitHubTokenProvider`; nothing
  else should ever see it.
- The base address is fixed in options. Do not accept a URL from a caller, a work item, or a
  configuration value that an agent can influence.

## Project writes

`GuardedProjectWriter` is the only write path and is disabled unless both `GitHub:Enabled` and
`GitHub:EnableProjectWrites` are true. Both the allowed and the blocked path write an audit event.
Do not add a second write path.
