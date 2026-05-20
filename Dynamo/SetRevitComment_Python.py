# -*- coding: utf-8 -*-
# Dynamo Python Script Node
# Paste this code into a Python Script node in Dynamo.
#
# Inputs:
#   IN[0]  - comment_text  : string to write (e.g. "Shree Radheladdumithumithuji")
#   IN[1]  - run           : Boolean toggle – set to True to execute
#
# Output:
#   OUT    - result summary string

import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
clr.AddReference('RevitServices')

from Autodesk.Revit.DB import BuiltInParameter
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc   = DocumentManager.Instance.CurrentDBDocument
uidoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument

comment_text = IN[0]
run_flag     = IN[1]

if not run_flag:
    OUT = "Toggle the Run input to True to execute."
else:
    selected_ids = uidoc.Selection.GetElementIds()
    elements     = [doc.GetElement(eid) for eid in selected_ids]

    if not elements:
        OUT = "No elements selected – select elements in Revit, then re-run."
    else:
        updated = []
        skipped = []

        TransactionManager.Instance.EnsureInTransaction(doc)

        for el in elements:
            param = el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
            if param is not None and not param.IsReadOnly:
                param.Set(comment_text)
                updated.append(el.Id.IntegerValue)
            else:
                skipped.append(el.Id.IntegerValue)

        TransactionManager.Instance.TransactionTaskDone()

        OUT = (
            "Comment written : '{}'\n"
            "Updated ({}) IDs : {}\n"
            "Skipped ({}) IDs : {}"
        ).format(
            comment_text,
            len(updated), updated,
            len(skipped), skipped
        )
