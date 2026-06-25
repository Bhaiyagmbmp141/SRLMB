using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Checks
{
    public sealed class RoomNamingCheck : IQaQcCheck
    {
        public string Name => "Room Name / Number";
        public string Category => "Rooms";
        public string Description => "Flags placed rooms that are missing a Name or Number.";

        public CheckResult Run(Document doc)
        {
            var stopwatch = Stopwatch.StartNew();
            var issues = new List<CheckIssue>();

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Location != null);

            foreach (Room room in rooms)
            {
                string? name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                string? number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    issues.Add(new CheckIssue(
                        room.Id,
                        ElementDescriptionHelper.Describe(doc, room.Id),
                        "Room has no Name.",
                        CheckSeverity.Warning,
                        "Assign a descriptive room name."));
                }

                if (string.IsNullOrWhiteSpace(number))
                {
                    issues.Add(new CheckIssue(
                        room.Id,
                        ElementDescriptionHelper.Describe(doc, room.Id),
                        "Room has no Number.",
                        CheckSeverity.Warning,
                        "Assign a room number."));
                }
            }

            return new CheckResult(Name, Category, issues, stopwatch.Elapsed);
        }
    }
}
