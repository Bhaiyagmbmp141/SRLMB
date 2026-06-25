using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class ViewsMissingTemplateCheck : IQaQcCheck
    {
        public string Name => "Views Missing a View Template";
        public string Category => "Views & Sheets";
        public string Description => "Flags plan, section, elevation and 3D views that have no view template assigned.";

        private static readonly HashSet<ViewType> CheckedTypes = new HashSet<ViewType>
        {
            ViewType.FloorPlan,
            ViewType.CeilingPlan,
            ViewType.Section,
            ViewType.Elevation,
            ViewType.ThreeD,
            ViewType.EngineeringPlan,
            ViewType.AreaPlan
        };

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && CheckedTypes.Contains(v.ViewType));

            foreach (View view in views)
            {
                if (view.ViewTemplateId == ElementId.InvalidElementId)
                {
                    issues.Add(new CheckIssue(
                        view.Id,
                        ElementDescriptionHelper.Describe(doc, view.Id),
                        $"\"{view.Name}\" has no view template assigned.",
                        CheckSeverity.Info,
                        "Apply a view template to enforce consistent graphic standards."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
