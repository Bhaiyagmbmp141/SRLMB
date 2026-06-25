using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class UnplacedRoomsCheck : IQaQcCheck
    {
        public string Name => "Unplaced Rooms";
        public string Category => "Rooms";
        public string Description => "Finds rooms that exist but have not been placed in the model (zero area / no location).";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>();

            foreach (Room room in rooms)
            {
                if (room.Location == null || room.Area <= 0.0)
                {
                    issues.Add(new CheckIssue(
                        room.Id,
                        ElementDescriptionHelper.Describe(doc, room.Id),
                        "Room is not placed (no location / zero area).",
                        CheckSeverity.Error,
                        "Place the room or delete it if it is no longer needed."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
