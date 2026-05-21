# -*- coding: utf-8 -*-
# Dynamo Python Script Node – Create Revit Levels
# Paste this code into a Python Script node in Dynamo.
#
# Inputs:
#   IN[0]  - level_names   : list of strings  (e.g. ["Ground", "L1", "L2"])
#   IN[1]  - elevations_mm : list of numbers  (elevation in millimetres, e.g. [0, 3000, 6000])
#   IN[2]  - run           : Boolean toggle – set True to execute
#
# Output:
#   OUT    - result summary string

import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
clr.AddReference('RevitServices')

from Autodesk.Revit.DB import Level
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

level_names   = IN[0] if isinstance(IN[0], list) else [IN[0]]
elevations_mm = IN[1] if isinstance(IN[1], list) else [IN[1]]
run_flag      = IN[2]

MM_TO_FT = 1.0 / 304.8

if not run_flag:
    OUT = "Toggle the Run input to True to execute."
elif len(level_names) != len(elevations_mm):
    OUT = "Error: Level names count ({}) must match elevations count ({}).".format(
        len(level_names), len(elevations_mm)
    )
else:
    created = []
    skipped = []

    TransactionManager.Instance.EnsureInTransaction(doc)

    for name, elev_mm in zip(level_names, elevations_mm):
        try:
            elev_ft = float(elev_mm) * MM_TO_FT
            level   = Level.Create(doc, elev_ft)
            level.Name = str(name)
            created.append("{} @ {} mm".format(name, elev_mm))
        except Exception as ex:
            skipped.append("{}: {}".format(name, str(ex)))

    TransactionManager.Instance.TransactionTaskDone()

    lines = ["Created ({}) Level(s):".format(len(created))]
    lines += ["  " + s for s in created]
    if skipped:
        lines += ["", "Skipped ({}) – already exists or name conflict:".format(len(skipped))]
        lines += ["  " + s for s in skipped]
    OUT = "\n".join(lines)
