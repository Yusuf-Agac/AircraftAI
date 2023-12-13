#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Components;


namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    [HelpURL("https://youtu.be/8pDn_PxCPjI")]
    public class SilantroPiston : MonoBehaviour
    {
        #region Properties

        public enum MixtureControls { Locked, Free }
        public MixtureControls m_mixtureControl = MixtureControls.Locked;
        public enum DisplacementUnit { Liter, CubicMeter, CubicInch, CubicCentimeter, CubicFoot }
        public DisplacementUnit displacementUnit = DisplacementUnit.Liter;
        // Connections
        public Transform exitPoint;
        public SilantroCore computer;
        public Controller controller;
        public EngineCore core;
        public bool initialized;
        public bool evaluate;
        private bool allOk;

        public float stroke = 5;
        public float bore = 6;
        public float displacement = 1000, actualDisplacement;
        public float compressionRatio = 10;
        [Range(4, 20)] public int numberOfCylinders = 4;
        [Range(2, 4)] public int numberOfCycles = 4;
        public AnimationCurve m_combustionEfficiency;
        public AnimationCurve pressureFactor, adiabaticFactor;
        [Range(0.5f, 1)] public float m_volumetricEfficiency = 0.85f;
        public float Q = 47300f;
        public float m_cylinderHeadMass = 2.1f;
        public double m_inertia = 2.0;

        public float m_baseRPM;
        public float m_setPercentage = 20f;
        public float m_functionalPistonSpeed;
        public float m_meanPistonSpeed;
        public float m_peakPistonSpeed = 50f;

        public float m_maximumManifold = 28.5f;
        public float m_minimumManifold = 6.5f;
        public float m_functionOilTemperature = 358f;
        public float m_viscocityIndex = 0.25f;
        [Range(0, 102)] public float m_oilEfficiency = 66.70f;
        public float m_pressureValve = 60f;

        [Header("Supercharger Data")]
        public float m_boostFactor = 1.2f;
        public float m_boostLossFactor = 1.20f;
        public float m_boostPower;
        public int m_boostStages = 1;
        public bool m_boost;

        [Header("Performance Data")]
        public float m_manifoldPressure;
        private float m_PMEP;
        private readonly float m_manifoldLag = 1.0f;
        private readonly float m_coolingFactor = 0.5144444f;
        public float m_pressureRPM, m_maximumMAP, m_minimumMAP;
        public float m_pitotPressure;
        public float m_ramPressure;
        public float m_ramFactor = 1.0f;
        public float m_engineImpedance;
        public float m_airboxImpedance = -999f;
        public float m_throttleImpedance;
        public float m_impedance;
        public float m_deltaMAP;
        public float m_tempMAP;


        public float νa;
        public float ma;
        public float mf;
        public float ρa;
        readonly float R = 287.3f;
        public float cpa = 1.005f;
        public float cpf = 1.700f;
        public float m_equivalence;
        public bool m_starved;
        public float m_cf;
        float δT;
        float eT;
        float δEGTδt;
        float m_heatCapcity;
        float EGT_K = 0f;
        readonly float cpm = 800f; // 
        readonly float m_h1 = -95.0f;
        readonly float m_h2 = -3.95f;
        float m_h3;
        float c_a;
        float mc;
        float λT;
        float mca;
        float δQdt_c;
        float δQdt_f1;
        float δQdt_f2;
        float δQdt_cylinder;
        float m_hC;
        float CHT_K;
        float Oil_K;
        float ts;
        float m_target_OT;


        //-----------------------------VARIABLES
        public float Pa, P02, P03, P04;
        public float Ta, T02, T03, T04;
        public float Ue;
        public float mm;
        public float V1, V2, V3, vc, vd;
        public float Qin, W3_4, W1_2, Wnet, Wb, Pb;
        public float PSFC, AF = 15f;
        public float m_rpmFactor;


        [Header("Output")]
        public float brakePower;
        public float wpower;
        public float torque;
        public float m_EGT;
        public float m_CHT;
        public float m_OilPressure;
        public float m_OilTemperature;
        public float Mf;
        readonly float inhgtopa = 3386.38f;
        [Range(0, 1)] public float m_throttle = 0f;
        [Range(0, 1)] public float m_mixture = 0f;


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
                core.controller = controller;
                core.Initialize();
                ConfigureProperties();
                initialized = true;
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
            if (displacementUnit == DisplacementUnit.CubicCentimeter) { actualDisplacement = displacement / 1000000; }
            if (displacementUnit == DisplacementUnit.CubicFoot) { actualDisplacement = displacement / 35.315f; }
            if (displacementUnit == DisplacementUnit.CubicInch) { actualDisplacement = displacement / 61023.744f; }
            if (displacementUnit == DisplacementUnit.CubicMeter) { actualDisplacement = displacement; }
            if (displacementUnit == DisplacementUnit.Liter) { actualDisplacement = displacement / 1000; }

            ConfigureProperties();
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        public void Compute(float _timestep)
        {
            if (initialized)
            {
                if (controller.m_view != null) { core.cameraSector = controller.m_view.AnalyseCameraAngle(); }

                // ----------------- //Core
                core.Compute();

                // ----------------- //Power
                AnalyseThermodynamics(_timestep);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void AnalyseThermodynamics(float _timestep)
        {
            // --------------------------------------------------- Ambient
            if (m_mixture < 0.01f) { m_mixture = 0.01f; }
            if (m_mixtureControl == MixtureControls.Locked) { m_mixture = 1; }
            float m_ambientPressure = (float)computer.m_atmosphere.Ps;
            float m_temperature = (float)computer.m_atmosphere.T;
            float m_ambientTemperature = m_temperature - 273.15f;
            float m_mach = (float)computer.m_atmosphere.M;
            float γ = adiabaticFactor.Evaluate(Ta);
            float cpa = pressureFactor.Evaluate(Ta);
            Q = controller.combustionEnergy;
            float m_airspeed = (float)computer.V;

            m_meanPistonSpeed = (core.coreRPM * stroke) / 360;
            if (m_maximumManifold > 29.9f) { m_maximumManifold = 29.9f; }


            // --------------------------------------------------- Calculate Impedance
            float m_standardPressure = 101320.73f;
            m_engineImpedance = m_peakPistonSpeed / m_functionalPistonSpeed;
            m_airboxImpedance = (m_standardPressure * m_engineImpedance / m_maximumMAP) - m_engineImpedance;
            m_throttleImpedance = (m_peakPistonSpeed / ((m_baseRPM * stroke) / 360)) * (m_standardPressure / m_minimumMAP - 1) - m_airboxImpedance;


            // --------------------------------------------------- Calculate Ram Pressure
            m_pitotPressure = PitotPressure(m_mach, m_ambientPressure);
            m_ramPressure = (m_pitotPressure - m_ambientPressure) * m_ramFactor + m_ambientPressure;
            float Ω0 = core.functionalRPM * 0.1047198f;
            float Ω = core.coreRPM * 0.1047198f;
            float rpm_norm = (Ω / Ω0);
            m_rpmFactor = 1.795206541f * Mathf.Pow(0.55620178f, rpm_norm) * Mathf.Pow(rpm_norm, 1.246708471f);


            // --------------------------------------------------- Calculate Manifold Pressure
            float m_zt = (1 - m_throttle) * (1 - m_throttle) * m_throttleImpedance;
            float m_ze = m_meanPistonSpeed > 0 ? m_peakPistonSpeed / m_meanPistonSpeed : 999999;
            m_impedance = m_ze / (m_ze + m_airboxImpedance + m_zt);
            m_deltaMAP = (m_tempMAP - m_ramPressure * m_impedance);
            if (m_manifoldLag > _timestep) { m_deltaMAP *= _timestep / m_manifoldLag; }
            m_tempMAP -= m_deltaMAP;
            m_PMEP = (m_tempMAP - m_ambientPressure) * m_volumetricEfficiency;
            if (m_boost)
            {
                m_boostPower = ((m_boostStages * m_tempMAP * νa * γ) / (γ - 1)) * (Mathf.Pow((m_manifoldPressure / m_tempMAP), ((γ - 1) / (m_boostStages * γ))) - 1) * m_boostLossFactor / 745.7f;
                m_manifoldPressure = m_tempMAP * m_boostFactor;
            }
            else { m_manifoldPressure = m_tempMAP; }


            // --------------------------------------------------- Calculate Airflow
            float gamma = 1.3f;
            float m_pressureRatio = m_manifoldPressure < 1.0f ? compressionRatio : m_ambientPressure / m_manifoldPressure;
            if (m_pressureRatio > compressionRatio) m_pressureRatio = compressionRatio;
            float ve = ((gamma - 1) / gamma) + (compressionRatio - (m_pressureRatio)) / (gamma * (compressionRatio - 1));
            ρa = m_ambientPressure / (R * m_temperature);
            float m_swept = (actualDisplacement * (core.coreRPM / 60)) / 2;
            float m_ve = m_volumetricEfficiency * ve;
            νa = m_swept * m_ve;
            float rho_air_manifold = m_manifoldPressure / (R * m_temperature);
            ma = νa * rho_air_manifold;


            // --------------------------------------------------- Calculate Fuel flow
            float θ = 1.3f * m_mixture + Mathf.Epsilon;
            m_equivalence = θ * m_standardPressure / m_ambientPressure;
            mf = (ma * m_equivalence) / 14.7f;
            if (m_starved) { mf = 0f; m_equivalence = 0f; }


            #region EGT and CHT

            // --------------------------------------------------- Calculate EGT
            if (core.active && ma > 0)
            {
                m_cf = m_combustionEfficiency.Evaluate(m_equivalence);
                eT = mf * Q * 1000f * m_cf * 0.30f;
                m_heatCapcity = (cpa * 1000f * ma) + (cpf * 1000f * mf);
                δT = eT / m_heatCapcity;
                float tEGT = m_temperature + δT;
                EGT_K += (((tEGT - EGT_K) / 100f) * 0.5f);
            }
            else
            {
                m_cf = 0f;
                δEGTδt = (m_temperature - EGT_K) / 100f;
                δT = δEGTδt * _timestep;
                EGT_K += δT;
            }
            m_EGT = EGT_K - 273.15f;
            if (m_EGT < 0) { m_EGT = 0f; }



            // --------------------------------------------------- Calculate CHT
            c_a = (actualDisplacement * 61023.70f) / 360f;
            mc = m_cylinderHeadMass * numberOfCylinders;
            λT = CHT_K - m_temperature;
            if (float.IsNaN(λT) || float.IsInfinity(λT)) { λT = 0.0f; }
            mca = c_a * (m_airspeed * m_coolingFactor) * ρa;

            δQdt_c = mf * Q * 1000f * m_cf * 0.33f;
            δQdt_f1 = (m_h2 * mca * λT) + (m_h3 * core.coreRPM * λT / core.functionalRPM);
            δQdt_f2 = m_h1 * λT * c_a;
            δQdt_cylinder = δQdt_c + δQdt_f1 + δQdt_f2;
            m_hC = cpm * mc;
            CHT_K += (δQdt_cylinder / m_hC) * _timestep;
            if (float.IsNaN(CHT_K) || float.IsInfinity(CHT_K)) { CHT_K = 0.0f; }
            m_CHT = CHT_K - 273.15f;
            if (float.IsNaN(m_CHT) || float.IsInfinity(m_CHT)) { m_CHT = 0.0f; }

            // --------------------------------------------------- Calculate Oil Temperature
            m_target_OT = CHT_K + (m_oilEfficiency / 100f) * (m_temperature - CHT_K);
            if (m_OilPressure > 5.0f) { ts = 5000f / m_OilPressure; }
            else { ts = 1000f; }
            float δTdt = (m_target_OT - Oil_K) / ts;
            Oil_K += (δTdt * _timestep);
            m_OilTemperature = Oil_K - 273.15f;

            // --------------------------------------------------- Calculate Oil Pressure
            m_OilPressure = (m_pressureValve / m_pressureRPM) * core.coreRPM;
            if (m_OilPressure >= m_pressureValve) { m_OilPressure = m_pressureValve; }
            m_OilPressure += (m_functionOilTemperature - Oil_K) * m_viscocityIndex * m_OilPressure / m_pressureValve;

            #endregion


            // --------------------------------------------------- Calculate Power
            Pa = m_ambientPressure / 1000f;
            Ta = m_ambientTemperature + 273.5f;
            vd = actualDisplacement / numberOfCylinders;
            vc = vd / (compressionRatio - 1);
            V1 = vc + vd;
            mm = (Pa * 1000 * V1) / (287f * Ta);

            //-------------------------------------- STAGE 2
            float supremeRatio = compressionRatio;
            P02 = Pa * Mathf.Pow(supremeRatio, 1.35f);
            T02 = Ta * Mathf.Pow(supremeRatio, 0.35f);
            V2 = V1 / compressionRatio;
            AF = ma / mf; AF = Mathf.Clamp(AF, 10, 20);

            //-------------------------------------- STAGE 3
            float m_f = mf / ((core.coreRPM / 60) * 0.5f * numberOfCylinders);
            Qin = m_f * Q;
            T03 = (Qin / (mm)) + T02;
            V3 = V1;
            P03 = P02 * (T03 / T02);

            //-------------------------------------- STAGE 4
            P04 = P03 * Mathf.Pow((1 / supremeRatio), 1.35f);
            T04 = T03 * Mathf.Pow((1 / supremeRatio), 0.35f);
            W3_4 = (mm * 0.287f * (T04 - T03)) / (-0.35f);
            W1_2 = (mm * 0.287f * (T02 - Ta)) / (-0.35f);
            Wnet = W3_4 + W1_2;

            //-------------------------------------- OUTPUT
            Wb = 1 * Wnet;
            float pb = (Wb * (core.coreRPM / 60f) * 0.5f) * numberOfCylinders;
            if (!float.IsNaN(pb) && !float.IsInfinity(pb)) { Pb = pb; }
            brakePower = ((Pb * 1000) / 745.6f) - m_boostPower;
            if (brakePower < 0) { brakePower = 0; }
            Ue = Mathf.Sqrt(1.4f * 287f * T04) * 0.5f;
            if (brakePower > 0) { PSFC = (mf * 3600f * 2.2046f) / (brakePower); }

            float omega = Mathf.PI * core.coreRPM / 30.0f;
            wpower = 745.7f * brakePower;
            torque = (omega > 1.0) ? wpower / omega : wpower;
            if (float.IsNaN(torque) || float.IsInfinity(torque)) { torque = 0; }
            core.TorqueInput = torque;
            Mf = mf;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_mach"></param>
        /// <param name="_pressure"></param>
        /// <returns></returns>
        protected float PitotPressure(float _mach, float _pressure)
        {
            if (_mach < 0) { return _pressure; }
            if (_mach < 1) { return _pressure * Mathf.Pow((1 + 0.2f * _mach * _mach), 3.5f); }
            else { return _pressure * 166.92158009316827f * Mathf.Pow(_mach, 7.0f) / Mathf.Pow(7 * _mach * _mach - 1, 2.5f); }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void ConfigureProperties()
        {
            m_baseRPM = (m_setPercentage / 100) * core.functionalRPM;
            m_functionalPistonSpeed = (core.functionalRPM * stroke) / 360f;
            float maximumRPM = (1 + (core.overspeedAllowance / 100)) * core.functionalRPM;
            m_peakPistonSpeed = (maximumRPM * stroke) / 360f;

            m_pressureRPM = core.functionalRPM * 0.75f;
            if (m_maximumManifold > 30) { m_maximumManifold = 30f; }
            m_minimumMAP = m_minimumManifold * inhgtopa;
            m_maximumMAP = m_maximumManifold * inhgtopa;
            m_h3 = -0.05f * core.functionalRPM;
            CHT_K = Oil_K = 288.15f;
            m_combustionEfficiency = MathBase.DrawCombustionEfficiency();
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
        }

        #endregion
    }
    #endregion

    #region Editor

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroPiston))]
    public class PistonEditor : Editor
    {

        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroPiston piston;
        SerializedProperty core;
        public int toolbarTab;
        public string currentTab;



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { piston = (SilantroPiston)target; core = serializedObject.FindProperty("core"); }


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
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mixtureControl"), new GUIContent("Mixture Control"));


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfCylinders"), new GUIContent("Cylinder Count"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfCycles"), new GUIContent("Stroke Count"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stroke"), new GUIContent("Stroke (Inches)"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bore"), new GUIContent("Bore (Inches)"));
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Displacement", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displacement"), new GUIContent(" "));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displacementUnit"), new GUIContent(" "));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField(" ", piston.actualDisplacement.ToString("0.00000000") + " m3");
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_volumetricEfficiency"), new GUIContent("Volumetric Efficiency"));


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("compressionRatio"), new GUIContent("Compression Ratio"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Functional RPM"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Manifold Pressure (In.Hg)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maximumManifold"), new GUIContent("Maximum Manifold"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_minimumManifold"), new GUIContent("Minimum Manifold"));
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Engine Oil Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_functionOilTemperature"), new GUIContent("Functional Temperature"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_viscocityIndex"), new GUIContent("Viscosity Index"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_oilEfficiency"), new GUIContent("Efficiency"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pressureValve"), new GUIContent("Valve Pressure"));

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Dynamics", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_inertia"), new GUIContent("Inertia"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_cylinderHeadMass"), new GUIContent("Cylinder Head Mass"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("PPS", piston.m_peakPistonSpeed.ToString("0.000") + " m/s");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("MPS", piston.m_meanPistonSpeed.ToString("0.000") + " m/s");


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
            if (piston.core.soundMode == EngineCore.SoundMode.Basic)
            {
                if (piston.core.interiorMode == EngineCore.InteriorMode.Off)
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
                if (piston.core.interiorMode == EngineCore.InteriorMode.Off)
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


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
            GUI.color = backgroundColor;
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
            if (piston.core.baseEffects)
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
            GUILayout.Space(3f);
            EditorGUILayout.LabelField(" ", piston.core.coreRPM.ToString("0.00") + " RPM");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Torque", piston.torque.ToString("0.00") + " Nm");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Power", (piston.brakePower).ToString("0.000") + " Hp");


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}
