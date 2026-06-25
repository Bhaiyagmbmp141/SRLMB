# SRLMB Model QA/QC (Revit 2027)

A Revit add-in that runs a set of automated model-health and standards checks
against the active project and presents the results in a reviewable,
exportable report.

## Why a separate project

Revit 2025 and later run on the .NET 8 runtime, replacing the .NET Framework
4.8 used by Revit 2024 and earlier. This project targets `net8.0-windows`
for Revit 2027, while `SRLMB.AddIn` (the original Comments-parameter tool)
stays on `net48` for Revit 2024. Each Revit major version needs an add-in
built against the matching API/runtime, so the two live as separate
projects rather than one multi-targeted assembly.

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

Copy `bin\Release\net8.0-windows\SRLMB.QAQC.dll` and
`Resources\SRLMB.QAQC.addin` into:

```
%ProgramData%\Autodesk\Revit\Addins\2027\
```

(or point the `.addin` file's location at wherever the DLL lives via its
own folder, then place just the `.addin` manifest in the Addins\2027
folder). Restart Revit 2027 to load the panel.
