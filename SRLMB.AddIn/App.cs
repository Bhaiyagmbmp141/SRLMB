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

                var btnSelection = new PushButtonData(
                    name:         "SetCommentBtn",
                    text:         "Set Comment\n(Selection)",
                    assemblyName: assemblyPath,
                    className:    "SRLMB.Commands.SetCommentCommand")
                {
                    ToolTip = "Write the text package into the Comments parameter of selected elements.",
                    LongDescription =
                        "Select one or more elements in the Revit model, then click this button.\n" +
                        "The Comments parameter of every selected element will be set to:\n" +
                        "\"Shree Radheladdumithumithuji\""
                };

                var btnAll = new PushButtonData(
                    name:         "SetCommentAllBtn",
                    text:         "Set Comment\n(All)",
                    assemblyName: assemblyPath,
                    className:    "SRLMB.Commands.SetCommentAllCommand")
                {
                    ToolTip = "Write the text package into the Comments parameter of ALL model components.",
                    LongDescription =
                        "No selection required. Applies to every view-independent model instance in the document.\n" +
                        "The Comments parameter will be set to:\n" +
                        "\"Shree Radheladdumithumithuji\""
                };

                panel.AddItem(btnSelection);
                panel.AddItem(btnAll);
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
