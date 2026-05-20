using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace SRLMB
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                RibbonPanel panel = app.CreateRibbonPanel("SRLMB");

                var btnData = new PushButtonData(
                    name:        "SetCommentBtn",
                    text:        "Set\nComment",
                    assemblyName: assemblyPath,
                    className:   "SRLMB.Commands.SetCommentCommand")
                {
                    ToolTip = "Write 'Shree Radheladdumithumithuji' into the Comments parameter of selected elements.",
                    LongDescription =
                        "Select one or more elements in the Revit model, then click this button.\n" +
                        "The Comments parameter of every selected element will be set to:\n" +
                        "\"Shree Radheladdumithumithuji\""
                };

                panel.AddItem(btnData);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("SRLMB – Startup Error", ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
