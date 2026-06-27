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

# Revit locks the add-in DLL while it is running, so the copy step would fail
# with "the process cannot access the file ... because it is being used by
# another process". Stop early with a clear message instead.
if (Get-Process -Name "Revit" -ErrorAction SilentlyContinue) {
    throw "Revit is currently running and has the add-in DLL locked. Close Revit 2027 completely, then re-run this script."
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

# The build output folder depends on configuration/platform (bin\Release,
# bin\x64\Release, etc.), so locate the built files under bin\ rather than
# assuming a fixed path.
$binRoot = Join-Path $projectDir "bin"
$dll = Get-ChildItem -Path $binRoot -Recurse -Filter "SRLMB.QAQC.dll" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like "*\$Configuration\*" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $dll) {
    throw "Could not find SRLMB.QAQC.dll under $binRoot for configuration '$Configuration'. Did the build succeed?"
}

$outputDir = $dll.DirectoryName

# The .addin manifest should sit next to the DLL, but fall back to searching
# the build tree (it may have been copied into a Resources\ subfolder).
$addin = Get-ChildItem -Path $outputDir -Filter "SRLMB.QAQC.addin" -ErrorAction SilentlyContinue |
    Select-Object -First 1
if (-not $addin) {
    $addin = Get-ChildItem -Path $binRoot -Recurse -Filter "SRLMB.QAQC.addin" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -like "*\$Configuration\*" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}
if (-not $addin) {
    throw "Could not find SRLMB.QAQC.addin under $binRoot. Rebuild and try again."
}

if ($AllUsers) {
    $addinsRoot = Join-Path $env:ProgramData "Autodesk\Revit\Addins\2027"
}
else {
    $addinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2027"
}

New-Item -ItemType Directory -Force -Path $addinsRoot | Out-Null

Write-Host "Installing to $addinsRoot..." -ForegroundColor Cyan

# Copy the assembly plus its runtime sidecar files (.deps.json,
# .runtimeconfig.json, any dependency DLLs/PDBs) and the manifest into the
# top level of the Addins folder, which is the only level Revit scans.
Get-ChildItem -Path $outputDir -File |
    Where-Object { $_.Extension -in '.dll', '.pdb', '.json' } |
    ForEach-Object { Copy-Item -Path $_.FullName -Destination $addinsRoot -Force }
Copy-Item -Path $addin.FullName -Destination (Join-Path $addinsRoot "SRLMB.QAQC.addin") -Force

Write-Host "Done. Restart Revit 2027 (choose 'Always Load' if prompted) to see the 'SRLMB QA/QC' panel on the Add-Ins tab." -ForegroundColor Green
