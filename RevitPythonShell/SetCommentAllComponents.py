# -*- coding: utf-8 -*-
"""RevitPythonShell script
Write a text package into the Comments parameter of every model component
(FamilyInstance + non-family host elements) in the active document.

Usage:
  Open RevitPythonShell (Add-Ins > RevitPythonShell > Open).
  Paste or load this file and press Run.

  Optional: change COMMENT_TEXT below before running.
"""

import clr
clr.AddReference("RevitAPI")
clr.AddReference("RevitAPIUI")

from Autodesk.Revit.DB import (
    FilteredElementCollector,
    FamilyInstance,
    Element,
    Transaction,
    BuiltInParameter,
    ElementIsElementTypeFilter,
)

# ── configuration ────────────────────────────────────────────────────────────
COMMENT_TEXT = "Shree Radheladdumithumithuji"
# ─────────────────────────────────────────────────────────────────────────────

doc = __revit__.ActiveUIDocument.Document  # noqa: F821  (injected by RPS)


def collect_all_components(document):
    """Return every model *instance* (non-type) element in the document."""
    collector = (
        FilteredElementCollector(document)
        .WhereElementIsNotElementType()
        .WhereElementIsViewIndependent()
    )
    return list(collector)


def set_comments(document, elements, text):
    updated, skipped = [], []

    t = Transaction(document, "Set Comment (all components): {}".format(text))
    t.Start()
    try:
        for el in elements:
            param = el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
            if param is not None and not param.IsReadOnly:
                param.Set(text)
                updated.append(el.Id.IntegerValue)
            else:
                skipped.append(el.Id.IntegerValue)
        t.Commit()
    except Exception as exc:
        t.RollBack()
        raise exc

    return updated, skipped


# ── main ─────────────────────────────────────────────────────────────────────
components = collect_all_components(doc)
print("Total model instances found : {}".format(len(components)))

updated_ids, skipped_ids = set_comments(doc, components, COMMENT_TEXT)

print(
    "\nComment text  : '{}'"
    "\nUpdated       : {} element(s)"
    "\nSkipped       : {} element(s)  (no Comments param or read-only)"
    .format(COMMENT_TEXT, len(updated_ids), len(skipped_ids))
)

if updated_ids:
    print("\nFirst 20 updated IDs : {}".format(updated_ids[:20]))
