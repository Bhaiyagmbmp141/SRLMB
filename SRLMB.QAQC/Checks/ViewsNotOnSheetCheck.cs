using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class ViewsNotOnSheetCheck : IQaQcCheck
    {
        public string Name => "Views Not Placed on a Sheet";
        public string Category => "Views & Sheets";
        public string Description => "Finds non-schedule, non-template views that are not placed on any sheet.";

        private static readonly HashSet<ViewType> ExcludedTypes = new HashSet<ViewType>
        {
            ViewType.Schedule,
            ViewType.DrawingSheet,
            ViewType.Legend,
            ViewType.ProjectBrowser,
            ViewType.SystemBrowser,
            ViewType.Internal,
            ViewType.Undefined
        };

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var viewIdsOnSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>()
                .Select(vp => vp.ViewId)
                .ToHashSet();

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && !ExcludedTypes.Contains(v.ViewType));

            foreach (View view in views)
            {
                if (!viewIdsOnSheets.Contains(view.Id))
                {
                    issues.Add(new CheckIssue(
                        view.Id,
                        ElementDescriptionHelper.Describe(doc, view.Id),
                        $"\"{view.Name}\" ({view.ViewType}) is not placed on any sheet.",
                        CheckSeverity.Info,
                        "Place the view on a sheet, or delete it if it is no longer needed."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
