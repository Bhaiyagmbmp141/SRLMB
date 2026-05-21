using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

[assembly: ExtensionApplication(typeof(SRLMB.Civil3D.App))]
[assembly: CommandClass(typeof(SRLMB.Civil3D.Commands.PipeNetworkReportCommand))]
[assembly: CommandClass(typeof(SRLMB.Civil3D.Commands.PipeNetworkSizeCommand))]

namespace SRLMB.Civil3D
{
    public class App : IExtensionApplication
    {
        public void Initialize()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage(
                "\nSRLMB Civil 3D loaded." +
                "\n  SRLMB_PIPEREPORT – export pipe network data to CSV" +
                "\n  SRLMB_PIPESIZE   – Manning's capacity check & sizing advice\n");
        }

        public void Terminate() { }
    }
}
