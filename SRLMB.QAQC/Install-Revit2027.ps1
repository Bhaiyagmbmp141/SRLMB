<#
.SYNOPSIS
    Builds SRLMB.QAQC and installs it into Revit 2027's Addins folder.

.DESCRIPTION
    Runs `dotnet build` against SRLMB.QAQC.csproj, then copies the build
    output (SRLMB.QAQC.dll, SRLMB.QAQC.addin, and any runtime config files)
    into the Revit 2027 Addins folder so the add-in loads on next launch.

.PARAMETER Configuration
    Build configuration to use. Defaults to "Release".

.PARAMETER RevitApiDir
    Folder containing RevitAPI.dll / RevitAPIUI.dll, if Revit 2027 is not
    installed at the default "C:\Program Files\Autodesk\Revit 2027".

.PARAMETER AllUsers
    Install into %ProgramData%\Autodesk\Revit\Addins\2027 (all users) instead
    of %APPDATA%\Autodesk\Revit\Addins\2027 (current user only). Writing to
    %ProgramData% may require an elevated PowerShell session.

.EXAMPLE
    .\Install-Revit2027.ps1

.EXAMPLE
    .\Install-Revit2027.ps1 -RevitApiDir "D:\Autodesk\Revit 2027" -AllUsers
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RevitApiDir,
    [switch]$AllUsers
)

$ErrorActionPreference = "Stop"

$projectDir = $PSScriptRoot
$csproj = Join-Path $projectDir "SRLMB.QAQC.csproj"

if (-not (Test-Path $csproj)) {
    throw "Could not find SRLMB.QAQC.csproj next to this script ($projectDir)."
}

$buildArgs = @($csproj, "-c", $Configuration)
if ($RevitApiDir) {
    $buildArgs += "-p:RevitApiDir=$RevitApiDir"
}

Write-Host "Building SRLMB.QAQC ($Configuration)..." -ForegroundColor Cyan
& dotnet build @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

# The build output folder depends on the configuration and platform
# (e.g. bin\x64\Release because the project sets <Platforms>x64</Platforms>),
# so locate the built DLL under bin\ rather than assuming a fixed path.
$binRoot = Join-Path $projectDir "bin"
$dll = Get-ChildItem -Path $binRoot -Recurse -Filter "SRLMB.QAQC.dll" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like "*\$Configuration\*" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $dll) {
    throw "Could not find SRLMB.QAQC.dll under $binRoot for configuration '$Configuration'. Did the build succeed?"
}

$outputDir = $dll.DirectoryName
$addin = Join-Path $outputDir "SRLMB.QAQC.addin"

if (-not (Test-Path $addin)) {
    throw "Found SRLMB.QAQC.dll in $outputDir but SRLMB.QAQC.addin is missing next to it."
}

if ($AllUsers) {
    $addinsRoot = Join-Path $env:ProgramData "Autodesk\Revit\Addins\2027"
}
else {
    $addinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2027"
}

New-Item -ItemType Directory -Force -Path $addinsRoot | Out-Null

Write-Host "Installing to $addinsRoot..." -ForegroundColor Cyan
Copy-Item -Path (Join-Path $outputDir '*') -Destination $addinsRoot -Force

Write-Host "Done. Restart Revit 2027 (choose 'Always Load' if prompted) to see the 'SRLMB QA/QC' panel on the Add-Ins tab." -ForegroundColor Green
