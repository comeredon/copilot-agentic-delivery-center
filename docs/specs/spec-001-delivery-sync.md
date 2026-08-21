# SPEC-001 — Delivery synchronisation

## Problem

The dashboard needs a local projection of a GitHub repository's issues and pull requests. It must
stay current without a public ingress, without exhausting the API rate limit, and without producing
spurious change events when nothing has actually changed.

## Requirements

| ID | Requirement |
|---|---|
| REQ-001 | Issues and pull requests are pulled on a configurable interval and projected into `WorkItem` records keyed by number. |
| REQ-002 | Each feed keeps a durable cursor that survives a process restart. |
| REQ-003 | Each poll sends the stored ETag as `If-None-Match`; a `304` transfers no items and updates nothing. |
| REQ-004 | Applying a remote snapshot whose content is unchanged mutates nothing, bumps no version, and invalidates no cache key. |
| REQ-005 | A feed that fails five consecutive times is dead-lettered and skipped until a human intervenes. |
| REQ-006 | A sync failure is recorded as an audit event; the failure reason is the exception class, never the response body. |
| REQ-007 | Throttling (`403`/`429`) preserves the previous cursor rather than discarding it. |
| REQ-008 | With `GitHub:Enabled` false, a deterministic seeded reader supplies the payload so the application runs with no credentials. |

## Non-goals

- Webhooks. Polling was chosen so the demonstration needs no public ingress and no shared secret.
- Writing anything back to GitHub beyond the separately guarded Project field.
- Syncing comments, reviews, check runs, or commit history.
- Real-time sub-second freshness. The interval is a minute by default and that is sufficient.

## Design

`SynchronizeWorkItemsHandler` orchestrates one feed per call:

1. `ISyncCursorRepository.GetOrCreateAsync(feed)` — durable position and ETag.
2. Dead-letter check. A parked feed returns `SyncOutcome.Skipped` without calling GitHub at all,
   so a broken feed costs nothing.
3. `IGitHubReader.Fetch*Async(etag)` — conditional request.
4. `RemoteSyncResult.NotModified` short-circuits: record success, save, return.
5. Otherwise reconcile by number. `WorkItem.ApplyRemoteSnapshot` returns `false` when nothing
   changed, and only a real change increments `changed`.
6. Invalidate the read model **only** when `changed > 0`, then notify.

The idempotency in step 5 is the subtle requirement. Without it, every poll that transferred a
payload would touch every entity, bump every version, and invalidate the cache — turning a
read-mostly system into a write-heavy one for no benefit.

Agentic metadata (`Stage`, `Risk`, `Workstream`, `LatestGateHash`, `HumanAuthorized`) is never
overwritten by a remote snapshot. GitHub owns title, URL, and state; this application owns
everything else.

## Verification

| Requirement | Test |
|---|---|
| REQ-001 | `SynchronizeWorkItemsHandlerTests.FirstSync_CreatesItemsAndInvalidatesTheReadModel` |
| REQ-002 | `DeliveryEndpointsTests.SyncCycle_PersistsWorkItemsAndTheyAppearOnTheApi` |
| REQ-003 | `SynchronizeWorkItemsHandlerTests.NotModified_DoesNotTouchTheReadModel`, `DeliveryEndpointsTests.SecondSyncCycle_IsShortCircuitedByTheETag` |
| REQ-004 | `WorkItemTests.ApplyRemoteSnapshot_IsIdempotentForUnchangedInput`, `SynchronizeWorkItemsHandlerTests.RepeatedSync_WithIdenticalPayload_ChangesNothing` |
| REQ-005 | `SyncCursorTests.RecordFailure_DeadLettersAfterFiveConsecutiveFailures`, `SynchronizeWorkItemsHandlerTests.DeadLetteredFeed_IsSkippedWithoutCallingGitHub` |
| REQ-006 | `SynchronizeWorkItemsHandlerTests.Failure_RecordsAnAuditEventAndAdvancesTheFailureStreak` |
| REQ-007 | `GitHubRestReader` returns `RemoteSyncResult.Unchanged` on `403`/`429` — covered by inspection; no live-throttle test exists |
| REQ-008 | `DeliveryEndpointsTests.Overview_IsServedFromTheRealStack` |

## Open questions

- REQ-007 has no automated test because simulating a throttle requires a fake `HttpMessageHandler`
  that the current fixture does not provide. This is a known coverage gap, recorded rather than
  hidden.
- Dead-lettered feeds currently need a manual database change to recover. A reset endpoint is
  deliberately out of scope until there is a reason to add one.
