#Requires -Version 7.0
<#
    errorOccurred: record the error class and the tool that raised it.

    Deliberately records no payload. An error message can contain a token, a connection string, or
    untrusted remote content, and the audit trail must be safe to read.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

$payload = Read-HookPayload
if ($null -eq $payload) { exit 0 }

$tool = Get-PayloadValue -Payload $payload -Names @('tool', 'toolName', 'name')
$errorType = Get-PayloadValue -Payload $payload -Names @('errorType', 'type', 'class', 'code')

if ([string]::IsNullOrWhiteSpace($errorType)) {
    $errorType = 'unclassified'
}

Write-HookAudit -Event 'error' -Tool $tool -Reason $errorType

exit 0
