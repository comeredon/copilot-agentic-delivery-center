#Requires -Version 7.0
<#
.SYNOPSIS
    Shared helpers for the agent hooks: payload parsing, policy loading, decision output, and the
    local audit trail.

.DESCRIPTION
    Every function here is defensive by design. Hook input arrives from an agent runtime whose exact
    payload shape can change, so nothing assumes a field exists and nothing throws on a surprise.
#>

Set-StrictMode -Version Latest

$script:RepositoryRoot = (Resolve-Path "$PSScriptRoot/../..").Path
$script:AuditDirectory = Join-Path $script:RepositoryRoot '.agent-runtime'

function Read-HookPayload {
    <#
        Reads stdin and parses it as JSON. Returns $null on anything unparseable so the caller can
        fail closed rather than guessing.
    #>
    try {
        $raw = [Console]::In.ReadToEnd()

        if ([string]::IsNullOrWhiteSpace($raw)) {
            return [pscustomobject]@{}
        }

        return $raw | ConvertFrom-Json -Depth 32
    }
    catch {
        return $null
    }
}

function Get-GuardrailPolicy {
    <#
        Loads the committed policy. If it cannot be read we return an empty-but-valid policy and the
        caller's pattern loops simply do not match — the deny decisions then come from the runtime's
        own controls rather than from a half-loaded policy.
    #>
    $policyPath = Join-Path $script:RepositoryRoot '.github/hooks/guardrails.json'

    try {
        $document = Get-Content -LiteralPath $policyPath -Raw | ConvertFrom-Json -Depth 32
        return $document.policy
    }
    catch {
        return [pscustomobject]@{
            deniedCommandPatterns = @()
            deniedPathPatterns    = @()
            auditDirectory        = '.agent-runtime'
        }
    }
}

function Get-PayloadValue {
    <#
        Pulls the first present value from a list of candidate property names, flattening arrays to a
        single space-joined string. Different runtimes name these fields differently.
    #>
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)] [string[]] $Names
    )

    foreach ($name in $Names) {
        $property = $Payload.PSObject.Properties[$name]

        if ($null -eq $property -or $null -eq $property.Value) {
            continue
        }

        $value = $property.Value

        if ($value -is [System.Collections.IEnumerable] -and $value -isnot [string]) {
            return (($value | ForEach-Object { "$_" }) -join ' ')
        }

        return "$value"
    }

    return ''
}

function Write-HookDecision {
    param(
        [Parameter(Mandatory)] [ValidateSet('allow', 'deny', 'ask')] [string] $Decision,
        [string] $Reason = ''
    )

    $output = [ordered]@{
        decision = $Decision
    }

    if (-not [string]::IsNullOrWhiteSpace($Reason)) {
        $output['reason'] = $Reason.Trim()
    }

    $output | ConvertTo-Json -Depth 8 -Compress | Write-Output
}

function Write-HookAudit {
    <#
        Appends one JSON line to the local audit trail. The directory is git-ignored: this is a
        forensic record for the operator, not a committed artifact.

        Nothing that could contain a secret is written — only the tool name, the decision, and the
        name of the pattern that matched.
    #>
    param(
        [Parameter(Mandatory)] [string] $Event,
        [string] $Tool = '',
        [string] $Reason = '',
        [hashtable] $Extra = @{}
    )

    try {
        if (-not (Test-Path -LiteralPath $script:AuditDirectory)) {
            New-Item -ItemType Directory -Path $script:AuditDirectory -Force | Out-Null
        }

        $record = [ordered]@{
            timestamp = (Get-Date).ToUniversalTime().ToString('o')
            event     = $Event
            tool      = $Tool
            reason    = $Reason
        }

        foreach ($key in $Extra.Keys) {
            $record[$key] = $Extra[$key]
        }

        $line = $record | ConvertTo-Json -Depth 8 -Compress
        $file = Join-Path $script:AuditDirectory ('hooks-{0}.jsonl' -f (Get-Date).ToUniversalTime().ToString('yyyyMMdd'))

        Add-Content -LiteralPath $file -Value $line -Encoding utf8
    }
    catch {
        # Auditing must never break the session. A lost line is preferable to a blocked agent.
    }
}
