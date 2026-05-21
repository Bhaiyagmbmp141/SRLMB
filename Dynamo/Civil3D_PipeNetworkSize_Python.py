# -*- coding: utf-8 -*-
# Dynamo for Civil 3D – Pipe Network Sizing Check (Manning's equation)
# Paste this code into a Python Script node in Dynamo for Civil 3D.
#
# Inputs:
#   IN[0] - design_flow_ls : design flow in L/s applied to all pipes (0 = capacity-display only)
#   IN[1] - run            : Boolean toggle – set True to execute
#
# Output:
#   OUT - formatted sizing report string

import clr, math

clr.AddReference('AeccDbMgd')
clr.AddReference('AeccXUiLand')
clr.AddReference('acmgd')
clr.AddReference('acdbmgd')
clr.AddReference('accoremgd')

from Autodesk.Civil.ApplicationServices import CivilApplication
from Autodesk.Civil.DatabaseServices import Network, Pipe
from Autodesk.AutoCAD.ApplicationServices import Application as AcadApp
from Autodesk.AutoCAD.DatabaseServices import OpenMode

# ── constants ─────────────────────────────────────────────────────────────────
STANDARD_SIZES_MM = [
    100, 150, 200, 225, 250, 300, 375, 450,
    525, 600, 675, 750, 825, 900, 1050, 1200, 1350, 1500,
]

N_VALUES = {
    'concrete': 0.013, 'reinforced concrete': 0.013, 'rcp': 0.013,
    'hdpe': 0.009, 'pvc': 0.009, 'upvc': 0.009,
    'ductile iron': 0.012, 'cast iron': 0.013, 'steel': 0.011,
    'clay': 0.013, 'vitrified clay': 0.012,
    'corrugated metal': 0.024, 'cmp': 0.024,
}

# ── helpers ───────────────────────────────────────────────────────────────────
def get_n(material):
    return N_VALUES.get((material or '').lower(), 0.013)

def full_flow_ls(dia_m, slope_mm, n):
    """Manning's full-flow capacity in L/s for a circular pipe."""
    if dia_m <= 0 or slope_mm <= 0 or n <= 0:
        return 0.0
    area = math.pi * dia_m * dia_m / 4.0
    r    = dia_m / 4.0
    return (1.0 / n) * area * (r ** (2.0 / 3.0)) * math.sqrt(slope_mm) * 1000.0

def recommend_dia_m(required_ls, slope, n):
    """Returns smallest standard diameter (metres) that meets required_ls."""
    for mm in STANDARD_SIZES_MM:
        d = mm / 1000.0
        if full_flow_ls(d, slope, n) >= required_ls:
            return d
    return STANDARD_SIZES_MM[-1] / 1000.0

def evaluate_status(slope, design_ls, qfull_ls, check_flow):
    if slope * 100.0 < 0.3:
        return 'WARN: slope < 0.3%',  True, False
    if slope * 100.0 > 20.0:
        return 'WARN: slope > 20%',   True, False
    if check_flow:
        if design_ls > qfull_ls:
            return 'FAIL: UNDERSIZED', False, True
        if design_ls > qfull_ls * 0.80:
            return 'WARN: >80% capacity', True, False
    return 'OK', False, False

# ── main ──────────────────────────────────────────────────────────────────────
design_flow_ls = IN[0] or 0
run_flag       = IN[1]

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
            check_flow = design_flow_ls > 0
            lines      = []
            warnings   = 0
            failures   = 0

            tr = db.TransactionManager.StartTransaction()
            try:
                for net_id in network_ids:
                    network = tr.GetObject(net_id, OpenMode.ForRead)
                    lines.append('\n== Network: {} =='.format(network.Name))

                    hdr = '{:<28} {:>7} {:>8} {:>10}'.format(
                        'Pipe', 'Dia mm', 'Slope%', 'QFull L/s')
                    if check_flow:
                        hdr += ' {:>10}'.format('QDes L/s')
                    hdr += '  {:<24}'.format('Status')
                    lines.append(hdr)
                    lines.append('-' * (88 if check_flow else 75))

                    for pipe_id in network.GetPipeIds():
                        pipe  = tr.GetObject(pipe_id, OpenMode.ForRead)
                        n     = get_n(pipe.PipeMaterial)
                        slope = pipe.Slope
                        dia   = pipe.InnerDiameterOrWidth
                        qfull = full_flow_ls(dia, slope, n)

                        status, is_warn, is_fail = evaluate_status(
                            slope, design_flow_ls, qfull, check_flow)
                        if is_warn: warnings += 1
                        if is_fail: failures += 1

                        row = '{:<28} {:>7.0f} {:>8.3f} {:>10.2f}'.format(
                            pipe.Name or '', dia * 1000.0, slope * 100.0, qfull)
                        if check_flow:
                            row += ' {:>10.2f}'.format(design_flow_ls)
                        row += '  {:<24}'.format(status)
                        lines.append(row)

                        if is_fail and check_flow:
                            sug   = recommend_dia_m(design_flow_ls, slope, n)
                            sug_q = full_flow_ls(sug, slope, n)
                            lines.append('   Recommend: {:.0f} mm  '
                                         '(QFull = {:.2f} L/s)'.format(sug * 1000.0, sug_q))

                tr.Commit()
            except Exception:
                tr.Abort()
                raise

            lines.append('\nSizing check complete.  '
                         'Warnings: {}   Failures: {}'.format(warnings, failures))
            OUT = '\n'.join(lines)

    except Exception as ex:
        OUT = 'Error: ' + str(ex)
