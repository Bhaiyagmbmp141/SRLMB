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
            UIDocument? uidoc = commandData.Application.ActiveUIDocument;

            if (uidoc == null)
            {
                TaskDialog.Show("SRLMB – Model QA/QC", "Open a project document first.");
                return Result.Cancelled;
            }

            var window = new QaQcResultsWindow(uidoc);
            new WindowInteropHelper(window).Owner = commandData.Application.MainWindowHandle;

            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
