$projects = @(
    "EricksonLopez.Result.csproj",
    "EricksonLopez.Result.AspNetCore.csproj",
    "EricksonLopez.Result.FluentValidation.csproj",
    "EricksonLopez.Result.MediatR.csproj",
    "EricksonLopez.Result.OpenTelemetry.csproj",
    "EricksonLopez.Result.Serialization.csproj",
    "EricksonLopez.Result.Testing.csproj"
)

$testsPath = "tests\EricksonLopez.Result.Tests"
$failed = $false

foreach ($proj in $projects) {
    Write-Host "Running Stryker for $proj..." -ForegroundColor Cyan
    Set-Location $PSScriptRoot\$testsPath
    dotnet stryker -p $proj -f ..\..\stryker-config.json
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Stryker failed or didn't reach 100% for $proj" -ForegroundColor Red
        $failed = $true
    }
}

Set-Location $PSScriptRoot
if ($failed) {
    exit 1
}
Write-Host "All projects passed Stryker with 100% mutation score!" -ForegroundColor Green
exit 0
