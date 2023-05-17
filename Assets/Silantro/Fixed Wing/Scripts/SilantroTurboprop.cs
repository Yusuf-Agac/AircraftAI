#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Oyedoyin.Common;
using System.Collections;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Components;


namespace Oyedoyin.FixedWing
{
    #region Component
    /// <summary>
    /// 
    /// </summary>
    [HelpURL("https://youtu.be/B3mOHT8lsXM")]
    public class SilantroTurboprop : MonoBehaviour
    {
        #region Properties

        //--------------------------------------- Selectibles
        public enum IntakeShape { Rectangular, Circular, Oval }
        public IntakeShape intakeType = IntakeShape.Circular;

        //--------------------------------------- Connections
        public Transform intakePoint, exitPoint;
        public SilantroCore computer;
        public FixedController controller;
        public EngineCore core;

        public bool initialized; public bool evaluate;
        float inletDiameter, exhaustDiameter;
        bool allOk;
        public float diffuserDrawDiameter, exhaustDrawDiameter;

        //----------------------------ENGINE DIMENSIONS
        public float engineDiameter = 2f;
        [Range(0, 100f)] public float intakePercentage = 90f;
        [Range(0, 100f)] public float exhaustPercentage = 90f;
        public float inletArea, di, exhaustArea, intakeFactor = 0.1f, Ma, Uc;

        //-----------------------------CURVES
        public AnimationCurve pressureFactor, adiabaticFactor;

        //-----------------------------VARIABLES
        public float Pa, P02, P03, P04, P05, P06, Pc, PPC;
        public float Ta, T02, T03, T04, T05, T06;
        public float γa, γ1, γ2, γ3, γ4, γ5;
        public float cpa, cp1, cp2, cp3, cp4, cp5;
        public float Ue, Te, Ae, Me;
        public float πc = 6, ρa;
        public float mf, ma, f, Q, TIT = 1000f;
        [Range(70, 95f)] public float nd = 92f;
        [Range(85, 99f)] public float nc = 95f;
        [Range(97, 100f)] public float nb = 98f;
        [Range(90, 100f)] public float nt = 97f;
        [Range(90, 100f)] public float nab = 92f;
        [Range(95, 98f)] public float nn = 96f;
        [Range(50, 90f)] public float ng = 90f;
        public float Wc, alpha, Wt, Wshaft, Pshaft, Tj, Pt, brakePower, Hc;
        [Range(0, 15)] public float pcc = 6f;
        public float PSFC;


        #endregion

        #region Call Functions

        public void ReturnIgnitionCall()
        {
            StartCoroutine(ReturnIgnition());
        }
        public IEnumerator ReturnIgnition() { yield return new WaitForSeconds(0.5f); core.start = false; core.shutdown = false; }

        #endregion

        #region Internal Functions

