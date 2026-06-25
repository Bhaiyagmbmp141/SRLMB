using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class DefaultWorksetCheck : IQaQcCheck
    {
        public string Name => "Elements on the Default Workset";
        public string Category => "Worksharing";
        public string Description => "Flags elements still left on the auto-created \"Workset1\" instead of a discipline-specific workset.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            if (!doc.IsWorkshared)
            {
                return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
            }

            var defaultWorksetIds = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToWorksets()
                .Where(w => string.Equals(w.Name, "Workset1", StringComparison.OrdinalIgnoreCase))
                .Select(w => w.Id)
                .ToHashSet();

            if (defaultWorksetIds.Count == 0)
            {
                return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
            }

            var elements = new FilteredElementCollector(doc).WhereElementIsNotElementType();
            int count = elements.Count(e => defaultWorksetIds.Contains(e.WorksetId));

            if (count > 0)
            {
                issues.Add(new CheckIssue(
                    ElementId.InvalidElementId,
                    "Workset1",
                    $"{count} element(s) are still on the default \"Workset1\".",
                    CheckSeverity.Info,
                    "Move elements to discipline-specific worksets."));
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
