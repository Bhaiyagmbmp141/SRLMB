using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class InPlaceFamilyCheck : IQaQcCheck
    {
        public string Name => "In-Place Families";
        public string Category => "Cleanup";
        public string Description => "Finds in-place (model-in-place) families, which increase file size and are harder to manage than loadable families.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var families = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.IsInPlace);

            foreach (Family family in families)
            {
                issues.Add(new CheckIssue(
                    family.Id,
                    ElementDescriptionHelper.Describe(doc, family.Id),
                    $"\"{family.Name}\" is an in-place family.",
                    CheckSeverity.Warning,
                    "Convert to a loadable family if it will be reused, or rebuild with native Revit elements."));
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
