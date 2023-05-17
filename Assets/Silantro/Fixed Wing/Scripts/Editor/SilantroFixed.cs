using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Linq;
using Oyedoyin.Common;
using System.Collections;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;
using UnityEngine.Networking;
using System.Collections.Generic;


/// <summary>
/// 
/// </summary>
namespace Oyedoyin.FixedWing.Editors
{
    #region Component Editors

    #region Aerofoil

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroAerofoil))]
    public class AerofoilEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroAerofoil aerofoil;
        public SerializedProperty cellList;


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { aerofoil = (SilantroAerofoil)target; cellList = serializedObject.FindProperty("m_cells"); }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Aerofoil Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_foilType"), new GUIContent("Type"));
            if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_wingType"), new GUIContent("Wing Type"));
            }

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Functionality", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceFinish"), new GUIContent("Surface Finish"));
            if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("wingAlignment"), new GUIContent("Alignment"));
            }
            if (aerofoil.m_foilType != SilantroAerofoil.AerofoilType.Balance)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_position"), new GUIContent(" "));
                if (aerofoil.m_position == SilantroAerofoil.Position.Right)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Left Copy Management", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    if (GUILayout.Button("Create Left Copy") && aerofoil.m_left == null)
                    {
                        GameObject leftFoil = Instantiate(aerofoil.gameObject, aerofoil.transform.position, Quaternion.identity, aerofoil.transform.parent);
                        leftFoil.name = "m_left_aerofoil";
                        aerofoil.m_left = leftFoil.GetComponent<SilantroAerofoil>();
                    }

                    if (aerofoil.m_left != null)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField(" ", aerofoil.m_left.name);
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_updateData"), new GUIContent("Bind and Update"));
                    }
                }
            }

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Airfoil Component", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("foilType"), new GUIContent("Type"));
            GUILayout.Space(3f);
            if (aerofoil.foilType == SilantroAerofoil.FoilType.Conventional)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rootAirfoil"), new GUIContent("Root Airfoil"));
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tipAirfoil"), new GUIContent("Tip Airfoil"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("drawFoil"), new GUIContent("Draw Foil"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_rootSuperfoil"), new GUIContent("Root Airfoil"));
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_tipSuperfoil"), new GUIContent("Tip Airfoil"));
            }

            if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
            {
                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Vortex Lift Component", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vortexLift"), new GUIContent(" "));

                if (aerofoil.vortexLift == SilantroAerofoil.VortexLift.Consider)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Lift Percentage", (Mathf.Abs(aerofoil.m_cells[0].ΔCLvort / aerofoil.m_cells[0].CL) * 100f).ToString("0.00") + " %");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Drag Percentage", (Mathf.Abs(aerofoil.m_cells[0].ΔCDvort / aerofoil.m_cells[0].CD) * 100f).ToString("0.00") + " %");
                }
            }

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (aerofoil.m_foilType != SilantroAerofoil.AerofoilType.Balance)
            {
                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Ground Effect Component", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("groundEffect"), new GUIContent(" "));

                if (aerofoil.groundEffect == SilantroAerofoil.GroundEffectState.Consider)
                {
                    GUILayout.Space(5f);
                    if (aerofoil.m_cells[0].m_groundCorrection < 0.99f)
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Lift Increase", (1 / (Mathf.Sqrt(aerofoil.m_cells[0].m_groundCorrection)) * 100f).ToString("0.00") + " %");
                        GUILayout.Space(2f);
                        EditorGUILayout.LabelField("Drag Reduction", (aerofoil.m_cells[0].m_groundCorrection * 100f).ToString("0.00") + " %");
                    }
                }

                if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilator)
                {
                    GUILayout.Space(10f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Stabilator Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorType"), new GUIContent("Type"));

                    if (aerofoil.stabilatorType == SilantroAerofoil.StabilatorType.Coupled)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Roll Coupling Percentage", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorCouplingPercentage"), new GUIContent(" "));
                    }
                }

                if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Canard)
                {
                    GUILayout.Space(10f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Canard Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorType"), new GUIContent("Type"));

                    if (aerofoil.stabilatorType == SilantroAerofoil.StabilatorType.Coupled)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Roll Coupling Percentage", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorCouplingPercentage"), new GUIContent(" "));
                    }
                }
            }

            GUILayout.Space(40f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Switch between general shape build and per-cell build.", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            aerofoil.toolbarCellTab = GUILayout.Toolbar(aerofoil.toolbarCellTab, new string[] { "Base Configuration", "Cell(s) Configuration" });
            switch (aerofoil.toolbarCellTab)
            {
                case 0: aerofoil.currentCellTab = "Base Configuration"; break;
                case 1: aerofoil.currentCellTab = "Cell(s) Configuration"; break;
            }


            switch (aerofoil.currentCellTab)
            {
                case "Base Configuration":

                    #region Base Config

                    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                    if (aerofoil.m_foilType != SilantroAerofoil.AerofoilType.Balance)
                    {
                        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                        GUILayout.Space(30f);
                        GUI.color = silantroColor;
                        EditorGUILayout.HelpBox("Aerofoil Dimensions", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(7f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Sweep", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sweepDirection"), new GUIContent("Sweep Direction"));

                        SilantroAerofoil.Cell cell = aerofoil.m_cells[aerofoil.m_cells.Count - 1];
                        if (aerofoil.sweepDirection != SilantroAerofoil.SweepDirection.Unswept)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_sweep"), new GUIContent("Sweep Angle"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("ɅLE", cell.ᴧLE.ToString("0.00") + " °");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Ʌc/4", cell.ᴧQT.ToString("0.00") + " °");
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("sweepCorrectionMethod"), new GUIContent("Correction Method"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Correction Factor", cell.m_Kθ.ToString("0.000"));

                        }


                        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Twist", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("twistDirection"), new GUIContent("Twist Direction"));

                        if (aerofoil.twistDirection != SilantroAerofoil.TwistDirection.Untwisted)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_twist"), new GUIContent("Twist Angle"));
                        }

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Structure", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_taper"), new GUIContent("Taper %"));
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("subdivision"), new GUIContent("Subdivisions"));


                    }


                    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                    GUILayout.Space(25f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Dimensions", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(1f);
                    EditorGUILayout.LabelField("Root Chord", aerofoil.m_rootChord.ToString("0.00") + " m");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Aspect Ratio", aerofoil.m_aspectRatio.ToString("0.000"));
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Surface Area", aerofoil.m_area.ToString("0.00") + " m2");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Wetted Area", aerofoil.m_wettedArea.ToString("0.00") + " m2");
                    if (aerofoil.m_foilType != SilantroAerofoil.AerofoilType.Balance)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Draw Options", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("drawFoil"), new GUIContent("Draw Foils"));

                        if (aerofoil.drawFoil)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawSplits"), new GUIContent("Draw Rib Splits"));
                        }

                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("drawAxes"), new GUIContent("Draw Axes"));
                        if (aerofoil.drawAxes)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axisScale"), new GUIContent("Scale"));
                        }



                        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                        GUILayout.Space(25f);
                        GUI.color = silantroColor;
                        EditorGUILayout.HelpBox("Controls", MessageType.None); GUI.color = backgroundColor;
                        GUILayout.Space(7f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("controlState"), new GUIContent("State"));
                    }


                    if (aerofoil.controlState == SilantroAerofoil.ControlType.Controllable)
                    {
                        GUILayout.Space(5f);
                        if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing) { EditorGUILayout.PropertyField(serializedObject.FindProperty("availableControls"), new GUIContent(" ")); }
                        else { aerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryOnly; }


                        if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
                        {
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Available Secondary Surfaces", MessageType.None); GUI.color = backgroundColor;
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapState"), new GUIContent("Flaps"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatState"), new GUIContent("Slats"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerState"), new GUIContent("Spoiler"));
                        }

                        // ---------------------------------------------------------PRIMARY ONLY-------------------------------------------------------------------------------------------------
                        if (aerofoil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly)
                        {
                            aerofoil.toolbarStrings = new List<string>();
                            if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                            if (!aerofoil.toolbarStrings.Contains("Primary")) { aerofoil.toolbarStrings.Add("Primary"); }
                            switch (aerofoil.toolbarTab)
                            {
                                case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                            }
                        }


                        // ---------------------------------------------------------MIXED ONLY-------------------------------------------------------------------------------------------------
                        if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls == SilantroAerofoil.AvailableControls.PrimaryPlusSecondary)
                        {
                            aerofoil.toolbarStrings = new List<string>();
                            if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                            if (!aerofoil.toolbarStrings.Contains("Primary")) { aerofoil.toolbarStrings.Add("Primary"); }
                            if (aerofoil.flapState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Flap")) { aerofoil.toolbarStrings.Add("Flap"); }
                            if (aerofoil.slatState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Slat")) { aerofoil.toolbarStrings.Add("Slat"); }
                            if (aerofoil.spoilerState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Spoiler")) { aerofoil.toolbarStrings.Add("Spoiler"); }
                            GUILayout.Space(5f);
                            aerofoil.toolbarTab = GUILayout.Toolbar(aerofoil.toolbarTab, aerofoil.toolbarStrings.ToArray());
                            //REFRESH IF VALUE IS NULL
                            if (!aerofoil.toolbarStrings.Contains(aerofoil.currentTab)) { aerofoil.toolbarTab = 0; }
                            //SWITCH TABS
                            switch (aerofoil.toolbarTab)
                            {
                                case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                                case 1: aerofoil.currentTab = aerofoil.toolbarStrings[1]; break;
                                case 2: aerofoil.currentTab = aerofoil.toolbarStrings[2]; break;
                                case 3: aerofoil.currentTab = aerofoil.toolbarStrings[3]; break;
                            }
                        }

                        // ---------------------------------------------------------SECONDARY ONLY-------------------------------------------------------------------------------------------------
                        if (aerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls == SilantroAerofoil.AvailableControls.SecondaryOnly)
                        {
                            aerofoil.toolbarStrings = new List<string>();
                            if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                            if (aerofoil.flapState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Flap")) { aerofoil.toolbarStrings.Add("Flap"); }
                            if (aerofoil.slatState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Slat")) { aerofoil.toolbarStrings.Add("Slat"); }
                            if (aerofoil.spoilerState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Spoiler")) { aerofoil.toolbarStrings.Add("Spoiler"); }


                            GUILayout.Space(5f);
                            aerofoil.toolbarTab = GUILayout.Toolbar(aerofoil.toolbarTab, aerofoil.toolbarStrings.ToArray());

                            //REFRESH IF VALUE IS NULL
                            if (!aerofoil.toolbarStrings.Contains(aerofoil.currentTab)) { aerofoil.toolbarTab = 0; }

                            if (aerofoil.toolbarStrings.Count < 1) { aerofoil.currentTab = " "; aerofoil.toolbarTab = 0; }
                            if (aerofoil.toolbarStrings.Count > 0)
                            {
                                //SWITCH TABS
                                switch (aerofoil.toolbarTab)
                                {
                                    case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                                    case 1: aerofoil.currentTab = aerofoil.toolbarStrings[1]; break;
                                    case 2: aerofoil.currentTab = aerofoil.toolbarStrings[2]; break;
                                }
                            }
                        }

                        GUILayout.Space(5f);
                        switch (aerofoil.currentTab)
                        {
                            // ---------------------------------------------------------Primary Controls
                            case "Primary":
                                if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
                                {
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Primary", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceType"), new GUIContent("Surface Type"));


                                    if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive)
                                    {
                                        GUILayout.Space(5f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("controlAnalysis"), new GUIContent("Analysis Method"));

                                        if (aerofoil.controlAnalysis != SilantroAerofoil.AnalysisMethod.GeometricOnly)
                                        {
                                            GUILayout.Space(3f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlCorrectionMethod"), new GUIContent("Numeric Correction"));
                                        }


                                        GUILayout.Space(3f);
                                        GUI.color = aerofoil.m_controlColor;
                                        EditorGUILayout.HelpBox("Control Chord Ratios (xc/c)", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(5f);
                                        GUI.color = backgroundColor;
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_controlRootChord"), new GUIContent(aerofoil.surfaceType.ToString() + " Root Chord"));
                                        GUILayout.Space(5f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_controlTipChord"), new GUIContent(aerofoil.surfaceType.ToString() + " Tip Chord"));
                                        GUILayout.Space(5f);
                                        EditorGUILayout.LabelField(aerofoil.surfaceType.ToString() + " Area", aerofoil.m_controlArea.ToString("0.000") + " m2");
                                        GUILayout.Space(10f);
                                        GUI.color = aerofoil.m_controlColor;
                                        EditorGUILayout.HelpBox(aerofoil.surfaceType.ToString() + " Panels", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(5f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.BeginVertical();
                                        SerializedProperty boolsControl = serializedObject.FindProperty("m_controlSections");
                                        for (int ci = 0; ci < boolsControl.arraySize; ci++)
                                        {
                                            GUIContent labelControl = new GUIContent();
                                            if (ci == 0)
                                            {
                                                labelControl = new GUIContent("Root Panel: ");
                                            }
                                            else if (ci == boolsControl.arraySize - 1)
                                            {
                                                labelControl = new GUIContent("Tip Panel: ");
                                            }
                                            else
                                            {
                                                labelControl = new GUIContent("Panel: " + (ci + 1).ToString());
                                            }
                                            EditorGUILayout.PropertyField(boolsControl.GetArrayElementAtIndex(ci), labelControl);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.EndVertical();

                                        GUILayout.Space(10f);
                                        GUI.color = aerofoil.m_controlColor;
                                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("deflectionType"), new GUIContent("Deflection Type"));

                                        if (aerofoil.deflectionType == SilantroAerofoil.DeflectionType.Symmetric)
                                        {
                                            GUILayout.Space(5f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("c_positiveLimit"), new GUIContent("Deflection Limit"));
                                        }
                                        else
                                        {
                                            GUILayout.Space(5f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("c_positiveLimit"), new GUIContent("Positive Deflection Limit"));
                                            GUILayout.Space(3f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("c_negativeLimit"), new GUIContent("Negative Deflection Limit"));
                                        }
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Current Deflection", aerofoil.m_controlDeflection.ToString("0.00") + " °");

                                        GUILayout.Space(5f);
                                        GUI.color = aerofoil.m_controlColor;
                                        EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(5f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_controlActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_maximumControlTorque"), new GUIContent("Torque Limit (Nm)"));
                                        GUILayout.Space(3f);
                                        float currentEff = (aerofoil._currentControlEffectiveness * 100f);
                                        currentEff = Mathf.Clamp(currentEff, 0, 100f);
                                        EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                                        GUILayout.Space(3f);
                                        EditorGUILayout.CurveField("Efficiency Curve", aerofoil._controlEfficiencyCurve);
                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        float machLock = aerofoil._controlLockPoint / 343f;
                                        EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil._controlLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                                        float zeroPoint = aerofoil._controlNullPoint / 343f;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Null Speed", (aerofoil._controlNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");


                                        GUILayout.Space(15f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Trim Configuration", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("trimState"), new GUIContent("Trim State"));

                                        if (aerofoil.trimState == SilantroAerofoil.TrimState.Available)
                                        {
                                            GUILayout.Space(3f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_positiveTrimLimit"), new GUIContent("Trim Limit"));
                                            GUILayout.Space(3f);
                                            EditorGUILayout.LabelField("Trim Deflection", aerofoil._trimDeflection.ToString("0.00") + " °");
                                            GUILayout.Space(3f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_positiveTrimTabLimit"), new GUIContent("Trim Tab Limit"));
                                            GUILayout.Space(3f);
                                            EditorGUILayout.LabelField("Tab Deflection", aerofoil._trimTabDeflection.ToString("0.00") + " °");
                                        }


                                        GUILayout.Space(10f);
                                        GUI.color = aerofoil.m_controlColor;
                                        EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(5f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Primary Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryControlModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportControlModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                                break;

                            // ---------------------------------------------------------Flap Controls
                            case "Flap":
                                if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.flapState == SilantroAerofoil.ControlState.Active)
                                {
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Flap", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flapType"), new GUIContent("Flap Type"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flapAnalysis"), new GUIContent("Analysis Method"));
                                    if (aerofoil.flapAnalysis != SilantroAerofoil.AnalysisMethod.GeometricOnly)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapCorrectionMethod"), new GUIContent("Numeric Correction"));
                                    }


                                    GUILayout.Space(5f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Flap Chord Ratios (xf/c)", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = backgroundColor;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flapRootChord"), new GUIContent("Flap Root Chord :"));
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flapTipChord"), new GUIContent("Flap Tip Chord :"));

                                    GUILayout.Space(5f);
                                    EditorGUILayout.LabelField("Flap Area", aerofoil.m_flapArea.ToString("0.000") + " m2");
                                    GUILayout.Space(10f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Flap Panels", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.BeginVertical();
                                    SerializedProperty boolsflap = serializedObject.FindProperty("m_flapSections");
                                    for (int i = 0; i < boolsflap.arraySize; i++)
                                    {
                                        if (aerofoil.m_controlSections[i] != true)
                                        {
                                            GUIContent labelFlap = new GUIContent();
                                            if (i == 0)
                                            {
                                                labelFlap = new GUIContent("Root Panel: ");
                                            }
                                            else if (i == boolsflap.arraySize - 1)
                                            {
                                                labelFlap = new GUIContent("Tip Panel: ");
                                            }
                                            else
                                            {
                                                labelFlap = new GUIContent("Panel: " + (i + 1).ToString());
                                            }
                                            EditorGUILayout.PropertyField(boolsflap.GetArrayElementAtIndex(i), labelFlap);
                                        }
                                        else
                                        {
                                            if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive)
                                            {
                                                string labelFlapNote;
                                                if (i == 0) { labelFlapNote = ("Root Panel: "); }
                                                else if (i == boolsflap.arraySize - 1) { labelFlapNote = ("Tip Panel: "); }
                                                else { labelFlapNote = ("Panel: " + (i + 1).ToString()); }
                                                EditorGUILayout.LabelField(labelFlapNote, aerofoil.surfaceType.ToString());
                                            }

                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.EndVertical();

                                    GUILayout.Space(10f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Base Flap Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flapSteps"), new GUIContent("Angle Setting"));

                                    // ----------------------------------------------------------------- Flaperon
                                    if (aerofoil.flapType == SilantroAerofoil.FlapType.Flaperon || aerofoil.flapType == SilantroAerofoil.FlapType.Flapevon)
                                    {
                                        GUILayout.Space(5f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox(aerofoil.flapType.ToString() + " Configuration", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(5f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("f_positiveLimit"), new GUIContent("Positive Deflection Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("f_negativeLimit"), new GUIContent("Negative Deflection Limit"));

                                    }

                                    GUILayout.Space(3f);
                                    EditorGUILayout.LabelField("Surface Deflection", aerofoil.m_flapDeflection.ToString("0.00") + " °");

                                    GUILayout.Space(10f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_flapActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_maximumFlapTorque"), new GUIContent("Torque Limit (Nm)"));
                                    GUILayout.Space(3f);
                                    float currentEff = (aerofoil._currentFlapEffectiveness * 100f); currentEff = Mathf.Clamp(currentEff, 0, 100f);
                                    EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                                    GUILayout.Space(3f);
                                    EditorGUILayout.CurveField("Efficiency Curve", aerofoil._flapEfficiencyCurve);
                                    GUILayout.Space(3f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    float machLock = aerofoil._flapLockPoint / 343f;
                                    EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil._flapLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                                    GUILayout.Space(3f);
                                    float zeroPoint = aerofoil._flapNullPoint / 343f;
                                    EditorGUILayout.LabelField("Null Speed", (aerofoil._flapNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");

                                    GUILayout.Space(10f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);

                                    if (aerofoil.flapType != SilantroAerofoil.FlapType.Flapevon && aerofoil.flapType != SilantroAerofoil.FlapType.Flaperon)
                                    {
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapModelType"), new GUIContent("Movement Type"));
                                        GUILayout.Space(5f);

                                        if (aerofoil.flapModelType == SilantroAerofoil.ModelType.Internal)
                                        {
                                            GUILayout.Space(3f);
                                            GUI.color = Color.white;
                                            EditorGUILayout.HelpBox("Primary Model", MessageType.None);
                                            GUI.color = backgroundColor;
                                            GUILayout.Space(3f);

                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapModel"), new GUIContent(""));
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapDeflectionAxis"), new GUIContent(""));
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapDeflectionDirection"), new GUIContent(""));
                                            EditorGUILayout.EndHorizontal();


                                            GUILayout.Space(3f);
                                            GUI.color = Color.white;
                                            EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                            GUI.color = backgroundColor;
                                            GUILayout.Space(3f);
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapModel"), new GUIContent(""));
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapDeflectionAxis"), new GUIContent(""));
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapDeflectionDirection"), new GUIContent(""));
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        if (aerofoil.flapModelType == SilantroAerofoil.ModelType.Actuator)
                                        {
                                            GUI.color = Color.white;
                                            EditorGUILayout.HelpBox("Actuator Properties", MessageType.None);
                                            GUI.color = backgroundColor;
                                            GUILayout.Space(3f);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flapActuator"), new GUIContent("Flap Actuator"));
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Space(5f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Primary Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primaryFlapDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();


                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportFlapDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();
                                    }

                                    GUILayout.Space(10f);
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_flapLoop"), new GUIContent("Loop Sound"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_flapClamp"), new GUIContent("Lock Sound"));

                                }
                                break;

                            // ---------------------------------------------------------Slat Controls
                            case "Slat":
                                if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.slatState == SilantroAerofoil.ControlState.Active)
                                {
                                    GUI.color = Color.magenta;
                                    EditorGUILayout.HelpBox("Slat Chord Ratios (xs/c)", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = backgroundColor;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_slatRootChord"), new GUIContent("Slat Root Chord :"));
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_slatTipChord"), new GUIContent("Slat Tip Chord :"));


                                    GUILayout.Space(5f);
                                    EditorGUILayout.LabelField("Slat Area", aerofoil.m_slatArea.ToString("0.000") + " m2");

                                    GUILayout.Space(10f);
                                    GUI.color = Color.magenta;
                                    EditorGUILayout.HelpBox("Slat Panels", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.BeginVertical();
                                    SerializedProperty boolsSlat = serializedObject.FindProperty("m_slatSections");
                                    for (int i = 0; i < boolsSlat.arraySize; i++)
                                    {
                                        GUIContent labelSlat = new GUIContent();
                                        if (i == 0)
                                        {
                                            labelSlat = new GUIContent("Root Panel: ");
                                        }
                                        else if (i == boolsSlat.arraySize - 1)
                                        {
                                            labelSlat = new GUIContent("Tip Panel: ");
                                        }
                                        else
                                        {
                                            labelSlat = new GUIContent("Panel: " + (i + 1).ToString());
                                        }
                                        EditorGUILayout.PropertyField(boolsSlat.GetArrayElementAtIndex(i), labelSlat);
                                    }
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.Space(10f);
                                    GUI.color = Color.magenta;
                                    EditorGUILayout.HelpBox("Movement Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("slatMovement"), new GUIContent("Movement Type"));


                                    if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Deflection)
                                    {
                                        GUILayout.Space(6f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Deflection Settings", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(4f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_slatMovementLimit"), new GUIContent("Deflection Limit  °"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_slatActuationSpeed"), new GUIContent("Actuation Speed  (°/s)"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Current Deflection", aerofoil.m_slatDeflection.ToString("0.00") + " °");
                                    }
                                    //SLIDING
                                    if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Extension)
                                    {
                                        GUILayout.Space(6f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Extension Settings", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(4f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_slatMovementLimit"), new GUIContent("Extension Limit (cm)"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_slatActuationSpeed"), new GUIContent("Actuation Speed  (°/s)"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Current Extension", (aerofoil.m_slatDeflection).ToString("0.00") + " cm");
                                    }



                                    GUILayout.Space(10f);
                                    GUI.color = Color.magenta;
                                    EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("slatModelType"), new GUIContent("Movement Type"));
                                    GUILayout.Space(3f);
                                    if (aerofoil.slatModelType == SilantroAerofoil.ModelType.Internal)
                                    {
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Primary Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySlatModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySlatDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySlatDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();


                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSlatModel"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSlatDeflectionAxis"), new GUIContent(""));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSlatDeflectionDirection"), new GUIContent(""));
                                        EditorGUILayout.EndHorizontal();

                                    }
                                    if (aerofoil.slatModelType == SilantroAerofoil.ModelType.Actuator)
                                    {

                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Actuator Properties", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_slatActuator"), new GUIContent("Slat Actuator"));
                                    }

                                }
                                break;

                            // ---------------------------------------------------------Spoiler Controls
                            case "Spoiler":
                                if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.spoilerState == SilantroAerofoil.ControlState.Active)
                                {
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Spoiler", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerType"), new GUIContent("Spoiler Type"));
                                    GUILayout.Space(3f);
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Spoiler Dimensions", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = backgroundColor;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_spoilerChord"), new GUIContent("Spoiler Chord (xst/c):"));
                                    GUILayout.Space(2f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_spoilerHinge"), new GUIContent("Spoiler Hinge: "));
                                    if (aerofoil.spoilerType == SilantroAerofoil.SpoilerType.Spoileron)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerRollCoupling"), new GUIContent("Roll Coupling %"));
                                    }
                                    GUILayout.Space(5f);
                                    EditorGUILayout.LabelField("Spoiler Area", aerofoil.m_spoilerArea.ToString("0.000") + " m2");

                                    GUILayout.Space(10f);
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Spoiler Panels", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.BeginVertical();
                                    SerializedProperty boolspoilers = serializedObject.FindProperty("m_spoilerSections");
                                    for (int i = 0; i < boolspoilers.arraySize; i++)
                                    {
                                        GUIContent labelSpoiler = new GUIContent();
                                        if (i == 0)
                                        {
                                            labelSpoiler = new GUIContent("Root Panel: ");
                                        }
                                        else if (i == boolspoilers.arraySize - 1)
                                        {
                                            labelSpoiler = new GUIContent("Tip Panel: ");
                                        }
                                        else
                                        {
                                            labelSpoiler = new GUIContent("Panel: " + (i + 1).ToString());
                                        }
                                        EditorGUILayout.PropertyField(boolspoilers.GetArrayElementAtIndex(i), labelSpoiler);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.EndVertical();

                                    GUILayout.Space(10f);
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sp_positiveLimit"), new GUIContent("Deflection Limit"));
                                    GUILayout.Space(5f);
                                    EditorGUILayout.LabelField("Spoiler Deflection", aerofoil.m_spoilerDeflection.ToString("0.00") + " °");

                                    GUILayout.Space(5f);
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_spoilerActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_maximumSpoilerTorque"), new GUIContent("Torque Limit (Nm)"));
                                    GUILayout.Space(3f);
                                    float currentEff = (aerofoil._currentSpoilerEffectiveness * 100f); currentEff = Mathf.Clamp(currentEff, 0, 100f);
                                    EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                                    GUILayout.Space(3f);
                                    EditorGUILayout.CurveField("Efficiency Curve", aerofoil._spoilerEfficiencyCurve);
                                    GUILayout.Space(3f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    float machLock = aerofoil._spoilerLockPoint / 343f;
                                    EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil._spoilerLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                                    GUILayout.Space(3f);
                                    float zeroPoint = aerofoil._spoilerNullPoint / 343f;
                                    EditorGUILayout.LabelField("Null Speed", (aerofoil._spoilerNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");

                                    GUILayout.Space(5f);
                                    GUI.color = Color.cyan;
                                    EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Base Model", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySpoilerModel"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySpoilerDeflectionAxis"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_primarySpoilerDeflectionDirection"), new GUIContent(""));
                                    EditorGUILayout.EndHorizontal();

                                    GUILayout.Space(3f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSpoilerModel"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSpoilerDeflectionAxis"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportSpoilerDeflectionDirection"), new GUIContent(""));
                                    EditorGUILayout.EndHorizontal();
                                }

                                break;
                        }
                    }

                    #endregion

                    break;
                case "Cell(s) Configuration":

                    #region Cell Config

                    GUILayout.Space(3f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(" Here you can adjust the shape and control properties of each subdivision panel and get more accurate aerofoil shapes", MessageType.Info);
                    GUI.color = backgroundColor;
                    GUILayout.Space(30f);

                    if (cellList != null)
                    {
                        for (int i = 0; i < cellList.arraySize; i++)
                        {
                            SerializedProperty reference = cellList.GetArrayElementAtIndex(i);
                            SilantroAerofoil.Cell cell = aerofoil.m_cells[i];

                            GUI.color = new Color(1, 0.8f, 0);
                            EditorGUILayout.HelpBox("Cell : " + (i + 1).ToString(), MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(4f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Dimensions", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_spanFill"), new GUIContent("Span Compression (%)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_taper"), new GUIContent("Taper (%)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_sweep"), new GUIContent("Sweep"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_twist"), new GUIContent("Twist"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_dihedral"), new GUIContent("Dihedral"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Mean Chord", cell.m_meanChord.ToString("0.00") + " m");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("λ", cell.λ.ToString("0.00"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Г", cell.Г.ToString("0.00") + " °");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("θ", cell.θ.ToString("0.00") + " °");
                            //GUILayout.Space(3f);
                            //EditorGUILayout.LabelField("Oswald Efficiency", (cell.m_e * 100).ToString("0.00") + " %");


                            if (aerofoil.controlState == SilantroAerofoil.ControlType.Controllable)
                            {
                                GUILayout.Space(10f);
                                GUI.color = silantroColor;
                                EditorGUILayout.HelpBox("Controls", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Primary Controls", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_controlRootCorrection"), new GUIContent("Root Correction"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_controlTipCorrection"), new GUIContent("Tip Correction"));

                                if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
                                {
                                    if (aerofoil.flapState == SilantroAerofoil.ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Flap Controls", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_flapRootCorrection"), new GUIContent("Root Correction"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_flapTipCorrection"), new GUIContent("Tip Correction"));
                                    }

                                    if (aerofoil.slatState == SilantroAerofoil.ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Slat Controls", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_slatRootCorrection"), new GUIContent("Root Correction"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_slatTipCorrection"), new GUIContent("Tip Correction"));
                                    }

                                    if (aerofoil.spoilerState == SilantroAerofoil.ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        GUI.color = Color.white;
                                        EditorGUILayout.HelpBox("Spoiler Controls", MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_spoilerRootCorrection"), new GUIContent("Root Correction"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_spoilerTipCorrection"), new GUIContent("Tip Correction"));
                                    }
                                }
                            }

                            GUILayout.Space(10f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Output", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("α", cell.α.ToString("0.00") + " °");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("β", cell.β.ToString("0.00") + " °");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Lift", cell.m_lift.ToString("0.00") + " N");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Drag", cell.m_drag.ToString("0.00") + " N");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Moment", cell.m_moment.ToString("0.00") + " Nm");


                            GUILayout.Space(10f);
                        }
                    }

                    #endregion

                    break;
            }




            GUILayout.Space(40f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Output Data", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Lift", aerofoil.Lift.ToString("0.0") + " N");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Drag", aerofoil.Drag.ToString("0.0") + " N");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Moment", aerofoil.Moment.ToString("0.0") + " Nm");


            serializedObject.ApplyModifiedProperties();
        }
    }

    #endregion

    #region Flight Computer

    #region Editor

    [CustomEditor(typeof(FixedComputer))]
    public class FixedComputerEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        FixedComputer computer;
        SerializedProperty sas;
        SerializedProperty cas;
        SerializedProperty autopilot;
        SerializedProperty gains;
        SerializedProperty autothrottle;

        SerializedProperty rollSASLimit;
        SerializedProperty pitchSASLimit;

        SerializedProperty rollGain;
        SerializedProperty pitchGain;

        Controller m_controller;
        SerializedProperty input;
        SerializedObject m_controllerObject;
        Rect curveRect = new Rect();

        private static readonly GUIContent deleteButton = new GUIContent("Delete", "Delete");
        private static readonly GUILayoutOption buttonWidth = GUILayout.Width(60f);

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            computer = (FixedComputer)target;
            sas = serializedObject.FindProperty("m_stabilityAugmentation");
            cas = serializedObject.FindProperty("m_commandAugmentation");
            gains = serializedObject.FindProperty("m_gainSystem");
            autopilot = serializedObject.FindProperty("m_autopilot");
            autothrottle = serializedObject.FindProperty("m_throttleAugmentation");

            rollGain = gains.FindPropertyRelative("m_rollGains");
            pitchGain = gains.FindPropertyRelative("m_pitchGains");

            rollSASLimit = sas.FindPropertyRelative("m_rollAuthorityLimits");
            pitchSASLimit = sas.FindPropertyRelative("m_pitchAuthorityLimits");

            if (computer != null)
            {
                m_controller = computer.transform.gameObject.GetComponentInParent<Controller>();
                m_controllerObject = new SerializedObject(m_controller);
                input = m_controllerObject.FindProperty("m_input");
            }

            curveRect.xMin = -1;
            curveRect.xMax = 1;
            curveRect.yMin = -1;
            curveRect.yMax = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Control Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mode"), new GUIContent("Mode"));
            if (computer.m_mode == Computer.Mode.Augmented)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_augmentation"), new GUIContent(" "));
            }


            GUILayout.Space(15f);
            if (computer.m_mode == Computer.Mode.Manual)
            {
                // Manual Controls
            }
            else
            {
                if (computer.m_mode == Computer.Mode.Autonomous)
                {
                    computer.toolbarTab = GUILayout.Toolbar(computer.toolbarTab, new string[] { "Autopilot", "Gain Schedule" });

                    switch (computer.toolbarTab)
                    {
                        case 0: computer.currentTab = "Autopilot"; break;
                        case 1: computer.currentTab = "Gain Schedule"; break;
                    }

                    switch (computer.currentTab)
                    {
                        case "Autopilot":

                            GUILayout.Space(3f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Lateral Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            if (computer.m_lateralAutopilot != ControlState.Active) { computer.m_lateralAutopilot = ControlState.Active; }
                            GUILayout.Space(5f);
                            if (computer.m_lateralAutopilot == ControlState.Active)
                            {
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_lateralMode"), new GUIContent("Mode"));
                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_rollRateSolver.m_multiplier.ToString("0.000"));

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Gains", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_bankGain"), new GUIContent("Bank Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_headingGain"), new GUIContent("Heading Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_turnGain"), new GUIContent("Turn Gain"));
                            }

                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Longitudinal Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            if (computer.m_longitudinalAutopilot != ControlState.Active) { computer.m_longitudinalAutopilot = ControlState.Active; }
                            GUILayout.Space(5f);
                            if (computer.m_longitudinalAutopilot == ControlState.Active)
                            {

                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_longitudinalMode"), new GUIContent("Mode"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_takeoffSpeed"), new GUIContent("Takeoff Speed (kts)"));

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_pitchRateSolver.m_multiplier.ToString("0.000"));

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Gains", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchGain"), new GUIContent("Pitch Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_climbGain"), new GUIContent("Climb Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_climbGainWashout"), new GUIContent("Climb Gain Washout"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeGain"), new GUIContent("Altitude Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeGainWashout"), new GUIContent("Altitude Gain Washout"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeIntegral"), new GUIContent("Altitude Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeWindup"), new GUIContent("Altitude Windup"));
                            }

                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Directional Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_yawRateSolver.m_multiplier.ToString("0.000"));
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Ground Yaw Solver", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_groundYawGain"), new GUIContent("Yaw Gain"));


                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Power Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_autoThrottle"), new GUIContent("AutoThrottle"));

                            if (computer.m_autoThrottle == ControlState.Active)
                            {
                                string engineType = "UnKnown";
                                if (m_controller != null) { engineType = m_controller.m_engineType.ToString(); }
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Engine Type", engineType);

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Throttle Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));

                                if (m_controller == null || m_controller != null && m_controller.m_engineType == Controller.EngineType.Piston)
                                {
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Mixture Solver", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));


                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Propeller Pitch Solver", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Limits", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_ratedRPM"), new GUIContent("Rated RPM"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_ratedAF"), new GUIContent("Rate AF"));
                                }
                            }


                            GUILayout.Space(20f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Limits", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchRate"), new GUIContent("Maximum Pitch Rate"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollRate"), new GUIContent("Maximum Roll Rate"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumYawRate"), new GUIContent("Maximum Yaw Rate"));

                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchAngle"), new GUIContent("Maximum Pitch Angle"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumPitchAngle"), new GUIContent("Minimum Pitch Angle"));

                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTurnBank"), new GUIContent("Maximum Turn Bank"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Maximum Turn Rate", computer.maximumTurnRate.ToString("0.000") + " °/s");


                            break;
                        case "Gain Schedule":

                            GUILayout.Space(3f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Schedule Mode", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_state"), new GUIContent("State"));

                            if (computer.m_gainSystem.m_state == Computer.GainSystem.GainState.Dynamic)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Lateral Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_rollGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_rollGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (rollGain != null)
                                {
                                    for (int i = 0; i < rollGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = rollGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Lateral Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_rollGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_rollRate"), new GUIContent("Gain Curve"));


                                GUILayout.Space(15f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Longitudinal Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_pitchGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_pitchGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (pitchGain != null)
                                {
                                    for (int i = 0; i < pitchGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = pitchGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Longitudinal Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_pitchGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_pitchRate"), new GUIContent("Gain Curve"));
                            }
                            break;
                    }
                }
                if (computer.m_mode == Computer.Mode.Augmented && computer.m_augmentation == FixedComputer.AugmentationType.CommandAugmentation)
                {
                    computer.toolbarTab = GUILayout.Toolbar(computer.toolbarTab, new string[] { "Command Augmentation", "Autopilot", "Gain Schedule" });

                    switch (computer.toolbarTab)
                    {
                        case 0: computer.currentTab = "Command Augmentation"; break;
                        case 1: computer.currentTab = "Autopilot"; break;
                        case 2: computer.currentTab = "Gain Schedule"; break;
                    }

                    switch (computer.currentTab)
                    {
                        case "Command Augmentation":

                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Command Augmentation", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(cas.FindPropertyRelative("m_commandPreset"), new GUIContent("Command Preset"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Gain State", computer.m_gainState.ToString());

                            GUILayout.Space(10f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Roll Performance ", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(cas.FindPropertyRelative("m_maximumRollRate"), new GUIContent("Maximum Rate (°/s)"));
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_rollRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_rollRateSolver.m_multiplier.ToString("0.000"));

                            if (computer.m_commandAugmentation.m_commandPreset == FixedComputer.CommandAugmentation.CommandPreset.Airliner)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Bank Limiter", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(2f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("bankLimiter"), new GUIContent("State"));
                                if (computer.bankLimiter == ControlState.Active)
                                {
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTurnBank"), new GUIContent("Maximum Bank"));
                                    GUILayout.Space(5f);
                                    EditorGUILayout.PropertyField(cas.FindPropertyRelative("rollBreakPoint"), new GUIContent("Roll Break Point"));
                                    GUILayout.Space(10f);
                                }
                            }

                            GUILayout.Space(10f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Pitch Performance", MessageType.Info);
                            GUI.color = backgroundColor;
                            if (computer.gLimiter != ControlState.Active) { computer.gLimiter = ControlState.Active; }

                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumLoadFactor"), new GUIContent("Maximum +G"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumLoadFactor"), new GUIContent("Maximum -G"));

                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_pitchRateSolver.m_multiplier.ToString("0.000"));

                            GUILayout.Space(8f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("G Warner", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("gWarner"), new GUIContent(" "));
                            if (computer.gWarner == ControlState.Active)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gClip"), new GUIContent("Warning Tone"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gAlarmVolume"), new GUIContent("Warner Volume"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("G Threshold", computer.gThreshold.ToString() + " G");
                            }
                            GUILayout.Space(10f);


                            GUILayout.Space(20f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Structural Functions", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Flap Control", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flapControl"), new GUIContent("Flap State"));
                            if (computer.m_flapControl == ControlMode.Automatic)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(cas.FindPropertyRelative("m_flapLimit"), new GUIContent("Deflection Limit"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Flap Command", computer.m_commandAugmentation.m_flapCommand.ToString() + " °");
                            }

                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Slat Control", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_autoSlat"), new GUIContent("Auto Slat"));
                            if (computer.m_autoSlat == ControlState.Active)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Slat Command", computer.m_commandAugmentation.m_slatCommand.ToString() + " °");
                            }

                            break;

                        case "Autopilot":
                            #region Autopilot

                            GUILayout.Space(3f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Lateral Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_lateralAutopilot"), new GUIContent("State"));

                            if (computer.m_lateralAutopilot == ControlState.Active)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_lateralMode"), new GUIContent("Mode"));

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Gains", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_bankGain"), new GUIContent("Bank Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_headingGain"), new GUIContent("Heading Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_turnGain"), new GUIContent("Turn Gain"));
                            }

                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Longitudinal Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_longitudinalAutopilot"), new GUIContent("State"));

                            if (computer.m_longitudinalAutopilot == ControlState.Active)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_longitudinalMode"), new GUIContent("Mode"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_takeoffSpeed"), new GUIContent("Takeoff Speed (kts)"));



                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Gains", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_pitchGain"), new GUIContent("Pitch Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_climbGain"), new GUIContent("Climb Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_climbGainWashout"), new GUIContent("Climb Gain Washout"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeGain"), new GUIContent("Altitude Gain"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeIntegral"), new GUIContent("Altitude Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_altitudeWindup"), new GUIContent("Altitude Windup"));
                            }

                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Directional Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_yawRateSolver.m_multiplier.ToString("0.000"));


                            GUILayout.Space(15f);
                            GUI.color = Color.yellow;
                            EditorGUILayout.HelpBox("Power Axis", MessageType.Info);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_autoThrottle"), new GUIContent("AutoThrottle"));

                            if (computer.m_autoThrottle == ControlState.Active)
                            {
                                string engineType = "UnKnown";
                                if (m_controller != null) { engineType = m_controller.m_engineType.ToString(); }
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Engine Type", engineType);

                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Throttle Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_throttleSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));

                                if (m_controller == null || m_controller != null && m_controller.m_engineType == Controller.EngineType.Piston)
                                {
                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Mixture Solver", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_mixtureSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));


                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Propeller Pitch Solver", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_pitchSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Limits", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_ratedRPM"), new GUIContent("Rated RPM"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(autothrottle.FindPropertyRelative("m_ratedAF"), new GUIContent("Rate AF"));
                                }
                            }


                            GUILayout.Space(20f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Limits", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchRate"), new GUIContent("Maximum Pitch Rate"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollRate"), new GUIContent("Maximum Roll Rate"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumYawRate"), new GUIContent("Maximum Yaw Rate"));

                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchAngle"), new GUIContent("Maximum Pitch Angle"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumPitchAngle"), new GUIContent("Minimum Pitch Angle"));

                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTurnBank"), new GUIContent("Maximum Turn Bank"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Maximum Turn Rate", computer.maximumTurnRate.ToString("0.000") + " °/s");

                            #endregion
                            break;

                        case "Gain Schedule":

                            GUILayout.Space(3f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Schedule Mode", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_state"), new GUIContent("State"));

                            if (computer.m_gainSystem.m_state == Computer.GainSystem.GainState.Dynamic)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Lateral Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_rollGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_rollGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (rollGain != null)
                                {
                                    for (int i = 0; i < rollGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = rollGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Lateral Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_rollGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_rollRate"), new GUIContent("Gain Curve"));


                                GUILayout.Space(15f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Longitudinal Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_pitchGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_pitchGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (pitchGain != null)
                                {
                                    for (int i = 0; i < pitchGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = pitchGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Longitudinal Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_pitchGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_pitchRate"), new GUIContent("Gain Curve"));
                            }
                            break;
                    }

                }
                if (computer.m_mode == Computer.Mode.Augmented && computer.m_augmentation == FixedComputer.AugmentationType.StabilityAugmentation)
                {
                    computer.toolbarTab = GUILayout.Toolbar(computer.toolbarTab, new string[] { "Stability Augmentation", "Gain Schedule" });

                    switch (computer.toolbarTab)
                    {
                        case 0: computer.currentTab = "Stability Augmentation"; break;
                        case 1: computer.currentTab = "Gain Schedule"; break;
                    }

                    switch (computer.currentTab)
                    {
                        case "Stability Augmentation":
                            // ------------------------------------------------------------------------------------ Stability Augmentation
                            GUI.color = silantroColor;
                            if (computer.m_augmentation == FixedComputer.AugmentationType.StabilityAugmentation)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.HelpBox("Stability Augmentation Configuration", MessageType.None);
                                GUI.color = backgroundColor;

                                GUILayout.Space(3f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Lateral Axis", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_rollSAS"), new GUIContent("Roll SAS"));
                                if (computer.m_stabilityAugmentation.m_rollSAS == ControlState.Active)
                                {
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_rollSASLimit"), new GUIContent("Limit"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δa.ToString("0.0000"));

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Roll Leveler", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_rollLeveler"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollLeveler == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kф"), new GUIContent("Kф"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(rollSASLimit.FindPropertyRelative("m_leveler"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δaф.ToString("0.0000"));
                                    }

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Roll Rate Limiter", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_rollRateLimiter"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollRateLimiter == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kp"), new GUIContent("Kp"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(rollSASLimit.FindPropertyRelative("m_rateLimiter"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δap.ToString("0.0000"));
                                    }

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Roll Attitude Rate Limiter", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_rollAttitudeRateLimiter"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollAttitudeRateLimiter == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kδф"), new GUIContent("Kδф"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(rollSASLimit.FindPropertyRelative("m_attitudeRateLimiter"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δaδф.ToString("0.0000"));
                                    }
                                }

                                GUILayout.Space(15f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Longitudinal Axis", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_pitchSAS"), new GUIContent("Pitch SAS"));
                                if (computer.m_stabilityAugmentation.m_pitchSAS == ControlState.Active)
                                {
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_pitchSASLimit"), new GUIContent("Limit"));
                                    GUILayout.Space(3f);
                                    EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δa.ToString("0.0000"));

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Pitch Leveler", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_pitchLeveler"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollLeveler == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kθ"), new GUIContent("Kθ"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(pitchSASLimit.FindPropertyRelative("m_leveler"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δeθ.ToString("0.0000"));
                                    }

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Pitch Rate Limiter", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_pitchRateLimiter"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollRateLimiter == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kq"), new GUIContent("Kq"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(pitchSASLimit.FindPropertyRelative("m_rateLimiter"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δeq.ToString("0.0000"));
                                    }

                                    GUILayout.Space(5f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Pitch Attitude Rate Limiter", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_pitchAttitudeRateLimiter"), new GUIContent("State"));

                                    if (computer.m_stabilityAugmentation.m_rollAttitudeRateLimiter == ControlState.Active)
                                    {
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(sas.FindPropertyRelative("Kδθ"), new GUIContent("Kδθ"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.PropertyField(pitchSASLimit.FindPropertyRelative("m_attitudeRateLimiter"), new GUIContent("Limit"));
                                        GUILayout.Space(3f);
                                        EditorGUILayout.LabelField("Output", computer.m_stabilityAugmentation.δeδθ.ToString("0.0000"));
                                    }
                                }

                                GUILayout.Space(15f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Directional Axis", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Rate Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kp"), new GUIContent("Proportional"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Ki"), new GUIContent("Integral"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(autopilot.FindPropertyRelative("m_yawRateSolver").FindPropertyRelative("m_Kd"), new GUIContent("Derivative"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Multiplier", computer.m_autopilot.m_yawRateSolver.m_multiplier.ToString("0.000"));
                                GUILayout.Space(5f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Ground Yaw Solver", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(sas.FindPropertyRelative("m_groundYawGain"), new GUIContent("Yaw Gain"));
                            }
                            break;

                        case "Gain Schedule":

                            GUILayout.Space(3f);
                            GUI.color = silantroColor;
                            EditorGUILayout.HelpBox("Schedule Mode", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_state"), new GUIContent("State"));

                            if (computer.m_gainSystem.m_state == Computer.GainSystem.GainState.Dynamic)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Lateral Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_rollGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_rollGains[computer.m_gainSystem.m_rollGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_rollGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (rollGain != null)
                                {
                                    for (int i = 0; i < rollGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = rollGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Lateral Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_rollGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_rollRate"), new GUIContent("Gain Curve"));


                                GUILayout.Space(15f);
                                GUI.color = Color.yellow;
                                EditorGUILayout.HelpBox("Longitudinal Gains", MessageType.Info);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                if (GUILayout.Button("Add Gain Point"))
                                {
                                    float speed = 100;
                                    float factor = 1;
                                    if (computer.m_gainSystem.m_pitchGains.Count > 0)
                                    {
                                        speed = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].speed + 50;
                                        factor = computer.m_gainSystem.m_pitchGains[computer.m_gainSystem.m_pitchGains.Count - 1].factor - 0.1f;
                                        if (speed < 0) { speed = 0; }
                                        if (factor < 0) { factor = 0; }
                                    }
                                    computer.m_gainSystem.m_pitchGains.Add(new Gain(speed, factor));
                                }
                                GUILayout.Space(5f);

                                if (pitchGain != null)
                                {
                                    for (int i = 0; i < pitchGain.arraySize; i++)
                                    {
                                        SerializedProperty reference = pitchGain.GetArrayElementAtIndex(i);
                                        GUI.color = new Color(1, 0.8f, 0);
                                        EditorGUILayout.HelpBox("Longitudinal Gain : " + (i + 1).ToString(), MessageType.None);
                                        GUI.color = backgroundColor;
                                        GUILayout.Space(3f);
                                        GUILayout.BeginHorizontal("box");
                                        EditorGUILayout.PropertyField(reference, new GUIContent(" "));
                                        GUILayout.Space(5f);
                                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth)) { computer.m_gainSystem.m_pitchGains.RemoveAt(i); }
                                        GUILayout.Space(3f);
                                        GUILayout.EndHorizontal();
                                    }
                                }

                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(gains.FindPropertyRelative("m_pitchRate"), new GUIContent("Gain Curve"));
                            }

                            break;
                    }

                }
            }

            if (computer.m_mode != Computer.Mode.Autonomous)
            {
                m_controller.m_input.PlotInputCurves();

                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Input Tuning", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Pitch", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_pitchDeadZone"), new GUIContent("Dead Zone"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_pitchScale"), new GUIContent("Curvature"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_pitchTrimDelta"), new GUIContent("Trim Step"));
                GUILayout.Space(3f);
                EditorGUILayout.CurveField("Curve", m_controller.m_input._pitchInputCurve, Color.green, curveRect, GUILayout.Height(100));

                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Roll", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_rollDeadZone"), new GUIContent("Dead Zone"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_rollScale"), new GUIContent("Curvature"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_rollTrimDelta"), new GUIContent("Trim Step"));
                GUILayout.Space(3f);
                EditorGUILayout.CurveField("Curve", m_controller.m_input._rollInputCurve, Color.green, curveRect, GUILayout.Height(100));


                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Yaw", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_yawDeadZone"), new GUIContent("Dead Zone"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_yawScale"), new GUIContent("Curvature"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("_yawTrimDelta"), new GUIContent("Trim Step"));
                GUILayout.Space(3f);
                EditorGUILayout.CurveField("Curve", m_controller.m_input._yawInputCurve, Color.green, curveRect, GUILayout.Height(100));


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Alerts", MessageType.None);
                GUI.color = backgroundColor;

                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("LoadFactor Alert", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gWarner"), new GUIContent("State"));

                if (computer.gWarner == ControlState.Active)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gClip"), new GUIContent("Warning Tone"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gAlarmVolume"), new GUIContent("Warner Volume"));
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("G Threshold", computer.gThreshold.ToString() + " G");
                }
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Stall Alert", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stallWarner"), new GUIContent("State"));
                if (computer.stallWarner == ControlState.Active)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_stallClip"), new GUIContent("Warning Tone"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_alarmVolume"), new GUIContent("Warner Volume"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("alphaThreshold"), new GUIContent("Stall Threshold (°)"));
                    GUILayout.Space(3f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Stall Alarm will not sound below these altitude and speed values", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumStallSpeed"), new GUIContent("Minimum Trigger Speed (knots)"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumStallAltitude"), new GUIContent("Minimum Trigger Altitude (ft)"));

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Base Stall α", computer.baseStallAngle.ToString("0.00") + " °");
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("Alpha Floor", computer.alphaFloor.ToString("0.00") + " °" + " (Alarm Point)");
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("Alpha Prot", computer.alphaProt.ToString("0.00") + " °");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Curren α Max", computer.maximumWingAlpha.ToString("0.00") + " °");
                    GUILayout.Space(10f);
                }
            }

            m_controllerObject.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }
    }

    #endregion

    #endregion

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public class FixedElements
    {
        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Wing", false, 3000)]
        private static void AddWing()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = new GameObject("m_right_wing");
                wing.transform.parent = Selection.activeGameObject.transform;
                wing.transform.localPosition = Vector3.zero;
            }
            else
            {
                wing = new GameObject("m_right_wing");
                GameObject parent = new GameObject("m_dynamics");
                wing.transform.parent = parent.transform;
            }

            SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil>();
            wingAerofoil.subdivision = 5;
            wingAerofoil.m_foilType = SilantroAerofoil.AerofoilType.Wing;
            wingAerofoil.m_position = SilantroAerofoil.Position.Right;
            SilantroAirfoil foil = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Airfoils/Wing/NACA 0015.asset", typeof(SilantroAirfoil));
            wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
            wingAerofoil.tipAirfoil = foil;
            wingAerofoil.rootAirfoil = foil;
            wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
            wingAerofoil.drawFoil = true;

            if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
            { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
            for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }

            wingAerofoil.m_controlRootChord = 30;
            wingAerofoil.m_controlTipChord = 30;

            EditorSceneManager.MarkSceneDirty(wing.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Vertical Stabilizer", false, 3100)]
        private static void AddRudder()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = new GameObject("m_vertical_stabilizer");
                wing.transform.parent = Selection.activeGameObject.transform; wing.transform.localPosition = Vector3.zero;
            }
            else
            {
                wing = new GameObject("m_vertical_stabilizer");
                GameObject parent = new GameObject("m_dynamics"); wing.transform.parent = parent.transform;
            }

            wing.transform.rotation = Quaternion.Euler(0, 0, 90);

            SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil>();
            wingAerofoil.subdivision = 4;
            wingAerofoil.m_foilType = SilantroAerofoil.AerofoilType.Stabilizer;
            wingAerofoil.m_position = SilantroAerofoil.Position.Right;
            SilantroAirfoil foil = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Airfoils/Wing/NACA 0010.asset", typeof(SilantroAirfoil));
            wingAerofoil.rootAirfoil = foil;
            wingAerofoil.tipAirfoil = foil;
            wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
            wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;
            wingAerofoil.drawFoil = true;

            if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
            { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
            for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }

            wingAerofoil.m_controlRootChord = 30;
            wingAerofoil.m_controlTipChord = 30;

            EditorSceneManager.MarkSceneDirty(wing.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Horizontal Stabilizer", false, 3200)]
        private static void AddTail()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = new GameObject("m_horizontal_stabilizer");
                wing.transform.parent = Selection.activeGameObject.transform;
                wing.transform.localPosition = Vector3.zero;

            }
            else
            {
                wing = new GameObject("m_horizontal_stabilizer");
                GameObject parent = new GameObject("m_dynamics");
                wing.transform.parent = parent.transform;
            }

            SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil>();
            wingAerofoil.subdivision = 4;
            wingAerofoil.m_foilType = SilantroAerofoil.AerofoilType.Stabilizer;
            wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
            wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
            SilantroAirfoil foil = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Airfoils/Wing/NACA 0010.asset", typeof(SilantroAirfoil));
            wingAerofoil.tipAirfoil = foil;
            wingAerofoil.rootAirfoil = foil;
            wingAerofoil.drawFoil = true;

            if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
            { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
            for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }

            wingAerofoil.m_controlRootChord = 30;
            wingAerofoil.m_controlTipChord = 30;

            EditorSceneManager.MarkSceneDirty(wing.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Stationary/Stabilizer", false, 3300)]
        private static void AddStationaryTail()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = new GameObject("Default Stationary Stabilizer");
                wing.transform.parent = Selection.activeGameObject.transform;
                wing.transform.localPosition = Vector3.zero;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
            }
            else
            {
                wing = new GameObject("Default Stationary Stabilizer");
                GameObject parent = new GameObject("m_dynamics");
                wing.transform.parent = parent.transform;
            }
            SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil>();
            wingAerofoil.subdivision = 4;
            wingAerofoil.m_foilType = SilantroAerofoil.AerofoilType.Stabilizer;
            SilantroAirfoil foil = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Airfoils/Wing/NACA 0010.asset", typeof(SilantroAirfoil));
            wingAerofoil.tipAirfoil = foil;
            wingAerofoil.rootAirfoil = foil;
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Stationary/Wing", false, 3400)]
        private static void AddStationaryWing()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = new GameObject("Balance Center Wing");
                wing.transform.parent = Selection.activeGameObject.transform;
                wing.transform.localPosition = Vector3.zero;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
            }
            else
            {
                wing = new GameObject("Balance Center Wing");
                GameObject parent = new GameObject("m_dynamics");
                wing.transform.parent = parent.transform;
            }
            SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil>();
            wingAerofoil.subdivision = 4;
            wingAerofoil.m_foilType = SilantroAerofoil.AerofoilType.Wing;
            SilantroAirfoil foil = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Airfoils/Wing/NACA 0010.asset", typeof(SilantroAirfoil));
            wingAerofoil.tipAirfoil = foil;
            wingAerofoil.rootAirfoil = foil;
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Ruddervator", false, 3500)]
        private static void AddRuddervator()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = Selection.activeGameObject;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Ruddervator;

                    if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
                    { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType != SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Ruddervator can only be used on a Stabilizer!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Flaperon", false, 3600)]
        private static void AddFlaperon()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = Selection.activeGameObject;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryPlusSecondary;
                    wingAerofoil.flapState = SilantroAerofoil.ControlState.Active;
                    wingAerofoil.flapType = SilantroAerofoil.FlapType.Flaperon;
                    AudioClip flock = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Flaps/Flaps Lock.wav", typeof(AudioClip));
                    AudioClip floop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Flaps/Flaps Loop.wav", typeof(AudioClip));
                    wingAerofoil._flapLoop = floop;
                    wingAerofoil._flapClamp = flock;

                    if (wingAerofoil.m_flapSections == null || wingAerofoil.subdivision != wingAerofoil.m_flapSections.Length)
                    { wingAerofoil.m_flapSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_flapSections.Length; i++) { wingAerofoil.m_flapSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType != SilantroAerofoil.AerofoilType.Wing)
                {
                    Debug.Log("Flaperon can only be used on a wing!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Elevon", false, 3700)]
        private static void AddElevon()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                wing = Selection.activeGameObject;
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevon;

                    if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
                    { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType != SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Elevon can only be used to control the Stabilizer!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Aileron", false, 3800)]
        private static void AddAileron()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                wing = Selection.activeGameObject;
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();

                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;

                    if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
                    { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Aileron can only be used to control the Wing!!!, Add Elevator or Rudder to the Tail");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Elevator", false, 3900)]
        private static void AddElevator()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                wing = Selection.activeGameObject;
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;

                    if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
                    { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    Debug.Log("Elevator can only be used to control the Stabilizer!!!, Add Aileron or Flap to the Wing");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Rudder", false, 4000)]
        private static void AddRudderControl()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = Selection.activeGameObject;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;

                    if (wingAerofoil.m_controlSections == null || wingAerofoil.subdivision != wingAerofoil.m_controlSections.Length)
                    { wingAerofoil.m_controlSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_controlSections.Length; i++) { wingAerofoil.m_controlSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    Debug.Log("Rudder can only be used to control the Stabilizer!!!, Add Aileron or Flap to the Wing");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Flaps", false, 4100)]
        private static void AddFlaps()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                wing = Selection.activeGameObject;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.flapState = SilantroAerofoil.ControlState.Active;
                    wingAerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryPlusSecondary;

                    if (wingAerofoil.m_flapSections == null || wingAerofoil.subdivision != wingAerofoil.m_flapSections.Length)
                    { wingAerofoil.m_flapSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_flapSections.Length; i++) { wingAerofoil.m_flapSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Flap can only be used to control the Wing!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Slats", false, 4200)]
        private static void AddSlats()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                wing = Selection.activeGameObject;
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.slatState = SilantroAerofoil.ControlState.Active;
                    wingAerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryPlusSecondary;

                    if (wingAerofoil.m_slatSections == null || wingAerofoil.subdivision != wingAerofoil.m_slatSections.Length)
                    { wingAerofoil.m_slatSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_slatSections.Length; i++) { wingAerofoil.m_slatSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Slats can only be used to control the Wing!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Spoilers", false, 4300)]
        private static void AddSpoilers()
        {
            GameObject wing;
            if (Selection.activeGameObject != null)
            {
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                wing = Selection.activeGameObject;
                SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil>();
                if (wingAerofoil != null && wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingAerofoil.controlState = SilantroAerofoil.ControlType.Controllable;
                    wingAerofoil.spoilerState = SilantroAerofoil.ControlState.Active;
                    wingAerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryPlusSecondary;

                    if (wingAerofoil.m_spoilerSections == null || wingAerofoil.subdivision != wingAerofoil.m_spoilerSections.Length)
                    { wingAerofoil.m_spoilerSections = new bool[wingAerofoil.subdivision]; }
                    for (int i = 0; i < wingAerofoil.m_spoilerSections.Length; i++) { wingAerofoil.m_spoilerSections[i] = true; }
                }
                else if (wingAerofoil == null)
                {
                    Debug.Log("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
                }
                else if (wingAerofoil.m_foilType == SilantroAerofoil.AerofoilType.Stabilizer)
                {
                    Debug.Log("Spoilers can only be used to control the Wing!!!");
                }
            }
            else
            {
                Debug.Log("Please select a foil gameObject and try again!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Reaction/TurboJet Engine", false, 4400)]
        private static void AddTurboJetEngine()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject thruster = new GameObject { name = "m_thruster" };
                thruster.transform.parent = Selection.activeGameObject.transform;
                thruster.transform.localPosition = new Vector3(0, 0, -2);
                EditorSceneManager.MarkSceneDirty(thruster.scene);

                GameObject fan = new GameObject { name = "m_fan" };
                fan.transform.parent = Selection.activeGameObject.transform;
                fan.transform.localPosition = new Vector3(0, 0, 2);
                Selection.activeGameObject.name = "Default TurboJet Engine";

                GameObject effects = new GameObject("m_effects");
                effects.transform.parent = Selection.activeGameObject.transform;
                effects.transform.localPosition = new Vector3(0, 0, -2);

                GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Effects/Engine/Exhaust Smoke.prefab", typeof(GameObject));
                GameObject smokeEffect = Object.Instantiate(smoke, effects.transform.position, Quaternion.Euler(0, -180, 0), effects.transform);

                SilantroTurbojet jet = Selection.activeGameObject.AddComponent<SilantroTurbojet>();
                jet.exitPoint = thruster.transform;
                jet.intakePoint = fan.transform;

                AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Startup.wav", typeof(AudioClip));
                AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Shutdown.wav", typeof(AudioClip));
                AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Rear Idle.wav", typeof(AudioClip));
                AudioClip intake = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Intake Filtered.wav", typeof(AudioClip));
                AudioClip side = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Side Idle.wav", typeof(AudioClip));


                jet.core = new Common.Components.EngineCore
                {
                    backIdle = run,
                    ignitionExterior = start,
                    shutdownExterior = stop,
                    sideIdle = side,
                    frontIdle = intake,
                    exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>()
                };
                jet.core.soundMode = Common.Components.EngineCore.SoundMode.Advanced;
            }
            else
            {
                Debug.Log("Please Select GameObject to add Engine to..");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Reaction/TurboFan Engine", false, 4500)]
        private static void AddTurboFanEngine()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject thruster = new GameObject { name = "m_thruster" };
                thruster.transform.parent = Selection.activeGameObject.transform;
                thruster.transform.localPosition = new Vector3(0, 0, -2);
                EditorSceneManager.MarkSceneDirty(thruster.scene);
                GameObject fan = new GameObject { name = "m_fan" };
                fan.transform.parent = Selection.activeGameObject.transform;
                fan.transform.localPosition = new Vector3(0, 0, 2);
                Selection.activeGameObject.name = "Default TurboFan Engine";

                GameObject effects = new GameObject("m_effects");
                effects.transform.parent = Selection.activeGameObject.transform;
                effects.transform.localPosition = new Vector3(0, 0, -2);

                GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Effects/Engine/Exhaust Smoke.prefab", typeof(GameObject));
                GameObject smokeEffect = Object.Instantiate(smoke, effects.transform.position, Quaternion.Euler(0, -180, 0), effects.transform);
                SilantroTurbofan jet = Selection.activeGameObject.AddComponent<SilantroTurbofan>();
                jet.exitPoint = thruster.transform;
                jet.intakePoint = fan.transform;
                jet.engineType = SilantroTurbofan.EngineType.Mixed;

                AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Startup.wav", typeof(AudioClip));
                AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Shutdown.wav", typeof(AudioClip));
                AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Rear Idle.wav", typeof(AudioClip));
                AudioClip intake = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Intake Filtered.wav", typeof(AudioClip));
                AudioClip side = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Side Idle.wav", typeof(AudioClip));

                jet.core = new Common.Components.EngineCore
                {
                    backIdle = run,
                    ignitionExterior = start,
                    shutdownExterior = stop,
                    sideIdle = side,
                    frontIdle = intake,
                    exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>()
                };
                jet.core.soundMode = Common.Components.EngineCore.SoundMode.Advanced;
            }
            else
            {
                Debug.Log("Please Select GameObject to add Engine to..");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Drive/TurboProp Engine", false, 4600)]
        private static void AddTurboPropEngine()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject thruster = new GameObject { name = "m_thruster" };
                thruster.transform.parent = Selection.activeGameObject.transform;
                thruster.transform.localPosition = new Vector3(0, 0, -1);
                EditorSceneManager.MarkSceneDirty(thruster.scene);

                SilantroTurboprop prop = Selection.activeGameObject.AddComponent<SilantroTurboprop>();
                prop.exitPoint = thruster.transform;
                Selection.activeGameObject.name = "Default TurboProp Engine";
                GameObject Props = new GameObject("m_propeller");
                Props.transform.parent = Selection.activeGameObject.transform;
                SilantroPropeller blade = Props.AddComponent<SilantroPropeller>();
                //blade.engineType = SilantroPropeller.EngineType.TurbopropEngine;
                //blade.propEngine = prop;

                GameObject effects = new GameObject("m_effects");
                effects.transform.parent = Selection.activeGameObject.transform;
                effects.transform.localPosition = new Vector3(0, 0, -2);
                GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Effects/Engine/Exhaust Smoke.prefab", typeof(GameObject));
                GameObject smokeEffect = Object.Instantiate(smoke, effects.transform.position, Quaternion.Euler(0, -180, 0), effects.transform);

                AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Startup.wav", typeof(AudioClip));
                AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Shutdown.wav", typeof(AudioClip));
                AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Fixed Wing/Sounds/Jet/Mixed/Exterior/Exterior Side Idle.wav", typeof(AudioClip));
                prop.core = new Common.Components.EngineCore
                {
                    exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>(),
                    ignitionExterior = start,
                    shutdownExterior = stop,
                    backIdle = run
                };
            }
            else
            {
                Debug.Log("Please Select GameObject to add Engine to..");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Drive/Piston Engine", false, 4700)]
        private static void AddPistonEngine()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject thruster = new GameObject { name = "m_thruster" };
                thruster.transform.parent = Selection.activeGameObject.transform;
                thruster.transform.localPosition = new Vector3(0, 0, -1);
                EditorSceneManager.MarkSceneDirty(thruster.scene);

                SilantroPiston prop = Selection.activeGameObject.AddComponent<SilantroPiston>();
                prop.exitPoint = thruster.transform;
                Selection.activeGameObject.name = "Default Piston Engine";
                GameObject Props = new GameObject("m_propeller");

                Props.transform.parent = Selection.activeGameObject.transform;
                SilantroPropeller blade = Props.AddComponent<SilantroPropeller>();
                //blade.engineType = SilantroPropeller.EngineType.PistonEngine;
                //blade.pistonEngine = prop;

                GameObject effects = new GameObject("Engine Effects");
                effects.transform.parent = Selection.activeGameObject.transform;
                effects.transform.localPosition = new Vector3(0, 0, -2);
                GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Effects/Engine/Exhaust Smoke.prefab", typeof(GameObject));
                GameObject smokeEffect = Object.Instantiate(smoke, effects.transform.position, Quaternion.Euler(0, -180, 0), effects.transform);

                AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Piston/Exterior/Piston Engine Start.wav", typeof(AudioClip));
                AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Piston/Exterior/Propeller Shutdown.wav", typeof(AudioClip));
                AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Piston/Exterior/Propeller Running.wav", typeof(AudioClip));
                prop.core = new Common.Components.EngineCore
                {
                    exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>(),
                    ignitionExterior = start,
                    shutdownExterior = stop,
                    backIdle = run
                };
            }
            else
            {
                Debug.Log("Please Select GameObject to add Engine to..");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Create Internals", false, 4800)]
        public static void Helper()
        {
            GameObject aircraft;
            if (Selection.activeGameObject != null)
            {
                aircraft = Selection.activeGameObject;
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.gameObject.scene);

                //Setup the controller
                Rigidbody sRigidbody = aircraft.GetComponent<Rigidbody>();
                if (sRigidbody == null) { sRigidbody = aircraft.AddComponent<Rigidbody>(); }
                sRigidbody.mass = 1000f;

                CapsuleCollider sCollider = aircraft.GetComponent<CapsuleCollider>();
                if (sCollider == null) { aircraft.AddComponent<CapsuleCollider>(); }

                FixedController sController = aircraft.GetComponent<FixedController>();
                if (sController == null) { aircraft.AddComponent<FixedController>(); }



                GameObject core = new GameObject("m_core");
                GameObject ecog = new GameObject("_empty_cog");
                GameObject aerodynamics = new GameObject("m_dynamics");
                GameObject propulsion = new GameObject("m_propulsion");
                GameObject structure = new GameObject("m_structure");
                GameObject avionics = new GameObject("m_avionics");
                GameObject weapons = new GameObject("m_hardpoints");
                GameObject computer = new GameObject("Flight Computer");

                GameObject body = new GameObject("_body");
                GameObject surfaces = new GameObject("_control_surfaces");
                GameObject gears = new GameObject("_wheels");
                GameObject actuators = new GameObject("_actuators");
                GameObject cameraSystem = new GameObject("_cameras");
                GameObject focusPoint = new GameObject("_focus_point");
                GameObject incamera = new GameObject("Interior Camera");
                GameObject outcamera = new GameObject("Exterior Camera");

                GameObject lights = new GameObject("_lights");
                GameObject pylon = new GameObject("Pylon A");
                //
                Transform aircraftParent = aircraft.transform;
                Vector3 defaultPosition = Vector3.zero;

                core.transform.parent = aircraftParent;
                core.transform.localPosition = defaultPosition;
                aerodynamics.transform.parent = aircraftParent;
                aerodynamics.transform.localPosition = defaultPosition;
                propulsion.transform.parent = aircraftParent;
                propulsion.transform.localPosition = defaultPosition;
                structure.transform.parent = aircraftParent;
                structure.transform.localPosition = defaultPosition;
                body.transform.parent = structure.transform;
                body.transform.localPosition = defaultPosition;
                avionics.transform.parent = aircraftParent;
                avionics.transform.localPosition = defaultPosition;


                gears.transform.parent = structure.transform;
                gears.transform.localPosition = defaultPosition;
                actuators.transform.parent = avionics.transform;
                actuators.transform.localPosition = defaultPosition;
                cameraSystem.transform.parent = avionics.transform;
                cameraSystem.transform.localPosition = defaultPosition;
                weapons.transform.parent = aircraftParent;
                weapons.transform.localPosition = defaultPosition;

                surfaces.transform.parent = body.transform;
                surfaces.transform.localPosition = defaultPosition;

                lights.transform.parent = avionics.transform; lights.transform.localPosition = defaultPosition;
                incamera.transform.parent = cameraSystem.transform; incamera.transform.localPosition = defaultPosition;
                outcamera.transform.parent = cameraSystem.transform; outcamera.transform.localPosition = defaultPosition;
                focusPoint.transform.parent = cameraSystem.transform; focusPoint.transform.localPosition = defaultPosition;
                pylon.transform.parent = weapons.transform; pylon.transform.localPosition = defaultPosition;

                computer.transform.parent = aircraftParent; computer.transform.localPosition = defaultPosition;


                //ADD CAMERAS
                Camera interior = incamera.AddComponent<Camera>();
                incamera.AddComponent<AudioListener>();
                Camera exterior = outcamera.AddComponent<Camera>();
                outcamera.AddComponent<AudioListener>();
                SilantroCamera view = cameraSystem.AddComponent<SilantroCamera>();
                view.normalExterior = exterior;
                view.normalInterior = interior;
                view.focusPoint = focusPoint.transform;

                //ADD GEAR
                gears.AddComponent<SilantroWheels>();
                core.AddComponent<SilantroCore>();
                computer.AddComponent<FixedComputer>();

                ecog.transform.parent = core.transform;
                ecog.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.Log("Please Select Aircraft GameObject to Setup..");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Help/Tutorials", false, 4900)]
        public static void Tutorial()
        {
            Application.OpenURL("https://youtube.com/playlist?list=PLJkxX6TkFwO92f8Qphy3ihB5T6-Fbp0Bf");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Fixed Wing/Help/Report Bug", false, 5000)]
        public static void ContactBug()
        {
            Application.OpenURL("mailto:" + "silantrosimulator@gmail.com" + "?subject:" + "Silantro Fixed-Wing Toolkit Bug" + "&body:" + " ");
        }
    }
}

/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Common.DefinitionSettings
{
    /// <summary>
    /// 
    /// </summary>
    [InitializeOnLoad]
    public class StartupFixed
    {
        static StartupFixed()
        {
            // Check Validity of pairs
            Definitions.ConfigurePairs();
            DefinitionSettings.SilantroTag tag = Definitions.CollectFixedTag();
            if (tag.on == false)
            {
                tag.on = true;
                Definitions.UpdatePairInFile(tag);
                Definitions.SortDefines();
                Definitions.SetScriptDefines();
                Debug.Log("Fixed-Wing Defines Added!");
            }
        }
    }
}

/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Communication
{
    [InitializeOnLoad]
    public class FixedWingUpdate : ScriptableObject
    {
        static FixedWingUpdate m_Instance = null;
        private static readonly string _location = "Assets/Silantro/Common/Storage/Silantro_UPDATE.txt";
        private float l_fixed_version;
        private string l_command;

        /// <summary>
        /// 
        /// </summary>
        static FixedWingUpdate()
        {
            EditorApplication.update += OnInit;
        }

        /// <summary>
        /// 
        /// </summary>
        static void OnInit()
        {
            EditorApplication.update -= OnInit;
            m_Instance = FindObjectOfType<FixedWingUpdate>();
            if (m_Instance == null)
            {
                m_Instance = CreateInstance<FixedWingUpdate>();
                if (!SessionState.GetBool("FirstInitFixedDone", false))
                {
                    m_Instance.CheckUpdate();
                    SessionState.SetBool("FirstInitFixedDone", true);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CheckUpdate()
        {
            StartCoroutine(GetRequest("https://raw.githubusercontent.com/Oyedoyin/Silantro/main/fixed_wing.txt"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_task"></param>
        protected void StartCoroutine(IEnumerator _task)
        {
            StaticCoroutine coworker = new GameObject("Worker_" + _task.ToString()).AddComponent<StaticCoroutine>();
            coworker.Work(_task);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IEnumerator GetRequest(string uri)
        {
            if (!File.Exists(_location))
            {
                File.WriteAllText(_location, "3.515;3.515;YES;");
            }
            else
            {
                // Local Data
                StreamReader m_localFile = new StreamReader(_location);
                string[] m_localData = m_localFile.ReadToEnd().Split(char.Parse(";"));
                l_fixed_version = float.Parse(string.Concat(m_localData[0].Where(c => !char.IsWhiteSpace(c))));
                l_command = string.Concat(m_localData[2].Where(c => !char.IsWhiteSpace(c)));
                m_localFile.Close();
            }

            UnityWebRequest www = new UnityWebRequest(uri) { downloadHandler = new DownloadHandlerBuffer() };
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (l_command == "YES")
                {
                    Debug.Log("Unable to check for Silantro updates :( will try again next startup");
                }
            }
            else
            {
                // Internet Data
                string[] m_onlineData = www.downloadHandler.text.Split(char.Parse(";"));
                if (m_onlineData != null && m_onlineData.Length > 3)
                {
                    string o_type = string.Concat(m_onlineData[0].Where(c => !char.IsWhiteSpace(c)));
                    float o_version = float.Parse(string.Concat(m_onlineData[1].Where(c => !char.IsWhiteSpace(c))));
                    string o_mode = string.Concat(m_onlineData[2].Where(c => !char.IsWhiteSpace(c)));
                    string o_notes = m_onlineData[3].ToString();

                    if (o_version > l_fixed_version && l_command == "YES")
                    {
                        EditorUtility.DisplayDialog("Fixed Wing Update Available.  " + o_mode + " " + o_version,
                          o_notes,
                         "Close");
                    }
                }
            }
        }
    }
}

