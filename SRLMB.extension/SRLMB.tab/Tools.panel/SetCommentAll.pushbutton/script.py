# -*- coding: utf-8 -*-
"""Write a text package into the Comments parameter of ALL model components
in the active Revit document (no pre-selection required)."""

from pyrevit import revit, DB, forms

COMMENT_TEXT = "Shree Radheladdumithumithuji"

doc = revit.doc

# ── collect every model instance (view-independent, non-type) ─────────────
collector = (
    DB.FilteredElementCollector(doc)
    .WhereElementIsNotElementType()
    .WhereElementIsViewIndependent()
)
components = list(collector)

if not components:
    forms.alert("No model components found in the document.", exitscript=True)

# ── confirm before touching the whole model ──────────────────────────────
confirmed = forms.alert(
    "This will write '{}' into the Comments parameter of all "
    "{} model instances.\n\nContinue?".format(COMMENT_TEXT, len(components)),
    yes=True,
    no=True,
)
if not confirmed:
    forms.alert("Cancelled.", exitscript=True)

# ── apply ─────────────────────────────────────────────────────────────────
updated = 0
skipped = 0

with revit.Transaction("Set Comment (all components): {}".format(COMMENT_TEXT)):
    for el in components:
        param = el.get_Parameter(DB.BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
        if param is not None and not param.IsReadOnly:
            param.Set(COMMENT_TEXT)
            updated += 1
        else:
            skipped += 1

forms.alert(
    "Done.\n\n"
    "Comment text : '{}'\n"
    "Updated      : {} element(s)\n"
    "Skipped      : {} element(s)  (no Comments param or read-only)".format(
        COMMENT_TEXT, updated, skipped
    )
)
