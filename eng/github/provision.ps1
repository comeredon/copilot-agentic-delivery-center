#Requires -Version 7.0
<#
.SYNOPSIS
    Provisions the GitHub repository, labels, and Project fields this demonstration expects.

.DESCRIPTION
    Idempotent: every step reads current state before writing, so re-running it is safe and quiet.

    This script is for a human operator. Agents cannot run it — it touches repository settings,
    which is outside any agent's authority, and the hook policy denies the commands it uses.

.PARAMETER Owner
    The repository owner.

.PARAMETER Repository
    The repository name.

.PARAMETER WhatIf
    Report the intended changes without making them.

.EXAMPLE
    ./eng/github/provision.ps1 -Owner comeredon -Repository copilot-agentic-delivery-center -WhatIf
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)] [string] $Owner,
    [Parameter(Mandatory)] [string] $Repository
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$slug = "$Owner/$Repository"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "The GitHub CLI (gh) is required. Install it and run 'gh auth login' first."
}

Write-Host "Provisioning $slug" -ForegroundColor Cyan

# --- Labels -----------------------------------------------------------------------------------
# 'needs-human' is the important one: it is how an agent signals that it has stopped rather than
# looping, and it is what a human filters on.
$labels = @(
    @{ Name = 'needs-human';       Color = 'B60205'; Description = 'An agent stopped and escalated. A human must decide.' }
    @{ Name = 'agent-delivered';   Color = '0E8A16'; Description = 'Implemented by an agent, verified by a second agent.' }
    @{ Name = 'gate-failed';       Color = 'D93F0B'; Description = 'The deterministic quality gate returned fail.' }
    @{ Name = 'guardrail';         Color = '5319E7'; Description = 'Touches the agent guardrails. Human authorship only.' }
    @{ Name = 'prompt-injection';  Color = 'B60205'; Description = 'Content containing instructions aimed at an agent.' }
)

$existingLabels = @(gh label list --repo $slug --json name --limit 200 | ConvertFrom-Json | ForEach-Object { $_.name })

foreach ($label in $labels) {
    if ($existingLabels -contains $label.Name) {
        Write-Host "  label '$($label.Name)' already exists" -ForegroundColor DarkGray
        continue
    }

    if ($PSCmdlet.ShouldProcess($label.Name, 'create label')) {
        gh label create $label.Name --repo $slug --color $label.Color --description $label.Description | Out-Null
        Write-Host "  created label '$($label.Name)'" -ForegroundColor Green
    }
}

# --- Branch protection ------------------------------------------------------------------------
# Deliberately NOT automated. Branch protection is the authoritative control that stops an agent
# merging; configuring it from a script that an agent could one day be pointed at would undermine
# the whole model. The settings are printed for a human to apply.
Write-Host ''
Write-Host 'Branch protection must be applied by hand at:' -ForegroundColor Yellow
Write-Host "  https://github.com/$slug/settings/rules" -ForegroundColor Yellow
Write-Host ''
Write-Host '  Required settings for main:' -ForegroundColor Yellow
Write-Host '    - Require a pull request before merging'
Write-Host '    - Require review from Code Owners'
Write-Host '    - Require status checks: "Quality gate (ci profile)", "Guardrail fixtures", "Protected paths untouched"'
Write-Host '    - Block force pushes'
Write-Host '    - Restrict deletions'
Write-Host '    - Do NOT enable auto-merge'
Write-Host ''

Write-Host "Provisioning complete for $slug." -ForegroundColor Cyan
