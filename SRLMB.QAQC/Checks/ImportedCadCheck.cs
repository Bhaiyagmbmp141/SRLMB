using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class ImportedCadCheck : IQaQcCheck
    {
        public string Name => "Imported CAD Geometry";
        public string Category => "Cleanup";
        public string Description => "Finds imported (not linked) CAD geometry, which bloats the model and should generally be linked instead.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var imports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>();

            foreach (ImportInstance import in imports)
            {
                if (!import.IsLinked)
                {
                    issues.Add(new CheckIssue(
                        import.Id,
                        ElementDescriptionHelper.Describe(doc, import.Id),
                        "CAD geometry was imported (not linked) into the model.",
                        CheckSeverity.Warning,
                        "Re-link the file instead of importing, or explode/delete it after extracting the needed geometry."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
