using System;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SRLMB.QAQC.UI;

namespace SRLMB.QAQC.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunQaQcCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument? uidoc = commandData.Application.ActiveUIDocument;

                if (uidoc?.Document == null)
                {
                    TaskDialog.Show("SRLMB – Model QA/QC", "Open a project document first.");
                    return Result.Cancelled;
                }

                var window = new QaQcResultsWindow(uidoc);
                new WindowInteropHelper(window).Owner = commandData.Application.MainWindowHandle;

                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Surface the real exception instead of letting Revit show the
                // generic "Contact the provider" dialog, which hides the cause.
                var dialog = new TaskDialog("SRLMB – Model QA/QC Error")
                {
                    MainInstruction = "The QA/QC command failed to run.",
                    MainContent = ex.Message,
                    ExpandedContent = ex.ToString()
                };
                dialog.Show();

                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
