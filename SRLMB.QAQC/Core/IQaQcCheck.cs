using Autodesk.Revit.DB;

namespace SRLMB.QAQC.Core
{
    /// <summary>A single, self-contained model health/standards check.</summary>
    public interface IQaQcCheck
    {
        string Name { get; }
        string Category { get; }
        string Description { get; }

        CheckResult Run(Document doc);
    }
}
