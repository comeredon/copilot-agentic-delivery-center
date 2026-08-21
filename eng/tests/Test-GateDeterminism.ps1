#Requires -Version 7.0
<#
.SYNOPSIS
    Proves the quality gate is deterministic.

.DESCRIPTION
    Runs the gate twice against the same commit and asserts that the canonical hash is identical
    while the envelope timestamp differs. That contrast is the whole determinism claim: the verdict
    is content-addressed, and the timing is not part of it.

    Run this after any change to the gate tool, the canonical serializer, or the report schema.

.PARAMETER Profile
    Gate profile. Defaults to cloud so the check runs without a container runtime.
#>

[CmdletBinding()]
param(
    [ValidateSet('cloud', 'ci', 'local')]
    [string] $Profile = 'cloud'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path "$PSScriptRoot/../..").Path
Push-Location $root

try {
    $commit = (git rev-parse HEAD).Trim()
    $results = @()

    foreach ($run in 1, 2) {
        $output = Join-Path $root "artifacts/determinism-run-$run.json"

        # --artifacts-path keeps the running gate binary out of the solution build the gate performs.
        dotnet run --project tools/DeliveryControlCenter.QualityGate `
            --artifacts-path artifacts/gate-host -- `
            --profile $Profile --commit $commit --root $root --output $output | Out-Null

        $report = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json

        $results += [pscustomobject]@{
            Run       = $run
            Hash      = $report.canonicalHash
            Generated = $report.envelope.generatedAtUtc
            Verdict   = $report.canonical.verdict
        }

        Write-Host ("  run {0}: verdict={1} hash={2} generatedAtUtc={3}" -f $run, $report.canonical.verdict, $report.canonicalHash, $report.envelope.generatedAtUtc)
    }

    Write-Host ''

    if ($results[0].Hash -ne $results[1].Hash) {
        Write-Host 'FAIL: canonical hashes differ. Something non-reproducible leaked into the canonical section.' -ForegroundColor Red
        exit 1
    }

    if ($results[0].Generated -eq $results[1].Generated) {
        Write-Host 'INCONCLUSIVE: the envelope timestamps are identical, so this run did not actually test the separation.' -ForegroundColor Yellow
        exit 1
    }

    Write-Host 'PASS: identical canonical hash, different envelope timestamp.' -ForegroundColor Green
    exit 0
}
finally {
    Pop-Location
}
