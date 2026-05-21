using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SRLMB.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetCommentAllCommand : IExternalCommand
    {
        private const string CommentText = "Shree Radheladdumithumithuji";

        public Result Execute(
            ExternalCommandData commandData,
            ref string           message,
            ElementSet           elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Element> components = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .ToElements()
                .ToList();

            if (components.Count == 0)
            {
                TaskDialog.Show(
                    "SRLMB – Set Comment (All)",
                    "No model components found in the document.");
                return Result.Cancelled;
            }

            TaskDialogResult confirm = TaskDialog.Show(
                "SRLMB – Set Comment (All)",
                string.Format(
                    "This will write '{0}' into the Comments parameter of all {1} model instances.\n\nContinue?",
                    CommentText, components.Count),
                TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

            if (confirm != TaskDialogResult.Yes)
                return Result.Cancelled;

            int updated = 0;
            int skipped = 0;
            var skippedIds = new List<int>();

            using (var tx = new Transaction(doc, string.Format("Set Comment (all): {0}", CommentText)))
            {
                tx.Start();

                foreach (Element el in components)
                {
                    Parameter param = el.get_Parameter(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                    if (param != null && !param.IsReadOnly)
                    {
                        param.Set(CommentText);
                        updated++;
                    }
                    else
                    {
                        skipped++;
                        skippedIds.Add((int)el.Id.Value);
                    }
                }

                tx.Commit();
            }

            string summary =
                string.Format(
                    "Comment written:\n\"{0}\"\n\nUpdated : {1} element(s)\nSkipped : {2} element(s)",
                    CommentText, updated, skipped) +
                (skippedIds.Any()
                    ? "\nSkipped IDs : " + string.Join(", ", skippedIds.Select(i => i.ToString()))
                    : string.Empty);

            TaskDialog.Show("SRLMB – Set Comment (All)", summary);

            return Result.Succeeded;
        }
    }
}
