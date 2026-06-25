using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    /// <summary>Groups the model's active Revit warnings by description so repeated
    /// occurrences of the same issue surface as a single, countable row.</summary>
    public sealed class ModelWarningsCheck : IQaQcCheck
    {
        public string Name => "Model Warnings";
        public string Category => "Model Health";
        public string Description => "Lists active Revit warnings, grouped by description.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            IList<FailureMessage> warnings = doc.GetWarnings();

            var grouped = warnings
                .GroupBy(w => w.GetDescriptionText())
                .OrderByDescending(g => g.Count());

            foreach (var group in grouped)
            {
                FailureMessage sample = group.First();

                CheckSeverity severity = sample.GetSeverity() == FailureSeverity.Error
                    ? CheckSeverity.Error
                    : CheckSeverity.Warning;

                ICollection<ElementId> failingIds = sample.GetFailingElements();
                ElementId firstId = failingIds.FirstOrDefault() ?? ElementId.InvalidElementId;

                issues.Add(new CheckIssue(
                    elementId: firstId,
                    elementDescription: $"{group.Count()} occurrence(s)",
                    message: group.Key,
                    severity: severity,
                    recommendation: "Open Manage > Review Warnings to resolve."));
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
