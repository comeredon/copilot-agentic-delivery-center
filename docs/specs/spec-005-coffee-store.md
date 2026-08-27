# SPEC-005 — Ninth Parallel coffee store baseline

## Problem

The repository needs a real, immediately understandable product for GitHub Copilot to evolve during
the demonstration. A dashboard about agent delivery makes the harness and product indistinguishable.
Ninth Parallel provides a rich coffee and commerce domain while keeping all GitHub/Copilot concerns
outside the product.

## Requirements

| ID | Requirement |
|---|---|
| COFFEE-001 | Persist 12 stable coffees spanning at least eight countries, six producers, five process types, and all required sensory/traceability fields. |
| COFFEE-002 | Reject cup scores outside 60–100 and inverted elevation ranges. |
| COFFEE-003 | Preserve 1–5 unique ordered primary tasting notes. |
| COFFEE-004 | Refuse publication without a hero image and active variant. |
| COFFEE-005 | Refuse stock reservation that would make stock negative; deactivate a zero-stock variant. |
| COFFEE-006 | Derive Resting, Peak, and PastPeak from roast date and configured rest days. |
| COFFEE-007 | Return a filterable, sortable, paged catalog with country/process/roast/note facets. |
| COFFEE-008 | Return complete coffee detail including producer, origin, process, roast, sensory, traceability, variants, images, and recipes. |
| COFFEE-009 | Cache catalog, detail, and producer reads through versioned HybridCache keys; cache failure degrades to direct reads. |
| COFFEE-010 | Web reaches product data through the API only. |
| COFFEE-011 | Home, shop, detail, producer, brew, transparency, and cart routes render at desktop and mobile widths without horizontal overflow. |
| COFFEE-012 | Every product image has useful alt text; each page has one H1 and a PageTitle; controls have accessible names. |
| COFFEE-013 | The Worker warms the catalog without changing stock or producing no-op writes. |
| COFFEE-014 | Checkout is visibly reserved for a future issue and never simulates a successful paid order. |
| COFFEE-015 | Product runtime and customer UI contain no GitHub issue, PR, agent-stage, gate, or merge concepts. |

## Non-goals

- Real payment processing, customer identity, or stored PII
- Checkout, subscriptions, promotions, reviews, email, and real tax calculation
- Product image uploads or an object-storage credential
- AI-generated origin, sensory, or traceability claims
- GitHub Project synchronization from the store runtime

These are explicit Project issues, not unfinished baseline code hidden behind a successful button.

## Design

The baseline is an additive pivot. Existing assembly names and generic gate tooling stay stable to
avoid coupling a broad rename with behavior. Product requests follow:

```text
Blazor component -> CoffeeStoreApiClient -> catalog endpoint -> CatalogQueries
                  -> ICoffeeCatalogRepository -> EF/PostgreSQL
                  -> IStoreCache -> HybridCache/Redis
```

PostgreSQL is authoritative for price and stock. Redis stores read projections only. Store pages do
not reference Application or Infrastructure.

## Verification

| Requirement | Evidence |
|---|---|
| COFFEE-001 | `CoffeeCatalogEndpointsTests.Catalog_ReturnsTheDeterministicTwelveCoffeeRange` |
| COFFEE-002 | `CoffeeTests.Create_RejectsCupScoresOutsideTheCuppingScale` |
| COFFEE-003 | `CoffeeTests.AddDescriptor_RejectsDuplicateAndMoreThanFiveTastingNotes` |
| COFFEE-004 | `CoffeeTests.Publish_RequiresAHeroImageAndAnActiveVariant` |
| COFFEE-005 | `CoffeeTests.Reserve_RefusesOversellingAndMarksTheVariantInactiveAtZero` |
| COFFEE-006 | `CoffeeTests.GetFreshnessState_UsesTheConfiguredRestWindow` |
| COFFEE-007 | catalog integration filter test and live `/api/catalog` validation |
| COFFEE-008 | `CoffeeCatalogEndpointsTests.CoffeeDetail_ContainsTraceabilitySensoryDataAndBrewRecipes` |
| COFFEE-009 | existing cache degradation behavior plus catalog live cache counters |
| COFFEE-010 | architecture test `Web_NeverReachesTheDatabaseOrGitHubDirectly` |
| COFFEE-011 | Playwright desktop/mobile reads and width assertion |
| COFFEE-012 | browser accessibility snapshots for primary routes |
| COFFEE-013 | Worker startup log contains active coffee count |
| COFFEE-014 | cart checkout button emits the demo-backlog message only |
| COFFEE-015 | visible-route browser review and route inventory |

## Known Follow-Up

The current cart is scoped to a Blazor circuit. Persisting it across reloads is deliberately
`NP-101`, the first recommended live Copilot feature because it crosses Web, API, Redis, privacy,
and tests without requiring payment handling.