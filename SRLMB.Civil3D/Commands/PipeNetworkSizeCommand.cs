using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using SRLMB.Civil3D.Helpers;

namespace SRLMB.Civil3D.Commands
{
    public class PipeNetworkSizeCommand
    {
        private const double MinSlopePct  = 0.3;   // self-cleaning minimum
        private const double MaxSlopePct  = 20.0;  // erosion/scour upper limit
        private const double WarnLoadPct  = 80.0;  // warn when design flow > 80 % of capacity

        /// <summary>
        /// Checks every pipe in every network against Manning's full-flow capacity.
        /// Optionally accepts a single design-flow value (L/s) to flag under-sized pipes
        /// and recommend the next standard diameter.
        /// Enter 0 to run in capacity-display-only mode.
        /// </summary>
        [CommandMethod("SRLMB", "SRLMB_PIPESIZE", CommandFlags.Modal)]
        public void CheckPipeSizing()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor   ed  = doc.Editor;
            Database db  = doc.Database;

            CivilDocument civilDoc;
            try
            {
                civilDoc = CivilApplication.ActiveDocument;
            }
            catch
            {
                ed.WriteMessage("\nError: active document is not a Civil 3D drawing.");
                return;
            }

            // ── User input: design flow ───────────────────────────────────────
            var opt = new PromptDoubleOptions(
                "\nEnter design flow in L/s applied to all pipes (0 = capacity-display only): ")
            {
                AllowNegative = false,
                AllowZero     = true,
                DefaultValue  = 0,
            };
            PromptDoubleResult res = ed.GetDouble(opt);
            if (res.Status != PromptStatus.OK) return;

            double designLs = res.Value;
            bool   checkFlow = designLs > 0;

            // ── Walk networks ─────────────────────────────────────────────────
            ObjectIdCollection networkIds = civilDoc.GetPipeNetworkIds();
            if (networkIds.Count == 0)
            {
                ed.WriteMessage("\nNo pipe networks found in this drawing.");
                return;
            }

            int warnings = 0;
            int failures = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId netId in networkIds)
                {
                    var network = tr.GetObject(netId, OpenMode.ForRead) as Network;
                    if (network == null) continue;

                    ed.WriteMessage($"\n\n══ Network: {network.Name} ══");

                    // Column header
                    ed.WriteMessage(
                        $"\n{"Pipe",-28} {"Dia mm",7} {"Slope%",8} {"QFull L/s",10}" +
                        (checkFlow ? $" {"QDes L/s",10}" : "") +
                        $"  {"Status",-22}");
                    ed.WriteMessage("\n" + new string('─', checkFlow ? 95 : 82));

                    foreach (ObjectId pipeId in network.GetPipeIds())
                    {
                        var pipe = tr.GetObject(pipeId, OpenMode.ForRead) as Pipe;
                        if (pipe == null) continue;

                        double n      = Manning.NForMaterial(pipe.PipeMaterial);
                        double slope  = pipe.Slope;                   // m/m
                        double dia    = pipe.InnerDiameterOrWidth;    // metres
                        double qFull  = Manning.FullFlowCapacity(dia, slope, n) * 1000.0; // L/s

                        string status = EvaluateStatus(slope, designLs, qFull, checkFlow,
                                                        out bool isWarn, out bool isFail);
                        if (isWarn) warnings++;
                        if (isFail) failures++;

                        ed.WriteMessage(
                            $"\n{pipe.Name,-28} {dia * 1000,7:F0} {slope * 100,8:F3} {qFull,10:F2}" +
                            (checkFlow ? $" {designLs,10:F2}" : "") +
                            $"  {status,-22}");

                        // Recommend next standard size for failed pipes
                        if (isFail && checkFlow)
                        {
                            double sugDia = Manning.RecommendDiameter(designLs / 1000.0, slope, n);
                            ed.WriteMessage($"   ↳ Recommend: {sugDia * 1000:F0} mm " +
                                            $"(QFull = {Manning.FullFlowCapacity(sugDia, slope, n) * 1000:F2} L/s)");
                        }
                    }
                }

                tr.Commit();
            }

            ed.WriteMessage(
                $"\n\nSizing check complete.  " +
                $"Warnings: {warnings}   Failures: {failures}\n");
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static string EvaluateStatus(
            double slope, double designLs, double qFullLs, bool checkFlow,
            out bool isWarn, out bool isFail)
        {
            isWarn = false;
            isFail = false;

            if (slope * 100.0 < MinSlopePct)
            {
                isWarn = true;
                return $"WARN: slope < {MinSlopePct}%";
            }
            if (slope * 100.0 > MaxSlopePct)
            {
                isWarn = true;
                return $"WARN: slope > {MaxSlopePct}%";
            }
            if (checkFlow)
            {
                if (designLs > qFullLs)
                {
                    isFail = true;
                    return "FAIL: UNDERSIZED";
                }
                if (designLs > qFullLs * (WarnLoadPct / 100.0))
                {
                    isWarn = true;
                    return $"WARN: >{WarnLoadPct:F0}% capacity";
                }
            }
            return "OK";
        }
    }
}