        /// <summary>
        /// For testing purposes only
        /// </summary>
        private void Start()
        {
            if (evaluate) { Initialize(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            // --------------------------------- CHECK SYSTEMS
            _checkPrerequisites();


            if (allOk)
            {
                // --------------------------------- Run Core
                core.engine = this.transform;
                core.controller = controller;
                core.intakeFan = intakePoint;
                core.controller = controller;
                core.Initialize();

                // --------------------------------- Calculate Engine Areas
                MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out inletDiameter, out exhaustDiameter); di = inletDiameter;
                inletArea = (Mathf.PI * inletDiameter * inletDiameter) / 4f; exhaustArea = (Mathf.PI * exhaustDiameter * exhaustDiameter) / 4f;

                // --------------------------------- Plot Factors
                pressureFactor = MathBase.DrawPressureFactor();
                adiabaticFactor = MathBase.DrawAdiabaticConstant();
                initialized = true;

                if (intakeType == IntakeShape.Circular) { intakeFactor = 0.431f; }
                else if (intakeType == IntakeShape.Oval) { intakeFactor = 0.395f; }
                else if (intakeType == IntakeShape.Rectangular) { intakeFactor = 0.32f; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void _checkPrerequisites()
        {
            //CHECK COMPONENTS
            if (computer != null && controller.m_rigidbody != null)
            {
                allOk = true;
            }
            else if (computer == null)
            {
                Debug.LogError("Prerequisites not met on Engine " + transform.name + "....Core not connected");
                allOk = false; return;
            }
            else if (controller.m_rigidbody == null)
            {
                Debug.LogError("Prerequisites not met on Engine " + transform.name + "....Aircraft not connected");
                allOk = false; return;
            }

            if (core.ignitionExterior != null && core.backIdle != null && core.shutdownExterior != null) { } else { Debug.LogError("Prerequisites not met on Engine " + transform.name + "....sound clips not assigned properly"); allOk = false; return; }
        }
        /// <summary>
        /// 
        /// </summary>
#if UNITY_EDITOR
        protected void OnDrawGizmos()
        {
            // Collect Diameters
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out diffuserDrawDiameter, out exhaustDrawDiameter);

            // Draw
            Handles.color = Color.red;
            if (exitPoint != null) { Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (exhaustDrawDiameter / 2f)); }
            Handles.color = Color.blue;
            if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }

            // Plot Gas Factors
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (initialized)
            {
                if (controller.m_view != null) { core.cameraSector = controller.m_view.AnalyseCameraAngle(); }

                // ----------------- //Core
                core.Compute();

                // ----------------- //Power
                AnalyseThermodynamics();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void AnalyseThermodynamics()
        {

            //-------------------------------------- AMBIENT
            Ae = exhaustArea;
            Pa = (float)computer.m_atmosphere.Ps / 1000;
            Ta = (float)computer.m_atmosphere.T;
            Ma = (float)computer.m_atmosphere.M;
            γa = adiabaticFactor.Evaluate(Ta);
            cpa = pressureFactor.Evaluate(Ta);
            Q = controller.combustionEnergy;
            Uc = (float)computer.V;


            //0. ----------------------------------- INLET
            float R = 287f;
            ρa = (Pa * 1000) / (R * Ta);
            float va = (3.142f * di * core.coreRPM) / 60f;
            ma = ρa * va * inletArea * intakeFactor;

            //1. ----------------------------------- DIFFUSER
            γ1 = γa; cp1 = cpa;
            T02 = Ta + ((Uc * Uc) / (2 * (cp1 * 1000)));
            if (double.IsNaN(T02) || double.IsInfinity(T02)) { T02 = 0.0f; }
            float p0 = 1 + ((nd / 100f) * ((T02 - Ta) / Ta));
            P02 = Pa * Mathf.Pow(p0, (γ1 / (γ1 - 1f)));
            if (double.IsNaN(P02) || double.IsInfinity(P02)) { P02 = 0.0f; }


            //2. ----------------------------------- COMPRESSOR
            γ2 = adiabaticFactor.Evaluate(T02);
            cp2 = pressureFactor.Evaluate(T02);
            P03 = P02 * πc * core.coreFactor;
            if (double.IsNaN(P03) || double.IsInfinity(P03)) { P03 = 0.0f; }
            T03 = T02 * (1 + ((Mathf.Pow((πc * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));
            if (double.IsNaN(T03) || double.IsInfinity(T03)) { T03 = 0.0f; }
            Wc = (cp2 * 1000f * (T03 - T02)) / (nc / 100f);
            if (double.IsNaN(Wc) || double.IsInfinity(Wc)) { Wc = 0.0f; }

            //3. ----------------------------------- COMBUSTION CHAMBER
            P04 = (1 - (pcc / 100)) * P03;
            if (double.IsNaN(P04) || double.IsInfinity(P04)) { P04 = 0.0f; }
            if (P04 < 1) { P04 = 1; } // Infinity block
            T04 = TIT;
            if (double.IsNaN(T04) || double.IsInfinity(T04)) { T04 = 0.0f; }
            γ3 = adiabaticFactor.Evaluate(T04);
            cp3 = pressureFactor.Evaluate(T04);
            float F1 = (cp3 * T04) - (cp2 * T03);
            float F2 = ((nb / 100) * Q) - (cp3 * T04);
            f = (F1 / F2) * (core.controlInput + 0.01f);
            if (double.IsNaN(f) || double.IsInfinity(f)) { f = 0.0f; }

            //4. ----------------------------------- TURBINE
            T05 = T04 - ((cp2 * (T03 - T02)) / (cp3 * (1 + f)));
            if (double.IsNaN(T05) || double.IsInfinity(T05)) { T05 = 0.0f; }
            γ4 = adiabaticFactor.Evaluate(T05); cp4 = pressureFactor.Evaluate(T05);
            P05 = P04 * (Mathf.Pow((T05 / T04), (γ4 / (γ4 - 1))));
            if (double.IsNaN(P05) || double.IsInfinity(P05)) { P05 = 0.0f; }
            float p6_p4 = Mathf.Pow((Pa / P04), ((γ4 - 1) / γ4));
            Hc = cp4 * 1000f * T04 * (1 - p6_p4);
            if (double.IsNaN(Hc) || double.IsInfinity(Hc)) { Hc = 0.0f; }


            //5. ----------------------------------- NOZZLE
            float pf = (Mathf.Pow(((γ4 + 1) / 2), (γ4 / (γ4 - 1))));
            if (double.IsNaN(pf) || double.IsInfinity(pf)) { pf = 0.0f; }
            P06 = P05; Pc = P06 / pf;
            if (double.IsNaN(Pc) || double.IsInfinity(Pc)) { Pc = 0.0f; }
            float t6 = T04 / (Mathf.Pow((P04 / Pa), ((γ4 - 1) / γ4)));
            if (!float.IsNaN(t6) && !float.IsInfinity(t6)) { T06 = t6; }
            γ5 = adiabaticFactor.Evaluate(T05);
            cp5 = pressureFactor.Evaluate(T05);


            //6. ----------------------------------- OUTPUT
            if (!float.IsNaN(T06) && !float.IsInfinity(T06) && T06 > 1)
            {
                Ue = Mathf.Sqrt(1.4f * 287f * T06) * 0.5f;
                PPC = ((P06 * 1000) / (287f * T06));
                Me = PPC * Ue * Ae;
                mf = f * ma;
                alpha = 1 - ((Mathf.Pow(Uc, 2)) / (2 * (nt / 100) * Hc));
                Wt = (nt / 100) * alpha * Hc;
                Wshaft = (Wt * (nn / 100)) - Wc;
                Pshaft = ma * Wshaft;
                Tj = ma * ((1 + f) * Ue - Uc);
                Te = T06 - 273.15f;
                Pt = ((ng / 100) * Pshaft) + (Tj / 8.5f);
                Pt /= 1000f;
                if (double.IsNaN(Pt) || double.IsInfinity(Pt)) { Pt = 0.0f; }
                brakePower = (Pt / 0.7457f) * core.coreFactor; if (brakePower < 0) { brakePower = 0; }
                if (brakePower > 1) { PSFC = ((mf * 3600f * 2.20462f) / (brakePower)); }
                if (double.IsNaN(brakePower) || double.IsInfinity(brakePower)) { brakePower = 0.0f; }
            }
        }

        #endregion
    }
    #endregion

    #region Editor

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroTurboprop))]
    public class TurboPropEditor : Editor
    {

        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroTurboprop prop;
        SerializedProperty core;
        public int toolbarTab;
        public string currentTab;



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { prop = (SilantroTurboprop)target; core = serializedObject.FindProperty("core"); }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            GUILayout.Space(2f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Identifier", MessageType.None);
            GUI.color = backgroundColor;
            EditorGUILayout.PropertyField(core.FindPropertyRelative("engineIdentifier"), new GUIContent(" "));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("enginePosition"), new GUIContent("Position"));


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineDiameter"), new GUIContent("Engine Diameter"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePercentage"), new GUIContent("Intake Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Intake Diameter", prop.diffuserDrawDiameter.ToString("0.000") + " m");

            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustPercentage"), new GUIContent("Exhaust Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Exhaust Diameter", prop.exhaustDrawDiameter.ToString("0.000") + " m");

            GUILayout.Space(8f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Core", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Intake RPM"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("overspeedAllowance"), new GUIContent("Overspeed Allowance"));
            GUILayout.Space(4f);
            EditorGUILayout.LabelField("Maximum RPM", prop.core.maximumRPM.ToString("0.0") + " RPM");
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Minimum RPM", prop.core.minimumRPM.ToString("0.0") + " RPM");
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Core RPM", prop.core.coreRPM.ToString("0.0") + " RPM");


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Thermodynamic Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("πc"), new GUIContent("Core Pressure Ratio"));
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Pressure Drop (%)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcc"), new GUIContent("Compressor"));
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Turbine Inlet Temperature (°K)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TIT"), new GUIContent(" "));
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("TSFC ", prop.PSFC.ToString("0.00") + " lb/lbf.hr");



            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Efficiency Configuration (%)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nd"), new GUIContent("Diffuser"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nc"), new GUIContent("Compressor"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nb"), new GUIContent("Burner"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nt"), new GUIContent("Turbine"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nn"), new GUIContent("Nozzle"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ng"), new GUIContent("Gear"));





            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Connections", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePoint"), new GUIContent("Intake Fan"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("rotationAxis"), new GUIContent("Rotation Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("rotationDirection"), new GUIContent("Rotation Direction"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("soundMode"), new GUIContent("Mode"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorMode"), new GUIContent("Cabin Sounds"));
            GUILayout.Space(5f);
            if (prop.core.soundMode == EngineCore.SoundMode.Basic)
            {
                if (prop.core.interiorMode == EngineCore.InteriorMode.Off)
                {
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                }
                else
                {
                    toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                    GUILayout.Space(5f);
                    switch (toolbarTab)
                    {
                        case 0: currentTab = "Exterior Sounds"; break;
                        case 1: currentTab = "Interior Sounds"; break;
                    }
                    switch (currentTab)
                    {
                        case "Exterior Sounds":
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                            break;

                        case "Interior Sounds":
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                            break;
                    }
                }
            }
            else
            {
                GUILayout.Space(3f);
                if (prop.core.interiorMode == EngineCore.InteriorMode.Off)
                {
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                }
                else
                {
                    toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                    GUILayout.Space(5f);
                    switch (toolbarTab)
                    {
                        case 0: currentTab = "Exterior Sounds"; break;
                        case 1: currentTab = "Interior Sounds"; break;
                    }
                    switch (currentTab)
                    {
                        case "Exterior Sounds":
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                            break;

                        case "Interior Sounds":
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                            GUILayout.Space(2f);
                            EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                            break;
                    }
                }
            }



            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
            GUI.color = backgroundColor;
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
            if (prop.core.baseEffects)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));
                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustDistortion"), new GUIContent("Exhaust Distortion"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("distortionEmissionLimit"), new GUIContent("Maximum Distortion"));
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Output", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Core Power", (prop.core.corePower * prop.core.coreFactor * 100f).ToString("0.00") + " %");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Brake Power", prop.brakePower.ToString("0.0") + " Hp");


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}
