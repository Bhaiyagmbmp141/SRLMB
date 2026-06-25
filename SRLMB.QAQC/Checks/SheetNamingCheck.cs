using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class SheetNamingCheck : IQaQcCheck
    {
        public string Name => "Sheet Number / Name";
        public string Category => "Views & Sheets";
        public string Description => "Flags sheets with a blank number/name, or values with leading/trailing whitespace.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>();

            foreach (ViewSheet sheet in sheets)
            {
                string number = sheet.SheetNumber;
                string name = sheet.Name;

                if (string.IsNullOrWhiteSpace(number))
                {
                    issues.Add(new CheckIssue(
                        sheet.Id,
                        ElementDescriptionHelper.Describe(doc, sheet.Id),
                        "Sheet has no number.",
                        CheckSeverity.Error,
                        "Assign a sheet number."));
                }
                else if (number != number.Trim())
                {
                    issues.Add(new CheckIssue(
                        sheet.Id,
                        ElementDescriptionHelper.Describe(doc, sheet.Id),
                        $"Sheet number \"{number}\" has leading/trailing whitespace.",
                        CheckSeverity.Warning,
                        "Trim the sheet number."));
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    issues.Add(new CheckIssue(
                        sheet.Id,
                        ElementDescriptionHelper.Describe(doc, sheet.Id),
                        "Sheet has no name.",
                        CheckSeverity.Error,
                        "Assign a sheet name."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
