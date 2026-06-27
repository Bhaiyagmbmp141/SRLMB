# SRLMB Model QA/QC (Revit 2027)

A Revit add-in that runs a set of automated model-health and standards checks
against the active project and presents the results in a reviewable,
exportable report.

## Why a separate project

Revit 2027's RevitAPI/RevitAPIUI assemblies are built against
`System.Runtime, Version=10.0.0.0`, so add-ins need the .NET 10 runtime.
This project targets `net10.0-windows` for Revit 2027, while `SRLMB.AddIn`
(the original Comments-parameter tool) stays on `net48` for Revit 2024.
Each Revit major version needs an add-in built against the matching
API/runtime, so the two live as separate projects rather than one
multi-targeted assembly.

## Checks included

| Category        | Check                              | What it flags |
|------------------|------------------------------------|----------------|
| Model Health     | Model Warnings                     | Active Revit warnings, grouped by description |
| Rooms            | Unplaced Rooms                     | Rooms with no location / zero area |
| Rooms            | Unenclosed Rooms                   | Placed rooms with no boundary loops |
| Rooms            | Room Name / Number                 | Rooms missing a Name or Number |
| Views & Sheets   | Views Not Placed on a Sheet        | Views (excluding schedules/legends/templates) not on any sheet |
| Views & Sheets   | Views Missing a View Template      | Plan/section/elevation/3D views with no template |
| Views & Sheets   | Sheet Number / Name                | Sheets with blank or whitespace-padded number/name |
| Cleanup          | Imported CAD Geometry              | CAD geometry imported (not linked) into the model |
| Cleanup          | In-Place Families                  | Model-in-place families |
| Cleanup          | Revit Links                        | Unloaded / not-found / invalid linked models |
| Worksharing      | Elements on the Default Workset    | Elements still on the auto-created "Workset1" |
| Annotations      | Dimension Value Overrides          | Dimensions with a manually overridden value |

Each check is an independent `IQaQcCheck` implementation under `Checks/`.
Add a new check by implementing the interface and registering it in
`Core/CheckRunner.GetAllChecks()`.

## Using it

1. Open a project in Revit 2027.
2. Click **Model QA/QC** on the **SRLMB QA/QC** panel (Add-Ins tab).
3. Pick which checks to run on the left, click **Run Selected Checks**.
4. Double-click a result row to select and zoom to that element.
5. Use **Export Report...** to save a standalone HTML report.

## Building

Requires the **.NET 10 SDK** (not .NET 8) — Revit 2027's RevitAPI.dll
depends on `System.Runtime, Version=10.0.0.0`, so building against an
older SDK fails with `CS1705` assembly-version-mismatch errors.

```
dotnet build SRLMB.QAQC.csproj -c Release
```

By default the project looks for the Revit 2027 API assemblies in
`C:\Program Files\Autodesk\Revit 2027`. Override this with the
`RevitApiDir` MSBuild property or environment variable if Revit is
installed elsewhere:

```
dotnet build SRLMB.QAQC.csproj -c Release -p:RevitApiDir="D:\Autodesk\Revit 2027"
```

## Installing

### Option A: one-step script

From a PowerShell prompt in this folder:

```powershell
.\Install-Revit2027.ps1
```

This builds Release and copies the output into
`%APPDATA%\Autodesk\Revit\Addins\2027\` (current user). Pass `-AllUsers` to
install into `%ProgramData%\...` instead (may need an elevated prompt), or
`-RevitApiDir "<path>"` if Revit 2027 isn't in the default location.

### Option B: manual copy

Copy `bin\Release\net10.0-windows\SRLMB.QAQC.dll` and
`Resources\SRLMB.QAQC.addin` into:

```
%ProgramData%\Autodesk\Revit\Addins\2027\
```

(or point the `.addin` file's location at wherever the DLL lives via its
own folder, then place just the `.addin` manifest in the Addins\2027
folder).

Either way, restart Revit 2027 (choosing "Always Load" if prompted for an
unsigned add-in) to load the panel.
