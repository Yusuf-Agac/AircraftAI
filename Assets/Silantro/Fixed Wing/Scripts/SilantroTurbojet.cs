using UnityEngine;
using Oyedoyin.Common;
using System.Collections;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Components;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.FixedWing
{
    #region Component
    [HelpURL("https://youtu.be/wFQngghaBoE")]
    public class SilantroTurbojet : MonoBehaviour
    {
        #region Properties
        //--------------------------------------- Selectibles
        public enum IntakeShape { Rectangular, Circular, Oval }
        public IntakeShape intakeType = IntakeShape.Circular;
        public enum ReheatSystem { Afterburning, noReheat }
        public ReheatSystem reheatSystem = ReheatSystem.noReheat;
        public enum ReverseThrust { Available, Absent }
        public ReverseThrust reverseThrustMode = ReverseThrust.Absent;

        //--------------------------------------- Connections
        public Transform intakePoint, exitPoint;
        public FixedController controller;
        public SilantroCore computer;
        public EngineCore core;
        float inletDiameter, exhaustDiameter;

        public bool initialized;
        public bool evaluate;
        bool allOk;

        //----------------------------ENGINE DIMENSIONS
        public float engineDiameter = 2f;
        [Range(0, 100f)] public float intakePercentage = 90f;
        [Range(0, 100f)] public float exhaustPercentage = 90f;
        public float inletArea, di, exhaustArea, intakeFactor = 0.1f, Ma, Uc;

        //-----------------------------CURVES
        public AnimationCurve pressureFactor, adiabaticFactor;

        //-----------------------------VARIABLES
        public float Pa, P02, P03, P04, P05, P06, P7, Pc, pf;
        public float Ta, T02, T03, T04, T05, T06, T7;
        public float πc = 5, ρa, ed;
        public float γa, γ1, γ2, γ3, γ4, γ5, γ6;
        public float cpa, cp1, cp2, cp3, cp4, cp5, cp6, cp7;
        public float mf, ma, f, fab, Q, TIT = 1000f, MaximumTemperature = 2000f;
        [Range(0, 15)] public float pcc = 6f, ppc, pcab = 3f;
        [Range(70, 95f)] public float nd = 92f;
        [Range(85, 99f)] public float nc = 95f;
        [Range(97, 100f)] public float nb = 98f;
        [Range(90, 100f)] public float nt = 97f;
        [Range(90, 100f)] public float nab = 92f;
        [Range(95, 98f)] public float nn = 96f;
        public float Ue, Te, Aeb, Ae, Me;
        public float coreThrust, pressureThrust, engineThrust, TSFC;
        public float diffuserDrawDiameter, exhaustDrawDiameter;
        public float baseThrust, maxThrust; float baseMf, maxMf;


        #endregion

        #region Call Functions

        /// <summary>
        /// 
        /// </summary>
        public void ReturnIgnitionCall()
        {
            StartCoroutine(ReturnIgnition());
        }
        public IEnumerator ReturnIgnition() { yield return new WaitForSeconds(0.5f); core.start = false; core.shutdown = false; }




        /// <summary>
        /// AFTERBURNER CONTROL
        /// </summary>
        public void ToggleAfterburner()
        {
            if (reheatSystem == ReheatSystem.Afterburning && core.corePower > 0.5f && core.controlInput > 0.5f) { core.afterburnerOperative = !core.afterburnerOperative; }
        }

        public void EngageAfterburner() { if (reheatSystem == ReheatSystem.Afterburning && core.corePower > 0.5f && core.controlInput > 0.5f && !core.reverseThrustEngaged) { core.afterburnerOperative = true; } }
        public void DisEngageAfterburner() { if (core.afterburnerOperative) { core.afterburnerOperative = false; } }

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
        public void Compute()
        {
            if (initialized)
            {
                if (controller.m_view != null) { core.cameraSector = controller.m_view.AnalyseCameraAngle(); }

                // ----------------- //Core
                core.Compute();

                // ----------------- //Power
                AnalyseThermodynamics();


                // ----------------- //Thrust
                if (engineThrust > 0 && !float.IsInfinity(engineThrust) && !float.IsNaN(engineThrust))
                {
                    if (controller != null && exitPoint != null)
                    {
                        Vector3 thrustForce = exitPoint.forward * engineThrust;
                        controller.m_rigidbody.AddForceAtPosition(thrustForce, exitPoint.position, ForceMode.Force);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Collect Diameters
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out diffuserDrawDiameter, out exhaustDrawDiameter);

            // Draw
            Handles.color = Color.red;
            if (exitPoint != null)
            {
                Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (exhaustDrawDiameter / 2f));
                Handles.color = Color.red; Handles.ArrowHandleCap(0, exitPoint.position, exitPoint.rotation * Quaternion.LookRotation(-Vector3.forward), 2f, EventType.Repaint);
            }
            Handles.color = Color.blue;
            if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }

            // Plot Gas Factors
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
            core.EvaluateRPMLimits();
        }
#endif
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
        public void Initialize()
        {
            // --------------------------------- CHECK SYSTEMS
            _checkPrerequisites();


            if (allOk)
            {
                // --------------------------------- Run Core
                core.engine = this.transform;
                core.controller = controller;
                if (reheatSystem == ReheatSystem.Afterburning) { core.canUseAfterburner = true; }
                if (reverseThrustMode == ReverseThrust.Available) { core.reverseThrustAvailable = true; }
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
        void AnalyseThermodynamics()
        {

            //-------------------------------------- AMBIENT
            Ae = Aeb = exhaustArea;
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
            T02 = Ta * (1 + (((γ1 - 1) / 2) * Ma * Ma));
            float p0 = 1 + (0.5f * Ma * Ma * (nd / 100f) * (γ1 - 1));
            P02 = Pa * Mathf.Pow(p0, (γ1 / (γ1 - 1f)));


            //2. ----------------------------------- COMPRESSOR
            γ2 = adiabaticFactor.Evaluate(T02);
            cp2 = pressureFactor.Evaluate(T02);
            P03 = P02 * πc * core.coreFactor;
            T03 = T02 * (1 + ((Mathf.Pow((πc * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));


            //3. ----------------------------------- COMBUSTION CHAMBER
            P04 = (1 - (pcc / 100)) * P03;
            T04 = TIT;
            γ3 = adiabaticFactor.Evaluate(T04);
            cp3 = pressureFactor.Evaluate(T04);
            float F1 = (cp3 * T04) - (cp2 * T03);
            float F2 = ((nb / 100) * Q) - (cp3 * T04);
            f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


            //4. ----------------------------------- TURBINE
            T05 = T04 - ((cp2 * (T03 - T02)) / (cp3 * (1 + f)));
            γ4 = adiabaticFactor.Evaluate(T05);
            cp4 = pressureFactor.Evaluate(T05);
            float p5_4 = 1 - ((T04 - T05) / ((nt / 100) * T04));
            P05 = P04 * (Mathf.Pow(p5_4, (γ4 / (γ4 - 1))));


            //5. ----------------------------------- NOZZLE
            pf = (Mathf.Pow(((γ4 + 1) / 2), (γ4 / (γ4 - 1))));
            P06 = P05; Pc = P06 / pf; T06 = T05;
            γ5 = adiabaticFactor.Evaluate(T05);
            cp5 = pressureFactor.Evaluate(T05);

            // ------ Check if Chocked
            if (Pc >= Pa) { P7 = Pc * core.coreFactor; }
            else
            {
                P7 = Pa;
                float T7Factor1 = (2 * cp5 * T06);
                float T7factor2 = (1 - ((Mathf.Pow((Pa / P06), (γ5 - 1) / γ5))));
                Ue = Mathf.Sqrt(T7Factor1 * T7factor2);
            }
            T7 = (T06 / ((γ5 + 1) / 2f)) * core.coreFactor;
            mf = (f) * ma;
            Ue = Mathf.Sqrt(1.4f * 287f * T7);



            //7. ----------------------------------- AFTERBURNER
            if (core.afterburnerOperative)
            {
                P06 = (1 - (pcab / 100)) * P05;
                T06 = MaximumTemperature;
                γ6 = adiabaticFactor.Evaluate(T06);
                cp6 = pressureFactor.Evaluate(T06);
                fab = ((cp5 * (T06 - T05)) / (((nab / 100) * Q) - (cp5 * T06)));


                //CHECK IF CHOCKED
                if (Pc >= Pa)
                {
                    P7 = Pc;
                    T7 = T06 / ((γ6 + 1) / 2f);
                    cp7 = pressureFactor.Evaluate(T7);
                    Ue = Mathf.Sqrt(1.4f * 287f * T7);
                }
                else
                {
                    P7 = Pa;
                    float T7Factor1 = (2 * cp6 * T06);
                    float T7factor2 = (1 - ((Mathf.Pow((Pa / P06), (γ6 - 1) / γ6))));
                    Ue = Mathf.Sqrt(T7Factor1 * T7factor2);
                }
                Aeb = (287f * T7 * ma * (1 + f + fab)) / (P7 * 1000 * Ue);
                if (Aeb < Ae) { Aeb = Ae; }
                if (Aeb > 0) { ed = Mathf.Sqrt((Aeb * 4f) / (3.142f)); }
                mf = ma * (f + fab);
            }



            //8. ----------------------------------- OUTPUT
            if (T7 > 0 && !float.IsInfinity(T7) && !float.IsNaN(T7)) { Te = T7 - 273.15f; }
            if (T7 > 0 && !float.IsInfinity(T7) && !float.IsNaN(T7))
            {
                cp6 = pressureFactor.Evaluate(T7);
                if (P7 > 0 && !float.IsInfinity(P7) && !float.IsNaN(P7)) { ppc = ((P7 * 1000) / (287f * T7)); }
            }
            if (ppc > 0 && !float.IsInfinity(ppc) && !float.IsNaN(ppc)) { Me = ppc * Ue * Ae; }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (!core.afterburnerOperative)
            {
                float ct = (((1 + f) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                float ptt = (Ae * ((P7 - Pa) * 1000)); if (ptt > 0 && !float.IsInfinity(ptt) && !float.IsNaN(ptt)) { pressureThrust = ptt; }
                baseThrust = (coreThrust + pressureThrust); baseMf = ma * (f);
            }
            else
            {
                float ct = (((1 + f + fab) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                float ptt = (Aeb * ((P7 - Pa) * 1000)); if (ptt > 0 && !float.IsInfinity(ptt) && !float.IsNaN(ptt)) { pressureThrust = ptt; }
                maxThrust = (coreThrust + pressureThrust); maxMf = ma * (f + fab);
            }

            engineThrust = (baseThrust) + (maxThrust - baseThrust) * core.burnerFactor;
            if (engineThrust < 0) { engineThrust = 0; }
            mf = baseMf + (maxMf - baseMf) * core.burnerFactor;
            if (engineThrust > (controller.currentWeight * 9.8f * 1.5f)) { engineThrust = (controller.currentWeight * 9.8f); }


            float pt = engineThrust * 0.2248f;
            if (pt > 0 && !float.IsInfinity(pt) && !float.IsNaN(pt)) { TSFC = ((mf * 3600f) / (pt * 0.4536f)); }
            if (core.afterburnerOperative && core.controlInput < 0.5f) { core.afterburnerOperative = false; }
        }
        #endregion
    }
    #endregion

    #region Editor

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroTurbojet))]
    public class SilantroTurboJetEditor : Editor
    {

        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroTurbojet jet;
        SerializedProperty core;
        public int toolbarTab;
        public string currentTab;



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { jet = (SilantroTurbojet)target; core = serializedObject.FindProperty("core"); }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector ();
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
            EditorGUILayout.LabelField("Intake Diameter", jet.diffuserDrawDiameter.ToString("0.000") + " m");

            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustPercentage"), new GUIContent("Exhaust Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Exhaust Diameter", jet.exhaustDrawDiameter.ToString("0.000") + " m");

            GUILayout.Space(8f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reheatSystem"), new GUIContent("Reheat System"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseThrustMode"), new GUIContent("Reverse Thrust"));

            if (jet.reverseThrustMode == SilantroTurbojet.ReverseThrust.Available)
            {
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Reverse Thrust Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Actuation Level", (jet.core.reverseThrustFactor * 100f).ToString("0.0") + " %");
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("reverseThrustPercentage"), new GUIContent("Extraction Percentage"));
            }

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
            EditorGUILayout.LabelField("Maximum RPM", jet.core.maximumRPM.ToString("0.0") + " RPM");
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Minimum RPM", jet.core.minimumRPM.ToString("0.0") + " RPM");
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Core RPM", jet.core.coreRPM.ToString("0.0") + " RPM");


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
            if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pcab"), new GUIContent("Afterburner Pipe"));
            }

            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Turbine Inlet Temperature (°K)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TIT"), new GUIContent(" "));
            if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
            {
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Maximum Engine Temperature (°K)", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumTemperature"), new GUIContent(" "));
            }
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("TSFC ", jet.TSFC.ToString("0.00") + " lb/lbf.hr");



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
            if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nab"), new GUIContent("Afterburner"));
            }




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

            if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
            {
                //GUILayout.Space(5f); 
                //EditorGUILayout.PropertyField(core.FindPropertyRelative("dataSource"), new GUIContent("Nozzle Actuator"));
                GUILayout.Space(5f);  
                EditorGUILayout.PropertyField(core.FindPropertyRelative("m_actuator"), new GUIContent("Nozzle Actuator"));
            }


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
            if (jet.core.soundMode == EngineCore.SoundMode.Basic)
            {
                if (jet.core.interiorMode == EngineCore.InteriorMode.Off)
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
                if (jet.core.interiorMode == EngineCore.InteriorMode.Off)
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
            if (jet.core.baseEffects)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));
                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustDistortion"), new GUIContent("Exhaust Distortion"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("distortionEmissionLimit"), new GUIContent("Maximum Distortion"));

                GUILayout.Space(10f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEmission"), new GUIContent("Emission Effect"));
                if (jet.core.baseEmission)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Exhaust Emission Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerCoreMaterial"), new GUIContent("Core Material"));

                    if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerPipeMaterial"), new GUIContent("Afterburner Pipe Material"));
                    }
                    else
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerPipeMaterial"), new GUIContent("Pipe Material"));
                    }

                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumNormalEmission"), new GUIContent("Maximum Emission"));
                    GUILayout.Space(2f);
                    if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
                    {
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumAfterburnerEmission"), new GUIContent("Maximum Afterburner Emission"));
                    }
                }



                GUILayout.Space(10f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("coreFlame"), new GUIContent("Flame Effect"));
                if (jet.core.coreFlame)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Exhaust Flame Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("flameType"), new GUIContent("Flame Type"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("flameObject"), new GUIContent("Flame Object"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("flameMaterial"), new GUIContent("Flame Material"));

                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Normal Mode", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("normalDiameter"), new GUIContent("Dry Flame Diameter"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("normalLength"), new GUIContent("Dry Flame Length"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("normalAlpha"), new GUIContent("Dry Flame Alpha"));

                    if (jet.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Afterburner Mode", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("wetDiameter"), new GUIContent("Wet Flame Diameter"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("wetLength"), new GUIContent("Wet Flame Length"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("wetAlpha"), new GUIContent("Wet Flame Alpha"));
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("alphaSpeed"), new GUIContent("Alpha Speed"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("scaleSpeed"), new GUIContent("Scale Speed"));
                }
            }



            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Output", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Core Power", (jet.core.corePower * jet.core.coreFactor * 100f).ToString("0.00") + " %");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Engine Thrust", jet.engineThrust.ToString("0.0") + " N");


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}
