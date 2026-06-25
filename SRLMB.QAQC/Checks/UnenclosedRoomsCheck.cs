using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class UnenclosedRoomsCheck : IQaQcCheck
    {
        public string Name => "Unenclosed Rooms";
        public string Category => "Rooms";
        public string Description => "Finds placed rooms whose boundary is not fully enclosed by walls or room separation lines.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Location != null && r.Area > 0.0);

            var options = new SpatialElementBoundaryOptions();

            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops = room.GetBoundarySegments(options);

                if (loops == null || loops.Count == 0)
                {
                    issues.Add(new CheckIssue(
                        room.Id,
                        ElementDescriptionHelper.Describe(doc, room.Id),
                        "Room has area but no boundary loops were returned, indicating it is likely unenclosed.",
                        CheckSeverity.Warning,
                        "Check for gaps in the bounding walls or room separation lines."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
