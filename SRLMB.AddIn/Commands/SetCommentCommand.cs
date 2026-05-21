using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SRLMB.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetCommentCommand : IExternalCommand
    {
        private const string CommentText = "Shree Radheladdumithumithuji";

        public Result Execute(
            ExternalCommandData commandData,
            ref string           message,
            ElementSet           elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document   doc   = uidoc.Document;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if (selectedIds.Count == 0)
            {
                TaskDialog.Show(
                    "SRLMB – Set Comment",
                    "No elements are selected.\nSelect one or more elements in the view and run the command again.");
                return Result.Cancelled;
            }

            int updated = 0;
            int skipped = 0;
            var skippedIds = new List<int>();

            using (var tx = new Transaction(doc, $"Set Comment: {CommentText}"))
            {
                tx.Start();

                foreach (ElementId id in selectedIds)
                {
                    Element element = doc.GetElement(id);
                    if (element == null) continue;

                    Parameter param = element.get_Parameter(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                    if (param != null && !param.IsReadOnly)
                    {
                        param.Set(CommentText);
                        updated++;
                    }
                    else
                    {
                        skipped++;
                        skippedIds.Add((int)id.Value);
                    }
                }

                tx.Commit();
            }

            string summary =
                $"Comment written:\n\"{CommentText}\"\n\n" +
                $"Updated : {updated} element(s)\n" +
                $"Skipped : {skipped} element(s)" +
                (skippedIds.Any()
                    ? $"\nSkipped IDs : {string.Join(", ", skippedIds)}"
                    : string.Empty);

            TaskDialog.Show("SRLMB – Set Comment", summary);

            return Result.Succeeded;
        }
    }
}
