---
description: C# and .NET conventions for this repository.
applyTo: "**/*.cs"
---

# .NET conventions

Target is `net10.0` with C# 14. `TreatWarningsAsErrors` is on, so a warning is a build break.

## Language and style

- File-scoped namespaces. One public type per file.
- Nullable reference types are enabled. Do not silence a nullable warning with `!` unless you can
  explain in a comment why the value cannot be null.
- Prefer primary constructors for services with injected dependencies.
- Prefer collection expressions (`[]`, `[.. source]`) over `new List<T>()` plus `AddRange`.
- Private fields are `_camelCase`. Constants are `PascalCase`.
- `sealed` by default. Only unseal when a type is genuinely designed for inheritance.

## Async

- Every I/O-bound method is `async` and takes a `CancellationToken` as the last parameter.
- Never `async void` outside an event handler. Never `.Result` or `.Wait()`.
- Do not add `ConfigureAwait(false)`; this repository has no synchronisation context that needs it,
  and CA2007 is disabled deliberately.

## Dependency injection

- Register against the interface, never the concrete type.
- Ports live in `Application/Abstractions`. Adapters live in `Infrastructure`. Application code
  that references `Microsoft.EntityFrameworkCore`, `Npgsql`, `StackExchange.Redis`, or
  `System.Net.Http` is a layering violation and the architecture tests will fail the build.
- Scoped lifetime for anything touching `DbContext`. Singleton only for genuinely stateless or
  internally-synchronised services.

## Error handling

- Throw `DomainException` for invariant violations in the domain layer.
- Catch narrowly. `catch (Exception)` is acceptable only at a process boundary (a background loop,
  a request pipeline) and must re-throw or log with the exception object.
- Never swallow `OperationCanceledException` — let cancellation propagate.
- Never put user input, tokens, or full response bodies into an exception message.

## Comments

Explain *why*, not *what*. A comment that restates the code is noise. Comment the non-obvious
decision: why polling instead of webhooks, why this cache TTL, why this check is not required in
the cloud profile.
