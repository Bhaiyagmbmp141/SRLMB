using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using SRLMB.Civil3D.Helpers;
using SRLMB.Civil3D.Models;

namespace SRLMB.Civil3D.Commands
{
    public class PipeNetworkReportCommand
    {
        /// <summary>
        /// Walks every pipe network in the active Civil 3D drawing, collects pipe and
        /// structure data (including Manning's full-flow capacity), and writes a CSV
        /// report next to the current drawing file.
        /// </summary>
        [CommandMethod("SRLMB", "SRLMB_PIPEREPORT", CommandFlags.Modal)]
        public void ExportPipeNetworkReport()
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

            ObjectIdCollection networkIds = civilDoc.GetPipeNetworkIds();
            if (networkIds.Count == 0)
            {
                ed.WriteMessage("\nNo pipe networks found in this drawing.");
                return;
            }

            var pipes      = new List<PipeData>();
            var structures = new List<StructureData>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId netId in networkIds)
                {
                    var network = tr.GetObject(netId, OpenMode.ForRead) as Network;
                    if (network == null) continue;

                    string netName = network.Name;

                    // ── Pipes ─────────────────────────────────────────────────
                    foreach (ObjectId pipeId in network.GetPipeIds())
                    {
                        var pipe = tr.GetObject(pipeId, OpenMode.ForRead) as Pipe;
                        if (pipe == null) continue;

                        double n      = Manning.NForMaterial(pipe.PipeMaterial);
                        double slope  = pipe.Slope;                      // m/m
                        double dia    = pipe.InnerDiameterOrWidth;       // metres
                        double qFull  = Manning.FullFlowCapacity(dia, slope, n) * 1000.0; // L/s

                        string status;
                        if      (slope < 0.003) status = "WARN:LOW_SLOPE";
                        else if (slope > 0.200) status = "WARN:HIGH_SLOPE";
                        else                    status = "OK";

                        pipes.Add(new PipeData
                        {
                            Network         = netName,
                            Name            = pipe.Name      ?? "",
                            Description     = pipe.Description ?? "",
                            Length2D        = R(pipe.Length2D),
                            Length3D        = R(pipe.Length3D),
                            InnerDiameterMm = R(dia * 1000.0, 1),
                            SlopePct        = R(slope * 100.0, 4),
                            StartInvert     = R(pipe.StartPoint.Z),
                            EndInvert       = R(pipe.EndPoint.Z),
                            Material        = pipe.PipeMaterial ?? "",
                            StartStructure  = StructName(tr, pipe.StartStructureId),
                            EndStructure    = StructName(tr, pipe.EndStructureId),
                            FullFlowCapLs   = R(qFull, 2),
                            Status          = status,
                        });
                    }

                    // ── Structures ────────────────────────────────────────────
                    foreach (ObjectId strId in network.GetStructureIds())
                    {
                        var str = tr.GetObject(strId, OpenMode.ForRead) as Structure;
                        if (str == null) continue;

                        structures.Add(new StructureData
                        {
                            Network         = netName,
                            Name            = str.Name        ?? "",
                            Description     = str.Description ?? "",
                            RimElevation    = R(str.RimElevation),
                            SumpElevation   = R(str.SumpElevation),
                            InnerDiameterMm = R(str.InnerDiameterOrWidth * 1000.0, 1),
                            ConnectedPipes  = str.GetConnectedPipes(
                                                 ConnectorPositionType.Unspecified).Count,
                        });
                    }
                }

                tr.Commit();
            }

            string csvPath = BuildOutputPath(doc, "_PipeNetworkReport.csv");
            WriteCsv(csvPath, pipes, structures);

            ed.WriteMessage($"\nReport saved: {csvPath}");
            ed.WriteMessage($"\nPipes: {pipes.Count}   Structures: {structures.Count}");

            try { System.Diagnostics.Process.Start(csvPath); } catch { }
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static string StructName(Transaction tr, ObjectId id)
        {
            if (id.IsNull) return "—";
            var s = tr.GetObject(id, OpenMode.ForRead) as Structure;
            return s?.Name ?? "—";
        }

        private static double R(double v, int decimals = 3) => Math.Round(v, decimals);

        private static string BuildOutputPath(Document doc, string suffix)
        {
            string dir  = string.IsNullOrEmpty(doc.Name)
                              ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                              : Path.GetDirectoryName(doc.Name)!;
            string stem = string.IsNullOrEmpty(doc.Name)
                              ? "Drawing"
                              : Path.GetFileNameWithoutExtension(doc.Name);
            return Path.Combine(dir, stem + suffix);
        }

        private static void WriteCsv(string path, List<PipeData> pipes, List<StructureData> structures)
        {
            using var sw = new StreamWriter(path);

            sw.WriteLine("PIPES");
            sw.WriteLine(
                "Network,Name,Description," +
                "Length 2D (m),Length 3D (m)," +
                "Inner Dia (mm),Slope (%)," +
                "Start Invert (m),End Invert (m)," +
                "Material,Start Structure,End Structure," +
                "Full-Flow Cap. (L/s),Status");

            foreach (var p in pipes)
                sw.WriteLine(
                    $"{Q(p.Network)},{Q(p.Name)},{Q(p.Description)}," +
                    $"{p.Length2D},{p.Length3D}," +
                    $"{p.InnerDiameterMm},{p.SlopePct}," +
                    $"{p.StartInvert},{p.EndInvert}," +
                    $"{Q(p.Material)},{Q(p.StartStructure)},{Q(p.EndStructure)}," +
                    $"{p.FullFlowCapLs},{Q(p.Status)}");

            sw.WriteLine();
            sw.WriteLine("STRUCTURES");
            sw.WriteLine(
                "Network,Name,Description," +
                "Rim Elev (m),Sump Elev (m)," +
                "Inner Dia (mm),Connected Pipes");

            foreach (var s in structures)
                sw.WriteLine(
                    $"{Q(s.Network)},{Q(s.Name)},{Q(s.Description)}," +
                    $"{s.RimElevation},{s.SumpElevation}," +
                    $"{s.InnerDiameterMm},{s.ConnectedPipes}");
        }

        // CSV-escape: wrap in quotes and double any internal quotes
        private static string Q(string v) => $"\"{(v ?? "").Replace("\"", "\"\"")}\"";
    }
}
