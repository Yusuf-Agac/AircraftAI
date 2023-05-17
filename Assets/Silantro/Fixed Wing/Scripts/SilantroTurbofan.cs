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
    public class SilantroTurbofan : MonoBehaviour
    {
        #region Properties

        //--------------------------------------- Selectibles
        public enum EngineType { Unmixed, Mixed }
        public EngineType engineType;
        public enum IntakeShape { Rectangular, Circular, Oval }
        public IntakeShape intakeType = IntakeShape.Circular;
        public enum ReheatSystem { Afterburning, noReheat }
        public ReheatSystem reheatSystem = ReheatSystem.noReheat;
        public enum ReverseThrust { Available, Absent }
        public ReverseThrust reverseThrustMode = ReverseThrust.Absent;

        //--------------------------------------- Connections
        public Transform intakePoint, exitPoint;
        public Transform fanExhaustPoint;
        public FixedController controller;
        public SilantroCore computer;
        public EngineCore core;
        public bool initialized; public bool evaluate;
        float cEd, inletDiameter, coreExhaustDiameter, fanExhaustDiameter, nullFloat;
        public float diffuserDrawDiameter, fanExhaustDrawDiameter, coreExhaustDrawDiameter;
        bool allOk;


        //----------------------------ENGINE DIMENSIONS
        public float engineDiameter = 2f;
        [Range(0, 100f)] public float intakePercentage = 90f;
        [Range(0, 100f)] public float coreExhaustPercentage = 90f, fanExhaustPercentage = 90f;
        public float inletArea, di, coreExhaustArea, fanExhaustArea, intakeFactor = 0.1f, Ma, Uc;


        //-----------------------------CURVES
        public AnimationCurve pressureFactor, adiabaticFactor;



        //-----------------------------VARIABLES
        public float Pa, P02, P03, P04, P05, P06, P7, P08, P09, P10, Pc, pf;
        public float Ta, T02, T03, T04, T05, T06, T7, T08, T09, T10;
        public float πc = 10f, β = 0.5f, ρa, ed;
        public float πf = 1f;
        public float γa, γ1, γ2, γ3, γ4, γ5, γ6, γ7, γ8, γ9;
        public float cpa, cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9;
        public float mf, ma, mh, f, fab, Q, TIT = 1000f, MaximumTemperature = 2000f;
        public float ppc;
        [Range(70, 95f)] public float nd = 92f;
        [Range(90, 99f)] public float nf = 95f;
        [Range(85, 99f)] public float nc = 95f;
        [Range(97, 100f)] public float nb = 98f;
        [Range(90, 100f)] public float nhpt = 97f;
        [Range(90, 100f)] public float nab = 92f;
        [Range(95, 98f)] public float nn = 96f;
        [Range(90, 100f)] public float nlpt = 97f;
        [Range(0, 15f)] public float pcc = 6f;
        [Range(0, 15f)] public float pcab = 3f;
        public float Ue, Te, Aeb, Ae, Me;
        public float coreThrust, pressureThrust, engineThrust, TSFC, fanThrust;
        public float cAe, fAe, Wc, Uef;
        public float fanPressureThrust, fanCoreThrust, coreCoreThrust, corePressureThrust;
        public float pcc8b, pcc8a, P8c, p6cp5, p7c, p7cp6;
        public bool showPerformance;
        public float baseThrust, maxThrust; float baseMf, maxMf;
        float thrustFactor = 1f;

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
                if (reheatSystem == ReheatSystem.Afterburning && engineType == EngineType.Mixed) { core.canUseAfterburner = true; }
                if (reverseThrustMode == ReverseThrust.Available) { core.reverseThrustAvailable = true; }
                core.intakeFan = intakePoint;
                core.controller = controller;
                core.Initialize();

                // --------------------------------- Calculate Engine Areas
                MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, coreExhaustPercentage, out inletDiameter, out coreExhaustDiameter);
                MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, fanExhaustPercentage, out nullFloat, out fanExhaustDiameter); di = inletDiameter;
                inletArea = (Mathf.PI * inletDiameter * inletDiameter) / 4f; cEd = coreExhaustDiameter;
                coreExhaustArea = (Mathf.PI * coreExhaustDiameter * coreExhaustDiameter) / 4f;
                fanExhaustArea = (Mathf.PI * fanExhaustDiameter * fanExhaustDiameter) / 4f;

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
        public void Compute()
        {
            if (initialized)
            {
                if (controller.m_view != null) { core.cameraSector = controller.m_view.AnalyseCameraAngle(); }

                // ----------------- //Core
                core.Compute();

                // ----------------- //Power
                AnalyseThermodynamics();

                if (reverseThrustMode == ReverseThrust.Available && core.reverseThrustEngaged) { thrustFactor = -1f * core.reverseThrustFactor * core.reverseThrustPercentage / 100f; }
                else { thrustFactor = 1f; }


                // ----------------- //Thrust
                if (engineThrust > 0 && !float.IsInfinity(engineThrust) && !float.IsNaN(engineThrust))
                {
                    if (controller.m_rigidbody != null && exitPoint != null)
                    {
                        Vector3 thrustForce = exitPoint.forward * engineThrust * thrustFactor;
                        controller.m_rigidbody.AddForceAtPosition(thrustForce, exitPoint.position, ForceMode.Force);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void AnalyseThermodynamics()
        {

            //-------------------------------------- AMBIENT
            if (engineType == EngineType.Mixed) { Ae = Aeb = coreExhaustArea; } else { cAe = Aeb = coreExhaustArea; fAe = fanExhaustArea; }
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

            if (engineType == EngineType.Mixed)
            {
                //2. ----------------------------------- FAN
                γ2 = adiabaticFactor.Evaluate(T02);
                cp2 = pressureFactor.Evaluate(T02);
                P03 = P02 * πf * core.coreFactor + 0.01f;
                T03 = T02 * (1 + ((Mathf.Pow((πf * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));


                //3. ----------------------------------- COMPRESSOR
                P04 = P03 * πc;
                float t4_t3 = Mathf.Pow((πc), ((γ2 - 1) / γ2));
                T04 = T03 * (1 + ((t4_t3 - 1) / (nc / 100)));
                γ3 = adiabaticFactor.Evaluate(T04); cp3 = pressureFactor.Evaluate(T04);


                //4. ----------------------------------- COMBUSTION CHAMBER
                P05 = (1 - (pcc / 100)) * P04;
                T05 = TIT;
                γ4 = adiabaticFactor.Evaluate(T04);
                cp4 = pressureFactor.Evaluate(T04);
                float F1 = (cp4 * T05) - (cp3 * T04);
                float F2 = ((nb / 100) * Q) - (cp3 * T05);
                f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


                //5. ----------------------------------- LOW PRESSURE TURBINE
                float p6A = cp3 * (T04 - T03);
                float p6B = cp4 * (1 + f);
                T06 = T05 - (p6A / p6B);
                float p6_p5 = 1 - (1 - (T06 / T05)) * (1 / (nlpt / 100));
                P06 = P05 * Mathf.Pow(p6_p5, (γ4 / (γ4 - 1)));


                //6. ----------------------------------- HIGH PRESSURE TURBINE
                γ5 = adiabaticFactor.Evaluate(T06);
                cp5 = pressureFactor.Evaluate(T06);
                P7 = P03;
                float p7_p6 = Mathf.Pow((P7 / P06), ((γ5 - 1) / γ5));
                T7 = T06 * (1 - ((nhpt / 100) * (1 - p7_p6)));
                Wc = ((cp5 * (T06 - T7)) / (nhpt / 100f) * ma) / 745f;


                //7. ----------------------------------- NOZZLE
                if (T7 > 0 && T7 < 10000) { γ6 = adiabaticFactor.Evaluate(T7); cp6 = pressureFactor.Evaluate(T7); }
                P08 = P03;
                float t8A = (β * cp5 * T03) + ((1 + f) * cp6 * T7);
                float t8B = ((1 + f) + β) * cp6;
                T08 = t8A / t8B;
                if (T08 > 0 && T08 < 10000) { γ7 = adiabaticFactor.Evaluate(T08); cp7 = pressureFactor.Evaluate(T08); }
                float pcc8 = 1 / (1 - (1 / (nn / 100)) * ((γ7 - 1) / (γ7 + 1)));
                float pc_p8 = Mathf.Pow(pcc8, (γ7 / (γ7 - 1)));
                Pc = P08 / (pc_p8);

                //float Aea;

                // ------ Check if Chocked
                float p8_pa = P08 / Pa;
                float p8_pc = P08 / Pc;
                if (p8_pa > p8_pc)
                {
                    P09 = Pc;
                    T09 = T08 / ((γ7 + 1) / 2);
                    float p9 = (P09 * 1000) / (287 * T09);
                    γ8 = adiabaticFactor.Evaluate(T09);
                    Ue = Mathf.Sqrt(γ8 * 287 * T09) * core.coreFactor;
                    Me = p9 * Ue * Ae;
                    //Aea = (Ma * (1 + f)) / (p9 * Ue);
                }
                else
                {
                    P09 = Pa;
                    T09 = T08 / ((γ7 + 1) / 2);//T9 = T8 * Mathf.Pow ((P8 / Pa), a);
                    float p9 = (P09 * 1000) / (287 * T09);
                    Ue = Mathf.Sqrt(2f * cp7 * 1000f * (T08 - T09)) * core.coreFactor;
                    Me = p9 * Ue * Ae;
                    //Aea = (Ma * (1 + f)) / (P09 * Ue);
                }

                Te = (T08 - 273.15f) * core.coreFactor;
                mf = (f) * ma;


                //8. ----------------------------------- AFTERBURNER
                if (core.afterburnerOperative)
                {
                    P10 = (1 - (pcab / 100)) * (P06 / (1.905f * pc_p8));
                    T10 = MaximumTemperature;
                    γ9 = adiabaticFactor.Evaluate(T10);
                    cp9 = pressureFactor.Evaluate(T10);

                    fab = ((cp7 * (T10 - T7)) / (((nab / 100) * Q) - (cp7 * T10)));
                    float a = (γ9 + 1) / 2;
                    float t11 = T10 / a;
                    Ue = Mathf.Sqrt(2f * cp9 * 1000f * (T10 - t11));
                    Te = (t11 - 273.15f) * core.coreFactor;

                    float Acf = (287f * t11 * Ma * (1 + f + fab)) / ((P08 / pc_p8) * 1000 * Ue);
                    if (Acf > 0) { ed = Mathf.Sqrt((Acf * 4f) / (3.142f)); }
                    if (ed < cEd) { ed = cEd; }


                    float p9 = (P09 * 1000) / (287 * T09);
                    float mc = p9 * Ue * Ae;
                    Me = (mc + (ma * fab)) * core.coreFactor;
                }


                // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                if (!core.afterburnerOperative)
                {
                    float ct = (((1 + f) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                    float ptt = Ae * (((P06 / (1.905f * pc_p8)) - Pa) * 1000); if (!float.IsInfinity(ptt) && !float.IsNaN(ptt) && ptt > 0) { pressureThrust = ptt; }
                    baseThrust = (coreThrust + pressureThrust); baseMf = ma * (f);
                }
                else
                {
                    float ct = (((1 + f + fab) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                    float ptt = (Aeb * ((P10 - Pa) * 1000)); if (!float.IsInfinity(ptt) && !float.IsNaN(ptt) && ptt > 0) { pressureThrust = ptt; }
                    maxThrust = (coreThrust + pressureThrust); maxMf = ma * (f + fab);
                }
                engineThrust = (baseThrust) + (maxThrust - baseThrust) * core.burnerFactor; mf = baseMf + (maxMf - baseMf) * core.burnerFactor;
                if (engineThrust < 0) { engineThrust = 0; }
                if (engineThrust > (controller.currentWeight * 9.8f * 1.5f)) { engineThrust = (controller.currentWeight * 9.8f); }
            }


            // --------------------- UNMIXED
            else
            {
                //2. ----------------------------------- FAN
                mh = ma / (1 + β);
                γ2 = adiabaticFactor.Evaluate(T02);
                cp2 = pressureFactor.Evaluate(T02);
                P03 = P02 * πf * core.coreFactor;
                T03 = T02 * (1 + ((Mathf.Pow((πf), ((γ2 - 1) / γ2))) - 1) / (nc / 100));
                float pcc3a = 1 / (1 - (1 / (nn / 100)) * ((γ2 - 1) / (γ2 + 1)));
                float pcc3b = Mathf.Pow(pcc3a, (γ2 / (γ2 - 1)));



                //3. ----------------------------------- FAN NOZZLE
                Pc = P03 / pcc3b;
                if (Pc > Pa)
                {
                    P09 = Pc;
                    T09 = T03 / pcc3a;
                    float p9 = (P09 * 1000) / (287 * T09);
                    Uef = Mathf.Sqrt(γ2 * 287f * T09);
                    fanCoreThrust = β * core.coreFactor * mh * (Uef - Uc);
                    fanPressureThrust = fAe * (P09 - Pa) * 1000f;
                }
                else
                {
                    P09 = Pa;
                    float p9c = 1 - Mathf.Pow((P09 / P03), ((γ2 - 1) / γ2));
                    T09 = T03 * (1 - ((nn / 100) * p9c));
                    float p9 = (P09 * 1000) / (287 * T09);
                    Uef = Mathf.Sqrt(γ2 * 287f * T09);
                    fanCoreThrust = β * core.coreFactor * mh * (Uef - Uc);
                }

                if (fanCoreThrust > 0 && !float.IsInfinity(fanCoreThrust) && !float.IsNaN(fanCoreThrust) &&
                   fanPressureThrust > 0 && !float.IsInfinity(fanPressureThrust) && !float.IsNaN(fanPressureThrust)) { fanThrust = fanCoreThrust + fanPressureThrust; }


                //4. ----------------------------------- COMPRESSOR
                P04 = P03 * πc;
                float t4_t3 = Mathf.Pow((πc), ((γ2 - 1) / γ2));
                T04 = T03 * (1 + ((t4_t3 - 1) / (nc / 100)));
                γ3 = adiabaticFactor.Evaluate(T04); cp3 = pressureFactor.Evaluate(T04);


                //5. ----------------------------------- COMBUSTION CHAMBER
                P05 = (1 - (pcc / 100)) * P04;
                T05 = TIT;
                γ4 = adiabaticFactor.Evaluate(T04);
                cp4 = pressureFactor.Evaluate(T04);
                float F1 = (cp4 * T05) - (cp3 * T04);
                float F2 = ((nb / 100) * Q) - (cp3 * T05);
                f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


                //6. ----------------------------------- HIGH PRESSURE TURBINE
                T06 = T05 - ((cp3 * (T04 - T03)) / ((1 + f) * cp4));
                γ5 = adiabaticFactor.Evaluate(T06); cp5 = pressureFactor.Evaluate(T06);
                float p6c = 1 - ((1 / (nhpt / 100)) * (1 - (T06 / T05)));
                p6cp5 = Mathf.Pow(p6c, (γ5 / (γ5 - 1)));
                P06 = P05 * p6cp5;


                //7. ----------------------------------- LOW PRESSURE TURBINE
                float t7_t6 = ((1 + β) * cp5 * (T03 - T02)) / ((1 + f) * cp4);
                T7 = T06 - t7_t6;
                γ6 = adiabaticFactor.Evaluate(T7); cp6 = pressureFactor.Evaluate(T7) * 1000f;
                p7c = 1 - ((1 / (nlpt / 100)) * (1 - (T7 / T06)));
                p7cp6 = Mathf.Pow(p7c, (γ6 / (γ6 - 1)));
                P7 = P06 * p7cp6;
                Wc = ((cp5 * (T06 - T7)) / (nhpt / 100f) * mh) / 745f;



                //8. ----------------------------------- NOZZLE
                pcc8a = 1 / (1 - (1 / (nn / 100)) * ((γ6 - 1) / (γ6 + 1)));
                pcc8b = Mathf.Pow(pcc8a, (γ6 / (γ6 - 1)));
                P8c = P7 / pcc8b;

                if (P8c > Pa)
                {
                    P08 = P8c;
                    T08 = T7 / pcc8a;
                    float p88 = (P08 * 1000) / (287f * T08);
                    Ue = Mathf.Sqrt(1.33f * 287 * T08);
                    coreCoreThrust = mh * ((1 + f) * Ue - Uc);
                    corePressureThrust = cAe * (P08 - Pa) * 1000f;
                }
                else
                {
                    P08 = Pa;
                    float p9c = 1 - Mathf.Pow((P08 / P7), ((γ6 - 1) / γ6));
                    T08 = T7 * (1 - ((nn / 100) * p9c));
                    Ue = Mathf.Sqrt(1.33f * 287 * T08);
                    coreCoreThrust = mh * ((1 + f) * Ue - Uc);
                    corePressureThrust = 0f;
                }

                mf = ma * f;
                if (coreCoreThrust > 0 && !float.IsInfinity(coreCoreThrust) && !float.IsNaN(coreCoreThrust)
                && fanCoreThrust > 0 && !float.IsInfinity(fanCoreThrust) && !float.IsNaN(fanCoreThrust)) { coreThrust = fanCoreThrust + coreCoreThrust; }

                if (!float.IsInfinity(corePressureThrust) && !float.IsNaN(corePressureThrust)
                && !float.IsInfinity(fanPressureThrust) && !float.IsNaN(fanPressureThrust)) { pressureThrust = fanPressureThrust + corePressureThrust; }

                engineThrust = coreThrust + pressureThrust;
                //if (controller.flightComputer.autoThrottle == SilantroFlightComputer.ControlState.Active) { engineThrust *= controller.flightComputer.processedThrottle; }
                if (engineThrust < 0) { engineThrust = 0; }
            }

            float pt = engineThrust * 0.2248f;
            if (pt > 1 && !float.IsInfinity(pt) && !float.IsNaN(pt)) { TSFC = ((mf * 3600f) / (pt * 0.4536f)); }
            if (core.afterburnerOperative && core.controlInput < 0.5f) { core.afterburnerOperative = false; }
        }
        /// <summary>
        /// 
        /// </summary>
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Collect Diameters
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, coreExhaustPercentage, out diffuserDrawDiameter, out coreExhaustDrawDiameter);
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, fanExhaustPercentage, out nullFloat, out fanExhaustDrawDiameter);

            // Draw
            Handles.color = Color.red;
            if (exitPoint != null)
            {
                Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (coreExhaustDrawDiameter / 2f));
                Handles.color = Color.red; Handles.ArrowHandleCap(0, exitPoint.position, exitPoint.rotation * Quaternion.LookRotation(-Vector3.forward), 2f, EventType.Repaint);
            }
            Handles.color = Color.blue;
            if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }

            if (engineType == EngineType.Unmixed)
            {
                Handles.color = Color.cyan;
                if (fanExhaustPoint != null) { Handles.DrawWireDisc(fanExhaustPoint.position, fanExhaustPoint.transform.forward, (fanExhaustDrawDiameter / 2f)); }
            }

            // Plot Gas Factors
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
            core.EvaluateRPMLimits();
        }
#endif

        #endregion
    }
    #endregion

    #region Editor

#if UNITY_EDITOR

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroTurbofan))]
    public class SilantroTurboFanEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroTurbofan jet;
        SerializedProperty core;
        public int toolbarTab;
        public string currentTab;
        public string pitchLabel = "Pitch";


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { jet = (SilantroTurbofan)target; core = serializedObject.FindProperty("core"); }


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
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("engineNumber"), new GUIContent("Number"));

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Engine Class", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
            GUI.color = backgroundColor;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineDiameter"), new GUIContent("Engine Diameter"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePercentage"), new GUIContent("Intake Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Intake Diameter", jet.diffuserDrawDiameter.ToString("0.000") + " m");

            if (jet.engineType == SilantroTurbofan.EngineType.Mixed)
            {
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("coreExhaustPercentage"), new GUIContent("Exhaust Ratio"));
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Exhaust Diameter", jet.coreExhaustDrawDiameter.ToString("0.000") + " m");
            }

            if (jet.engineType == SilantroTurbofan.EngineType.Unmixed)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fanExhaustPercentage"), new GUIContent("Fan Exhaust Ratio"));
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Fan Exhaust Diameter", jet.fanExhaustDrawDiameter.ToString("0.000") + " m");
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("coreExhaustPercentage"), new GUIContent("Core Exhaust Ratio"));
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Core Exhaust Diameter", jet.coreExhaustDrawDiameter.ToString("0.000") + " m");
            }

            GUILayout.Space(8f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));
            if (jet.engineType == SilantroTurbofan.EngineType.Mixed)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reheatSystem"), new GUIContent("Reheat System"));
            }


            GUILayout.Space(7f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("vectoringType"), new GUIContent("Thrust Vectoring"));




            GUILayout.Space(7f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseThrustMode"), new GUIContent("Reverse Thrust"));

            if (jet.reverseThrustMode == SilantroTurbofan.ReverseThrust.Available)
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("β"), new GUIContent("By-Pass Ratio"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("πc"), new GUIContent("Core Pressure Ratio"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("πf"), new GUIContent("Fan Pressure Ratio"));
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Pressure Drop (%)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcc"), new GUIContent("Compressor"));
            if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
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
            if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nf"), new GUIContent("Fan"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nc"), new GUIContent("Compressor"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nb"), new GUIContent("Burner"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nhpt"), new GUIContent("Turbine (HPT)"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nlpt"), new GUIContent("Turbine (LPT)"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nn"), new GUIContent("Nozzle"));
            if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nab"), new GUIContent("Afterburner"));
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Thermodynamic Performance", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPerformance"), new GUIContent("Show"));
            if (jet.showPerformance)
            {
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("AMBIENT", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Pa: " + jet.Pa.ToString("0.00") + " KPa" + " || Ta: " + jet.Ta.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("DIFFUSER", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P2: " + jet.P02.ToString("0.00") + " KPa" + " || T2: " + jet.T02.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("FAN", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P3: " + jet.P03.ToString("0.00") + " KPa" + " || T3: " + jet.T03.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("COMPRESSOR", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P4: " + jet.P04.ToString("0.00") + " KPa" + " || T4: " + jet.T04.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("BURNER", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P5: " + jet.P05.ToString("0.00") + " KPa" + " || T5: " + jet.T05.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("HIGH PRESSURE TURBINE", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P6: " + jet.P06.ToString("0.00") + " KPa" + " || T6: " + jet.T06.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox(" LOW PRESSURE TURBINE", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P7: " + jet.P7.ToString("0.00") + " KPa" + " || T7: " + jet.T7.ToString("0.00") + " °K");
                if (!jet.core.afterburnerOperative)
                {
                    GUILayout.Space(3f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("NOZZLE", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("P8: " + jet.P08.ToString("0.00") + " KPa" + " || T8: " + jet.T08.ToString("0.00") + " °K");
                }
                else
                {
                    GUILayout.Space(3f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("AFTERBURER PIPE", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("P9: " + jet.P10.ToString("0.00") + " KPa" + " || T9: " + jet.T10.ToString("0.00") + " °K");
                    GUILayout.Space(3f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("NOZZLE", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    float t10_t9 = (jet.T10 / ((jet.γ9 + 1) / 2));
                    EditorGUILayout.LabelField("P10: " + (jet.P08 / 1.94f).ToString("0.00") + " KPa" + " || T10: " + t10_t9.ToString("0.00") + " °K");
                }
                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Exhaust Gas Properties", MessageType.None);
                GUI.color = backgroundColor;
                if (jet.engineType == SilantroTurbofan.EngineType.Mixed)
                {
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("Velocity", jet.Ue.ToString("0.00") + " m/s");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Temperature (EGT)", jet.Te.ToString("0.00") + " °C");
                }
                else
                {
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("Fan Exhaust", jet.Uef.ToString("0.00") + " m/s");
                    GUILayout.Space(3f);
                    float fEGT = jet.T09 - 273.15f; float cEGT = jet.T08 - 273.15f;
                    EditorGUILayout.LabelField("Fan (EGT)", fEGT.ToString("0.00") + " °C");
                    //
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Core Exhaust", jet.Ue.ToString("0.00") + " m/s");
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Core (EGT)", cEGT.ToString("0.00") + " °C");
                }
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Flows Rates", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Intake Air", jet.ma.ToString("0.00") + " kg/s");
                if (jet.engineType == SilantroTurbofan.EngineType.Mixed)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Exhaust gas", jet.Me.ToString("0.00") + " kg/s");
                }
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Fuel", jet.mf.ToString("0.00") + " kg/s");
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





            if (jet.engineType == SilantroTurbofan.EngineType.Mixed)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));
            }
            if (jet.engineType == SilantroTurbofan.EngineType.Unmixed)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Core Exhaust Point"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fanExhaustPoint"), new GUIContent("Fan Exhaust Point"));
            }
            if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("m_actuator"), new GUIContent("Nozzle Actuator"));
            }

            if (jet.core.vectoringType != EngineCore.ThrustVectoring.None)
            {
                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Thrust Vectoring Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("nozzlePivot"), new GUIContent("Nozzle Hinge"));
                GUILayout.Space(8f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchLabel"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchAxis"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchDirection"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumPitchDeflection"), new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (jet.core.vectoringType == EngineCore.ThrustVectoring.TwoAxis || jet.core.vectoringType == EngineCore.ThrustVectoring.ThreeAxis)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("rollLabel"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("rollAxis"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("rollDirection"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumRollDeflection"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                }

                if (jet.core.vectoringType == EngineCore.ThrustVectoring.ThreeAxis)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("yawLabel"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("yawAxis"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("yawDirection"), new GUIContent(""));
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumYawDeflection"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                }
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
                EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEmission"), new GUIContent("Core Emission"));
                if (jet.core.baseEmission)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Exhaust Emission Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerCoreMaterial"), new GUIContent("Core Material"));

                    if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerPipeMaterial"), new GUIContent("Pipe Material"));
                    }
                    else
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("burnerPipeMaterial"), new GUIContent("Pipe Material"));
                    }

                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumNormalEmission"), new GUIContent("Maximum Emission"));
                    GUILayout.Space(2f);
                    if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
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

                    if (jet.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning)
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
