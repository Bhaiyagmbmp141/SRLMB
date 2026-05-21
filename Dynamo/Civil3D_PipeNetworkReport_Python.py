# -*- coding: utf-8 -*-
# Dynamo for Civil 3D – Pipe Network Report
# Paste this code into a Python Script node in Dynamo for Civil 3D.
#
# Inputs:
#   IN[0] - output_folder : folder path string for the CSV (leave "" to use drawing folder)
#   IN[1] - run           : Boolean toggle – set True to execute
#
# Output:
#   OUT - file path of saved CSV + summary counts

import clr, os, math

clr.AddReference('AeccDbMgd')
clr.AddReference('AeccXUiLand')
clr.AddReference('acmgd')
clr.AddReference('acdbmgd')
clr.AddReference('accoremgd')

from Autodesk.Civil.ApplicationServices import CivilApplication
from Autodesk.Civil.DatabaseServices import Network, Pipe, Structure, ConnectorPositionType
from Autodesk.AutoCAD.ApplicationServices import Application as AcadApp
from Autodesk.AutoCAD.DatabaseServices import OpenMode

# ── Manning's n values ────────────────────────────────────────────────────────
N_VALUES = {
    'concrete': 0.013, 'reinforced concrete': 0.013, 'rcp': 0.013,
    'hdpe': 0.009, 'pvc': 0.009, 'upvc': 0.009,
    'ductile iron': 0.012, 'cast iron': 0.013, 'steel': 0.011,
    'clay': 0.013, 'vitrified clay': 0.012,
    'corrugated metal': 0.024, 'cmp': 0.024,
}

def get_n(material):
    return N_VALUES.get((material or '').lower(), 0.013)

def full_flow_ls(dia_m, slope_mm, n):
    """Manning's full-flow capacity in L/s for a circular pipe."""
    if dia_m <= 0 or slope_mm <= 0 or n <= 0:
        return 0.0
    area = math.pi * dia_m * dia_m / 4.0
    r    = dia_m / 4.0
    return (1.0 / n) * area * (r ** (2.0 / 3.0)) * math.sqrt(slope_mm) * 1000.0

def slope_status(slope):
    if slope < 0.003: return 'WARN:LOW_SLOPE'
    if slope > 0.200: return 'WARN:HIGH_SLOPE'
    return 'OK'

def csv_quote(v):
    return '"' + str(v or '').replace('"', '""') + '"'

# ── main ──────────────────────────────────────────────────────────────────────
output_folder = IN[0]
run_flag      = IN[1]

if not run_flag:
    OUT = "Set Run = True to execute."
else:
    try:
        doc       = AcadApp.DocumentManager.MdiActiveDocument
        db        = doc.Database
        civil_doc = CivilApplication.ActiveDocument

        network_ids = civil_doc.GetPipeNetworkIds()
        if network_ids.Count == 0:
            OUT = "No pipe networks found in this drawing."
        else:
            pipe_rows   = []
            struct_rows = []

            tr = db.TransactionManager.StartTransaction()
            try:
                for net_id in network_ids:
                    network  = tr.GetObject(net_id, OpenMode.ForRead)
                    net_name = network.Name

                    for pipe_id in network.GetPipeIds():
                        pipe  = tr.GetObject(pipe_id, OpenMode.ForRead)
                        n     = get_n(pipe.PipeMaterial)
                        slope = pipe.Slope
                        dia   = pipe.InnerDiameterOrWidth
                        qfull = full_flow_ls(dia, slope, n)

                        s_start = s_end = u'—'
                        if not pipe.StartStructureId.IsNull:
                            s = tr.GetObject(pipe.StartStructureId, OpenMode.ForRead)
                            s_start = s.Name
                        if not pipe.EndStructureId.IsNull:
                            s = tr.GetObject(pipe.EndStructureId, OpenMode.ForRead)
                            s_end = s.Name

                        pipe_rows.append([
                            net_name,
                            pipe.Name        or '',
                            pipe.Description or '',
                            round(pipe.Length2D, 3),
                            round(pipe.Length3D, 3),
                            round(dia * 1000.0, 1),     # mm
                            round(slope * 100.0, 4),    # %
                            round(pipe.StartPoint.Z, 3),
                            round(pipe.EndPoint.Z, 3),
                            pipe.PipeMaterial or '',
                            s_start,
                            s_end,
                            round(qfull, 2),
                            slope_status(slope),
                        ])

                    for str_id in network.GetStructureIds():
                        st = tr.GetObject(str_id, OpenMode.ForRead)
                        struct_rows.append([
                            net_name,
                            st.Name        or '',
                            st.Description or '',
                            round(st.RimElevation, 3),
                            round(st.SumpElevation, 3),
                            round(st.InnerDiameterOrWidth * 1000.0, 1),
                            st.GetConnectedPipes(ConnectorPositionType.Unspecified).Count,
                        ])

                tr.Commit()
            except Exception:
                tr.Abort()
                raise

            # ── write CSV ─────────────────────────────────────────────────────
            out_dir  = output_folder if output_folder \
                       else (os.path.dirname(doc.Name) if doc.Name else os.path.expanduser('~'))
            stem     = os.path.splitext(os.path.basename(doc.Name))[0] if doc.Name else 'Drawing'
            csv_path = os.path.join(out_dir, stem + '_PipeNetworkReport.csv')

            with open(csv_path, 'w') as f:
                f.write('PIPES\n')
                f.write('Network,Name,Description,Length 2D (m),Length 3D (m),'
                        'Inner Dia (mm),Slope (%),Start Invert (m),End Invert (m),'
                        'Material,Start Structure,End Structure,'
                        'Full-Flow Cap. (L/s),Status\n')
                for r in pipe_rows:
                    f.write(','.join([
                        csv_quote(r[0]),  csv_quote(r[1]),  csv_quote(r[2]),
                        str(r[3]),        str(r[4]),        str(r[5]),
                        str(r[6]),        str(r[7]),        str(r[8]),
                        csv_quote(r[9]),  csv_quote(r[10]), csv_quote(r[11]),
                        str(r[12]),       csv_quote(r[13]),
                    ]) + '\n')

                f.write('\nSTRUCTURES\n')
                f.write('Network,Name,Description,Rim Elev (m),'
                        'Sump Elev (m),Inner Dia (mm),Connected Pipes\n')
                for r in struct_rows:
                    f.write(','.join([
                        csv_quote(r[0]), csv_quote(r[1]), csv_quote(r[2]),
                        str(r[3]),       str(r[4]),       str(r[5]),
                        str(r[6]),
                    ]) + '\n')

            OUT = 'Report saved: {}\nPipes: {}   Structures: {}'.format(
                csv_path, len(pipe_rows), len(struct_rows))

    except Exception as ex:
        OUT = 'Error: ' + str(ex)
