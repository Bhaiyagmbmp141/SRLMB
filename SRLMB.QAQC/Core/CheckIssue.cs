using Autodesk.Revit.DB;

namespace SRLMB.QAQC.Core
{
    public sealed class CheckIssue
    {
        public ElementId ElementId { get; }
        public string ElementDescription { get; }
        public string Message { get; }
        public CheckSeverity Severity { get; }
        public string Recommendation { get; }

        public CheckIssue(
            ElementId elementId,
            string elementDescription,
            string message,
            CheckSeverity severity,
            string recommendation = "")
        {
            ElementId = elementId;
            ElementDescription = elementDescription;
            Message = message;
            Severity = severity;
            Recommendation = recommendation;
        }
    }
}
