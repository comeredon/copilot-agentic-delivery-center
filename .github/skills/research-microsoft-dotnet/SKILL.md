---
name: research-microsoft-dotnet
description: Look up .NET, ASP.NET Core, EF Core, Blazor, or Aspire APIs and guidance on Microsoft Learn before choosing an approach. Use whenever you are about to pick an API, an overload, a configuration key, or a package version and you are working from recall rather than from a source. Do NOT use for repository-specific conventions — those live in the instructions files.
---

# Research Microsoft and .NET before you choose

A hallucinated overload costs a full implementation and repair cycle. A lookup costs one tool call.

## When to look it up

Look it up if any of these are true:

- You are about to name a method, an overload, an attribute, or a configuration key from memory.
- The API arrived or changed recently (Aspire, HybridCache, `Guid.CreateVersion7`, minimal API
  result types, xUnit v3).
- You are choosing between two approaches and the trade-off depends on framework behaviour.
- You are about to add a package.

## Procedure

1. `microsoft_docs_search` with a specific query. "EF Core owned collection navigation field access"
   beats "EF Core relationships".
2. If the result is a summary and you need exactness, `microsoft_docs_fetch` the page.
3. `microsoft_code_sample_search` when you need a working shape rather than prose.
4. Record what you found and where. The reference belongs in your evidence, not just in your head.

## Verify against this repository

Documentation tells you what is possible. It does not tell you what this repository has decided.
After the lookup, confirm against:

- `Directory.Packages.props` — the pinned version. Do not assume the latest.
- `global.json` — the pinned SDK.
- The relevant `.github/instructions/*.instructions.md` — local conventions win over general
  guidance.

If the documentation and the repository conventions disagree, the repository wins, and the
disagreement is worth mentioning in your report.

## Reporting

Cite the page you used. "Per the EF Core owned-types documentation, `OwnsMany` with
`PropertyAccessMode.Field` is the supported pattern for a private backing collection" is evidence.
"EF Core supports this" is not.
