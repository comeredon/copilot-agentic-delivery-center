#Requires -Version 7.0
<#
.SYNOPSIS
    preToolUse guardrail. Denies merges, pushes to main, destructive commands, credential reads,
    and edits to the guardrail files themselves.

.DESCRIPTION
    Reads the hook payload from stdin as JSON and writes a decision object to stdout. The script
    fails closed on a malformed payload: if we cannot understand what is being attempted, we do not
    approve it.

    This is a deterrent and an audit trail, not the authoritative control. Branch protection,
    required checks, least-privilege tokens, and the human merge signal remain authoritative.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

$payload = Read-HookPayload

if ($null -eq $payload) {
    Write-HookDecision -Decision 'deny' -Reason 'The hook payload could not be parsed. Failing closed.'
    exit 0
}

$policy = Get-GuardrailPolicy
$toolName = Get-PayloadValue -Payload $payload -Names @('tool', 'toolName', 'name')
$command = Get-PayloadValue -Payload $payload -Names @('command', 'commandLine', 'input', 'arguments')
$path = Get-PayloadValue -Payload $payload -Names @('path', 'filePath', 'file', 'target')

# 1. Command patterns: merge, force push, destructive filesystem, piped remote execution.
if (-not [string]::IsNullOrWhiteSpace($command)) {
    foreach ($pattern in $policy.deniedCommandPatterns) {
        if ($command -match $pattern) {
            Write-HookAudit -Event 'deny' -Tool $toolName -Reason "command pattern: $pattern"
            Write-HookDecision -Decision 'deny' -Reason @"
This command is blocked by repository policy (matched: $pattern).

Agents in this repository may not merge, push to main, force push, bypass verification, delete
recursively, or pipe remote content into a shell. A pull request at a passing quality gate is the
end of agent authority; a human authorises the merge.

If you believe the policy is wrong, open an issue describing the problem. Do not attempt to edit
the hook or route around it.
"@
            exit 0
        }
    }
}

# 2. Path patterns: guardrail files and anything credential-shaped.
$candidatePaths = @($path, $command) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

foreach ($candidate in $candidatePaths) {
    $normalized = ($candidate -replace '\\', '/').Trim()

    foreach ($pattern in $policy.deniedPathPatterns) {
        if ($normalized -match $pattern) {
            # Deliberately do not echo the matched text; it may be a credential path.
            Write-HookAudit -Event 'deny' -Tool $toolName -Reason "path pattern: $pattern"
            Write-HookDecision -Decision 'deny' -Reason @"
This path is blocked by repository policy (matched: $pattern).

Guardrail files (.github/hooks, .github/workflows, .mcp.json, CODEOWNERS) are not agent-editable,
and credential files are not agent-readable. If a guardrail needs to change, open an issue. If you
need a secret to proceed, stop and say so.
"@
            exit 0
        }
    }
}

Write-HookAudit -Event 'allow' -Tool $toolName -Reason 'no policy match'
Write-HookDecision -Decision 'allow'
exit 0
