# MCP configuration

## What is configured

`.mcp.json` at the repository root registers exactly one server.

| Server | Transport | Authentication | Tools |
|---|---|---|---|
| `microsoft-learn` | HTTP | None | `microsoft_docs_search`, `microsoft_docs_fetch`, `microsoft_code_sample_search` |

Three tools, enumerated explicitly. No wildcards. The server is read-only and unauthenticated, so
there is no token to leak and nothing it can mutate.

## What is deliberately *not* configured

**GitHub.** The built-in GitHub MCP server is used as-is. Registering a second, hand-configured
GitHub server would mean managing a token in configuration — exactly the pattern this repository
tells agents not to use. The built-in server is already correctly scoped.

**Projects.** A separately named Projects profile is added only after live tool discovery confirms
which tools exist, and then every tool is enumerated by name. A wildcard here would silently grant
whatever the server adds in a future release.

## Rules for changing this file

`.mcp.json` is in the hook deny list. Agents cannot edit it. If a server or a tool needs to be
added:

1. Open an issue stating which tool, why it is needed, and what it can write.
2. A human makes the change.

The reasoning is straightforward: MCP configuration is a capability grant. An agent that can widen
its own capability grant has no capability boundary at all.

## Cloud agent

`config/github/copilot-mcp.json` mirrors this configuration in the shape the cloud agent's
repository settings expect, for manual entry. Keep the two in sync — a drift between them means
the cloud agent and the local agent have different capabilities, which makes their reports
incomparable.
