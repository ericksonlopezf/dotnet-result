# Copyright © Erickson Lopez. MIT License.
<#
.SYNOPSIS
    Runs Stryker.NET mutation testing locally across all or selected ecosystem packages.
.DESCRIPTION
    Executes Stryker with package-specific configuration files and designated output directories.
.PARAMETER Package
    Target package to mutate (default: "All"). Accepts any package key or "All".
.PARAMETER MutationLevel
    Mutation level to run (Basic, Standard, Advanced). Default is Standard.
#>

[CmdletBinding()]
param (
    [string]$Package = "All",
    [ValidateSet("Basic", "Standard", "Advanced")]
    [string]$MutationLevel = "Standard"
)

$ErrorActionPreference = "Stop"

$packages = [ordered]@{
    "Core"                    = @{ config = "stryker-config.json"; output = "StrykerOutput/core" }
    "Generic"                 = @{ config = "stryker-generic-config.json"; output = "StrykerOutput/generic" }
    "Maybe"                   = @{ config = "stryker-maybe-config.json"; output = "StrykerOutput/maybe" }
    "AspNetCore"              = @{ config = "stryker-aspnetcore-config.json"; output = "StrykerOutput/aspnetcore" }
    "OpenApi"                 = @{ config = "stryker-openapi-config.json"; output = "StrykerOutput/openapi" }
    "OpenTelemetry"           = @{ config = "stryker-opentelemetry-config.json"; output = "StrykerOutput/opentelemetry" }
    "FluentValidation"        = @{ config = "stryker-fluentvalidation-config.json"; output = "StrykerOutput/fluentvalidation" }
    "MediatR"                 = @{ config = "stryker-mediatr-config.json"; output = "StrykerOutput/mediatr" }
    "Serialization"           = @{ config = "stryker-serialization-config.json"; output = "StrykerOutput/serialization" }
    "SerializationGenerators" = @{ config = "stryker-serialization-generators-config.json"; output = "StrykerOutput/serialization-generators" }
    "DomainErrorsGenerators"  = @{ config = "stryker-domainerrors-generators-config.json"; output = "StrykerOutput/domainerrors-generators" }
    "EntityFrameworkCore"     = @{ config = "stryker-efcore-config.json"; output = "StrykerOutput/efcore" }
    "Polly"                   = @{ config = "stryker-polly-config.json"; output = "StrykerOutput/polly" }
    "MassTransit"             = @{ config = "stryker-masstransit-config.json"; output = "StrykerOutput/masstransit" }
    "Analyzers"               = @{ config = "stryker-analyzers-config.json"; output = "StrykerOutput/analyzers" }
    "Testing"                 = @{ config = "stryker-testing-config.json"; output = "StrykerOutput/testing" }
    "TestingNUnit"            = @{ config = "stryker-testing-nunit-config.json"; output = "StrykerOutput/testing-nunit" }
    "TestingXUnit"            = @{ config = "stryker-testing-xunit-config.json"; output = "StrykerOutput/testing-xunit" }
    "Dapr"                    = @{ config = "stryker-dapr-config.json"; output = "StrykerOutput/dapr" }
    "Grpc"                    = @{ config = "stryker-grpc-config.json"; output = "StrykerOutput/grpc" }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repoRoot

$targets = @()
if ($Package -eq "All") {
    $targets = $packages.Keys
} elseif ($packages.Contains($Package)) {
    $targets = @($Package)
} else {
    Write-Error "Unknown package '$Package'. Available options: $($packages.Keys -join ', '), All"
    exit 1
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  STRYKER.NET LOCAL MUTATION TESTING RUNNER       " -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Target Packages: $($targets -join ', ')"
Write-Host "Mutation Level : $MutationLevel`n"

$failed = $false

foreach ($t in $targets) {
    $entry = $packages[$t]
    $configPath = Join-Path $repoRoot $entry.config
    $outputDir = Join-Path $repoRoot $entry.output

    if (-not (Test-Path $configPath)) {
        Write-Warning "Configuration file '$($entry.config)' not found. Skipping $t."
        continue
    }

    Write-Host "Running Stryker for $t ($($entry.config))..." -ForegroundColor Yellow
    dotnet stryker --config-file $entry.config --mutation-level $MutationLevel --output $entry.output
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Stryker failed or fell below break threshold for $t" -ForegroundColor Red
        $failed = $true
    } else {
        Write-Host "✅ $t passed Stryker mutation testing." -ForegroundColor Green
    }
}

if ($failed) {
    Write-Host "`nOne or more packages failed the mutation quality gate." -ForegroundColor Red
    exit 1
}

Write-Host "`nAll tested packages passed the Stryker mutation quality gate!" -ForegroundColor Green
exit 0
