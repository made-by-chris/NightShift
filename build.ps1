param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [string]$Runtime = "win-x64",

    [string]$OutputDir = "dist"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceFile = Join-Path $scriptDir "NightShift.cs"
$publishDir = Join-Path $scriptDir $OutputDir
$exePath = Join-Path $publishDir "NightShift.exe"

if (-not (Test-Path $sourceFile)) {
    throw "Source file not found: $sourceFile"
}

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host "Publishing NightShift..."
dotnet publish $sourceFile `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishAot=false `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code: $LASTEXITCODE"
}

if (-not (Test-Path $exePath)) {
    throw "Publish succeeded but executable not found: $exePath"
}

Write-Host "完成: $exePath"