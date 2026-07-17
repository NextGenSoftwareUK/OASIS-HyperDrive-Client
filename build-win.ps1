#!/usr/bin/env pwsh
param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = "dist\win"
)

$ErrorActionPreference = "Stop"
$project = "src\OasisHyperDriveClient\OasisHyperDriveClient.csproj"

Write-Host "Building OASIS HyperDrive Client $Version for Windows..."

dotnet publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $OutputDir `
    -p:PublishSingleFile=true `
    -p:Version=$Version `
    -p:IncludeNativeLibrariesForSelfExtract=true

Write-Host "Output: $OutputDir"
