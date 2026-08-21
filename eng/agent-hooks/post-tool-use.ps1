#Requires -Version 7.0
<# postToolUse: append a sanitized record of the invocation to the local audit trail. #>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

$payload = Read-HookPayload
if ($null -eq $payload) { exit 0 }

$tool = Get-PayloadValue -Payload $payload -Names @('tool', 'toolName', 'name')
$status = Get-PayloadValue -Payload $payload -Names @('status', 'outcome', 'result')

# Only the shape of what happened is recorded. Arguments and outputs may contain untrusted
# content or credentials, so they never reach the trail.
Write-HookAudit -Event 'tool-use' -Tool $tool -Reason $status

exit 0
