using Autodesk.Revit.DB;

namespace SRLMB.QAQC.Core
{
    internal static class ElementDescriptionHelper
    {
        public static string Describe(Document doc, ElementId id)
        {
            if (id == null || id == ElementId.InvalidElementId)
            {
                return "(model-level)";
            }

            Element? element = doc.GetElement(id);
            if (element == null)
            {
                return $"Id {id.Value}";
            }

            string categoryName = element.Category?.Name ?? element.GetType().Name;
            string name = element.Name;

            return string.IsNullOrWhiteSpace(name)
                ? $"{categoryName} (Id {id.Value})"
                : $"{categoryName}: {name} (Id {id.Value})";
        }
    }
}
