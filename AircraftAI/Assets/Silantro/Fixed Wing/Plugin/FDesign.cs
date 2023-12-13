#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using Oyedoyin.Mathematics;
using System.Collections.Generic;


/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Analysis
{
    /// <summary>
    /// Does all the "Unclean" stuff for the aerofoils :))
    /// </summary>
    public class AerofoilDesign
    {
        public enum SurfaceType { leadingEdge, trailingEdge }
        public enum SurfaceMode { _fixed, _floating }
        public enum Surface { Control, Flap, Slat, Spoiler }

        private static void ResetCells(SilantroAerofoil foil)
        {
            foreach (SilantroAerofoil.Cell cell in foil.m_cells) { cell.m_reset = true; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        public static void AnalyseCell(SilantroAerofoil foil) { _analyseCell(foil); }
        protected static void _analyseCell(SilantroAerofoil foil)
        {
            // ------------------------------------------------------------------ Cell Count
            if (foil.m_cells == null) { foil.m_cells = new List<SilantroAerofoil.Cell>(foil.subdivision); }
            if (foil.subdivision != foil.m_cells.Count)
            {
                if (foil.subdivision > foil.m_cells.Count)
                {
                    SilantroAerofoil.Cell newCell = new SilantroAerofoil.Cell();
                    foil.m_cells.Add(newCell);
                    ResetCells(foil);
                }
                if (foil.subdivision < foil.m_cells.Count) { foil.m_cells.RemoveAt(foil.subdivision - 1); ResetCells(foil); }
            }

            // ------------------------------------------------------------------ Cell Design
            if (foil.subdivision == foil.m_cells.Count)
            {
                for (int p = 0; p < foil.subdivision; p++)
                {
                    SilantroAerofoil.Cell cell = foil.m_cells[p];

                    if (cell.m_reset)
                    {
                        cell.m_spanFill = 0;
                        cell.m_taper = 0;
                        cell.m_sweep = 0;
                        cell.m_twist = 0;
                        cell.m_dihedral = 0;
                        cell.m_reset = false;
                    }
                    cell.m_span = (foil.m_span / foil.subdivision) * ((100 - cell.m_spanFill) / 100);
                    if (cell.m_foil == null) { cell.m_foil = foil._foil; }
                    cell._frc = (float)p / (float)foil.m_cells.Count;
                    cell._ftc = (float)(p + 1) / (float)foil.m_cells.Count;
                    cell._mfx = MathBase.EstimateSection(cell._frc, cell._ftc, 0.5f);

                    if (p == 0)
                    {
                        cell.m_rootCenter = foil.transform.position;
                        cell.m_rθ = 0;
                        cell.m_tθ = (foil._twist * 1 / foil.subdivision) + cell.m_twist;
                        cell._dihedral = cell.m_dihedral;
                    }
                    else
                    {
                        cell.m_rootCenter = foil.m_cells[p - 1].m_tipCenter;
                        cell.m_rθ = foil.m_cells[p - 1].m_tθ;
                        cell.m_tθ = (foil._twist * (p + 1) / foil.subdivision) + cell.m_twist;
                        cell._dihedral = foil.m_cells[p - 1].m_dihedral + cell.m_dihedral;
                    }

                    float _sweep = foil._sweep + cell.m_sweep;
                    float _incidence = (foil._twist * (p + 0) / (foil.subdivision - 1)) + cell.m_twist;
                    Quaternion xRotation = Quaternion.AngleAxis(_incidence, Vector3.right);
                    Quaternion zRotation = Quaternion.AngleAxis(cell._dihedral * foil.m_section, Vector3.forward);

                    Vector3 m_rf = foil.transform.rotation * zRotation * Quaternion.AngleAxis(cell.m_rθ, Vector3.right) * Vector3.forward;
                    Vector3 m_tf = foil.transform.rotation * zRotation * Quaternion.AngleAxis(cell.m_tθ, Vector3.right) * Vector3.forward;

                    float m_dihedralDistance = cell.m_span * Mathf.Tan(cell._dihedral * Mathf.Deg2Rad);
                    float m_kTheta = Mathf.Tan(_sweep * Mathf.Deg2Rad);
                    float m_sweepDistance = cell.m_span * m_kTheta;
                    cell.m_tipCenter = cell.m_rootCenter
                        + (cell.m_span * foil.m_section * foil.transform.right)
                        + (foil.transform.forward * m_sweepDistance)
                        + (foil.transform.up * m_dihedralDistance);
                    if (p == 0) { cell.m_rootChord = foil.m_rootChord; } else { cell.m_rootChord = foil.m_cells[p - 1].m_tipChord; }
                    float m_baseTip = foil.m_rootChord * ((100 - foil.m_taper) / 100);
                    cell.m_tipChord = (foil.m_rootChord + ((m_baseTip - foil.m_rootChord) * (p + 1) / foil.subdivision)) * ((100 - cell.m_taper) / 100);

                    Vector3 m_leading_root, m_trailing_root, m_leading_tip, m_trailing_tip;
                    if (p == 0)
                    {
                        m_leading_root = cell.m_rootCenter + (0.5f * foil.m_rootChord * m_rf);
                        m_trailing_root = cell.m_rootCenter - (0.5f * foil.m_rootChord * m_rf);
                        foil.leadingRoot = m_leading_root;
                    }
                    else
                    {
                        m_leading_root = foil.transform.TransformPoint(foil.m_cells[p - 1].m_leading_tipLocal);
                        m_trailing_root = foil.transform.TransformPoint(foil.m_cells[p - 1].m_trailing_tipLocal);
                    }

                    m_leading_tip = cell.m_tipCenter + (0.5f * cell.m_tipChord * m_tf);
                    m_trailing_tip = cell.m_tipCenter - (0.5f * cell.m_tipChord * m_tf);
                    cell.m_quater_root = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, 0.75f);
                    cell.m_quater_tip = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, 0.75f);
                    cell.m_quaterCenter = MathBase.EstimateSectionPosition(cell.m_quater_root, cell.m_quater_tip, 0.5f);

                    cell.m_leading_rootLocal = foil.transform.InverseTransformPoint(m_leading_root);
                    cell.m_trailing_rootLocal = foil.transform.InverseTransformPoint(m_trailing_root);
                    cell.m_leading_tipLocal = foil.transform.InverseTransformPoint(m_leading_tip);
                    cell.m_trailing_tipLocal = foil.transform.InverseTransformPoint(m_trailing_tip);
                }
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        public static void DrawCells(SilantroAerofoil foil) { _drawCells(foil); }
        protected static void _drawCells(SilantroAerofoil foil)
        {
            if (foil.subdivision == foil.m_cells.Count)
            {
                for (int p = 0; p < foil.subdivision; p++)
                {
                    SilantroAerofoil.Cell cell = foil.m_cells[p];
                    _cellPoints(foil, cell, out Vector3 m_leading_root, out Vector3 m_trailing_root, out Vector3 m_leading_tip, out Vector3 m_trailing_tip);

                    // ------------------------------------------------------------------ Draw Base
                    Handles.color = Color.yellow; Handles.DrawDottedLine(cell.m_quater_root, cell.m_quater_tip, 4f);
                    Handles.color = Color.yellow; Handles.DrawDottedLine(cell.m_rootCenter, cell.m_tipCenter, 4f);
                    if (p == foil.m_cells.Count - 1)
                    {
                        Handles.color = Color.yellow;
                        Handles.ArrowHandleCap(0, m_leading_tip, Quaternion.LookRotation(cell.m_up), 0.3f, EventType.Repaint);
                    }
                    Gizmos.color = Color.red; Gizmos.DrawLine(m_trailing_tip, m_trailing_root);
                    Gizmos.color = Color.yellow; Gizmos.DrawLine(m_leading_tip, m_leading_root);
                    Vector3 m_quaterCenter = MathBase.EstimateSectionPosition(cell.m_quater_root, cell.m_quater_tip, 0.5f);

                    // ------------------------------------------------------------------ Draw Subdivision
                    if (p != foil.subdivision - 1)
                    {
                        if (foil.tipAirfoil != null && foil.drawFoil) { PlotRibAirfoil(m_leading_tip, m_trailing_tip, cell.m_up, cell._mfx, Color.yellow, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.transform); }
                        else { Gizmos.color = Color.yellow; Gizmos.DrawLine(m_leading_tip, m_trailing_tip); }
                    }

                    // ------------------------------------------------------------------ Draw Gizmo Handler
                    if (foil.drawAxes)
                    {
                        cell.m_forward = cell.n_rotation * Vector3.forward;
                        cell.m_up = cell.n_rotation * Vector3.up;
                        cell.m_right = cell.n_rotation * Vector3.right;
                        Handles.color = Color.blue;
                        Handles.ArrowHandleCap(0, m_quaterCenter, Quaternion.LookRotation(cell.m_forward), foil.m_axisScale, EventType.Repaint);
                        Handles.color = Color.green;
                        Handles.ArrowHandleCap(0, m_quaterCenter, Quaternion.LookRotation(cell.m_up), foil.m_axisScale, EventType.Repaint);
                        Handles.color = Color.red;
                        Handles.ArrowHandleCap(0, m_quaterCenter, Quaternion.LookRotation(cell.m_right * foil.m_section), foil.m_axisScale, EventType.Repaint);
                    }
                }

                // ----------------------------------------------- Draw Ends
                if (foil.drawFoil)
                {
                    if (foil.rootAirfoil != null)
                    {
                        PlotAirfoil(foil.transform.TransformPoint(foil.m_cells[0].m_leading_rootLocal), foil.transform.TransformPoint(foil.m_cells[0].m_trailing_rootLocal), foil.m_cells[0].m_up, foil.rootAirfoil, out foil.m_cells[0].m_rootArea);
                    }
                    if (foil.tipAirfoil != null)
                    {
                        PlotAirfoil(foil.transform.TransformPoint(foil.m_cells[foil.subdivision - 1].m_leading_tipLocal), foil.transform.TransformPoint(foil.m_cells[foil.subdivision - 1].m_trailing_tipLocal), foil.m_cells[foil.subdivision - 1].m_up, foil.tipAirfoil, out foil.m_cells[foil.subdivision - 1].m_tipArea);
                    }
                }
                else
                {
                    Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.transform.TransformPoint(foil.m_cells[0].m_leading_rootLocal), foil.transform.TransformPoint(foil.m_cells[0].m_trailing_rootLocal));
                    Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.transform.TransformPoint(foil.m_cells[foil.subdivision - 1].m_leading_tipLocal), foil.transform.TransformPoint(foil.m_cells[foil.subdivision - 1].m_trailing_tipLocal));
                }
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadingPoint"></param>
        /// <param name="trailingPoint"></param>
        /// <param name="m_up"></param>
        /// <param name="foil"></param>
        /// <param name="foilArea"></param>
        public static void PlotAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, Vector3 m_up, SilantroAirfoil foil, out float foilArea)
        {
            List<Vector3> points = new List<Vector3>();
            float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
            Vector3 PointB = Vector3.zero;

            //FIND POINTS
            if (foil.x.Count > 0)
            {
                for (int j = 0; (j < foil.x.Count); j++)
                {
                    //BASE POINT
                    Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j] * chordDistance));
                    Vector3 liftDirection = m_up.normalized;
                    Vector3 PointA = XA + (liftDirection * ((foil.y[j]) * chordDistance));
                    points.Add(PointA);
                    if ((j + 1) < foil.x.Count)
                    {
                        Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j + 1] * chordDistance)));
                        PointB = XB + (liftDirection.normalized * ((foil.y[j + 1]) * chordDistance));
                    }

                    //CONNECT
                    Gizmos.color = Color.white; Gizmos.DrawLine(PointA, PointB);
                }
            }


            //PERFORM CALCULATIONS
            for (int j = 0; (j < points.Count); j++)
            {
                Gizmos.DrawLine(points[j], points[(points.Count - j - 1)]);
            }
            foilArea = Mathf.Pow(chordDistance, 2f) * (((foil.xtc * 0.01f) + 3) / 6f) * (foil.tc * 0.01f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadingPoint"></param>
        /// <param name="trailingPoint"></param>
        /// <param name="m_up"></param>
        /// <param name="_mpf"></param>
        /// <param name="ribColor"></param>
        /// <param name="drawSplits"></param>
        /// <param name="rootAirfoil"></param>
        /// <param name="tipAirfoil"></param>
        /// <param name="foilTransform"></param>
        public static void PlotRibAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, Vector3 m_up, float _mpf, Color ribColor, bool drawSplits, SilantroAirfoil rootAirfoil, SilantroAirfoil tipAirfoil, Transform foilTransform)
        {
            List<Vector3> points = new List<Vector3>();
            float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
            Vector3 PointA = Vector3.zero, PointXA = Vector3.zero, PointXB = Vector3.zero;

            //FIND POINTS
            if (rootAirfoil.x.Count > 0)
            {
                for (int j = 0; (j < rootAirfoil.x.Count); j++)
                {
                    float xi = MathBase.EstimateSection(rootAirfoil.x[j], tipAirfoil.x[j], _mpf);
                    float yi = MathBase.EstimateSection(rootAirfoil.y[j], tipAirfoil.y[j], _mpf);
                    //BASE POINT
                    Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (xi * chordDistance));
                    Vector3 liftDirection = m_up; PointXA = XA + (chordDistance * yi * liftDirection); points.Add(PointXA);
                    if ((j + 1) < rootAirfoil.x.Count)
                    {
                        float xii = MathBase.EstimateSection(rootAirfoil.x[j + 1], tipAirfoil.x[j + 1], _mpf);
                        float yii = MathBase.EstimateSection(rootAirfoil.y[j + 1], tipAirfoil.y[j + 1], _mpf);
                        Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (xii * chordDistance))); PointXB = XB + (liftDirection.normalized * (yii * chordDistance));
                    }
                    //CONNECT
                    Gizmos.color = ribColor; Gizmos.DrawLine(PointXA, PointXB);
                }
            }
            if (drawSplits) { for (int jx = 0; (jx < points.Count); jx++) { Gizmos.color = ribColor; Gizmos.DrawLine(points[jx], points[(points.Count - jx - 1)]); } }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        public static void AnalyseDimension(SilantroAerofoil foil) { _analyseDimension(foil); }
        protected static void _analyseDimension(SilantroAerofoil foil)
        {
            foil.m_area = 0;
            foil.m_wettedArea = 0f;
            foil.SWf = 0;
            foil.SWc = 0;
            if (foil.subdivision == foil.m_cells.Count)
            {
                for (int p = 0; p < foil.subdivision; p++)
                {
                    // ------------------------------------- Basic
                    SilantroAerofoil.Cell cell = foil.m_cells[p];
                    _cellPoints(foil, cell, out Vector3 m_leading_root, out Vector3 m_trailing_root, out Vector3 m_leading_tip, out Vector3 m_trailing_tip);

                    // ------------------------------------- Reset
                    cell.m_flapChord = cell.m_controlChord = cell.m_slatChord = cell.m_spoilerChord = 0;

                    cell.λ = cell.m_tipChord / foil.m_rootChord;
                    float θ;
                    if (p == 0) { θ = cell.m_rθ; } else { θ = cell.m_tθ; }
                    float _incidence = (foil._twist * (p + 0) / (foil.subdivision - 1)) + cell.m_twist;
                    cell.Г = (cell._dihedral * foil.m_section) + foil.transform.localEulerAngles.z;
                    if (cell.Г > 180) { cell.Г -= 360; }
                    cell.Г *= foil.m_section;
                    cell.θ = _incidence + foil.transform.localEulerAngles.x;
                    if (cell.θ > 180) { cell.θ -= 360; }

                    // ------------------------------------- Sweep
                    Vector3 m_dir = foil.transform.forward;
                    float m_le = Vector3.Dot(m_dir, m_leading_root - m_leading_tip);
                    float m_qe = Vector3.Dot(m_dir, cell.m_quater_root - cell.m_quater_tip);
                    float m_ce = Vector3.Dot(m_dir, cell.m_rootCenter - cell.m_tipCenter);
                    cell.ᴧLE = Mathf.Atan2(m_le, cell.m_span) * Mathf.Rad2Deg;
                    cell.ᴧQT = Mathf.Atan2(m_qe, cell.m_span) * Mathf.Rad2Deg;
                    cell.ᴧCT = Mathf.Atan2(m_ce, cell.m_span) * Mathf.Rad2Deg;
                    cell.m_Mcritᴧ = 1 / Mathf.Cos(Mathf.Atan2(m_le, cell.m_span));
                    double eA = Mathf.Pow((Mathf.Tan(cell.ᴧQT * Mathf.Deg2Rad)), 2);
                    double eB = 4f + ((foil.m_aspectRatio * foil.m_aspectRatio) * (1 + eA));
                    cell.m_e = (float)(2 / (2 - foil.m_aspectRatio + Math.Sqrt(eB)));
                    double ar1 = (2 * (foil.m_aspectRatio + 4)) / (foil.m_aspectRatio + 2);
                    cell.m_ARf = (float)(foil.m_aspectRatio / (foil.m_aspectRatio + ar1));

                    // ------------------------------------- Efficiency
                    cell.m_area = MathBase.EstimatePanelSectionArea(m_leading_root, m_leading_tip, m_trailing_root, m_trailing_tip);
                    cell.groundAxis = new Vector3(0.0f, -1.0f, 0.0f); cell.groundAxis.Normalize();
                    cell.m_meanChord = MathBase.EstimateMeanChord(cell.m_rootChord, cell.m_tipChord);

                    if (foil.sweepCorrectionMethod == SilantroAerofoil.SweepCorrection.DATCOM)
                    {
                        float a = Mathf.Pow(Mathf.Cos(Mathf.Abs(cell.ᴧQT) * Mathf.Deg2Rad), 2f);
                        float b = Mathf.Pow(Mathf.Cos(Mathf.Abs(cell.ᴧQT) * Mathf.Deg2Rad), 0.75f);
                        cell.m_Kθ = (1 - (0.08f * a)) * b;
                    }
                    else if (foil.sweepCorrectionMethod == SilantroAerofoil.SweepCorrection.YoungLE)
                    {
                        cell.m_Kθ = Mathf.Cos(cell.ᴧQT * Mathf.Deg2Rad);
                    }
                    else if (foil.sweepCorrectionMethod == SilantroAerofoil.SweepCorrection.None) { cell.m_Kθ = 1; }

                    foil.m_area += cell.m_area;
                    foil.m_wettedArea += cell.m_wettedArea;
                    if (p == 0) { cell.m_foilArea = cell.m_rootArea; } else { cell.m_foilArea = cell.m_tipArea; }

                    if (foil.rootAirfoil && foil.tipAirfoil)
                    {
                        cell.m_effectiveThickness = MathBase.EstimateSection(foil.rootAirfoil.maximumThickness, foil.tipAirfoil.maximumThickness, cell._mfx);
                        cell.m_wettedArea = cell.m_area * (1.977f + (0.52f * cell.m_effectiveThickness));
                        cell.m_edgeRadius = MathBase.EstimateSection(foil.rootAirfoil.leadingEdgeRadius, foil.tipAirfoil.leadingEdgeRadius, cell._mfx);
                        cell.m_liftSlope = MathBase.EstimateSection(foil.rootAirfoil.centerLiftSlope, foil.tipAirfoil.centerLiftSlope, cell._mfx);
                    }
                    else { cell.m_liftSlope = 5.73f; }

                    cell.n_rotation = foil.transform.rotation * Quaternion.Euler(θ, foil.m_section * cell.ᴧQT, cell.Г - (foil.m_section * foil.transform.localEulerAngles.z));
                    cell.m_forward = cell.n_rotation * Vector3.forward;
                    cell.m_up = cell.n_rotation * Vector3.up;
                    cell.m_right = cell.n_rotation * Vector3.right;



                    // ------------------------------------- Check Controls
                    if (foil.controlState == SilantroAerofoil.ControlType.Controllable &&
                        foil.surfaceType != SilantroAerofoil.SurfaceType.Inactive &&
                        foil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly &&
                        foil.controlAnalysis != SilantroAerofoil.AnalysisMethod.NumericOnly)
                    {
                        if (foil.m_controlSections != null && foil.m_controlSections.Length == foil.subdivision && foil.m_controlSections.Length > 2 && foil.m_controlSections[p] == true)
                        {
                            cell.m_controlActive = true; foil.SWc += cell.m_wettedArea;
                        }
                    }
                    else { cell.m_controlActive = false; }

                    // ------------------------------------- Check Flaps
                    if (foil.controlState == SilantroAerofoil.ControlType.Controllable &&
                        foil.flapState == SilantroAerofoil.ControlState.Active &&
                        foil.m_foilType == SilantroAerofoil.AerofoilType.Wing &&
                        foil.flapAnalysis != SilantroAerofoil.AnalysisMethod.NumericOnly)
                    {
                        if (foil.m_flapSections != null && foil.m_flapSections.Length == foil.subdivision && foil.m_flapSections.Length > 2 && foil.m_flapSections[p] == true)
                        {
                            cell.m_flapActive = true; foil.SWf += cell.m_wettedArea;
                        }
                    }
                    else { cell.m_flapActive = false; }

                    // ------------------------------------- Check Slats
                    if (foil.controlState == SilantroAerofoil.ControlType.Controllable &&
                        foil.slatState == SilantroAerofoil.ControlState.Active &&
                        foil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                    {
                        if (foil.m_slatSections != null && foil.m_slatSections.Length == foil.subdivision && foil.m_slatSections.Length > 2 && foil.m_slatSections[p] == true)
                        {
                            cell.m_slatActive = true;
                        }
                    }
                    else { cell.m_flapActive = false; }

                    double m_aspectRatio = foil.m_aspectRatio;
                    float cosᴧQT = Mathf.Cos(cell.ᴧQT);
                    float sinᴧQT = Mathf.Sin(cell.ᴧQT);
                    float tanᴧQT = Mathf.Tan(cell.ᴧQT);
                    float cy1 = 6 * tanᴧQT * sinᴧQT;
                    double cy2 = Mathf.PI * m_aspectRatio * (m_aspectRatio + (4 * cosᴧQT));
                    cell.CYβ_CL = (float)(cy1 / cy2) * (1 / 57.3f);
                    cell.ΔCYβ = -0.0001f * Mathf.Abs(cell.Г);
                    float xc = 0.035f;
                    double cn1 = tanᴧQT / (Mathf.PI * m_aspectRatio * (m_aspectRatio + (4 * cosᴧQT)));
                    double cn2 = cosᴧQT - (0.5f * m_aspectRatio) - (Math.Pow(m_aspectRatio, 2) / (8 * cosᴧQT)) + (6 * xc * (sinᴧQT / m_aspectRatio));
                    cell.Cnβ_CL = (float)((1 / 57.3f) * ((1 / (4 * Mathf.PI * m_aspectRatio)) - (cn1 * cn2)));
                }
                foil.m_aspectRatio = (foil.m_span * foil.m_span) / foil.m_area;
                if (foil.rootAirfoil && foil.tipAirfoil) { foil.m_stallAngle = Mathf.Min(foil.rootAirfoil.stallAngle, foil.tipAirfoil.stallAngle); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        /// <param name="cell"></param>
        /// <param name="_deflection"></param>
        /// <param name="rc"></param>
        /// <param name="tc"></param>
        /// <returns></returns>
        public static Vector3 _controlCell(SilantroAerofoil foil, SilantroAerofoil.Cell cell, float _deflection, float rc, float tc)
        {
            _cellPoints(foil, cell, out Vector3 m_leading_root, out Vector3 m_trailing_root, out Vector3 m_leading_tip, out Vector3 m_trailing_tip);

            float ct = tc * 0.01f;
            float cr = rc * 0.01f;
            Vector3 c_extension = Quaternion.AngleAxis(_deflection,
                MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, ct) -
                MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, cr)) *
                (m_trailing_root - MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, cr));

            return c_extension;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        /// <param name="draw"></param>
        public static void AnalyseCellControl(SilantroAerofoil foil, bool draw) { _analyseCellControl(foil, draw); }
        protected static void _analyseCellControl(SilantroAerofoil foil, bool draw)
        {
            // --------------------------------------------------- Set Control Colors
            if (foil.surfaceType == SilantroAerofoil.SurfaceType.Aileron) { foil.m_controlColor = Color.green; }
            if (foil.surfaceType == SilantroAerofoil.SurfaceType.Elevon) { foil.m_controlColor = new Color(0, 0.5f, 0.5f); }
            if (foil.surfaceType == SilantroAerofoil.SurfaceType.Elevator) { foil.m_controlColor = Color.blue; }
            if (foil.surfaceType == SilantroAerofoil.SurfaceType.Rudder) { foil.m_controlColor = Color.red; }
            if (foil.surfaceType == SilantroAerofoil.SurfaceType.Ruddervator) { foil.m_controlColor = new Color(0.5f, 0, 0f); }

            // --------------------------------------------------- Base Control
            if (foil.m_controlSections == null || foil.subdivision != foil.m_controlSections.Length) { foil.m_controlSections = new bool[foil.subdivision]; }
            if (foil.surfaceType != SilantroAerofoil.SurfaceType.Inactive && foil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
            {
                _analyseSurface(foil,
                    foil.m_controlRootChord,
                    foil.m_controlTipChord,
                    foil.m_controlSections,
                    foil.m_controlDeflection,
                    SurfaceType.trailingEdge,
                    SurfaceMode._fixed,
                    Surface.Control,
                    foil.m_controlColor,
                    draw, out foil.m_controlArea);
            }
            if (foil.availableControls == SilantroAerofoil.AvailableControls.SecondaryOnly) { foil.m_controlSections = new bool[foil.subdivision]; }

            // --------------------------------------------------- Flap
            if (foil.m_flapSections == null || foil.subdivision != foil.m_flapSections.Length) { foil.m_flapSections = new bool[foil.subdivision]; }

            if (foil.flapState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                if (foil.flapType == SilantroAerofoil.FlapType.Flapevon) { foil.m_flapColor = new Color(0, 0, 0.5f); }
                else if (foil.flapType == SilantroAerofoil.FlapType.Flaperon) { foil.m_flapColor = new Color(1, 0.42f, 0); }
                else { foil.m_flapColor = Color.yellow; }
                _analyseSurface(foil,
                    foil.m_flapRootChord,
                    foil.m_flapTipChord,
                    foil.m_flapSections,
                    foil.m_flapDeflection,
                    SurfaceType.trailingEdge,
                    SurfaceMode._fixed,
                    Surface.Flap,
                    foil.m_flapColor,
                    draw, out foil.m_flapArea);
            }
            if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.m_flapSections = new bool[foil.subdivision]; }

            // --------------------------------------------------- Slat
            if (foil.m_slatSections == null || foil.subdivision != foil.m_slatSections.Length)
            {
                foil.m_slatSections = new bool[foil.subdivision];
            }
            if (foil.slatState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                _analyseSurface(foil,
                    foil.m_slatRootChord,
                    foil.m_slatTipChord,
                    foil.m_slatSections,
                    foil.m_slatDeflection,
                    SurfaceType.leadingEdge,
                    SurfaceMode._fixed,
                    Surface.Slat,
                    Color.magenta,
                    draw, out foil.m_slatArea);
            }
            if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.m_slatSections = new bool[foil.subdivision]; }

            // --------------------------------------------------- Spoilers
            if (foil.m_spoilerSections == null || foil.subdivision != foil.m_spoilerSections.Length) { foil.m_spoilerSections = new bool[foil.subdivision]; }
            if (foil.spoilerState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                _analyseSurface(foil,
                    100 - foil.m_spoilerChord,
                    100 - foil.m_spoilerChord,
                    foil.m_spoilerSections,
                    foil.m_spoilerDeflection,
                    SurfaceType.leadingEdge,
                    SurfaceMode._floating,
                    Surface.Spoiler,
                    Color.cyan, draw,
                    out foil.m_spoilerArea);
            }
            if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.m_spoilerSections = new bool[foil.subdivision]; }
        }
        protected static void _analyseSurface(SilantroAerofoil foil,
            float m_rootChord,
            float m_tipChord,
            bool[] sections,
            float deflection,
            SurfaceType type,
            SurfaceMode mode,
            Surface surface,
            Color _surfaceColor,
            bool draw, out float controlArea)
        {
            controlArea = 0; float m_deflection;
            if (foil.subdivision == foil.m_cells.Count)
            {
                for (int p = 0; p < foil.subdivision; p++)
                {
                    SilantroAerofoil.Cell cell = foil.m_cells[p];
                    _cellPoints(foil, cell, out Vector3 m_leading_root, out Vector3 m_trailing_root, out Vector3 m_leading_tip, out Vector3 m_trailing_tip);
                    _cellPoints(foil, foil.m_cells[0], out Vector3 m_leading_root0, out Vector3 m_trailing_root0, out _, out _);
                    _cellPoints(foil, foil.m_cells[foil.m_cells.Count - 1], out _, out Vector3 _, out Vector3 m_leading_tip1, out Vector3 m_trailing_tip1);
                    Vector3 ta = (MathBase.EstimateSectionPosition(m_leading_root0, m_leading_tip1, cell._frc));
                    Vector3 tb = (MathBase.EstimateSectionPosition(m_leading_root0, m_leading_tip1, cell._ftc));
                    Vector3 ra = (MathBase.EstimateSectionPosition(m_trailing_root0, m_trailing_tip1, cell._frc));
                    Vector3 rb = (MathBase.EstimateSectionPosition(m_trailing_root0, m_trailing_tip1, cell._ftc));


                    float cr = 0;
                    float ct = 0;
                    if (p == 0)
                    {
                        if (surface == Surface.Control) { cr = cell.m_controlRootCorrection; ct = cell.m_controlTipCorrection; }
                        if (surface == Surface.Flap) { cr = cell.m_flapRootCorrection; ct = cell.m_flapTipCorrection; }
                        if (surface == Surface.Slat) { cr = cell.m_slatRootCorrection; ct = cell.m_slatTipCorrection; }
                        if (surface == Surface.Spoiler) { cr = cell.m_spoilerRootCorrection; ct = cell.m_spoilerTipCorrection; }
                    }
                    else if (p > 0 && p < (foil.m_cells.Count - 1))
                    {
                        if (surface == Surface.Control)
                        {
                            cr = cell.m_controlRootCorrection + foil.m_cells[p - 1].m_controlTipCorrection;
                            ct = cell.m_controlTipCorrection + foil.m_cells[p + 1].m_controlRootCorrection;
                        }
                        if (surface == Surface.Flap)
                        {
                            cr = cell.m_flapRootCorrection + foil.m_cells[p - 1].m_flapTipCorrection;
                            ct = cell.m_flapTipCorrection + foil.m_cells[p + 1].m_flapRootCorrection;
                        }
                        if (surface == Surface.Slat)
                        {
                            cr = cell.m_slatRootCorrection + foil.m_cells[p - 1].m_slatTipCorrection;
                            ct = cell.m_slatTipCorrection + foil.m_cells[p + 1].m_slatRootCorrection;
                        }
                        if (surface == Surface.Spoiler)
                        {
                            cr = cell.m_spoilerRootCorrection + foil.m_cells[p - 1].m_spoilerTipCorrection;
                            ct = cell.m_spoilerTipCorrection + foil.m_cells[p + 1].m_spoilerRootCorrection;
                        }
                    }
                    if (p == foil.m_cells.Count - 1)
                    {
                        if (surface == Surface.Control)
                        {
                            cr = cell.m_controlRootCorrection + foil.m_cells[p - 1].m_controlTipCorrection;
                            ct = cell.m_controlTipCorrection;
                        }
                        if (surface == Surface.Flap)
                        {
                            cr = cell.m_flapRootCorrection + foil.m_cells[p - 1].m_flapTipCorrection;
                            ct = cell.m_flapTipCorrection;
                        }
                        if (surface == Surface.Slat)
                        {
                            cr = cell.m_slatRootCorrection + foil.m_cells[p - 1].m_slatRootCorrection;
                            ct = cell.m_slatTipCorrection;
                        }
                        if (surface == Surface.Spoiler)
                        {
                            cr = cell.m_spoilerRootCorrection + foil.m_cells[p - 1].m_spoilerRootCorrection;
                            ct = cell.m_spoilerTipCorrection;
                        }
                    }


                    if (sections[p] == true)
                    {
                        // ----------------------------------------------------- Initialize
                        Vector3 m_rootTrailing;
                        Vector3 m_tipTrailing;
                        Vector3 m_rootLeading;
                        Vector3 m_tipLeading;


                        if (mode == SurfaceMode._floating)
                        {
                            m_rootLeading = MathBase.EstimateSectionPosition(m_leading_root, m_trailing_root, foil.m_spoilerHinge * 0.01f);
                            m_tipLeading = MathBase.EstimateSectionPosition(m_leading_tip, m_trailing_tip, foil.m_spoilerHinge * 0.01f);
                            m_rootTrailing = MathBase.EstimateSectionPosition(m_trailing_root, m_rootLeading, MathBase.EstimateSection(m_rootChord, m_tipChord, cell._frc) * 0.01f);
                            m_tipTrailing = MathBase.EstimateSectionPosition(m_trailing_tip, m_tipLeading, MathBase.EstimateSection(m_rootChord, m_tipChord, cell._ftc) * 0.01f);
                            m_deflection = deflection;
                        }
                        else
                        {
                            if (type == SurfaceType.trailingEdge)
                            {
                                Vector3 la = MathBase.EstimateSectionPosition(m_trailing_root0, m_leading_root0, (m_rootChord * 0.01f));
                                Vector3 lb = MathBase.EstimateSectionPosition(m_trailing_tip1, m_leading_tip1, (m_tipChord * 0.01f));
                                Vector3 rx = MathBase.EstimateSectionPosition(la, lb, cell._frc);
                                Vector3 tx = MathBase.EstimateSectionPosition(la, lb, cell._ftc);

                                float baseRootChord = Vector3.Distance(ta, ra);
                                float baseTipChord = Vector3.Distance(tb, rb);
                                float baseRootControl = Vector3.Distance(rx, ra);
                                float baseTipControl = Vector3.Distance(tx, rb);
                                float baseRootFactor = (baseRootControl + (baseRootChord * cr * 0.01f)) / baseRootChord;
                                float baseTipFactor = (baseTipControl + (baseTipChord * ct * 0.01f)) / baseTipChord;
                                baseRootFactor = Mathf.Clamp(baseRootFactor, 0, 1);
                                baseTipFactor = Mathf.Clamp(baseTipFactor, 0, 1);

                                m_rootTrailing = m_trailing_root;
                                m_tipTrailing = m_trailing_tip;
                                m_rootLeading = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, baseRootFactor);
                                m_tipLeading = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, baseTipFactor);
                                m_deflection = -deflection;
                            }
                            else
                            {
                                Vector3 la = MathBase.EstimateSectionPosition(m_trailing_root0, m_leading_root0, ((100 - m_rootChord) * 0.01f));
                                Vector3 lb = MathBase.EstimateSectionPosition(m_trailing_tip1, m_leading_tip1, ((100 - m_tipChord) * 0.01f));
                                Vector3 rx = MathBase.EstimateSectionPosition(la, lb, cell._frc);
                                Vector3 tx = MathBase.EstimateSectionPosition(la, lb, cell._ftc);

                                float baseRootChord = Vector3.Distance(ta, ra);
                                float baseTipChord = Vector3.Distance(tb, rb);
                                float baseRootControl = Vector3.Distance(rx, ra);
                                float baseTipControl = Vector3.Distance(tx, rb);
                                float baseRootFactor = (baseRootControl + (baseRootChord * cr * 0.01f)) / baseRootChord;
                                float baseTipFactor = (baseTipControl + (baseTipChord * ct * 0.01f)) / baseTipChord;
                                baseRootFactor = Mathf.Clamp(baseRootFactor, 0, 1);
                                baseTipFactor = Mathf.Clamp(baseTipFactor, 0, 1);

                                m_rootLeading = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, baseRootFactor);
                                m_tipLeading = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, baseTipFactor);
                                m_rootTrailing = m_leading_root;
                                m_tipTrailing = m_leading_tip;
                                m_deflection = deflection;
                            }
                        }

                        // ----------------------------------------------------- Deflect
                        m_rootTrailing = m_rootLeading + Quaternion.AngleAxis(foil.m_section * m_deflection, (m_tipLeading - m_rootLeading).normalized) * (m_rootTrailing - m_rootLeading);
                        m_tipTrailing = m_tipLeading + (Quaternion.AngleAxis(foil.m_section * m_deflection, (m_tipLeading - m_rootLeading).normalized)) * (m_tipTrailing - m_tipLeading);

                        // ----------------------------------------------------- Draw Airfoils
                        if (foil.tipAirfoil != null && foil.rootAirfoil != null && foil.drawFoil && draw) { PlotRibAirfoil(m_tipLeading, m_tipTrailing, cell.m_up, cell._mfx, Color.yellow, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.transform); }
                        if (foil.tipAirfoil != null && foil.rootAirfoil != null && foil.drawFoil && draw) { PlotRibAirfoil(m_rootLeading, m_rootTrailing, cell.m_up, cell._mfx, Color.yellow, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.transform); }

                        Vector3[] controlRect = new Vector3[4];
                        controlRect[0] = m_rootTrailing;
                        controlRect[1] = m_rootLeading;
                        controlRect[2] = m_tipLeading;
                        controlRect[3] = m_tipTrailing;
                        if (surface == Surface.Control)
                        {
                            cell._controlRootChord = (Vector3.Distance(m_rootLeading, m_rootTrailing) / cell.m_rootChord) * 100;
                            cell._controlTipChord = (Vector3.Distance(m_tipLeading, m_tipTrailing) / cell.m_tipChord) * 100;
                            cell.m_controlChord = MathBase.EstimateMeanChord(cell._controlRootChord, cell._controlTipChord) * 0.01f;
                        }
                        if (surface == Surface.Flap)
                        {
                            cell._flapRootChord = (Vector3.Distance(m_rootLeading, m_rootTrailing) / cell.m_rootChord) * 100;
                            cell._flapTipChord = (Vector3.Distance(m_tipLeading, m_tipTrailing) / cell.m_tipChord) * 100;
                            cell.m_flapChord = MathBase.EstimateMeanChord(cell._flapRootChord, cell._flapTipChord) * 0.01f;
                        }
                        if (surface == Surface.Slat)
                        {
                            float rootChord = (Vector3.Distance(m_rootLeading, m_rootTrailing) / cell.m_rootChord);
                            float tipChord = (Vector3.Distance(m_tipLeading, m_tipTrailing) / cell.m_tipChord);
                            cell.m_slatChord = MathBase.EstimateMeanChord(rootChord, tipChord);
                        }
                        if (surface == Surface.Spoiler)
                        {
                            float rootChord = (Vector3.Distance(m_rootLeading, m_rootTrailing) / cell.m_rootChord);
                            float tipChord = (Vector3.Distance(m_tipLeading, m_tipTrailing) / cell.m_tipChord);
                            cell.m_spoilerChord = MathBase.EstimateMeanChord(rootChord, tipChord);
                        }

                        // ----------------------------------------------------- Find area and draw surface
