# -*- coding: utf-8 -*-
# Dynamo Python Script Node
# Paste this code into a Python Script node in Dynamo.
#
# Inputs:
#   IN[0]  - comment_text   : string to write (e.g. "Shree Radheladdumithumithuji")
#   IN[1]  - all_components : Boolean – True  → process ALL model instances
#                                       False → process only the current Revit selection
#   IN[2]  - run            : Boolean toggle – set to True to execute
#
# Output:
#   OUT    - result summary string

import clr
clr.AddReference("RevitAPI")
clr.AddReference("RevitAPIUI")
clr.AddReference("RevitServices")

from Autodesk.Revit.DB import (
    BuiltInParameter,
    FilteredElementCollector,
)


def _eid(element_id):
    # ElementId.IntegerValue removed in Revit 2024; .Value is the replacement.
    try:
        return element_id.Value
    except AttributeError:
        return element_id.IntegerValue
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc   = DocumentManager.Instance.CurrentDBDocument
uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument

# IN[] is injected by Dynamo; absent in RevitPythonShell, or may have
# fewer items than expected when not all ports are wired.
try:
    _in = list(IN)  # noqa: F821
except NameError:
    _in = []

comment_text   = _in[0] if len(_in) > 0 else "Shree Radheladdumithumithuji"
all_components = _in[1] if len(_in) > 1 else True
run_flag       = _in[2] if len(_in) > 2 else True

if not run_flag:
    OUT = "Set the Run input to True to execute."
else:
    # ── element collection ────────────────────────────────────────────────
    if all_components:
        elements = list(
            FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .WhereElementIsViewIndependent()
        )
        source_label = "all model components"
    else:
        selected_ids = uidoc.Selection.GetElementIds()
        elements     = [doc.GetElement(eid) for eid in selected_ids]
        source_label = "selection"

    if not elements:
        OUT = (
            "No elements found (source: {}).\n"
            "If using selection mode, select elements in Revit first."
        ).format(source_label)
    else:
        updated = []
        skipped = []

        TransactionManager.Instance.EnsureInTransaction(doc)

        for el in elements:
            param = el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
            if param is not None and not param.IsReadOnly:
                param.Set(comment_text)
                updated.append(_eid(el.Id))
            else:
                skipped.append(_eid(el.Id))

        TransactionManager.Instance.TransactionTaskDone()

        OUT = (
            "Source        : {}\n"
            "Comment text  : '{}'\n"
            "Updated ({})  IDs : {}\n"
            "Skipped ({})  IDs : {}"
        ).format(
            source_label,
            comment_text,
            len(updated), updated,
            len(skipped), skipped,
        )
