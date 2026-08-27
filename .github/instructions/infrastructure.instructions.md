---
description: Store persistence, caching, seeding, and external adapter conventions.
applyTo: "src/DeliveryControlCenter.Infrastructure/**"
---

# Store infrastructure conventions

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

- Store keys come from `StoreCacheKeys` and carry `ninth-parallel:v1:`. Bump the version when
  a cached shape changes; never hand-roll a key at a call site.
- Reads are cache-aside via `IStoreCache`. Writes invalidate explicitly — never rely on TTL
  expiry to publish a change.
- A cache failure must degrade to a direct read and increment the error counter. It must never
  fail the request.

## Current catalog baseline

- Catalog list and detail projections may be cached; authoritative stock reservation may not.
- Seed data is deterministic: fixed aggregate IDs and stable slugs support screenshots and tests.
- The current repository and API are read-only. `Coffee.ReserveVariant` proves the stock invariant,
  but no checkout endpoint or stock-reservation transaction exists yet.

## When a commerce issue adds writes

- Prices come from persisted variants. Never trust a client-supplied price.
- Reserve stock against PostgreSQL with the domain invariant and `Version` concurrency token in one
  transaction. Cached stock is never checkout authority.
- A write that changes a catalog projection invalidates list, detail, and producer tags explicitly.
- Retryable operations such as checkout, payment, email, and scheduled work require idempotency.

## External adapters

The baseline has no payment, email, or image-storage adapter. When a bounded issue introduces one,
define the port in Application and the adapter here. A credential-bearing integration requires a
separate threat review and configuration through user-secrets or environment variables.