#if UNITY_EDITOR
                        Handles.color = _surfaceColor;
                        if (draw) { Handles.DrawSolidRectangleWithOutline(controlRect, _surfaceColor, _surfaceColor); }
                        controlArea += MathBase.EstimatePanelSectionArea(m_rootLeading, m_tipLeading, m_rootTrailing, m_tipTrailing);
#endif
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PlotControlEffectiveness(SilantroAerofoil foil) { _plotEffectiveness(foil); }
        private static void _plotEffectiveness(SilantroAerofoil foil)
        {
            // --------------------------------- Base Surface
            if (foil.m_controlArea > 0 && foil.surfaceType != SilantroAerofoil.SurfaceType.Inactive && foil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
            {
                if (foil.m_controlArea > 0)
                {
                    foil._controlNullPoint = MathBase.EstimateEfficiencySpeed(0.01f, foil.m_controlArea, foil._maximumControlTorque);
                    foil._controlLockPoint = MathBase.EstimateEfficiencySpeed(0.1f, foil.m_controlArea, foil._maximumControlTorque);
                    foil._controlFullPoint = MathBase.EstimateEfficiencySpeed(1, foil.m_controlArea, foil._maximumControlTorque);

                    foil._controlEfficiencyCurve = new AnimationCurve();
                    for (float i = foil._controlFullPoint; i < 1000f; i += 50f)
                    {
                        float efficiency = MathBase.EstimateControlEfficiency(i, foil.m_controlArea, foil._maximumControlTorque);
                        foil._controlEfficiencyCurve.AddKey(new Keyframe(i * 1.944f, efficiency * 100f));
                    }
                    MathBase.LinearizeCurve(foil._controlEfficiencyCurve);
                }
            }

            // --------------------------------- Flap Surface
            if (foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                if (foil.flapState == SilantroAerofoil.ControlState.Active)
                {
                    if (foil.m_flapArea > 0)
                    {
                        foil._flapNullPoint = MathBase.EstimateEfficiencySpeed(0.01f, foil.m_flapArea, foil._maximumFlapTorque);
                        foil._flapLockPoint = MathBase.EstimateEfficiencySpeed(0.1f, foil.m_flapArea, foil._maximumFlapTorque);
                        foil._flapFullPoint = MathBase.EstimateEfficiencySpeed(1, foil.m_flapArea, foil._maximumFlapTorque);

                        foil._flapEfficiencyCurve = new AnimationCurve();
                        for (float i = foil._flapFullPoint; i < 1000f; i += 50f)
                        {
                            float efficiency = MathBase.EstimateControlEfficiency(i, foil.m_flapArea, foil._maximumFlapTorque);
                            foil._flapEfficiencyCurve.AddKey(new Keyframe(i * 1.944f, efficiency * 100f));
                        }
                        MathBase.LinearizeCurve(foil._flapEfficiencyCurve);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        /// <param name="cell"></param>
        /// <param name="rl"></param>
        /// <param name="rt"></param>
        /// <param name="tl"></param>
        /// <param name="tt"></param>
        public static void _cellPoints(SilantroAerofoil foil, SilantroAerofoil.Cell cell, out Vector3 rl, out Vector3 rt, out Vector3 tl, out Vector3 tt)
        {
            tl = foil.transform.TransformPoint(cell.m_leading_tipLocal);
            rl = foil.transform.TransformPoint(cell.m_leading_rootLocal);
            rt = foil.transform.TransformPoint(cell.m_trailing_rootLocal);
            tt = foil.transform.TransformPoint(cell.m_trailing_tipLocal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="foil"></param>
        /// <param name="slat"></param>
        public static void PlotControlCurves(SilantroAerofoil foil, bool slat)
        {
            // ---------------------------------- CL Base
            foil.clBaseCurve = new AnimationCurve();
            foil.clBaseCurve.AddKey(new Keyframe(6.0144f, 0.9199f));
            foil.clBaseCurve.AddKey(new Keyframe(6.5054f, 0.8883f));
            foil.clBaseCurve.AddKey(new Keyframe(6.8520f, 0.8680f));
            foil.clBaseCurve.AddKey(new Keyframe(7.2274f, 0.8455f));
            foil.clBaseCurve.AddKey(new Keyframe(7.6462f, 0.8252f));
            foil.clBaseCurve.AddKey(new Keyframe(8.0505f, 0.8117f));
            foil.clBaseCurve.AddKey(new Keyframe(8.4693f, 0.8050f));
            foil.clBaseCurve.AddKey(new Keyframe(9.1047f, 0.8051f));
            foil.clBaseCurve.AddKey(new Keyframe(9.8267f, 0.8052f));
            foil.clBaseCurve.AddKey(new Keyframe(10.491f, 0.8280f));
            foil.clBaseCurve.AddKey(new Keyframe(11.083f, 0.8553f));
            foil.clBaseCurve.AddKey(new Keyframe(11.704f, 0.8938f));
            foil.clBaseCurve.AddKey(new Keyframe(12.2238f, 0.9301f));
            foil.clBaseCurve.AddKey(new Keyframe(12.7726f, 0.9619f));
            foil.clBaseCurve.AddKey(new Keyframe(13.2202f, 1.0027f));
            foil.clBaseCurve.AddKey(new Keyframe(13.639f, 1.0458f));
            foil.clBaseCurve.AddKey(new Keyframe(14.1733f, 1.1002f));
            foil.clBaseCurve.AddKey(new Keyframe(14.5921f, 1.1455f));
            foil.clBaseCurve.AddKey(new Keyframe(15.0108f, 1.1954f));
            foil.clBaseCurve.AddKey(new Keyframe(15.4007f, 1.2429f));
            foil.clBaseCurve.AddKey(new Keyframe(15.8339f, 1.2905f));
            foil.clBaseCurve.AddKey(new Keyframe(16.2383f, 1.3404f));
            foil.clBaseCurve.AddKey(new Keyframe(16.657f, 1.3902f));
            foil.clBaseCurve.AddKey(new Keyframe(17.1336f, 1.4446f));
            foil.clBaseCurve.AddKey(new Keyframe(17.5812f, 1.4832f));
            foil.clBaseCurve.AddKey(new Keyframe(17.9134f, 1.5172f));
            MathBase.LinearizeCurve(foil.clBaseCurve);


            // ---------------------------------- K1
            foil.k1Curve = new AnimationCurve();
            foil.k1Curve.AddKey(new Keyframe(0f, 0f));
            foil.k1Curve.AddKey(new Keyframe(0.8361f, 0.089f));
            foil.k1Curve.AddKey(new Keyframe(1.5174f, 0.1572f));
            foil.k1Curve.AddKey(new Keyframe(2.1677f, 0.2166f));
            foil.k1Curve.AddKey(new Keyframe(2.9729f, 0.2759f));
            foil.k1Curve.AddKey(new Keyframe(3.9948f, 0.3383f));
            foil.k1Curve.AddKey(new Keyframe(4.9858f, 0.4006f));
            foil.k1Curve.AddKey(new Keyframe(6.0077f, 0.463f));
            foil.k1Curve.AddKey(new Keyframe(7.0606f, 0.5224f));
            foil.k1Curve.AddKey(new Keyframe(8.1445f, 0.5788f));
            foil.k1Curve.AddKey(new Keyframe(9.2903f, 0.6263f));
            foil.k1Curve.AddKey(new Keyframe(10.3432f, 0.665f));
            foil.k1Curve.AddKey(new Keyframe(11.3652f, 0.7007f));
            foil.k1Curve.AddKey(new Keyframe(12.449f, 0.7393f));
            foil.k1Curve.AddKey(new Keyframe(13.471f, 0.7691f));
            foil.k1Curve.AddKey(new Keyframe(14.5239f, 0.8018f));
            foil.k1Curve.AddKey(new Keyframe(15.5148f, 0.8286f));
            foil.k1Curve.AddKey(new Keyframe(16.5058f, 0.8554f));
            foil.k1Curve.AddKey(new Keyframe(17.6206f, 0.8762f));
            foil.k1Curve.AddKey(new Keyframe(18.5187f, 0.8941f));
            foil.k1Curve.AddKey(new Keyframe(19.5097f, 0.915f));
            foil.k1Curve.AddKey(new Keyframe(20.6555f, 0.9359f));
            foil.k1Curve.AddKey(new Keyframe(21.6155f, 0.9597f));
            foil.k1Curve.AddKey(new Keyframe(22.5755f, 0.9746f));
            foil.k1Curve.AddKey(new Keyframe(23.9381f, 0.9955f));
            MathBase.LinearizeCurve(foil.k1Curve);


            // ---------------------------------- K2
            foil.k2Curve = new AnimationCurve();
            foil.k2Curve.AddKey(new Keyframe(0f, 0f));
            foil.k2Curve.AddKey(new Keyframe(2.2607f, 0.0772f));
            foil.k2Curve.AddKey(new Keyframe(3.6007f, 0.1269f));
            foil.k2Curve.AddKey(new Keyframe(5.2748f, 0.1766f));
            foil.k2Curve.AddKey(new Keyframe(7.2011f, 0.2483f));
            foil.k2Curve.AddKey(new Keyframe(9.5442f, 0.309f));
            foil.k2Curve.AddKey(new Keyframe(11.9714f, 0.3779f));
            foil.k2Curve.AddKey(new Keyframe(14.0639f, 0.4386f));
            foil.k2Curve.AddKey(new Keyframe(16.323f, 0.491f));
            foil.k2Curve.AddKey(new Keyframe(18.6653f, 0.5407f));
            foil.k2Curve.AddKey(new Keyframe(21.0916f, 0.5959f));
            foil.k2Curve.AddKey(new Keyframe(23.8518f, 0.6483f));
            foil.k2Curve.AddKey(new Keyframe(26.4446f, 0.6952f));
            foil.k2Curve.AddKey(new Keyframe(29.0375f, 0.7448f));
            foil.k2Curve.AddKey(new Keyframe(31.7972f, 0.789f));
            foil.k2Curve.AddKey(new Keyframe(34.9739f, 0.8248f));
            foil.k2Curve.AddKey(new Keyframe(37.649f, 0.8552f));
            foil.k2Curve.AddKey(new Keyframe(40.4075f, 0.8828f));
            foil.k2Curve.AddKey(new Keyframe(43.0821f, 0.9048f));
            foil.k2Curve.AddKey(new Keyframe(46.0908f, 0.9269f));
            foil.k2Curve.AddKey(new Keyframe(48.8487f, 0.9462f));
            foil.k2Curve.AddKey(new Keyframe(52.1074f, 0.96f));
            foil.k2Curve.AddKey(new Keyframe(55.1994f, 0.9793f));
            foil.k2Curve.AddKey(new Keyframe(57.7062f, 0.9903f));
            foil.k2Curve.AddKey(new Keyframe(60.0458f, 1.0014f));
            MathBase.LinearizeCurve(foil.k2Curve);


            // ---------------------------------- K3
            foil.k3Curve = new AnimationCurve();
            foil.k3Curve.AddKey(new Keyframe(0f, 0f));
            foil.k3Curve.AddKey(new Keyframe(0.0445f, 0.0648f));
            foil.k3Curve.AddKey(new Keyframe(0.0915f, 0.1296f));
            foil.k3Curve.AddKey(new Keyframe(0.1288f, 0.1782f));
            foil.k3Curve.AddKey(new Keyframe(0.1613f, 0.2245f));
            foil.k3Curve.AddKey(new Keyframe(0.1974f, 0.2708f));
            foil.k3Curve.AddKey(new Keyframe(0.236f, 0.3148f));
            foil.k3Curve.AddKey(new Keyframe(0.2841f, 0.3727f));
            foil.k3Curve.AddKey(new Keyframe(0.3226f, 0.4167f));
            foil.k3Curve.AddKey(new Keyframe(0.3611f, 0.456f));
            foil.k3Curve.AddKey(new Keyframe(0.3948f, 0.4977f));
            foil.k3Curve.AddKey(new Keyframe(0.4297f, 0.5324f));
            foil.k3Curve.AddKey(new Keyframe(0.4634f, 0.5671f));
            foil.k3Curve.AddKey(new Keyframe(0.4971f, 0.6088f));
            foil.k3Curve.AddKey(new Keyframe(0.5392f, 0.6458f));
            foil.k3Curve.AddKey(new Keyframe(0.591f, 0.6921f));
            foil.k3Curve.AddKey(new Keyframe(0.6319f, 0.7292f));
            foil.k3Curve.AddKey(new Keyframe(0.6824f, 0.7708f));
            foil.k3Curve.AddKey(new Keyframe(0.7257f, 0.8079f));
            foil.k3Curve.AddKey(new Keyframe(0.769f, 0.8472f));
            foil.k3Curve.AddKey(new Keyframe(0.8124f, 0.8773f));
            foil.k3Curve.AddKey(new Keyframe(0.8581f, 0.9097f));
            foil.k3Curve.AddKey(new Keyframe(0.9002f, 0.9375f));
            foil.k3Curve.AddKey(new Keyframe(0.9399f, 0.963f));
            foil.k3Curve.AddKey(new Keyframe(0.9976f, 0.9977f));
            MathBase.LinearizeCurve(foil.k3Curve);


            if (slat)
            {
                // ------------------------------------ dCl/dd
                foil.liftDeltaCurve = new AnimationCurve();
                foil.liftDeltaCurve.AddKey(new Keyframe(0.0125f, 0.0063f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.025f, 0.0101f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.0412f, 0.0132f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.0587f, 0.016f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.0774f, 0.0182f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.0961f, 0.0207f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.1173f, 0.0227f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.1398f, 0.0244f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.1672f, 0.026f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.1984f, 0.0279f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.2296f, 0.0294f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.259f, 0.0306f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.2883f, 0.0316f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.3183f, 0.0324f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.3482f, 0.0331f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.3757f, 0.0337f));
                foil.liftDeltaCurve.AddKey(new Keyframe(0.3994f, 0.034f));
                MathBase.LinearizeCurve(foil.liftDeltaCurve);

                // ------------------------------------- nMax
                foil.nMaxCurve = new AnimationCurve();
                foil.nMaxCurve.AddKey(new Keyframe(0f, 0.675f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0063f, 0.765f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0139f, 0.88f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0215f, 0.985f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0293f, 1.105f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0382f, 1.23f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0454f, 1.34f));
                foil.nMaxCurve.AddKey(new Keyframe(0.053f, 1.45f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0618f, 1.565f));
                foil.nMaxCurve.AddKey(new Keyframe(0.07f, 1.65f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0795f, 1.7f));
                foil.nMaxCurve.AddKey(new Keyframe(0.0902f, 1.705f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1022f, 1.675f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1158f, 1.595f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1271f, 1.485f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1394f, 1.375f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1502f, 1.275f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1603f, 1.18f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1691f, 1.1f));
                foil.nMaxCurve.AddKey(new Keyframe(0.1804f, 1f));
                foil.nMaxCurve.AddKey(new Keyframe(0.2003f, 0.815f));
                MathBase.LinearizeCurve(foil.nMaxCurve);

                // -------------------------------------- nDelta
                foil.nDeltaCurve = new AnimationCurve();
                foil.nDeltaCurve.AddKey(new Keyframe(-0.0482f, 0.9988f));
                foil.nDeltaCurve.AddKey(new Keyframe(5.0513f, 0.9953f));
                foil.nDeltaCurve.AddKey(new Keyframe(9.9183f, 1.0059f));
                foil.nDeltaCurve.AddKey(new Keyframe(14.844f, 1.0024f));
                foil.nDeltaCurve.AddKey(new Keyframe(19.0775f, 0.9318f));
                foil.nDeltaCurve.AddKey(new Keyframe(22.3273f, 0.8329f));
                foil.nDeltaCurve.AddKey(new Keyframe(25.6939f, 0.7165f));
                foil.nDeltaCurve.AddKey(new Keyframe(29.4085f, 0.5929f));
                foil.nDeltaCurve.AddKey(new Keyframe(32.0784f, 0.5012f));
                foil.nDeltaCurve.AddKey(new Keyframe(34.8647f, 0.4024f));
                MathBase.LinearizeCurve(foil.nDeltaCurve);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static AnimationCurve PlotControlEffectiveness()
        {
            AnimationCurve ƞ = new AnimationCurve();
            ƞ.AddKey(new Keyframe(0.0f, 0.8f));
            ƞ.AddKey(new Keyframe(9.862f, 0.803f));
            ƞ.AddKey(new Keyframe(14.315f, 0.769f));
            ƞ.AddKey(new Keyframe(18.244f, 0.717f));
            ƞ.AddKey(new Keyframe(20.940f, 0.652f));
            ƞ.AddKey(new Keyframe(24.246f, 0.584f));
            ƞ.AddKey(new Keyframe(29.343f, 0.527f));
            ƞ.AddKey(new Keyframe(34.509f, 0.485f));
            ƞ.AddKey(new Keyframe(40.916f, 0.446f));
            ƞ.AddKey(new Keyframe(46.608f, 0.418f));
            ƞ.AddKey(new Keyframe(54.252f, 0.385f));
            ƞ.AddKey(new Keyframe(60.650f, 0.364f));
            ƞ.AddKey(new Keyframe(69.710f, 0.338f));
            return ƞ;
        }
    }
}
