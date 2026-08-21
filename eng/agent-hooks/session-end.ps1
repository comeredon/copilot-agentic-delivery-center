#Requires -Version 7.0
<#
    sessionEnd: emit a summary of the session's tool use, denials, and errors.

    The summary is derived entirely from the day's audit trail, so it reflects what actually
    happened rather than what the agent reported.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

$payload = Read-HookPayload
$sessionId = if ($null -ne $payload) {
    Get-PayloadValue -Payload $payload -Names @('sessionId', 'session', 'id')
} else {
    ''
}

$auditDirectory = Join-Path (Resolve-Path "$PSScriptRoot/../..").Path '.agent-runtime'
$today = (Get-Date).ToUniversalTime().ToString('yyyyMMdd')
$trailPath = Join-Path $auditDirectory "hooks-$today.jsonl"

$records = @()
if (Test-Path -LiteralPath $trailPath) {
    $records = Get-Content -LiteralPath $trailPath |
        ForEach-Object {
            try { $_ | ConvertFrom-Json -Depth 8 } catch { $null }
        } |
        Where-Object { $null -ne $_ }
}

$summary = [ordered]@{
    schemaVersion = '1.0.0'
    sessionId     = $sessionId
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    counts        = [ordered]@{
        toolUse       = @($records | Where-Object { $_.event -eq 'tool-use' }).Count
        allowed       = @($records | Where-Object { $_.event -eq 'allow' }).Count
        denied        = @($records | Where-Object { $_.event -eq 'deny' }).Count
        errors        = @($records | Where-Object { $_.event -eq 'error' }).Count
        subagentStops = @($records | Where-Object { $_.event -eq 'subagent-stop' }).Count
    }
    # Denials are the interesting part of the trail: they show where policy actually bit.
    denials       = @($records |
        Where-Object { $_.event -eq 'deny' } |
        ForEach-Object { [ordered]@{ tool = $_.tool; reason = $_.reason } })
}

try {
    if (-not (Test-Path -LiteralPath $auditDirectory)) {
        New-Item -ItemType Directory -Path $auditDirectory -Force | Out-Null
    }

    $summaryPath = Join-Path $auditDirectory "session-summary-$today.json"
    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath -Encoding utf8
}
catch {
    # Never fail a session on a summary write.
}

$summary | ConvertTo-Json -Depth 8 -Compress | Write-Output
exit 0
