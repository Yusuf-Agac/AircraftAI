using System;
using UnityEngine;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    public class SilantroPropeller : MonoBehaviour
    {
        public enum Level { HighFidelity, LowFidelity }
        public enum EngineMode { Piston, Electric, Turboprop, Turboshaft }
        public enum PropellerBlendMode { None, Partial, Complete }
        public enum Type { FixedPitch, VariablePitch }
        public enum Mode { m_2BladeN640A75, m_2BladeN640A76, m_4BladeNACA641 }
        public enum FeatherState { Feathered, UnFeathered }

        public Level m_modelLevel = Level.LowFidelity;
        public EngineMode m_engineMode = EngineMode.Piston;
        /// <summary>
        /// Specifies if the propeller uses a fixed or variable pitch system i.e can the pitch of the propeller blade be adjust in flight
        /// </summary>
        [Tooltip("Specifies if the propeller uses a fixed or variable pitch system")] public Type m_type = Type.FixedPitch;
        public Mode m_mode = Mode.m_2BladeN640A75;
        public RotationAxis m_axis = RotationAxis.X;
        /// <summary>
        /// Specifies if propeller rotates clockwise or counter clockwise
        /// </summary>
        [Tooltip(" Specifies if propeller rotates clockwise or counter clockwise")] public RotationDirection m_direction = RotationDirection.CW;
        /// <summary>
        /// Specifies if the variable pitch propeller is feathered or not
        /// </summary>
        [Tooltip("Specifies if the variable pitch propeller is feathered or not")] public FeatherState m_featherState = FeatherState.Feathered;
        public List<Blade> _blades = new List<Blade>();

        //US Navy Bureau of Aeronautics 5868-9, 2 blades, blade angle at 0.75R 20 deg, NACA-TR-640
        public AnimationCurve m_CT15_75, m_CT20_75, m_CT25_75, m_CT30_75; //Thrust Coefficient 75-inch
        [HideInInspector] public AnimationCurve m_CP15_75, m_CP20_75, m_CP25_75, m_CP30_75; //Power Coefficient 75-inch
        [HideInInspector] public AnimationCurve m_CTFixed_75, m_CPFixed_75;

        //US Navy Bureau of Aeronautics 5868-9, 2 blades, blade angle at 0.75R 20 deg, NACA-TR-640
        [HideInInspector] public AnimationCurve m_CT15_76, m_CT20_76, m_CT25_76; //Thrust Coefficient 76-inch
        [HideInInspector] public AnimationCurve m_CP15_76, m_CP20_76, m_CP25_76; //Power Coefficient 76-inch
        [HideInInspector] public AnimationCurve m_CTFixed_76, m_CPFixed_76;

        //US Navy Bureau of Aeronautics 5868-9, 4 blades, NACA-TR-640, NACA-TR-641
        [HideInInspector] public AnimationCurve m_CT23_4, m_CT30_4, m_CT40_4, m_CT45_4, m_CT65_4;
        [HideInInspector] public AnimationCurve m_CP23_4, m_CP30_4, m_CP40_4, m_CP45_4, m_CP65_4;


        public double m_diameter = 1;
        private double m_gearRatio = 1, m_targetDeflection;
        /// <summary>
        /// Specifies the rotational inertia of the propeller
        /// </summary>
        [Tooltip("Specifies the rotational inertia of the propeller")] public double m_inertia = 1;
        double m_area;
        /// <summary>
        /// Specifies the rated (normal operation model) RPM of the propeller
        /// </summary>
        public double m_ratedRPM = 2500;
        public double Ωr, Ω;
        public double m_speed;

        [System.Serializable]
        public class Blade
        {
            public string _identifier;
            public Transform m_model;
            public RotationAxis m_rotationAxis = RotationAxis.X;
            [HideInInspector] public float m_deflection;
            [HideInInspector] public Vector3 m_controlAxis;
            [HideInInspector] public Quaternion m_baseRotation;
        }



        public double m_pitchInput;
        [Range(0, 5)] public double m_J;
        public double m_maxPitch;
        public double m_minPitch;
        public double m_fixedPitch = 25;
        public double m_featherAngle = 80;
        public float m_featherSpeed = 30;
        float m_commandDeflection;
        public float m_deflection;
        public Controller controller;
        public SilantroPiston m_pistonengine;
#if SILANTRO_FIXED
        public Oyedoyin.FixedWing.SilantroTurboprop m_turbopropengine;
#endif

        public Transform m_axle;
        public Material[] normalRotor;
        public Material[] blurredRotor;

        public Color blurredRotorColor;
        public Color normalRotorColor;
        public double m_alpha;
        public PropellerBlendMode blendMode = PropellerBlendMode.None;
        [Range(0.01f, 1)] public float normalBalance = 0.2f;

        public double m_coreRPM;
        public double m_ν, ma;
        public double m_CT;
        public double m_CP;
        public double m_Thrust;
        public double m_Torque;
        private double m_enginePower;






        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            PlotCurves();
            m_area = Constants.Pi * Math.Pow(0.5 * m_diameter, 2);
            if (m_type == Type.VariablePitch && _blades != null && _blades.Count > 1)
            {
                foreach (Blade _blade in _blades)
                {
                    if (_blade.m_rotationAxis == RotationAxis.X) { _blade.m_controlAxis = new Vector3(1, 0, 0); }
                    else if (_blade.m_rotationAxis == RotationAxis.Y) { _blade.m_controlAxis = new Vector3(0, 1, 0); }
                    else if (_blade.m_rotationAxis == RotationAxis.Z) { _blade.m_controlAxis = new Vector3(0, 0, 1); }
                    if (_blade.m_model != null) { _blade.m_baseRotation = _blade.m_model.localRotation; }
                }
            }

            if (blendMode == PropellerBlendMode.Complete || blendMode == PropellerBlendMode.Partial)
            {
                blurredRotorColor = blurredRotor[0].color; m_alpha = 0;
                if (normalRotor.Length > 0) { normalRotorColor = normalRotor[0].color; }
                ConfigMaterials();
            }

            if (m_engineMode == EngineMode.Piston && m_pistonengine) { m_gearRatio = m_pistonengine.core.functionalRPM / m_ratedRPM; }
#if SILANTRO_FIXED
            if (m_engineMode == EngineMode.Turboprop && m_turbopropengine) { m_gearRatio = m_turbopropengine.core.functionalRPM / m_ratedRPM; }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_timestep"></param>
        public void Compute(float _timestep)
        {
            if (controller != null)
            {
                if (m_engineMode == EngineMode.Piston && m_pistonengine)
                {
                    m_coreRPM = (float)(m_pistonengine.core.coreRPM / m_gearRatio);
                    m_pistonengine.core.inputLoad = (float)m_Torque;
                    m_enginePower = m_pistonengine.brakePower;
                }
#if SILANTRO_FIXED
                if (m_engineMode == EngineMode.Turboprop && m_turbopropengine)
                {
                    m_coreRPM = (float)(m_turbopropengine.core.coreRPM / m_gearRatio);
                    m_turbopropengine.core.inputLoad = (float)m_Torque;
                    m_enginePower = m_turbopropengine.brakePower;
                }
#endif


                AnalyseForces();
                Ωr = m_coreRPM / 60.0;
                Ω = 2 * Mathf.PI * Ωr;
                if (m_axle != null) { Handler.Rotate(m_coreRPM, _timestep, m_axle, m_direction, m_axis); }


                // --------------------------------- Rotate Blades
                if (m_type == Type.FixedPitch) { m_targetDeflection = m_fixedPitch; }
                else { m_targetDeflection = m_minPitch + ((m_maxPitch - m_minPitch) * m_pitchInput); }

                if (m_featherState == FeatherState.Feathered)
                {
                    m_commandDeflection = (float)m_featherAngle;
                }
                else { m_commandDeflection = (float)m_targetDeflection; }
                m_deflection = Mathf.MoveTowards(m_deflection, m_commandDeflection, m_featherSpeed * 0.5f * _timestep);
                AnalyseBlades(m_deflection);
                m_alpha = m_coreRPM / m_ratedRPM;
                ConfigMaterials();
            }
        }

        #region Internal Functions

        /// <summary>
        /// 
        /// </summary>
        private void AnalyseForces()
        {
            double ρ = controller.m_core.ρ;
            m_speed = controller.m_core.V;
            if (double.IsNaN(m_deflection) || double.IsInfinity(m_deflection)) { m_deflection = 0.0f; }
            if (double.IsNaN(m_J) || double.IsInfinity(m_J)) { m_J = 0.0; }

            if (m_modelLevel == Level.LowFidelity)
            {
                if (Ωr > 0)
                {
                    m_area = Constants.Pi * Math.Pow(0.5 * m_diameter, 2);
                    m_J = m_speed / (m_diameter * Ωr);
                }
                ma = (m_CT20_75.Evaluate((float)m_J)) / 0.106;
                double propellerArea = (3.142f * Math.Pow((3.28084f * m_diameter), 2f)) / 4f;
                double dynamicPower = Math.Pow((m_enginePower * 550f), 2 / 3f);
                double dynamicArea = Math.Pow((2f * ρ * 0.0624f * propellerArea), 1 / 3f);
                m_Thrust = dynamicArea * dynamicPower * ma;
                if (Ωr > 1f) { m_Torque = ((m_enginePower * 60 * 746) / (2 * 3.142f * Ωr * 1000f)) * m_diameter; }
            }
            // ------------------------------------------------- Calculate Coefficients
            else
            {
                // ----------- NACA 640 (5868-9) 75-inch Two-Blade Propeller
                if (m_mode == Mode.m_2BladeN640A75)
                {
                    if (m_type == Type.VariablePitch)
                    {
                        m_minPitch = 15;
                        m_maxPitch = 30;
                        m_CT = Interpolate4(m_CT15_75, m_CT20_75, m_CT25_75, m_CT30_75, m_J, m_deflection);
                        m_CP = Interpolate4(m_CP15_75, m_CP20_75, m_CP25_75, m_CP30_75, m_J, m_deflection);
                    }
                    if (m_type == Type.FixedPitch)
                    {
                        m_CT = Interpolate4(m_CT15_75, m_CT20_75, m_CT25_75, m_CT30_75, m_J, m_deflection);
                        m_CP = Interpolate4(m_CP15_75, m_CP20_75, m_CP25_75, m_CP30_75, m_J, m_deflection);
                    }
                }

                // ----------- NACA 640 (5868-9) 76-inch Two-Blade Propeller
                if (m_mode == Mode.m_2BladeN640A76)
                {
                    if (m_type == Type.VariablePitch)
                    {
                        m_maxPitch = 25;
                        m_minPitch = 15;
                        m_CT = Interpolate3(m_CT15_76, m_CT20_76, m_CT25_76, m_J, m_deflection);
                        m_CP = Interpolate3(m_CP15_76, m_CP20_76, m_CP25_76, m_J, m_deflection);
                    }
                    if (m_type == Type.FixedPitch)
                    {
                        m_CT = Interpolate3(m_CT15_76, m_CT20_76, m_CT25_76, m_J, m_deflection);
                        m_CP = Interpolate3(m_CP15_76, m_CP20_76, m_CP25_76, m_J, m_deflection);
                    }
                }

                // ----------- NACA-TR-641 5868-9, 4 blades
                if (m_mode == Mode.m_4BladeNACA641)
                {
                    m_maxPitch = 65;
                    m_minPitch = 23;
                    m_CT = Interpolate5(m_CT23_4, m_CT30_4, m_CT40_4, m_CT45_4, m_CT65_4, m_J, m_deflection);
                    m_CP = Interpolate5(m_CP23_4, m_CP30_4, m_CP40_4, m_CP45_4, m_CP65_4, m_J, m_deflection);
                }


                // ------------------------------------------------- Thrust
                if (Ωr > 0)
                {
                    m_area = Constants.Pi * Math.Pow(0.5 * m_diameter, 2);
                    m_J = m_speed / (m_diameter * Ωr);
                    m_Thrust = m_CT * ρ * Math.Pow(Ωr, 2) * Math.Pow(m_diameter, 4);
                }
                else { m_Thrust = 0; }

                // ------------------------------------------------- Induced Velocity
                double a = 0.5 * ρ * m_area;
                double b = ρ * m_area * m_speed;
                double c = -m_Thrust;
                double Δ = b * b - 4.0 * a * c;
                if (Δ >= 0.0) { m_ν = (-b + Math.Sqrt(Δ)) / (2.0 * a); }

                double m_power = m_CP * ρ * Math.Pow(Ωr, 3) * Math.Pow(m_diameter, 5);
                m_Torque = m_power / (Ω > 1.0 ? Ω : 1.0);
            }

            if (controller != null && m_axle != null)
            {
                Vector3 v_force = controller.transform.forward * (float)m_Thrust;
                controller.force += v_force;
                controller.m_rigidbody.AddForceAtPosition(v_force, m_axle.position, ForceMode.Force);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_deflection"></param>
        private void AnalyseBlades(float m_deflection)
        {
            if (m_type == Type.VariablePitch)
            {
                foreach (Blade _blade in _blades)
                {
                    if (_blade.m_model != null)
                    {
                        _blade.m_deflection = m_deflection;
                        _blade.m_model.localRotation = _blade.m_baseRotation;
                        _blade.m_model.Rotate(_blade.m_controlAxis, m_deflection);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlotCurves()
        {
            #region Thrust 75-inch Variable
            m_CT15_75 = new AnimationCurve();
            m_CT15_75.AddKey(new Keyframe(0.0f, 0.0990f));
            m_CT15_75.AddKey(new Keyframe(0.1f, 0.0950f));
            m_CT15_75.AddKey(new Keyframe(0.2f, 0.0880f));
            m_CT15_75.AddKey(new Keyframe(0.3f, 0.0780f));
            m_CT15_75.AddKey(new Keyframe(0.4f, 0.0645f));
            m_CT15_75.AddKey(new Keyframe(0.5f, 0.0495f));
            m_CT15_75.AddKey(new Keyframe(0.6f, 0.0340f));
            m_CT15_75.AddKey(new Keyframe(0.7f, 0.0185f));
            m_CT15_75.AddKey(new Keyframe(0.8f, 0.0040f));
            m_CT15_75.AddKey(new Keyframe(0.9f, -0.0160f));
            m_CT15_75.AddKey(new Keyframe(1.0f, -0.0300f));
            m_CT15_75.AddKey(new Keyframe(1.1f, -0.0400f));
            m_CT15_75.AddKey(new Keyframe(1.2f, -0.0500f));
            m_CT15_75.AddKey(new Keyframe(1.3f, -0.0550f));
            m_CT15_75.AddKey(new Keyframe(1.4f, -0.06f));
            m_CT15_75.AddKey(new Keyframe(1.5f, -0.0650f));
            m_CT15_75.AddKey(new Keyframe(1.6f, -0.0700f));
            m_CT15_75.AddKey(new Keyframe(2.0f, -0.0750f));
            m_CT15_75.AddKey(new Keyframe(3.0f, -0.0800f));

            m_CT20_75 = new AnimationCurve();
            m_CT20_75.AddKey(new Keyframe(0.0f, 0.1040f));
            m_CT20_75.AddKey(new Keyframe(0.1f, 0.1040f));
            m_CT20_75.AddKey(new Keyframe(0.2f, 0.1030f));
            m_CT20_75.AddKey(new Keyframe(0.3f, 0.1010f));
            m_CT20_75.AddKey(new Keyframe(0.4f, 0.0930f));
            m_CT20_75.AddKey(new Keyframe(0.5f, 0.0820f));
            m_CT20_75.AddKey(new Keyframe(0.6f, 0.0700f));
            m_CT20_75.AddKey(new Keyframe(0.7f, 0.0550f));
            m_CT20_75.AddKey(new Keyframe(0.8f, 0.0390f));
            m_CT20_75.AddKey(new Keyframe(0.9f, 0.0240f));
            m_CT20_75.AddKey(new Keyframe(1.0f, 0.0060f));
            m_CT20_75.AddKey(new Keyframe(1.1f, -0.0140f));
            m_CT20_75.AddKey(new Keyframe(1.2f, -0.0330f));
            m_CT20_75.AddKey(new Keyframe(1.3f, -0.0450f));
            m_CT20_75.AddKey(new Keyframe(1.4f, -0.0550f));
            m_CT20_75.AddKey(new Keyframe(1.5f, -0.0600f));
            m_CT20_75.AddKey(new Keyframe(1.6f, -0.0650f));
            m_CT20_75.AddKey(new Keyframe(2.0f, -0.0720f));
            m_CT20_75.AddKey(new Keyframe(3.0f, -0.0800f));

            m_CT25_75 = new AnimationCurve();
            m_CT25_75.AddKey(new Keyframe(0.0f, 0.1125f));
            m_CT25_75.AddKey(new Keyframe(0.1f, 0.1100f));
            m_CT25_75.AddKey(new Keyframe(0.2f, 0.1075f));
            m_CT25_75.AddKey(new Keyframe(0.3f, 0.1060f));
            m_CT25_75.AddKey(new Keyframe(0.4f, 0.1055f));
            m_CT25_75.AddKey(new Keyframe(0.5f, 0.1045f));
            m_CT25_75.AddKey(new Keyframe(0.6f, 0.0970f));
            m_CT25_75.AddKey(new Keyframe(0.7f, 0.0870f));
            m_CT25_75.AddKey(new Keyframe(0.8f, 0.0750f));
            m_CT25_75.AddKey(new Keyframe(0.9f, 0.0600f));
            m_CT25_75.AddKey(new Keyframe(1.0f, 0.0450f));
            m_CT25_75.AddKey(new Keyframe(1.1f, 0.0300f));
            m_CT25_75.AddKey(new Keyframe(1.2f, 0.0150f));
            m_CT25_75.AddKey(new Keyframe(1.3f, -0.0010f));
            m_CT25_75.AddKey(new Keyframe(1.4f, -0.0130f));
            m_CT25_75.AddKey(new Keyframe(1.5f, -0.0250f));
            m_CT25_75.AddKey(new Keyframe(1.6f, -0.0380f));
            m_CT25_75.AddKey(new Keyframe(2.0f, -0.0700f));
            m_CT25_75.AddKey(new Keyframe(3.0f, -0.0900f));

            m_CT30_75 = new AnimationCurve();
            m_CT30_75.AddKey(new Keyframe(0f, 0.1175f));
            m_CT30_75.AddKey(new Keyframe(0.1f, 0.1170f));
            m_CT30_75.AddKey(new Keyframe(0.2f, 0.1150f));
            m_CT30_75.AddKey(new Keyframe(0.3f, 0.1125f));
            m_CT30_75.AddKey(new Keyframe(0.4f, 0.1100f));
            m_CT30_75.AddKey(new Keyframe(0.5f, 0.1080f));
            m_CT30_75.AddKey(new Keyframe(0.6f, 0.1075f));
            m_CT30_75.AddKey(new Keyframe(0.7f, 0.1075f));
            m_CT30_75.AddKey(new Keyframe(0.8f, 0.1040f));
            m_CT30_75.AddKey(new Keyframe(0.9f, 0.0950f));
            m_CT30_75.AddKey(new Keyframe(1.0f, 0.0825f));
            m_CT30_75.AddKey(new Keyframe(1.1f, 0.0695f));
            m_CT30_75.AddKey(new Keyframe(1.2f, 0.0540f));
            m_CT30_75.AddKey(new Keyframe(1.3f, 0.0380f));
            m_CT30_75.AddKey(new Keyframe(1.4f, 0.0225f));
            m_CT30_75.AddKey(new Keyframe(1.5f, 0.0075f));
            m_CT30_75.AddKey(new Keyframe(1.6f, -0.0090f));
            m_CT30_75.AddKey(new Keyframe(2.0f, -0.0550f));
            m_CT30_75.AddKey(new Keyframe(3.0f, -0.1000f));


            m_CTFixed_75 = new AnimationCurve();
            m_CTFixed_75.AddKey(new Keyframe(0.0f, 0.068f));
            m_CTFixed_75.AddKey(new Keyframe(0.1f, 0.068f));
            m_CTFixed_75.AddKey(new Keyframe(0.2f, 0.067f));
            m_CTFixed_75.AddKey(new Keyframe(0.3f, 0.066f));
            m_CTFixed_75.AddKey(new Keyframe(0.4f, 0.064f));
            m_CTFixed_75.AddKey(new Keyframe(0.5f, 0.062f));
            m_CTFixed_75.AddKey(new Keyframe(0.6f, 0.059f));
            m_CTFixed_75.AddKey(new Keyframe(0.7f, 0.054f));
            m_CTFixed_75.AddKey(new Keyframe(0.8f, 0.043f));
            m_CTFixed_75.AddKey(new Keyframe(0.9f, 0.031f));
            m_CTFixed_75.AddKey(new Keyframe(1.0f, 0.019f));
            m_CTFixed_75.AddKey(new Keyframe(1.1f, 0.008f));
            m_CTFixed_75.AddKey(new Keyframe(1.2f, -0.001f));
            m_CTFixed_75.AddKey(new Keyframe(1.3f, -0.008f));
            m_CTFixed_75.AddKey(new Keyframe(1.4f, -0.019f));
            m_CTFixed_75.AddKey(new Keyframe(1.5f, -0.029f));
            m_CTFixed_75.AddKey(new Keyframe(1.6f, -0.040f));
            m_CTFixed_75.AddKey(new Keyframe(1.7f, -0.050f));
            m_CTFixed_75.AddKey(new Keyframe(1.8f, -0.057f));
            m_CTFixed_75.AddKey(new Keyframe(1.9f, -0.061f));
            m_CTFixed_75.AddKey(new Keyframe(2.0f, -0.064f));
            m_CTFixed_75.AddKey(new Keyframe(2.1f, -0.066f));
            m_CTFixed_75.AddKey(new Keyframe(2.2f, -0.067f));
            m_CTFixed_75.AddKey(new Keyframe(2.3f, -0.068f));
            m_CTFixed_75.AddKey(new Keyframe(5.0f, -0.068f));

            m_CPFixed_75 = new AnimationCurve();
            m_CPFixed_75.AddKey(new Keyframe(0.0f, 0.0580f));
            m_CPFixed_75.AddKey(new Keyframe(0.1f, 0.0620f));
            m_CPFixed_75.AddKey(new Keyframe(0.2f, 0.0600f));
            m_CPFixed_75.AddKey(new Keyframe(0.3f, 0.0580f));
            m_CPFixed_75.AddKey(new Keyframe(0.4f, 0.0520f));
            m_CPFixed_75.AddKey(new Keyframe(0.5f, 0.0457f));
            m_CPFixed_75.AddKey(new Keyframe(0.6f, 0.0436f));
            m_CPFixed_75.AddKey(new Keyframe(0.7f, 0.0420f));
            m_CPFixed_75.AddKey(new Keyframe(0.8f, 0.0372f));
            m_CPFixed_75.AddKey(new Keyframe(0.9f, 0.0299f));
            m_CPFixed_75.AddKey(new Keyframe(1.0f, 0.0202f));
            m_CPFixed_75.AddKey(new Keyframe(1.1f, -0.0111f));
            m_CPFixed_75.AddKey(new Keyframe(1.2f, -0.0075f));
            m_CPFixed_75.AddKey(new Keyframe(1.3f, -0.0111f));
            m_CPFixed_75.AddKey(new Keyframe(1.4f, -0.0202f));
            m_CPFixed_75.AddKey(new Keyframe(1.5f, -0.0280f));
            m_CPFixed_75.AddKey(new Keyframe(1.6f, -0.0346f));
            m_CPFixed_75.AddKey(new Keyframe(1.7f, -0.0389f));
            m_CPFixed_75.AddKey(new Keyframe(1.8f, -0.0421f));
            m_CPFixed_75.AddKey(new Keyframe(1.9f, -0.0436f));
            m_CPFixed_75.AddKey(new Keyframe(2.0f, -0.0445f));
            m_CPFixed_75.AddKey(new Keyframe(2.1f, -0.0445f));
            m_CPFixed_75.AddKey(new Keyframe(2.2f, -0.0442f));
            m_CPFixed_75.AddKey(new Keyframe(2.3f, -0.0431f));
            m_CPFixed_75.AddKey(new Keyframe(2.4f, -0.0424f));
            m_CPFixed_75.AddKey(new Keyframe(5.0f, -0.0413f));

            MathBase.SmoothCurve(m_CT15_75);
            MathBase.SmoothCurve(m_CT20_75);
            MathBase.SmoothCurve(m_CT25_75);
            MathBase.SmoothCurve(m_CT30_75);
            MathBase.SmoothCurve(m_CP15_75);
            MathBase.SmoothCurve(m_CP20_75);
            MathBase.SmoothCurve(m_CP25_75);
            MathBase.SmoothCurve(m_CP30_75);
            MathBase.LinearizeCurve(m_CTFixed_75);
            MathBase.LinearizeCurve(m_CPFixed_75);
            #endregion Thrust 75-inch Variable

            #region Power 75-inch Variable
            m_CP15_75 = new AnimationCurve();
            m_CP15_75.AddKey(new Keyframe(0.0f, 0.0400f));
            m_CP15_75.AddKey(new Keyframe(0.1f, 0.0406f));
            m_CP15_75.AddKey(new Keyframe(0.2f, 0.0406f));
            m_CP15_75.AddKey(new Keyframe(0.3f, 0.0400f));
            m_CP15_75.AddKey(new Keyframe(0.4f, 0.0366f));
            m_CP15_75.AddKey(new Keyframe(0.5f, 0.0318f));
            m_CP15_75.AddKey(new Keyframe(0.6f, 0.0250f));
            m_CP15_75.AddKey(new Keyframe(0.7f, 0.0160f));
            m_CP15_75.AddKey(new Keyframe(0.8f, 0.0050f));
            m_CP15_75.AddKey(new Keyframe(0.9f, -0.0067f));
            m_CP15_75.AddKey(new Keyframe(1.0f, -0.0150f));
            m_CP15_75.AddKey(new Keyframe(1.1f, -0.0200f));
            m_CP15_75.AddKey(new Keyframe(1.2f, -0.0250f));
            m_CP15_75.AddKey(new Keyframe(1.3f, -0.0270f));
            m_CP15_75.AddKey(new Keyframe(1.4f, -0.0285f));
            m_CP15_75.AddKey(new Keyframe(1.5f, -0.0300f));
            m_CP15_75.AddKey(new Keyframe(1.6f, -0.0315f));
            m_CP15_75.AddKey(new Keyframe(2.0f, -0.0330f));
            m_CP15_75.AddKey(new Keyframe(3.0f, -0.0350f));

            m_CP20_75 = new AnimationCurve();
            m_CP20_75.AddKey(new Keyframe(0.0f, 0.0660f));
            m_CP20_75.AddKey(new Keyframe(0.1f, 0.0650f));
            m_CP20_75.AddKey(new Keyframe(0.2f, 0.0640f));
            m_CP20_75.AddKey(new Keyframe(0.3f, 0.0625f));
            m_CP20_75.AddKey(new Keyframe(0.4f, 0.0600f));
            m_CP20_75.AddKey(new Keyframe(0.5f, 0.0580f));
            m_CP20_75.AddKey(new Keyframe(0.6f, 0.0540f));
            m_CP20_75.AddKey(new Keyframe(0.7f, 0.0475f));
            m_CP20_75.AddKey(new Keyframe(0.8f, 0.0370f));
            m_CP20_75.AddKey(new Keyframe(0.9f, 0.0260f));
            m_CP20_75.AddKey(new Keyframe(1.0f, 0.0100f));
            m_CP20_75.AddKey(new Keyframe(1.1f, 0.0000f));
            m_CP20_75.AddKey(new Keyframe(1.2f, -0.0100f));
            m_CP20_75.AddKey(new Keyframe(1.3f, -0.0200f));
            m_CP20_75.AddKey(new Keyframe(1.4f, -0.0278f));
            m_CP20_75.AddKey(new Keyframe(1.5f, -0.0350f));
            m_CP20_75.AddKey(new Keyframe(1.6f, -0.0390f));
            m_CP20_75.AddKey(new Keyframe(2.0f, -0.0500f));
            m_CP20_75.AddKey(new Keyframe(3.0f, -0.0550f));

            m_CP25_75 = new AnimationCurve();
            m_CP25_75.AddKey(new Keyframe(0.0f, 0.1080f));
            m_CP25_75.AddKey(new Keyframe(0.1f, 0.1060f));
            m_CP25_75.AddKey(new Keyframe(0.2f, 0.1020f));
            m_CP25_75.AddKey(new Keyframe(0.3f, 0.0975f));
            m_CP25_75.AddKey(new Keyframe(0.4f, 0.0910f));
            m_CP25_75.AddKey(new Keyframe(0.5f, 0.0860f));
            m_CP25_75.AddKey(new Keyframe(0.6f, 0.0825f));
            m_CP25_75.AddKey(new Keyframe(0.7f, 0.0790f));
            m_CP25_75.AddKey(new Keyframe(0.8f, 0.0730f));
            m_CP25_75.AddKey(new Keyframe(0.9f, 0.0640f));
            m_CP25_75.AddKey(new Keyframe(1.0f, 0.0520f));
            m_CP25_75.AddKey(new Keyframe(1.1f, 0.0375f));
            m_CP25_75.AddKey(new Keyframe(1.2f, 0.0220f));
            m_CP25_75.AddKey(new Keyframe(1.3f, 0.0040f));
            m_CP25_75.AddKey(new Keyframe(1.4f, -0.02f));
            m_CP25_75.AddKey(new Keyframe(1.5f, -0.0400f));
            m_CP25_75.AddKey(new Keyframe(1.6f, -0.0530f));
            m_CP25_75.AddKey(new Keyframe(2.0f, -0.0750f));
            m_CP25_75.AddKey(new Keyframe(3.0f, -0.1000f));

            m_CP30_75 = new AnimationCurve();
            m_CP30_75.AddKey(new Keyframe(000f, 0.1420f));
            m_CP30_75.AddKey(new Keyframe(0.1f, 0.1405f));
            m_CP30_75.AddKey(new Keyframe(0.2f, 0.1380f));
            m_CP30_75.AddKey(new Keyframe(0.3f, 0.1360f));
            m_CP30_75.AddKey(new Keyframe(0.4f, 0.1330f));
            m_CP30_75.AddKey(new Keyframe(0.5f, 0.1280f));
            m_CP30_75.AddKey(new Keyframe(0.6f, 0.1230f));
            m_CP30_75.AddKey(new Keyframe(0.7f, 0.1180f));
            m_CP30_75.AddKey(new Keyframe(0.8f, 0.1140f));
            m_CP30_75.AddKey(new Keyframe(0.9f, 0.1080f));
            m_CP30_75.AddKey(new Keyframe(1.0f, 0.1000f));
            m_CP30_75.AddKey(new Keyframe(1.1f, 0.0895f));
            m_CP30_75.AddKey(new Keyframe(1.2f, 0.0750f));
            m_CP30_75.AddKey(new Keyframe(1.3f, 0.0580f));
            m_CP30_75.AddKey(new Keyframe(1.4f, 0.0380f));
            m_CP30_75.AddKey(new Keyframe(1.5f, 0.0180f));
            m_CP30_75.AddKey(new Keyframe(1.6f, -0.0070f));
            m_CP30_75.AddKey(new Keyframe(2.0f, -0.0750f));
            m_CP30_75.AddKey(new Keyframe(3.0f, -0.1200f));


            MathBase.SmoothCurve(m_CP15_75);
            MathBase.SmoothCurve(m_CP20_75);
            MathBase.SmoothCurve(m_CP25_75);
            MathBase.SmoothCurve(m_CP30_75);
            #endregion Power 75-inch Variable

            #region Thrust 76-inch Variable
            m_CT15_76 = new AnimationCurve();
            m_CT15_76.AddKey(new Keyframe(0.0f, 0.0990f));
            m_CT15_76.AddKey(new Keyframe(0.1f, 0.0950f));
            m_CT15_76.AddKey(new Keyframe(0.2f, 0.0880f));
            m_CT15_76.AddKey(new Keyframe(0.3f, 0.0780f));
            m_CT15_76.AddKey(new Keyframe(0.4f, 0.0645f));
            m_CT15_76.AddKey(new Keyframe(0.5f, 0.0495f));
            m_CT15_76.AddKey(new Keyframe(0.6f, 0.0340f));
            m_CT15_76.AddKey(new Keyframe(0.7f, 0.0185f));
            m_CT15_76.AddKey(new Keyframe(0.8f, 0.0040f));
            m_CT15_76.AddKey(new Keyframe(0.9f, -0.0160f));
            m_CT15_76.AddKey(new Keyframe(1.0f, -0.0300f));
            m_CT15_76.AddKey(new Keyframe(1.1f, -0.0400f));
            m_CT15_76.AddKey(new Keyframe(1.2f, -0.0500f));
            m_CT15_76.AddKey(new Keyframe(1.3f, -0.0550f));
            m_CT15_76.AddKey(new Keyframe(1.5f, -0.0650f));
            m_CT15_76.AddKey(new Keyframe(2.0f, -0.0750f));
            m_CT15_76.AddKey(new Keyframe(3.0f, -0.0800f));

            m_CT20_76 = new AnimationCurve();
            m_CT20_76.AddKey(new Keyframe(0.0f, 0.1040f));
            m_CT20_76.AddKey(new Keyframe(0.1f, 0.1040f));
            m_CT20_76.AddKey(new Keyframe(0.2f, 0.1030f));
            m_CT20_76.AddKey(new Keyframe(0.3f, 0.1010f));
            m_CT20_76.AddKey(new Keyframe(0.4f, 0.0930f));
            m_CT20_76.AddKey(new Keyframe(0.5f, 0.0820f));
            m_CT20_76.AddKey(new Keyframe(0.6f, 0.0700f));
            m_CT20_76.AddKey(new Keyframe(0.7f, 0.0550f));
            m_CT20_76.AddKey(new Keyframe(0.8f, 0.0390f));
            m_CT20_76.AddKey(new Keyframe(0.9f, 0.0240f));
            m_CT20_76.AddKey(new Keyframe(1.0f, 0.0060f));
            m_CT20_76.AddKey(new Keyframe(1.1f, -0.0140f));
            m_CT20_76.AddKey(new Keyframe(1.2f, -0.0330f));
            m_CT20_76.AddKey(new Keyframe(1.3f, -0.0450f));
            m_CT20_76.AddKey(new Keyframe(1.5f, -0.0600f));
            m_CT20_76.AddKey(new Keyframe(2.0f, -0.0720f));
            m_CT20_76.AddKey(new Keyframe(3.0f, -0.0800f));

            m_CT25_76 = new AnimationCurve();
            m_CT25_76.AddKey(new Keyframe(0.0f, 0.1125f));
            m_CT25_76.AddKey(new Keyframe(0.1f, 0.1100f));
            m_CT25_76.AddKey(new Keyframe(0.2f, 0.1075f));
            m_CT25_76.AddKey(new Keyframe(0.3f, 0.1060f));
            m_CT25_76.AddKey(new Keyframe(0.4f, 0.1055f));
            m_CT25_76.AddKey(new Keyframe(0.5f, 0.1045f));
            m_CT25_76.AddKey(new Keyframe(0.6f, 0.0970f));
            m_CT25_76.AddKey(new Keyframe(0.7f, 0.0870f));
            m_CT25_76.AddKey(new Keyframe(0.8f, 0.0750f));
            m_CT25_76.AddKey(new Keyframe(0.9f, 0.0600f));
            m_CT25_76.AddKey(new Keyframe(1.0f, 0.0450f));
            m_CT25_76.AddKey(new Keyframe(1.1f, 0.0300f));
            m_CT25_76.AddKey(new Keyframe(1.2f, 0.0150f));
            m_CT25_76.AddKey(new Keyframe(1.3f, -0.0010f));
            m_CT25_76.AddKey(new Keyframe(1.5f, -0.0250f));
            m_CT25_76.AddKey(new Keyframe(2.0f, -0.0750f));
            m_CT25_76.AddKey(new Keyframe(3.0f, -0.0900f));

            m_CTFixed_76 = new AnimationCurve();
            m_CTFixed_76.AddKey(new Keyframe(0.0f, 0.1040f));
            m_CTFixed_76.AddKey(new Keyframe(0.1f, 0.1040f));
            m_CTFixed_76.AddKey(new Keyframe(0.2f, 0.1030f));
            m_CTFixed_76.AddKey(new Keyframe(0.3f, 0.1010f));
            m_CTFixed_76.AddKey(new Keyframe(0.4f, 0.0930f));
            m_CTFixed_76.AddKey(new Keyframe(0.5f, 0.0820f));
            m_CTFixed_76.AddKey(new Keyframe(0.6f, 0.0700f));
            m_CTFixed_76.AddKey(new Keyframe(0.7f, 0.0550f));
            m_CTFixed_76.AddKey(new Keyframe(0.8f, 0.0390f));
            m_CTFixed_76.AddKey(new Keyframe(0.9f, 0.0240f));
            m_CTFixed_76.AddKey(new Keyframe(1.0f, 0.0060f));
            m_CTFixed_76.AddKey(new Keyframe(1.1f, -0.0140f));
            m_CTFixed_76.AddKey(new Keyframe(1.2f, -0.0330f));
            m_CTFixed_76.AddKey(new Keyframe(1.3f, -0.0450f));
            m_CTFixed_76.AddKey(new Keyframe(1.5f, -0.0600f));
            m_CTFixed_76.AddKey(new Keyframe(2.0f, -0.0720f));
            m_CTFixed_76.AddKey(new Keyframe(3.0f, -0.0800f));
            #endregion

            #region Power 76-inch Variable
            m_CP15_76 = new AnimationCurve();
            m_CP15_76.AddKey(new Keyframe(0.0f, 0.0400f));
            m_CP15_76.AddKey(new Keyframe(0.1f, 0.0406f));
            m_CP15_76.AddKey(new Keyframe(0.2f, 0.0406f));
            m_CP15_76.AddKey(new Keyframe(0.3f, 0.0400f));
            m_CP15_76.AddKey(new Keyframe(0.4f, 0.0366f));
            m_CP15_76.AddKey(new Keyframe(0.5f, 0.0318f));
            m_CP15_76.AddKey(new Keyframe(0.6f, 0.0250f));
            m_CP15_76.AddKey(new Keyframe(0.7f, 0.0160f));
            m_CP15_76.AddKey(new Keyframe(0.8f, 0.0050f));
            m_CP15_76.AddKey(new Keyframe(0.9f, -0.0067f));
            m_CP15_76.AddKey(new Keyframe(1.0f, -0.0150f));
            m_CP15_76.AddKey(new Keyframe(1.1f, -0.0200f));
            m_CP15_76.AddKey(new Keyframe(1.2f, -0.0250f));
            m_CP15_76.AddKey(new Keyframe(1.3f, -0.0270f));
            m_CP15_76.AddKey(new Keyframe(1.5f, -0.0300f));
            m_CP15_76.AddKey(new Keyframe(2.0f, -0.0330f));

            m_CP20_76 = new AnimationCurve();
            m_CP20_76.AddKey(new Keyframe(0.0f, 0.0660f));
            m_CP20_76.AddKey(new Keyframe(0.1f, 0.0650f));
            m_CP20_76.AddKey(new Keyframe(0.2f, 0.0640f));
            m_CP20_76.AddKey(new Keyframe(0.3f, 0.0625f));
            m_CP20_76.AddKey(new Keyframe(0.4f, 0.0600f));
            m_CP20_76.AddKey(new Keyframe(0.5f, 0.0580f));
            m_CP20_76.AddKey(new Keyframe(0.6f, 0.0540f));
            m_CP20_76.AddKey(new Keyframe(0.7f, 0.0475f));
            m_CP20_76.AddKey(new Keyframe(0.8f, 0.0370f));
            m_CP20_76.AddKey(new Keyframe(0.9f, 0.0260f));
            m_CP20_76.AddKey(new Keyframe(1.0f, 0.0100f));
            m_CP20_76.AddKey(new Keyframe(1.1f, 0.0000f));
            m_CP20_76.AddKey(new Keyframe(1.2f, -0.0100f));
            m_CP20_76.AddKey(new Keyframe(1.3f, -0.0200f));
            m_CP20_76.AddKey(new Keyframe(1.5f, -0.0350f));
            m_CP20_76.AddKey(new Keyframe(2.0f, -0.0500f));

            m_CP25_76 = new AnimationCurve();
            m_CP25_76.AddKey(new Keyframe(0.0f, 0.1080f));
            m_CP25_76.AddKey(new Keyframe(0.1f, 0.1060f));
            m_CP25_76.AddKey(new Keyframe(0.2f, 0.1020f));
            m_CP25_76.AddKey(new Keyframe(0.3f, 0.0975f));
            m_CP25_76.AddKey(new Keyframe(0.4f, 0.0910f));
            m_CP25_76.AddKey(new Keyframe(0.5f, 0.0860f));
            m_CP25_76.AddKey(new Keyframe(0.6f, 0.0825f));
            m_CP25_76.AddKey(new Keyframe(0.7f, 0.0790f));
            m_CP25_76.AddKey(new Keyframe(0.8f, 0.0730f));
            m_CP25_76.AddKey(new Keyframe(0.9f, 0.0640f));
            m_CP25_76.AddKey(new Keyframe(1.0f, 0.0520f));
            m_CP25_76.AddKey(new Keyframe(1.1f, 0.0375f));
            m_CP25_76.AddKey(new Keyframe(1.2f, 0.0220f));
            m_CP25_76.AddKey(new Keyframe(1.3f, 0.0040f));
            m_CP25_76.AddKey(new Keyframe(1.5f, -0.0450f));
            m_CP25_76.AddKey(new Keyframe(2.0f, -0.0750f));

            m_CPFixed_76 = new AnimationCurve();
            m_CPFixed_76.AddKey(new Keyframe(0.0f, 0.0660f));
            m_CPFixed_76.AddKey(new Keyframe(0.1f, 0.0650f));
            m_CPFixed_76.AddKey(new Keyframe(0.2f, 0.0640f));
            m_CPFixed_76.AddKey(new Keyframe(0.3f, 0.0625f));
            m_CPFixed_76.AddKey(new Keyframe(0.4f, 0.0600f));
            m_CPFixed_76.AddKey(new Keyframe(0.5f, 0.0580f));
            m_CPFixed_76.AddKey(new Keyframe(0.6f, 0.0540f));
            m_CPFixed_76.AddKey(new Keyframe(0.7f, 0.0475f));
            m_CPFixed_76.AddKey(new Keyframe(0.8f, 0.0370f));
            m_CPFixed_76.AddKey(new Keyframe(0.9f, 0.0260f));
            m_CPFixed_76.AddKey(new Keyframe(1.0f, 0.0100f));
            m_CPFixed_76.AddKey(new Keyframe(1.1f, 0.0000f));
            m_CPFixed_76.AddKey(new Keyframe(1.2f, -0.0100f));
            m_CPFixed_76.AddKey(new Keyframe(1.3f, -0.0200f));
            m_CPFixed_76.AddKey(new Keyframe(1.5f, -0.0350f));
            m_CPFixed_76.AddKey(new Keyframe(2.0f, -0.0500f));
            m_CPFixed_76.AddKey(new Keyframe(3.0f, -0.0550f));

            MathBase.SmoothCurve(m_CT15_76);
            MathBase.SmoothCurve(m_CT20_76);
            MathBase.SmoothCurve(m_CT25_76);
            MathBase.SmoothCurve(m_CP15_76);
            MathBase.SmoothCurve(m_CP20_76);
            MathBase.SmoothCurve(m_CP25_76);
            MathBase.LinearizeCurve(m_CTFixed_76);
            MathBase.LinearizeCurve(m_CPFixed_76);
            #endregion


            #region Thrust 4-Blade Variable
            m_CT23_4 = new AnimationCurve();
            m_CT23_4.AddKey(new Keyframe(0.0f, 0.192f));
            m_CT23_4.AddKey(new Keyframe(0.1f, 0.190f));
            m_CT23_4.AddKey(new Keyframe(0.2f, 0.188f));
            m_CT23_4.AddKey(new Keyframe(0.3f, 0.185f));
            m_CT23_4.AddKey(new Keyframe(0.4f, 0.178f));
            m_CT23_4.AddKey(new Keyframe(0.5f, 0.167f));
            m_CT23_4.AddKey(new Keyframe(0.6f, 0.150f));
            m_CT23_4.AddKey(new Keyframe(0.7f, 0.128f));
            m_CT23_4.AddKey(new Keyframe(0.8f, 0.105f));
            m_CT23_4.AddKey(new Keyframe(0.9f, 0.080f));
            m_CT23_4.AddKey(new Keyframe(1.0f, 0.053f));
            m_CT23_4.AddKey(new Keyframe(1.1f, 0.025f));
            m_CT23_4.AddKey(new Keyframe(1.2f, -0.002f));
            m_CT23_4.AddKey(new Keyframe(1.3f, -0.030f));
            m_CT23_4.AddKey(new Keyframe(1.4f, -0.057f));
            m_CT23_4.AddKey(new Keyframe(1.5f, -0.087f));
            m_CT23_4.AddKey(new Keyframe(1.6f, -0.116f));
            m_CT23_4.AddKey(new Keyframe(1.7f, -0.144f));
            m_CT23_4.AddKey(new Keyframe(1.8f, -0.171f));
            m_CT23_4.AddKey(new Keyframe(1.9f, -0.197f));
            m_CT23_4.AddKey(new Keyframe(2.0f, -0.223f));
            m_CT23_4.AddKey(new Keyframe(2.1f, -0.247f));
            m_CT23_4.AddKey(new Keyframe(2.2f, -0.271f));
            m_CT23_4.AddKey(new Keyframe(2.3f, -0.296f));
            m_CT23_4.AddKey(new Keyframe(2.4f, -0.321f));
            m_CT23_4.AddKey(new Keyframe(2.5f, -0.346f));
            m_CT23_4.AddKey(new Keyframe(3.0f, -0.484f));
            m_CT23_4.AddKey(new Keyframe(3.5f, -0.637f));
            m_CT23_4.AddKey(new Keyframe(4.0f, -0.804f));
            m_CT23_4.AddKey(new Keyframe(4.5f, -1.005f));
            m_CT23_4.AddKey(new Keyframe(5.0f, -1.206f));

            m_CT30_4 = new AnimationCurve();
            m_CT30_4.AddKey(new Keyframe(0.0f, 0.208f));
            m_CT30_4.AddKey(new Keyframe(0.1f, 0.206f));
            m_CT30_4.AddKey(new Keyframe(0.2f, 0.205f));
            m_CT30_4.AddKey(new Keyframe(0.3f, 0.202f));
            m_CT30_4.AddKey(new Keyframe(0.4f, 0.200f));
            m_CT30_4.AddKey(new Keyframe(0.5f, 0.197f));
            m_CT30_4.AddKey(new Keyframe(0.6f, 0.196f));
            m_CT30_4.AddKey(new Keyframe(0.7f, 0.193f));
            m_CT30_4.AddKey(new Keyframe(0.8f, 0.181f));
            m_CT30_4.AddKey(new Keyframe(0.9f, 0.163f));
            m_CT30_4.AddKey(new Keyframe(1.0f, 0.142f));
            m_CT30_4.AddKey(new Keyframe(1.1f, 0.120f));
            m_CT30_4.AddKey(new Keyframe(1.2f, 0.098f));
            m_CT30_4.AddKey(new Keyframe(1.3f, 0.072f));
            m_CT30_4.AddKey(new Keyframe(1.4f, 0.043f));
            m_CT30_4.AddKey(new Keyframe(1.5f, 0.013f));
            m_CT30_4.AddKey(new Keyframe(1.6f, -0.010f));
            m_CT30_4.AddKey(new Keyframe(1.7f, -0.034f));
            m_CT30_4.AddKey(new Keyframe(1.8f, -0.063f));
            m_CT30_4.AddKey(new Keyframe(1.9f, -0.091f));
            m_CT30_4.AddKey(new Keyframe(2.0f, -0.119f));
            m_CT30_4.AddKey(new Keyframe(2.1f, -0.145f));
            m_CT30_4.AddKey(new Keyframe(2.2f, -0.171f));
            m_CT30_4.AddKey(new Keyframe(2.3f, -0.194f));
            m_CT30_4.AddKey(new Keyframe(2.4f, -0.216f));
            m_CT30_4.AddKey(new Keyframe(2.5f, -0.239f));
            m_CT30_4.AddKey(new Keyframe(3.0f, -0.358f));
            m_CT30_4.AddKey(new Keyframe(3.5f, -0.492f));
            m_CT30_4.AddKey(new Keyframe(4.0f, -0.639f));
            m_CT30_4.AddKey(new Keyframe(4.5f, -0.820f));
            m_CT30_4.AddKey(new Keyframe(5.0f, -1.000f));

            m_CT40_4 = new AnimationCurve();
            m_CT40_4.AddKey(new Keyframe(0.0f, 0.219f));
            m_CT40_4.AddKey(new Keyframe(0.1f, 0.219f));
            m_CT40_4.AddKey(new Keyframe(0.2f, 0.219f));
            m_CT40_4.AddKey(new Keyframe(0.3f, 0.218f));
            m_CT40_4.AddKey(new Keyframe(0.4f, 0.218f));
            m_CT40_4.AddKey(new Keyframe(0.5f, 0.216f));
            m_CT40_4.AddKey(new Keyframe(0.6f, 0.214f));
            m_CT40_4.AddKey(new Keyframe(0.7f, 0.211f));
            m_CT40_4.AddKey(new Keyframe(0.8f, 0.206f));
            m_CT40_4.AddKey(new Keyframe(0.9f, 0.201f));
            m_CT40_4.AddKey(new Keyframe(1.0f, 0.197f));
            m_CT40_4.AddKey(new Keyframe(1.1f, 0.196f));
            m_CT40_4.AddKey(new Keyframe(1.2f, 0.194f));
            m_CT40_4.AddKey(new Keyframe(1.3f, 0.188f));
            m_CT40_4.AddKey(new Keyframe(1.4f, 0.175f));
            m_CT40_4.AddKey(new Keyframe(1.5f, 0.159f));
            m_CT40_4.AddKey(new Keyframe(1.6f, 0.138f));
            m_CT40_4.AddKey(new Keyframe(1.7f, 0.115f));
            m_CT40_4.AddKey(new Keyframe(1.8f, 0.093f));
            m_CT40_4.AddKey(new Keyframe(1.9f, 0.070f));
            m_CT40_4.AddKey(new Keyframe(2.0f, 0.048f));
            m_CT40_4.AddKey(new Keyframe(2.1f, 0.024f));
            m_CT40_4.AddKey(new Keyframe(2.2f, 0.001f));
            m_CT40_4.AddKey(new Keyframe(2.3f, -0.023f));
            m_CT40_4.AddKey(new Keyframe(2.4f, -0.047f));
            m_CT40_4.AddKey(new Keyframe(2.5f, -0.071f));
            m_CT40_4.AddKey(new Keyframe(3.0f, -0.185f));
            m_CT40_4.AddKey(new Keyframe(3.5f, -0.286f));
            m_CT40_4.AddKey(new Keyframe(4.0f, -0.395f));
            m_CT40_4.AddKey(new Keyframe(4.5f, -0.530f));
            m_CT40_4.AddKey(new Keyframe(5.0f, -0.666f));


            m_CT45_4 = new AnimationCurve();
            m_CT45_4.AddKey(new Keyframe(0.0f, 0.230f));
            m_CT45_4.AddKey(new Keyframe(0.1f, 0.230f));
            m_CT45_4.AddKey(new Keyframe(0.2f, 0.230f));
            m_CT45_4.AddKey(new Keyframe(0.3f, 0.229f));
            m_CT45_4.AddKey(new Keyframe(0.4f, 0.228f));
            m_CT45_4.AddKey(new Keyframe(0.5f, 0.226f));
            m_CT45_4.AddKey(new Keyframe(0.6f, 0.222f));
            m_CT45_4.AddKey(new Keyframe(0.7f, 0.219f));
            m_CT45_4.AddKey(new Keyframe(0.8f, 0.214f));
            m_CT45_4.AddKey(new Keyframe(0.9f, 0.208f));
            m_CT45_4.AddKey(new Keyframe(1.0f, 0.203f));
            m_CT45_4.AddKey(new Keyframe(1.1f, 0.198f));
            m_CT45_4.AddKey(new Keyframe(1.2f, 0.195f));
            m_CT45_4.AddKey(new Keyframe(1.3f, 0.194f));
            m_CT45_4.AddKey(new Keyframe(1.4f, 0.193f));
            m_CT45_4.AddKey(new Keyframe(1.5f, 0.191f));
            m_CT45_4.AddKey(new Keyframe(1.6f, 0.185f));
            m_CT45_4.AddKey(new Keyframe(1.7f, 0.174f));
            m_CT45_4.AddKey(new Keyframe(1.8f, 0.160f));
            m_CT45_4.AddKey(new Keyframe(1.9f, 0.143f));
            m_CT45_4.AddKey(new Keyframe(2.0f, 0.125f));
            m_CT45_4.AddKey(new Keyframe(2.1f, 0.108f));
            m_CT45_4.AddKey(new Keyframe(2.2f, 0.089f));
            m_CT45_4.AddKey(new Keyframe(2.3f, 0.068f));
            m_CT45_4.AddKey(new Keyframe(2.4f, 0.048f));
            m_CT45_4.AddKey(new Keyframe(2.5f, 0.027f));
            m_CT45_4.AddKey(new Keyframe(3.0f, -0.085f));
            m_CT45_4.AddKey(new Keyframe(3.5f, -0.193f));
            m_CT45_4.AddKey(new Keyframe(4.0f, -0.291f));
            m_CT45_4.AddKey(new Keyframe(4.5f, -0.410f));
            m_CT45_4.AddKey(new Keyframe(5.0f, -0.529f));

            m_CT65_4 = new AnimationCurve();
            m_CT65_4.AddKey(new Keyframe(0.0f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.1f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.2f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.3f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.4f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.5f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.6f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.7f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.8f, 0.247f));
            m_CT65_4.AddKey(new Keyframe(0.9f, 0.246f));
            m_CT65_4.AddKey(new Keyframe(1.0f, 0.244f));
            m_CT65_4.AddKey(new Keyframe(1.1f, 0.242f));
            m_CT65_4.AddKey(new Keyframe(1.2f, 0.238f));
            m_CT65_4.AddKey(new Keyframe(1.3f, 0.235f));
            m_CT65_4.AddKey(new Keyframe(1.4f, 0.229f));
            m_CT65_4.AddKey(new Keyframe(1.5f, 0.224f));
            m_CT65_4.AddKey(new Keyframe(1.6f, 0.219f));
            m_CT65_4.AddKey(new Keyframe(1.7f, 0.214f));
            m_CT65_4.AddKey(new Keyframe(1.8f, 0.212f));
            m_CT65_4.AddKey(new Keyframe(1.9f, 0.211f));
            m_CT65_4.AddKey(new Keyframe(2.0f, 0.209f));
            m_CT65_4.AddKey(new Keyframe(2.1f, 0.206f));
            m_CT65_4.AddKey(new Keyframe(2.2f, 0.199f));
            m_CT65_4.AddKey(new Keyframe(2.3f, 0.187f));
            m_CT65_4.AddKey(new Keyframe(2.4f, 0.172f));
            m_CT65_4.AddKey(new Keyframe(2.5f, 0.154f));
            m_CT65_4.AddKey(new Keyframe(3.0f, 0.058f));
            m_CT65_4.AddKey(new Keyframe(3.5f, -0.052f));
            m_CT65_4.AddKey(new Keyframe(4.0f, -0.162f));
            m_CT65_4.AddKey(new Keyframe(4.5f, -0.260f));
            m_CT65_4.AddKey(new Keyframe(5.0f, -0.377f));

            MathBase.LinearizeCurve(m_CT23_4);
            MathBase.LinearizeCurve(m_CT30_4);
            MathBase.LinearizeCurve(m_CT40_4);
            MathBase.LinearizeCurve(m_CT45_4);
            MathBase.LinearizeCurve(m_CT65_4);
            #endregion

            #region Power 4-Blade Variable
            m_CP23_4 = new AnimationCurve();
            m_CP23_4.AddKey(new Keyframe(0.0f, 0.153f));
            m_CP23_4.AddKey(new Keyframe(0.1f, 0.151f));
            m_CP23_4.AddKey(new Keyframe(0.2f, 0.148f));
            m_CP23_4.AddKey(new Keyframe(0.3f, 0.146f));
            m_CP23_4.AddKey(new Keyframe(0.4f, 0.143f));
            m_CP23_4.AddKey(new Keyframe(0.5f, 0.138f));
            m_CP23_4.AddKey(new Keyframe(0.6f, 0.131f));
            m_CP23_4.AddKey(new Keyframe(0.7f, 0.121f));
            m_CP23_4.AddKey(new Keyframe(0.8f, 0.106f));
            m_CP23_4.AddKey(new Keyframe(0.9f, 0.088f));
            m_CP23_4.AddKey(new Keyframe(1.0f, 0.065f));
            m_CP23_4.AddKey(new Keyframe(1.1f, 0.038f));
            m_CP23_4.AddKey(new Keyframe(1.2f, 0.009f));
            m_CP23_4.AddKey(new Keyframe(1.3f, -0.019f));
            m_CP23_4.AddKey(new Keyframe(1.4f, -0.049f));
            m_CP23_4.AddKey(new Keyframe(1.5f, -0.078f));
            m_CP23_4.AddKey(new Keyframe(1.6f, -0.105f));
            m_CP23_4.AddKey(new Keyframe(1.7f, -0.129f));
            m_CP23_4.AddKey(new Keyframe(1.8f, -0.153f));
            m_CP23_4.AddKey(new Keyframe(1.9f, -0.176f));
            m_CP23_4.AddKey(new Keyframe(2.0f, -0.199f));
            m_CP23_4.AddKey(new Keyframe(2.1f, -0.222f));
            m_CP23_4.AddKey(new Keyframe(2.2f, -0.245f));
            m_CP23_4.AddKey(new Keyframe(2.3f, -0.269f));
            m_CP23_4.AddKey(new Keyframe(2.4f, -0.293f));
            m_CP23_4.AddKey(new Keyframe(2.5f, -0.317f));
            m_CP23_4.AddKey(new Keyframe(3.0f, -0.453f));
            m_CP23_4.AddKey(new Keyframe(3.5f, -0.606f));
            m_CP23_4.AddKey(new Keyframe(4.0f, -0.770f));
            m_CP23_4.AddKey(new Keyframe(4.5f, -0.965f));
            m_CP23_4.AddKey(new Keyframe(5.0f, -1.160f));

            m_CP30_4 = new AnimationCurve();
            m_CP30_4.AddKey(new Keyframe(0.0f, 0.266f));
            m_CP30_4.AddKey(new Keyframe(0.1f, 0.256f));
            m_CP30_4.AddKey(new Keyframe(0.2f, 0.246f));
            m_CP30_4.AddKey(new Keyframe(0.3f, 0.236f));
            m_CP30_4.AddKey(new Keyframe(0.4f, 0.228f));
            m_CP30_4.AddKey(new Keyframe(0.5f, 0.222f));
            m_CP30_4.AddKey(new Keyframe(0.6f, 0.219f));
            m_CP30_4.AddKey(new Keyframe(0.7f, 0.213f));
            m_CP30_4.AddKey(new Keyframe(0.8f, 0.204f));
            m_CP30_4.AddKey(new Keyframe(0.9f, 0.193f));
            m_CP30_4.AddKey(new Keyframe(1.0f, 0.178f));
            m_CP30_4.AddKey(new Keyframe(1.1f, 0.160f));
            m_CP30_4.AddKey(new Keyframe(1.2f, 0.139f));
            m_CP30_4.AddKey(new Keyframe(1.3f, 0.110f));
            m_CP30_4.AddKey(new Keyframe(1.4f, 0.074f));
            m_CP30_4.AddKey(new Keyframe(1.5f, 0.038f));
            m_CP30_4.AddKey(new Keyframe(1.6f, 0.000f));
            m_CP30_4.AddKey(new Keyframe(1.7f, -0.040f));
            m_CP30_4.AddKey(new Keyframe(1.8f, -0.084f));
            m_CP30_4.AddKey(new Keyframe(1.9f, -0.116f));
            m_CP30_4.AddKey(new Keyframe(2.0f, -0.146f));
            m_CP30_4.AddKey(new Keyframe(2.1f, -0.174f));
            m_CP30_4.AddKey(new Keyframe(2.2f, -0.202f));
            m_CP30_4.AddKey(new Keyframe(2.3f, -0.230f));
            m_CP30_4.AddKey(new Keyframe(2.4f, -0.257f));
            m_CP30_4.AddKey(new Keyframe(2.5f, -0.285f));
            m_CP30_4.AddKey(new Keyframe(3.0f, -0.437f));
            m_CP30_4.AddKey(new Keyframe(3.5f, -0.611f));
            m_CP30_4.AddKey(new Keyframe(4.0f, -0.803f));
            m_CP30_4.AddKey(new Keyframe(4.5f, -1.036f));
            m_CP30_4.AddKey(new Keyframe(5.0f, -1.270f));

            m_CP40_4 = new AnimationCurve();
            m_CP40_4.AddKey(new Keyframe(0.0f, 0.384f));
            m_CP40_4.AddKey(new Keyframe(0.1f, 0.384f));
            m_CP40_4.AddKey(new Keyframe(0.2f, 0.382f));
            m_CP40_4.AddKey(new Keyframe(0.3f, 0.380f));
            m_CP40_4.AddKey(new Keyframe(0.4f, 0.378f));
            m_CP40_4.AddKey(new Keyframe(0.5f, 0.375f));
            m_CP40_4.AddKey(new Keyframe(0.6f, 0.370f));
            m_CP40_4.AddKey(new Keyframe(0.7f, 0.364f));
            m_CP40_4.AddKey(new Keyframe(0.8f, 0.355f));
            m_CP40_4.AddKey(new Keyframe(0.9f, 0.346f));
            m_CP40_4.AddKey(new Keyframe(1.0f, 0.341f));
            m_CP40_4.AddKey(new Keyframe(1.1f, 0.337f));
            m_CP40_4.AddKey(new Keyframe(1.2f, 0.332f));
            m_CP40_4.AddKey(new Keyframe(1.3f, 0.323f));
            m_CP40_4.AddKey(new Keyframe(1.4f, 0.310f));
            m_CP40_4.AddKey(new Keyframe(1.5f, 0.291f));
            m_CP40_4.AddKey(new Keyframe(1.6f, 0.265f));
            m_CP40_4.AddKey(new Keyframe(1.7f, 0.235f));
            m_CP40_4.AddKey(new Keyframe(1.8f, 0.201f));
            m_CP40_4.AddKey(new Keyframe(1.9f, 0.164f));
            m_CP40_4.AddKey(new Keyframe(2.0f, 0.128f));
            m_CP40_4.AddKey(new Keyframe(2.1f, 0.089f));
            m_CP40_4.AddKey(new Keyframe(2.2f, 0.048f));
            m_CP40_4.AddKey(new Keyframe(2.3f, 0.004f));
            m_CP40_4.AddKey(new Keyframe(2.4f, -0.052f));
            m_CP40_4.AddKey(new Keyframe(2.5f, -0.108f));
            m_CP40_4.AddKey(new Keyframe(3.0f, -0.327f));
            m_CP40_4.AddKey(new Keyframe(3.5f, -0.508f));
            m_CP40_4.AddKey(new Keyframe(4.0f, -0.696f));
            m_CP40_4.AddKey(new Keyframe(4.5f, -0.928f));
            m_CP40_4.AddKey(new Keyframe(5.0f, -1.160f));

            m_CP45_4 = new AnimationCurve();
            m_CP45_4.AddKey(new Keyframe(0.0f, 0.483f));
            m_CP45_4.AddKey(new Keyframe(0.1f, 0.481f));
            m_CP45_4.AddKey(new Keyframe(0.2f, 0.478f));
            m_CP45_4.AddKey(new Keyframe(0.3f, 0.475f));
            m_CP45_4.AddKey(new Keyframe(0.4f, 0.471f));
            m_CP45_4.AddKey(new Keyframe(0.5f, 0.465f));
            m_CP45_4.AddKey(new Keyframe(0.6f, 0.457f));
            m_CP45_4.AddKey(new Keyframe(0.7f, 0.448f));
            m_CP45_4.AddKey(new Keyframe(0.8f, 0.439f));
            m_CP45_4.AddKey(new Keyframe(0.9f, 0.430f));
            m_CP45_4.AddKey(new Keyframe(1.0f, 0.422f));
            m_CP45_4.AddKey(new Keyframe(1.1f, 0.415f));
            m_CP45_4.AddKey(new Keyframe(1.2f, 0.409f));
            m_CP45_4.AddKey(new Keyframe(1.3f, 0.405f));
            m_CP45_4.AddKey(new Keyframe(1.4f, 0.404f));
            m_CP45_4.AddKey(new Keyframe(1.5f, 0.402f));
            m_CP45_4.AddKey(new Keyframe(1.6f, 0.393f));
            m_CP45_4.AddKey(new Keyframe(1.7f, 0.376f));
            m_CP45_4.AddKey(new Keyframe(1.8f, 0.355f));
            m_CP45_4.AddKey(new Keyframe(1.9f, 0.330f));
            m_CP45_4.AddKey(new Keyframe(2.0f, 0.304f));
            m_CP45_4.AddKey(new Keyframe(2.1f, 0.276f));
            m_CP45_4.AddKey(new Keyframe(2.2f, 0.245f));
            m_CP45_4.AddKey(new Keyframe(2.3f, 0.208f));
            m_CP45_4.AddKey(new Keyframe(2.4f, 0.165f));
            m_CP45_4.AddKey(new Keyframe(2.5f, 0.121f));
            m_CP45_4.AddKey(new Keyframe(3.0f, -0.157f));
            m_CP45_4.AddKey(new Keyframe(3.5f, -0.397f));
            m_CP45_4.AddKey(new Keyframe(4.0f, -0.601f));
            m_CP45_4.AddKey(new Keyframe(4.5f, -0.819f));
            m_CP45_4.AddKey(new Keyframe(5.0f, -1.036f));

            m_CP65_4 = new AnimationCurve();
            m_CP65_4.AddKey(new Keyframe(0.0f, 0.672f));
            m_CP65_4.AddKey(new Keyframe(0.1f, 0.670f));
            m_CP65_4.AddKey(new Keyframe(0.2f, 0.667f));
            m_CP65_4.AddKey(new Keyframe(0.3f, 0.664f));
            m_CP65_4.AddKey(new Keyframe(0.4f, 0.660f));
            m_CP65_4.AddKey(new Keyframe(0.5f, 0.654f));
            m_CP65_4.AddKey(new Keyframe(0.6f, 0.646f));
            m_CP65_4.AddKey(new Keyframe(0.7f, 0.637f));
            m_CP65_4.AddKey(new Keyframe(0.8f, 0.628f));
            m_CP65_4.AddKey(new Keyframe(0.9f, 0.619f));
            m_CP65_4.AddKey(new Keyframe(1.0f, 0.611f));
            m_CP65_4.AddKey(new Keyframe(1.1f, 0.604f));
            m_CP65_4.AddKey(new Keyframe(1.2f, 0.598f));
            m_CP65_4.AddKey(new Keyframe(1.3f, 0.594f));
            m_CP65_4.AddKey(new Keyframe(1.4f, 0.593f));
            m_CP65_4.AddKey(new Keyframe(1.5f, 0.591f));
            m_CP65_4.AddKey(new Keyframe(1.6f, 0.582f));
            m_CP65_4.AddKey(new Keyframe(1.7f, 0.565f));
            m_CP65_4.AddKey(new Keyframe(1.8f, 0.544f));
            m_CP65_4.AddKey(new Keyframe(1.9f, 0.519f));
            m_CP65_4.AddKey(new Keyframe(2.0f, 0.493f));
            m_CP65_4.AddKey(new Keyframe(2.1f, 0.465f));
            m_CP65_4.AddKey(new Keyframe(2.2f, 0.434f));
            m_CP65_4.AddKey(new Keyframe(2.3f, 0.397f));
            m_CP65_4.AddKey(new Keyframe(2.4f, 0.354f));
            m_CP65_4.AddKey(new Keyframe(2.5f, 0.310f));
            m_CP65_4.AddKey(new Keyframe(3.0f, 0.032f));
            m_CP65_4.AddKey(new Keyframe(3.5f, -0.208f));
            m_CP65_4.AddKey(new Keyframe(4.0f, -0.412f));
            m_CP65_4.AddKey(new Keyframe(4.5f, -0.630f));
            m_CP65_4.AddKey(new Keyframe(5.0f, -0.847f));

            MathBase.LinearizeCurve(m_CP23_4);
            MathBase.LinearizeCurve(m_CP30_4);
            MathBase.LinearizeCurve(m_CP40_4);
            MathBase.LinearizeCurve(m_CP45_4);
            MathBase.LinearizeCurve(m_CP65_4);
            #endregion

        }
        /// <summary>
        /// 
        /// </summary>
        private void ConfigMaterials()
        {
            float m_al = (float)m_alpha;
            // ---------------------------------- Prop Visuals
            if (blendMode == PropellerBlendMode.Complete)
            {
                if (blurredRotor != null && normalRotor != null)
                {
                    foreach (Material brotor in blurredRotor) { brotor.color = new Color(blurredRotorColor.r, blurredRotorColor.g, blurredRotorColor.b, m_al); }
                    foreach (Material nrotor in normalRotor) { nrotor.color = new Color(normalRotorColor.r, normalRotorColor.g, normalRotorColor.b, (1 - m_al) + normalBalance); }
                }
            }
            if (blendMode == PropellerBlendMode.Partial)
            {
                if (blurredRotor != null)
                {
                    foreach (Material brotor in blurredRotor) { brotor.color = new Color(blurredRotorColor.r, blurredRotorColor.g, blurredRotorColor.b, m_al); }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_1"></param>
        /// <param name="m_2"></param>
        /// <param name="m_3"></param>
        /// <param name="m_4"></param>
        /// <param name="m_5"></param>
        /// <param name="J"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        private double Interpolate5(AnimationCurve m_1, AnimationCurve m_2, AnimationCurve m_3, AnimationCurve m_4, AnimationCurve m_5, double J, double P)
        {
            double m_f1 = m_1.Evaluate((float)J); //23.0
            double m_f2 = m_2.Evaluate((float)J); //30.0
            double m_f3 = m_3.Evaluate((float)J); //40.0
            double m_f4 = m_4.Evaluate((float)J); //45.0
            double m_f5 = m_5.Evaluate((float)J); //65.0
            double m_t = 0;
            if (P < 23) { m_t = MathBase.Interpolate(P, m_f1, 0, 23, 0); }
            if (P >= 23.0 && P < 30.0) { m_t = MathBase.Interpolate(P, m_f2, m_f1, 30.0, 23.0); }
            if (P >= 30.0 && P < 40.0) { m_t = MathBase.Interpolate(P, m_f3, m_f2, 40.0, 30.0); }
            if (P >= 40.0 && P < 45.0) { m_t = MathBase.Interpolate(P, m_f4, m_f3, 45.0, 40.0); }
            if (P >= 45.0 && P <= 065) { m_t = MathBase.Interpolate(P, m_f5, m_f4, 65.0, 45.0); }
            if (P > 65) { m_t = m_f5; }
            return m_t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_1"></param>
        /// <param name="m_2"></param>
        /// <param name="m_3"></param>
        /// <param name="m_4"></param>
        /// <param name="J"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        private double Interpolate4(AnimationCurve m_1, AnimationCurve m_2, AnimationCurve m_3, AnimationCurve m_4, double J, double P)
        {
            double m_f1 = m_1.Evaluate((float)J); //15
            double m_f2 = m_2.Evaluate((float)J); //20
            double m_f3 = m_3.Evaluate((float)J); //25
            double m_f4 = m_4.Evaluate((float)J); //30
            double m_t = 0;
            if (P < 15) { m_t = MathBase.Interpolate(P, m_f1, 0, 15, 0); }
            if (P >= 15 && P < 20) { m_t = MathBase.Interpolate(P, m_f2, m_f1, 20, 15); }
            if (P >= 20 && P < 25) { m_t = MathBase.Interpolate(P, m_f3, m_f2, 25, 20); }
            if (P >= 25 && P <= 30) { m_t = MathBase.Interpolate(P, m_f4, m_f3, 30, 25); }
            if (P > 25) { m_t = m_f4; }
            return m_t;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_1"></param>
        /// <param name="m_2"></param>
        /// <param name="m_3"></param>
        /// <param name="J"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        private double Interpolate3(AnimationCurve m_1, AnimationCurve m_2, AnimationCurve m_3, double J, double P)
        {
            double m_f1 = m_1.Evaluate((float)J); //15
            double m_f2 = m_2.Evaluate((float)J); //20
            double m_f3 = m_3.Evaluate((float)J); //25
            double m_t = 0;
            if (P < 15) { m_t = MathBase.Interpolate(P, m_f1, 0, 15, 0); }
            if (P >= 15 && P < 20) { m_t = MathBase.Interpolate(P, m_f2, m_f1, 20, 15); }
            if (P >= 20 && P <= 25) { m_t = MathBase.Interpolate(P, m_f3, m_f2, 25, 20); }
            if (P > 25) { m_t = m_f3; }
            return m_t;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() 
        {
            // Handles.color = Color.red; Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.LookRotation(Vector3.forward), 2f, EventType.Repaint);
            //
            // // -------------------------------------------- Draw Propeller
            // if (m_axle != null)
            // {
            //     float propRadius = (float)m_diameter / 2f;
            //     float hub = ((float)m_diameter * 0.2f) / 2f;
            //     if (m_axle != null)
            //     {
            //         Handles.color = Color.red; Handles.DrawWireDisc(m_axle.position, this.transform.forward, propRadius);
            //         Handles.color = Color.cyan; Handles.DrawWireDisc(m_axle.position, this.transform.forward, hub);
            //     }
            //
            //     // -------------------------------------------- Draw Disc and Blades
            //     int Nb = _blades.Count;
            //     if (Nb < 2) { Nb = 2; }
            //     float sectorAngle = 360 / Nb;
            //     for (int i = 0; i < Nb; i++)
            //     {
            //         float currentSector = sectorAngle * (i + 1);
            //         Quaternion sectorRotation = Quaternion.AngleAxis(currentSector, this.transform.forward);
            //         Vector3 sectorTipPosition = m_axle.position + (sectorRotation * (this.transform.right * propRadius));
            //         Debug.DrawLine(m_axle.position, sectorTipPosition, Color.yellow);
            //         Handles.color = Color.yellow; Handles.ArrowHandleCap(0, sectorTipPosition, transform.rotation * Quaternion.LookRotation(-Vector3.forward), 0.3f, EventType.Repaint);
            //     }
            // }

        }
#endif

        #endregion
    }
    #endregion

    #region Editor


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroPropeller))]
    public class PropellerEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroPropeller propeller;
        SerializedProperty bladeList;

        private static readonly GUIContent deleteButton = new GUIContent("Remove", "Delete");
        private static readonly GUILayoutOption buttonWidth = GUILayout.Width(60f);

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { propeller = (SilantroPropeller)target; bladeList = serializedObject.FindProperty("_blades"); }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            GUILayout.Space(3f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Power", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_modelLevel"), new GUIContent("Mode"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_engineMode"), new GUIContent("Engine"));
            if (propeller.m_engineMode == SilantroPropeller.EngineMode.Piston)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pistonengine"), new GUIContent(" "));
            }
#if SILANTRO_FIXED
            if (propeller.m_engineMode == SilantroPropeller.EngineMode.Turboprop)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_turbopropengine"), new GUIContent(" "));
            }
#endif

            GUILayout.Space(15f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Propeller Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent("Type"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mode"), new GUIContent("Mode"));


            GUILayout.Space(20f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_diameter"), new GUIContent("Diameter"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_inertia"), new GUIContent("Inertia"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ratedRPM"), new GUIContent("Rated RPM"));

            if (propeller.m_modelLevel == SilantroPropeller.Level.HighFidelity)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Pitch Settings", MessageType.None);
                GUI.color = backgroundColor;
                if (propeller.m_type == SilantroPropeller.Type.FixedPitch)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_fixedPitch"), new GUIContent("Blade Pitch"));
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maxPitch"), new GUIContent("Maximum Pitch"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_minPitch"), new GUIContent("Minimum Pitch"));

                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_featherAngle"), new GUIContent("Feather Angle"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_featherSpeed"), new GUIContent("Feather Speed (°/s)"));
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Feather State", propeller.m_featherState.ToString());
                }
            }

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Rotation Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axle"), new GUIContent("Axle"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axis"), new GUIContent("Rotation Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_direction"), new GUIContent("Rotation Direction"));



            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Blade Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if (bladeList != null) { EditorGUILayout.LabelField("Blade Count", bladeList.arraySize.ToString()); }
            GUILayout.Space(5f);
            if (GUILayout.Button("Create Wheel")) { propeller._blades.Add(new SilantroPropeller.Blade()); }

            //--------------------------------------------WHEEL ELEMENTS
            if (bladeList != null)
            {
                GUILayout.Space(3f);
                //DISPLAY WHEEL ELEMENTS
                for (int i = 0; i < bladeList.arraySize; i++)
                {
                    SerializedProperty reference = bladeList.GetArrayElementAtIndex(i);

                    GUI.color = new Color(1, 0.8f, 0);
                    EditorGUILayout.HelpBox("Blade : " + (i + 1).ToString(), MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(reference.FindPropertyRelative("_identifier"), new GUIContent("iD"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_model"), new GUIContent("Hinge"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(reference.FindPropertyRelative("m_rotationAxis"), new GUIContent("Rotation Axis"));

                    GUILayout.Space(3f);
                    if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
                    { propeller._blades.RemoveAt(i); }
                    GUILayout.Space(10f);
                }
            }


            GUILayout.Space(15f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Blur Effect", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blendMode"), new GUIContent("Blend Mode"));

            if (propeller.blendMode != SilantroPropeller.PropellerBlendMode.None)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("normalBalance"), new GUIContent("Normal Balance"));
            }
            if (propeller.blendMode == SilantroPropeller.PropellerBlendMode.Complete)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Blurred Prop Materials", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                //
                SerializedProperty bmaterials = serializedObject.FindProperty("blurredRotor");
                GUIContent barrelLabel = new GUIContent("Material Count");
                //
                EditorGUILayout.PropertyField(bmaterials.FindPropertyRelative("Array.size"), barrelLabel);
                GUILayout.Space(3f);
                for (int i = 0; i < bmaterials.arraySize; i++)
                {
                    GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                    EditorGUILayout.PropertyField(bmaterials.GetArrayElementAtIndex(i), label);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                //
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Normal Prop Materials", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                //
                SerializedProperty nmaterials = serializedObject.FindProperty("normalRotor");
                GUIContent nbarrelLabel = new GUIContent("Material Count");
                //
                EditorGUILayout.PropertyField(nmaterials.FindPropertyRelative("Array.size"), nbarrelLabel);
                GUILayout.Space(3f);
                for (int i = 0; i < nmaterials.arraySize; i++)
                {
                    GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                    EditorGUILayout.PropertyField(nmaterials.GetArrayElementAtIndex(i), label);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            if (propeller.blendMode == SilantroPropeller.PropellerBlendMode.Partial)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Blurred Prop Materials", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                //
                SerializedProperty bmaterials = serializedObject.FindProperty("blurredRotor");
                GUIContent barrelLabel = new GUIContent("Material Count");
                //
                EditorGUILayout.PropertyField(bmaterials.FindPropertyRelative("Array.size"), barrelLabel);
                GUILayout.Space(3f);
                for (int i = 0; i < bmaterials.arraySize; i++)
                {
                    GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                    EditorGUILayout.PropertyField(bmaterials.GetArrayElementAtIndex(i), label);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }



            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Output", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Speed", propeller.m_coreRPM.ToString("0.00") + " RPM");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Thrust", propeller.m_Thrust.ToString("0.00") + " N");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Torque", propeller.m_Torque.ToString("0.00") + " Nm");


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    #endregion
}