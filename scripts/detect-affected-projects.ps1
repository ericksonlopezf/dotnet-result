# Copyright © Erickson Lopez. MIT License.
<#
.SYNOPSIS
    Detects which packages are affected by a Git diff (e.g. Pull Request vs main).
.DESCRIPTION
    Analyzes changed files between a base commit and head commit.
    Categorizes changes to determine:
    1. Whether production C# code or tests were modified (skipping Stryker if docs-only).
    2. Which specific package(s) were modified, building a targeted matrix for Stryker.
.PARAMETER BaseRef
    Base git reference (default: "origin/main").
.PARAMETER HeadRef
    Head git reference (default: "HEAD").
.PARAMETER RunAll
    Forces inclusion of all packages regardless of git diff (useful for main push, schedule, or manual release).
#>

[CmdletBinding()]
param (
    [string]$BaseRef = "origin/main",
    [string]$HeadRef = "HEAD",
    [switch]$RunAll,
    [string]$RunAllMode = "false"
)

$ErrorActionPreference = "Stop"

$shouldRunAll = $RunAll.IsPresent -or ($RunAllMode -eq "true" -or $RunAllMode -eq "True" -or $RunAllMode -eq "1" -or $RunAllMode -eq "all")

$packageMap = [ordered]@{
    "Core" = @{
        patterns = @("src/EricksonLopez.Result/", "tests/EricksonLopez.Result.Tests/")
        config = "stryker-config.json"
        outputDir = "StrykerOutput/core"
        artifact = "stryker-report-core"
    }
    "Generic" = @{
        patterns = @("src/EricksonLopez.Result.Generic/", "tests/EricksonLopez.Result.Generic.Tests/")
        config = "stryker-generic-config.json"
        outputDir = "StrykerOutput/generic"
        artifact = "stryker-report-generic"
    }
    "Maybe" = @{
        patterns = @("src/EricksonLopez.Result.Maybe/", "tests/EricksonLopez.Result.Maybe.Tests/")
        config = "stryker-maybe-config.json"
        outputDir = "StrykerOutput/maybe"
        artifact = "stryker-report-maybe"
    }
    "AspNetCore" = @{
        patterns = @("src/EricksonLopez.Result.AspNetCore/", "tests/EricksonLopez.Result.AspNetCore.Tests/")
        config = "stryker-aspnetcore-config.json"
        outputDir = "StrykerOutput/aspnetcore"
        artifact = "stryker-report-aspnetcore"
    }
    "FluentValidation" = @{
        patterns = @("src/EricksonLopez.Result.FluentValidation/", "tests/EricksonLopez.Result.FluentValidation.Tests/")
        config = "stryker-fluentvalidation-config.json"
        outputDir = "StrykerOutput/fluentvalidation"
        artifact = "stryker-report-fluentvalidation"
    }
    "MediatR" = @{
        patterns = @("src/EricksonLopez.Result.MediatR/", "tests/EricksonLopez.Result.MediatR.Tests/")
        config = "stryker-mediatr-config.json"
        outputDir = "StrykerOutput/mediatr"
        artifact = "stryker-report-mediatr"
    }
    "OpenApi" = @{
        patterns = @("src/EricksonLopez.Result.OpenApi/", "tests/EricksonLopez.Result.OpenApi.Tests/")
        config = "stryker-openapi-config.json"
        outputDir = "StrykerOutput/openapi"
        artifact = "stryker-report-openapi"
    }
    "OpenTelemetry" = @{
        patterns = @("src/EricksonLopez.Result.OpenTelemetry/", "tests/EricksonLopez.Result.OpenTelemetry.Tests/")
        config = "stryker-opentelemetry-config.json"
        outputDir = "StrykerOutput/opentelemetry"
        artifact = "stryker-report-opentelemetry"
    }
    "Serialization" = @{
        patterns = @("src/EricksonLopez.Result.Serialization/", "tests/EricksonLopez.Result.Serialization.Tests/")
        config = "stryker-serialization-config.json"
        outputDir = "StrykerOutput/serialization"
        artifact = "stryker-report-serialization"
    }
    "SerializationGenerators" = @{
        patterns = @("src/EricksonLopez.Result.Serialization.Generators/", "tests/EricksonLopez.Result.Serialization.Generators.Tests/")
        config = "stryker-serialization-generators-config.json"
        outputDir = "StrykerOutput/serialization-generators"
        artifact = "stryker-report-serialization-generators"
    }
    "Analyzers" = @{
        patterns = @("src/EricksonLopez.Result.Analyzers/", "tests/EricksonLopez.Result.Analyzers.Tests/")
        config = "stryker-analyzers-config.json"
        outputDir = "StrykerOutput/analyzers"
        artifact = "stryker-report-analyzers"
    }
    "Testing" = @{
        patterns = @("src/EricksonLopez.Result.Testing/", "tests/EricksonLopez.Result.Testing.Tests/")
        config = "stryker-testing-config.json"
        outputDir = "StrykerOutput/testing"
        artifact = "stryker-report-testing"
    }
    "TestingNUnit" = @{
        patterns = @("src/EricksonLopez.Result.Testing.NUnit/", "tests/EricksonLopez.Result.Testing.NUnit.Tests/")
        config = "stryker-testing-nunit-config.json"
        outputDir = "StrykerOutput/testing-nunit"
        artifact = "stryker-report-testing-nunit"
    }
    "TestingXUnit" = @{
        patterns = @("src/EricksonLopez.Result.Testing.XUnit/", "tests/EricksonLopez.Result.Testing.XUnit.Tests/")
        config = "stryker-testing-xunit-config.json"
        outputDir = "StrykerOutput/testing-xunit"
        artifact = "stryker-report-testing-xunit"
    }
    "DomainErrorsGenerators" = @{
        patterns = @("src/EricksonLopez.Result.DomainErrors.Generators/", "tests/EricksonLopez.Result.DomainErrors.Generators.Tests/")
        config = "stryker-domainerrors-generators-config.json"
        outputDir = "StrykerOutput/domainerrors-generators"
        artifact = "stryker-report-domainerrors-generators"
    }
    "EntityFrameworkCore" = @{
        patterns = @("src/EricksonLopez.Result.EntityFrameworkCore/", "tests/EricksonLopez.Result.EntityFrameworkCore.Tests/")
        config = "stryker-efcore-config.json"
        outputDir = "StrykerOutput/efcore"
        artifact = "stryker-report-efcore"
    }
    "Polly" = @{
        patterns = @("src/EricksonLopez.Result.Polly/", "tests/EricksonLopez.Result.Polly.Tests/")
        config = "stryker-polly-config.json"
        outputDir = "StrykerOutput/polly"
        artifact = "stryker-report-polly"
    }
    "MassTransit" = @{
        patterns = @("src/EricksonLopez.Result.MassTransit/", "tests/EricksonLopez.Result.MassTransit.Tests/")
        config = "stryker-masstransit-config.json"
        outputDir = "StrykerOutput/masstransit"
        artifact = "stryker-report-masstransit"
    }
    "Dapr" = @{
        patterns = @("src/EricksonLopez.Result.Dapr/", "tests/EricksonLopez.Result.Dapr.Tests/")
        config = "stryker-dapr-config.json"
        outputDir = "StrykerOutput/dapr"
        artifact = "stryker-report-dapr"
    }
    "Grpc" = @{
        patterns = @("src/EricksonLopez.Result.Grpc/", "tests/EricksonLopez.Result.Grpc.Tests/")
        config = "stryker-grpc-config.json"
        outputDir = "StrykerOutput/grpc"
        artifact = "stryker-report-grpc"
    }
}

