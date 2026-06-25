using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class UnresolvedRevitLinkCheck : IQaQcCheck
    {
        public string Name => "Revit Links";
        public string Category => "Cleanup";
        public string Description => "Flags linked Revit models that are unloaded or cannot be found.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var linkTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>();

            foreach (RevitLinkType linkType in linkTypes)
            {
                LinkedFileStatus status = linkType.GetLinkedFileStatus();
                string description = ElementDescriptionHelper.Describe(doc, linkType.Id);

                switch (status)
                {
                    case LinkedFileStatus.NotFound:
                        issues.Add(new CheckIssue(
                            linkType.Id, description,
                            "Linked file could not be found.",
                            CheckSeverity.Error,
                            "Reload the link from its correct location."));
                        break;

                    case LinkedFileStatus.Unloaded:
                    case LinkedFileStatus.LocallyUnloaded:
                        issues.Add(new CheckIssue(
                            linkType.Id, description,
                            "Linked file is unloaded.",
                            CheckSeverity.Warning,
                            "Reload the link if it is still required for coordination."));
                        break;

                    case LinkedFileStatus.Invalid:
                        issues.Add(new CheckIssue(
                            linkType.Id, description,
                            "Linked file reference is invalid.",
                            CheckSeverity.Error,
                            "Remove or repair the link."));
                        break;
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
