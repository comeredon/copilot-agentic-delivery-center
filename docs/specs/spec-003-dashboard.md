# SPEC-003 — Delivery dashboard

## Problem

A human deciding whether to merge needs to see the delivery state, the gate evidence, and the
verification chain in one place — and needs the only state-changing action in the product
(authorising a merge) to be visibly and provably restricted to them.

## Requirements

| ID | Requirement |
|---|---|
| REQ-201 | An overview shows open issues, open pull requests, items awaiting a human, gate failures, gate pass rate, and last sync time. |
| REQ-202 | A board lists every work item with client-side filtering by title, number, and stage. |
| REQ-203 | A detail page shows the latest gate report, the ledger chain, and the recorded evidence for one item. |
| REQ-204 | An evidence explorer lists every gated item with its verdict and canonical hash. |
| REQ-205 | An operations page shows per-feed sync health, dead-letter state, and cache counters. |
| REQ-206 | Realtime notifications trigger a refetch through the API; pushed payloads are never rendered directly. |
| REQ-207 | Authorisation is available only to the repository owner and only after a passing gate. |
| REQ-208 | Both a refused and an allowed authorisation attempt write an audit event. |
| REQ-209 | The front end never reaches the database or GitHub directly. |
| REQ-210 | An unreachable API renders an explanatory empty state, not an error page. |

## Non-goals

- Authentication and multi-user authorisation. The demonstration uses a declared actor compared
  against the configured repository owner. Real deployment would put an identity provider in front.
- Editing GitHub content from the UI.
- Mobile-optimised layouts.
- Charting. Counters and tables are enough to make the point.

## Design

### The data path

`Component` → `DeliveryApiClient` → HTTP → `Api` → `Application` → `Infrastructure`.

That is the only path, and `LayeringTests.Web_NeverReachesTheDatabaseOrGitHubDirectly` fails the
build if it is violated. The reason is not purity: the API is where rate limiting, auditing, and
authorisation live, so a UI shortcut past it would be a shortcut past all three.

### Realtime as a hint, not a payload

`DeliveryHub` is server-to-client only. A notification carries a kind and an optional id; the
component refetches. Rendering the pushed payload would mean displaying data that never went
through the audited read path — the same objection as reaching past the API, arriving by a
different route.

Hub failure is non-fatal: `DeliveryRealtimeClient` logs and returns `false`, and the pages still
work with manual refresh.

### Authorisation

`POST /api/delivery/work-items/{id}/authorize` is the only state-changing endpoint. It:

- carries the tighter `write` rate-limit policy (10/minute versus 240/minute for reads),
- compares the declared actor to `Delivery:RepositoryOwner`,
- returns `403` for a non-owner and `409` when the gate has not passed,
- writes an audit event on **both** paths.

`WorkItem.AuthorizeByHuman` throws unless the stage is `GatePassed` or already
`AwaitingHumanAuthorization`. That check is in the domain rather than the endpoint, so any future
caller inherits it.

## Verification

| Requirement | Test |
|---|---|
| REQ-201 | `DeliveryEndpointsTests.Overview_IsServedFromTheRealStack` |
| REQ-202 | Manual — `Board.razor` filtering is client-side over an already-fetched list |
| REQ-203 | `DeliveryEndpointsTests.UnknownWorkItem_Returns404` covers the negative path |
| REQ-204 | Manual — `Evidence.razor` |
| REQ-205 | `DeliveryEndpointsTests.CacheHealth_ReportsCounters` |
| REQ-206 | Manual — every page refetches in its `Changed` handler |
| REQ-207 | `AuthorizeWorkItemHandlerTests.Owner_AuthorizesAndInvalidatesTheReadModel`, `Owner_CannotAuthorizeBeforeTheGate`, `WorkItemTests.AuthorizeByHuman_IsRejectedBeforeTheGate` |
| REQ-208 | `AuthorizeWorkItemHandlerTests.NonOwner_IsRefusedAndAudited` |
| REQ-209 | `LayeringTests.Web_NeverReachesTheDatabaseOrGitHubDirectly` |
| REQ-210 | Manual — `DeliveryApiClient` returns `default` on `HttpRequestException` |

## Open questions

- Several requirements are verified manually because there is no component-level test project.
  Adding `bunit` would close REQ-202, REQ-204, REQ-206, and REQ-210. Recorded as a known gap.
- The actor is currently self-declared in the request body. That is honest for a demonstration and
  unacceptable for a deployment; the fix is an identity provider, not a cleverer check.
