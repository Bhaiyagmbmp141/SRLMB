# -*- coding: utf-8 -*-
# Revit Python Shell script – Create SRLMB Levels
# Run via:  Add-In  >  Revit Python Shell  >  Open / Run
# or paste into the RPS interactive console.
#
# 'doc' is injected automatically by RPS – do not redefine it.

import clr
clr.AddReference('RevitAPI')

from Autodesk.Revit.DB import Level, Transaction

# ── EDIT THESE TWO LINES ──────────────────────────────────────────────────────
level_names   = ["SRLMB - GF", "SRLMB - L1", "SRLMB - L2", "SRLMB - L3", "SRLMB - Roof"]
elevations_mm = [0, 3000, 6000, 9000, 12000]
# ─────────────────────────────────────────────────────────────────────────────

MM_TO_FT = 1.0 / 304.8          # Revit internal unit is decimal feet

if len(level_names) != len(elevations_mm):
    print("ERROR: level_names and elevations_mm must have the same length.")
else:
    created = []
    skipped = []

    t = Transaction(doc, "Create SRLMB Levels")
    t.Start()

    for name, elev_mm in zip(level_names, elevations_mm):
        try:
            level      = Level.Create(doc, elev_mm * MM_TO_FT)
            level.Name = name
            created.append("  {} @ {} mm".format(name, elev_mm))
        except Exception as ex:
            skipped.append("  {} – {}".format(name, ex))

    t.Commit()

    print("Created ({}) Level(s):".format(len(created)))
    print("\n".join(created))
    if skipped:
        print("\nSkipped ({}):".format(len(skipped)))
        print("\n".join(skipped))
