using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class DimensionOverrideCheck : IQaQcCheck
    {
        public string Name => "Dimension Value Overrides";
        public string Category => "Annotations";
        public string Description => "Flags dimensions whose displayed value has been manually overridden, which can hide the true measured distance.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var dimensions = new FilteredElementCollector(doc)
                .OfClass(typeof(Dimension))
                .Cast<Dimension>();

            foreach (Dimension dim in dimensions)
            {
                DimensionSegmentArray segments = dim.Segments;

                if (segments != null && segments.Size > 0)
                {
                    foreach (DimensionSegment segment in segments)
                    {
                        if (!string.IsNullOrEmpty(segment.ValueOverride))
                        {
                            issues.Add(new CheckIssue(
                                dim.Id,
                                ElementDescriptionHelper.Describe(doc, dim.Id),
                                $"Dimension segment value overridden with \"{segment.ValueOverride}\".",
                                CheckSeverity.Warning,
                                "Confirm the override still matches the true distance, or clear it."));
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(dim.ValueOverride))
                {
                    issues.Add(new CheckIssue(
                        dim.Id,
                        ElementDescriptionHelper.Describe(doc, dim.Id),
                        $"Dimension value overridden with \"{dim.ValueOverride}\".",
                        CheckSeverity.Warning,
                        "Confirm the override still matches the true distance, or clear it."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
