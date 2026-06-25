using Autodesk.Revit.DB;

namespace SRLMB.QAQC.UI
{
    public sealed class IssueRow
    {
        public string Severity { get; set; } = "";
        public string Category { get; set; } = "";
        public string Check { get; set; } = "";
        public string Element { get; set; } = "";
        public string Message { get; set; } = "";
        public string Recommendation { get; set; } = "";
        public ElementId ElementId { get; set; } = ElementId.InvalidElementId;
    }
}
