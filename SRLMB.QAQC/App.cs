using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace SRLMB.QAQC
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                RibbonPanel panel = app.CreateRibbonPanel("SRLMB QA/QC");

                var btnData = new PushButtonData(
                    name:         "RunQaQcBtn",
                    text:         "Model\nQA/QC",
                    assemblyName: assemblyPath,
                    className:    "SRLMB.QAQC.Commands.RunQaQcCommand")
                {
                    ToolTip = "Run automated quality-assurance checks against the active model.",
                    LongDescription =
                        "Scans the active project for common modelling issues: unresolved warnings, " +
                        "unplaced/unenclosed rooms, missing room data, views not on sheets, missing view " +
                        "templates, sheet naming, in-place families, imported CAD, unloaded Revit links, " +
                        "default workset usage and dimension overrides.\n\n" +
                        "Results can be reviewed, filtered, selected in the model, and exported to an HTML report."
                };

                panel.AddItem(btnData);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("SRLMB QA/QC – Startup Error", ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
