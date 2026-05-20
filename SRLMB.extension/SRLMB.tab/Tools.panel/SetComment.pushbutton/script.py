# -*- coding: utf-8 -*-
"""Write 'Shree Radheladdumithumithuji' into the Comments parameter
of all selected Revit elements."""

from pyrevit import revit, DB, forms

COMMENT_TEXT = "Shree Radheladdumithumithuji"

doc = revit.doc
selection = revit.get_selection()

if not selection:
    forms.alert("Please select one or more elements first.", exitscript=True)

updated = 0
skipped = 0

with revit.Transaction("Set Comment: {}".format(COMMENT_TEXT)):
    for element in selection:
        param = element.get_Parameter(DB.BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
        if param and not param.IsReadOnly:
            param.Set(COMMENT_TEXT)
            updated += 1
        else:
            skipped += 1

forms.alert(
    "Done.\nUpdated: {}\nSkipped (no Comments param or read-only): {}".format(
        updated, skipped
    )
)