$globalTriggers = @("Directory.Build.props", "Directory.Packages.props", "EricksonLopez.Result.slnx", ".editorconfig")

$hasCodeChanges = $false
$allAffected = $false
$affectedPackages = [System.Collections.Generic.HashSet[string]]::new()

if ($shouldRunAll) {
    Write-Host "Forced RunAll mode enabled: running Stryker across all $($packageMap.Count) packages." -ForegroundColor Cyan
    $hasCodeChanges = $true
    $allAffected = $true
    foreach ($pkg in $packageMap.Keys) {
        [void]$affectedPackages.Add($pkg)
    }
} else {
    $changedFiles = @()
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    try {
        # Attempt 1: 3-dot diff between BaseRef and HeadRef (standard PR comparison)
        $rawOutput = git diff --name-only "$BaseRef...$HeadRef" 2>&1
        $changedFiles = @($rawOutput | Where-Object { $_ -is [string] -and $_ -notmatch '^(warning:|fatal:)' -and $_.Trim() -ne "" })
        
        if ($LASTEXITCODE -ne 0 -or $changedFiles.Count -eq 0) {
            # Attempt 2: 2-dot diff
            $rawOutput2 = git diff --name-only "$BaseRef" "$HeadRef" 2>&1
            $changedFiles = @($rawOutput2 | Where-Object { $_ -is [string] -and $_ -notmatch '^(warning:|fatal:)' -and $_.Trim() -ne "" })
        }
    } catch {
        $changedFiles = @()
    }

    if ($changedFiles.Count -eq 0) {
        try {
            # Fallback to local unstaged/staged diff
            $rawOutput3 = git diff --name-only HEAD 2>&1
            $changedFiles = @($rawOutput3 | Where-Object { $_ -is [string] -and $_ -notmatch '^(warning:|fatal:)' -and $_.Trim() -ne "" })
        } catch {
            $changedFiles = @()
        }
    }
    $ErrorActionPreference = $prevEap

    Write-Host "Evaluating $(($changedFiles | Measure-Object).Count) changed file(s)..." -ForegroundColor Cyan

    foreach ($file in $changedFiles) {
        $norm = $file.Replace("\", "/")
        
        # Check global build triggers
        foreach ($gt in $globalTriggers) {
            if ($norm -eq $gt) {
                $hasCodeChanges = $true
                $allAffected = $true
                Write-Host "  Trigger: Global file modified -> $norm" -ForegroundColor Yellow
            }
        }
        
        # Check package-specific paths
        foreach ($pkg in $packageMap.Keys) {
            foreach ($pat in $packageMap[$pkg].patterns) {
                if ($norm.StartsWith($pat)) {
                    $hasCodeChanges = $true
                    [void]$affectedPackages.Add($pkg)
                    Write-Host "  Trigger: $pkg affected by -> $norm" -ForegroundColor Green
                }
            }
        }
    }

    if ($allAffected) {
        foreach ($pkg in $packageMap.Keys) {
            [void]$affectedPackages.Add($pkg)
        }
    }
}

$matrixInclude = @()
foreach ($pkg in $packageMap.Keys) {
    if ($affectedPackages.Contains($pkg)) {
        $info = $packageMap[$pkg]
        $matrixInclude += [ordered]@{
            name = $pkg
            config = $info.config
            "output-dir" = $info.outputDir
            "artifact-name" = $info.artifact
        }
    }
}

# Ensure matrix is always a valid JSON array format `[{...}]`
if ($matrixInclude.Count -eq 0) {
    $matrixJson = "[]"
} elseif ($matrixInclude.Count -eq 1) {
    $singleJson = $matrixInclude[0] | ConvertTo-Json -Compress
    $matrixJson = "[$singleJson]"
} else {
    $matrixJson = $matrixInclude | ConvertTo-Json -Compress
}

Write-Host "`nSummary:" -ForegroundColor Cyan
Write-Host "  Has code changes        : $hasCodeChanges"
Write-Host "  Affected packages count : $($affectedPackages.Count)"
Write-Host "  Matrix JSON             : $matrixJson"

# Output for GitHub Actions
if ($env:GITHUB_OUTPUT) {
    $hasChangesStr = if ($hasCodeChanges) { "true" } else { "false" }
    $hasPkgsStr = if ($affectedPackages.Count -gt 0) { "true" } else { "false" }
    
    "has_code_changes=$hasChangesStr" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "has_affected_packages=$hasPkgsStr" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "affected_count=$($affectedPackages.Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "matrix=$matrixJson" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}
