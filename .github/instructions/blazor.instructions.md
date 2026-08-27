---
description: Blazor component conventions for the Web project.
applyTo: "src/DeliveryControlCenter.Web/**"
---

# Blazor conventions

## Data access

The Web project reaches product data **only** through `CoffeeStoreApiClient`. It has no reference to
`Application` or `Infrastructure`, and the architecture tests enforce that. If you find yourself
wanting a database call from a component, add an API endpoint instead.

## Components

- Interactive pages declare `@rendermode InteractiveServer` explicitly.
- Fetch in `OnInitializedAsync` (or `OnParametersSetAsync` when the route parameter drives the
  fetch). Never fetch inside the render body.
- `StoreCart` is scoped in-memory state in the baseline. Do not imply that it survives refresh,
  reserves stock, or performs checkout.
- There is no realtime client today. If a scoped issue adds notifications, treat them as hints and
  refetch through `CoffeeStoreApiClient` before rendering authoritative product state.
- Unsubscribe from shared event sources in `DisposeAsync`. A leaked handler keeps a
  disposed circuit alive.
- Call `InvokeAsync(StateHasChanged)` when updating from a non-UI thread (any hub callback).

## Rendering and state

- Keep component state private. Pass data down through parameters, raise events up through
  `EventCallback`.
- Guard every nullable model with an explicit empty state. A page that throws because the API was
  unreachable is a bug; a page that says "the API is unreachable" is correct behaviour.

## Accessibility and markup

- Every page has exactly one `<h1>` and a `<PageTitle>`.
- Form controls have associated labels or a `placeholder` that is genuinely descriptive.
- Do not convey status by colour alone — pair colour with text ("pass"/"fail", not just green/red).
- External links carry `rel="noopener noreferrer"`.
- UI changes are not complete until desktop (1440 × 1000) and mobile (390 × 844) browser checks
  show no horizontal overflow, clipped controls, failed resources, or console errors.

## Security

- Never render raw HTML from catalog, producer, or review content. Blazor escapes by default; do
  not reach for `MarkupString` on product data.
- Do not put identifiers or hashes in the page title or the URL beyond what routing needs.
