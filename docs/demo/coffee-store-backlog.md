# Ninth Parallel — GitHub Project Demo Backlog

These issues are deliberately prepared for live Copilot delivery. Each has a visible product result,
a meaningful engineering boundary, and a verification story.

## NP-101 — Persist guest carts

**Problem:** The current Blazor circuit cart is lost on a full reload.

**Acceptance criteria**

- `REQ-101` Create an opaque guest-cart token in a secure, same-site cookie.
- `REQ-102` Persist cart lines through an Application port; the client submits variant ID and
  quantity, never price.
- `REQ-103` Cache cart reads in Redis with a 30-minute sliding expiration and PostgreSQL fallback.
- `REQ-104` A refresh, reconnect, or second tab shows the same cart.
- `REQ-105` Cart telemetry contains no cookie token or customer data.

**Demo surfaces:** Plan mode, Redis research through Learn MCP, backend + Blazor specialists,
privacy threat review, Testcontainers.

## NP-102 — Idempotent guest checkout

**Problem:** Customers can build a cart but cannot place an order.

**Acceptance criteria**

- Server computes price, shipping, tax, and total.
- One idempotency key creates at most one order and one stock reservation.
- Payment is a deterministic simulator behind `IPaymentAuthorizer`; no card data is stored.
- Decline reserves nothing; replay returns the original confirmation.
- Confirmation snapshots coffee name, lot, bag, grind, and unit price.

**Demo surfaces:** architecture review, transactions, security review, deterministic tests, PR repair.

## NP-103 — Prevent concurrent overselling

**Problem:** Two checkouts for the final bag must not both succeed.

**Acceptance criteria**

- Use the existing domain `Version` concurrency token.
- Exactly one of two concurrent reservations for the final unit succeeds.
- The loser receives a stable out-of-stock ProblemDetails response.
- Catalog cache is invalidated only after commit.

**Demo surfaces:** adversarial test, EF concurrency, Testcontainers, Rubber Duck.

## NP-104 — Promotion rules

Add date-bounded codes, minimum totals, fixed/percentage discounts, one-use limits, and bundle rules.
Require property tests proving totals never become negative and reordering lines does not change the
discount.

## NP-105 — Taste preference quiz

Guide visitors through acidity, body, roast, brew method, and familiar flavors. Produce explainable
recommendations from catalog data; no generative AI in the first version.

## NP-106 — Coffee subscriptions

Support cadence, bag/grind selection, skip/pause, next-run calculation, and an idempotent Worker job.
No real recurring payment in the demo adapter.

## NP-107 — Reviews and moderation

Add authenticated reviews, verified-purchase status, moderation state, output encoding, abuse limits,
and an explicit prompt-injection test for review text consumed by agents.

## NP-108 — Live low-stock notifications

Publish stock-change hints over Redis and SignalR; API instances invalidate local L1 and Web refetches
through HTTP. A Redis outage must degrade to TTL-based convergence.

## NP-109 — Accessibility audit

Add Playwright + axe, keyboard flows, focus restoration, reduced-motion behavior, and desktop/mobile
screenshots. Use Copilot code review to identify and repair findings.

## NP-110 — AI coffee guide

Use Microsoft Foundry only after the deterministic quiz exists. Ground answers in current catalog
data, distinguish facts from suggestions, evaluate recommendation quality, and never invent producer
or traceability claims.