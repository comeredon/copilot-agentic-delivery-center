#Requires -Version 7.0
<#
    subagentStop: capture a subagent's terminal verdict so the gate ledger can cite it.

    This is what makes "a different agent verified it" checkable after the fact rather than
    asserted in prose.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

$payload = Read-HookPayload
if ($null -eq $payload) { exit 0 }

$agent = Get-PayloadValue -Payload $payload -Names @('agent', 'agentName', 'subagent', 'name')
$model = Get-PayloadValue -Payload $payload -Names @('model', 'modelId')
$verdict = Get-PayloadValue -Payload $payload -Names @('verdict', 'status', 'outcome', 'result')

Write-HookAudit -Event 'subagent-stop' -Tool $agent -Reason $verdict -Extra @{ model = $model }

exit 0
