using System;
using UnityEngine;
using Oyedoyin.Common;
using Oyedoyin.Analysis;
using Oyedoyin.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Handles the collection and organization of all the connected aircraft components
/// </summary>
/// <remarks>
/// This component will collect the components connected to the aircraft root and set them up with the variables and components they
/// need to function properly. It also runs the core control functions in the dependent child components
/// </remarks>

namespace Oyedoyin.FixedWing
{
    #region Component
    public class FixedController : Controller
    {
        #region Properties
        public enum JetType { Turbofan, Turboprop, Turbojet }

        public JetType jetType = JetType.Turbofan;

        [Header("Connections")]
        public Fuselage m_fuselage;
        public FixedController m_aircraft;
        public FixedComputer m_computer;
        public SilantroAerofoil[] m_wings;
        public SilantroPropeller[] m_propellers;
        public SilantroTurbofan[] m_fans;
        public SilantroTurbojet[] m_jets;
        public SilantroTurboprop[] m_props;
        private SilantroAerofoil m_flapWing, m_slatWing, m_spoilerWing;

        [Header("Data")]
        public float m_flapDeflection;
        public float m_slatDeflection;
        public float m_spoilerDeflection;
        public float m_flapLimit, m_slatLimit, m_spoilerLimit;


        #endregion

        #region Components

        [Serializable]
        public class Fuselage
        {
            [Header("Connections")]
            public Controller _aircraft;
            public SilantroAerofoil _rightWing;
            public SilantroAerofoil _leftWing;

            [Header("Data")]
            public float m_referenceLength = 10;
            [Range(0, 100)] public float m_forwardLength = 10;
            [Range(0, 100)] public float m_rearLength = 10;
            [Range(0, 100)] public float m_meanDiameter = 10;

            public float Lf, d, F, Sfront;
            public float m_gapSpan;
            public double m_gapArea;
            public double m_wingArea;
            public double m_wingSpan;
            public double m_aspectRatio;
            public double Clβ = -0.08f;
            public double CYβ = -.07563;
            public double Cnβ = -.0080;
            private double[] Cαsb;
            private double[] CXsb;
            private double[] CZsb;
            private double[] Cmsb;
            private AnimationCurve ΔCXsb;
            private AnimationCurve ΔCZsb;
            private AnimationCurve ΔCmsb;
            private AnimationCurve ΔCXgear, ΔCZgear;

            public double meanChord;
            public double m_CXsb;
            public double m_CZsb;
            public double m_Cmsb;
            public double m_CXgear = -0.0344;
            public double m_CZgear = 0.00015;
            public double m_gearNorm;
            public double m_speedbrakeNorm;

            [Header("Coefficients")]
            public double βRad;
            public double δCLδα, CLα;
            public double CX, CY, CZ, Cl, Cm, Cn;


            [Header("Output")]
            bool initialized;
            public Vector m_componentForce, m_componentMoment;
            public Vector m_force, m_moment;
            public double FX, FY, FZ;

            /// <summary>
            /// 
            /// </summary>
            public void Initialize()
            {
                float clα = 5.73f;
                if (_leftWing != null && _rightWing != null)
                {
                    AerofoilDesign._cellPoints(_rightWing, _rightWing.m_cells[0], out Vector3 m_leading_rootR, out Vector3 m_trailing_rootR, out _, out _);
                    AerofoilDesign._cellPoints(_leftWing, _leftWing.m_cells[0], out Vector3 m_leading_rootL, out Vector3 m_trailing_rootL, out _, out _);
                    m_gapSpan = Vector3.Distance(_leftWing.m_cells[0].m_rootCenter, _rightWing.m_cells[0].m_rootCenter);
                    m_gapArea = MathBase.EstimatePanelSectionArea(m_leading_rootL, m_leading_rootR, m_trailing_rootL, m_trailing_rootR);
                    m_wingSpan = Vector3.Distance(_leftWing.m_cells[_leftWing.m_cells.Count - 1].m_tipCenter,
                        _rightWing.m_cells[_rightWing.m_cells.Count - 1].m_tipCenter);
                    m_wingArea = _leftWing.m_area + _rightWing.m_area + m_gapArea;
                    m_aspectRatio = (m_wingSpan * m_wingSpan) / m_wingArea;
                    if (_leftWing.rootAirfoil != null && _rightWing.rootAirfoil != null)
                    {
                        clα = (_leftWing.rootAirfoil.centerLiftSlope + _rightWing.rootAirfoil.centerLiftSlope) * 0.5f;
                    }
                    double lmc = MathBase.EstimateMeanChord(_leftWing.m_rootChord, _leftWing.m_tipChord);
                    double rmc = MathBase.EstimateMeanChord(_leftWing.m_rootChord, _leftWing.m_tipChord);
                    meanChord = (lmc + rmc) * 0.5;
                }

                float m_fl = m_referenceLength * (m_forwardLength * 0.01f);
                float m_rl = m_referenceLength * (m_rearLength * 0.01f);
                Lf = m_fl + m_rl;
                d = m_meanDiameter * 0.01f * m_referenceLength;
                Sfront = 0.25f * Mathf.PI * d * d;
                F = Lf / d;
                δCLδα = 1 + (0.25f * (d / m_wingSpan)) - (0.025f * Math.Pow((d / m_wingSpan), 2));
                CLα = δCLδα * clα;

                #region Component Coefficients

                Cαsb = new double[] { -20, -15, -10, -05, 000, 005, 010, 015, 020, 025, 030, 035, 040, 045, 050, 055, 060, 070, 080, 090 };
                // -------------- Delta CX speed brake vs alpha
                CXsb = new double[] { -0.0101, -0.0101, -0.0101, -0.01010, -0.01010, -0.03580, -0.07900, -0.12270, -0.18270, -0.18920, -0.19880, -0.20000, -0.18740, -0.16730, -0.14760, -0.13100, -0.12790, -0.13250, -0.12500, -0.12500 };
                // -------------- Delta CZ speed brake vs alpha
                CZsb = new double[] { -0.3858, -0.3858, -0.3858, -0.38580, -0.38580, -0.26850, -0.30210, -0.42480, -0.20940, -0.09690, 0.04380f, 0.09470f, 0.00140f, -0.00970, -0.01530, -0.05200, -0.00100, -0.02020, -0.03690, -0.03690 };
                // -------------- Cm speed brake vs alpha
                Cmsb = new double[] { -0.0034, -0.0034, -0.0034, -0.0034, -0.00340, 0.02890, 0.02150f, 0.01220f, 0.02410f, 0.02630f, -0.01630, -0.04280, -0.07040, -0.08440, -0.07890, -0.06030, -0.04500, -0.05780, -0.01070, -0.01070 };

                ΔCXsb = MathBase.PlotCurveArray(Cαsb, CXsb);
                ΔCZsb = MathBase.PlotCurveArray(Cαsb, CZsb);
                ΔCmsb = MathBase.PlotCurveArray(Cαsb, Cmsb);
                MathBase.LinearizeCurve(ΔCXsb);
                MathBase.LinearizeCurve(ΔCZsb);
                MathBase.LinearizeCurve(ΔCmsb);

                ΔCZgear = new AnimationCurve();
                ΔCZgear.AddKey(new Keyframe(-20.0f, 0.0118f));
                ΔCZgear.AddKey(new Keyframe(-15.0f, 0.0089f));
                ΔCZgear.AddKey(new Keyframe(-10.0f, 0.0060f));
                ΔCZgear.AddKey(new Keyframe(-5.0f, 0.0030f));
                ΔCZgear.AddKey(new Keyframe(0.0f, 0.0000f));
                ΔCZgear.AddKey(new Keyframe(5.0f, -0.0030f));
                ΔCZgear.AddKey(new Keyframe(10.0f, -0.0060f));
                ΔCZgear.AddKey(new Keyframe(15.0f, -0.0089f));
                ΔCZgear.AddKey(new Keyframe(20.0f, -0.0118f));
                ΔCZgear.AddKey(new Keyframe(25.0f, -0.0145f));
                ΔCZgear.AddKey(new Keyframe(30.0f, -0.0172f));
                ΔCZgear.AddKey(new Keyframe(35.0f, -0.0197f));
                ΔCZgear.AddKey(new Keyframe(40.0f, -0.0221f));
                ΔCZgear.AddKey(new Keyframe(45.0f, -0.0243f));
                ΔCZgear.AddKey(new Keyframe(50.0f, -0.0264f));
                ΔCZgear.AddKey(new Keyframe(55.0f, -0.0282f));
                ΔCZgear.AddKey(new Keyframe(60.0f, -0.0298f));
                ΔCZgear.AddKey(new Keyframe(70.0f, -0.0323f));
                ΔCZgear.AddKey(new Keyframe(80.0f, -0.0339f));
                ΔCZgear.AddKey(new Keyframe(90.0f, -0.0344f));

                ΔCXgear = new AnimationCurve();
                ΔCXgear.AddKey(new Keyframe(-20.0f, -0.0323f));
                ΔCXgear.AddKey(new Keyframe(15.0f, -0.0332f));
                ΔCXgear.AddKey(new Keyframe(10.0f, -0.0339f));
                ΔCXgear.AddKey(new Keyframe(-5.0f, -0.0343f));
                ΔCXgear.AddKey(new Keyframe(0.0f, -0.0344f));
                ΔCXgear.AddKey(new Keyframe(5.0f, -0.0343f));
                ΔCXgear.AddKey(new Keyframe(10.0f, -0.0339f));
                ΔCXgear.AddKey(new Keyframe(15.0f, -0.0332f));
                ΔCXgear.AddKey(new Keyframe(20.0f, -0.0323f));
                ΔCXgear.AddKey(new Keyframe(25.0f, -0.0312f));
                ΔCXgear.AddKey(new Keyframe(30.0f, -0.0298f));
                ΔCXgear.AddKey(new Keyframe(35.0f, -0.0282f));
                ΔCXgear.AddKey(new Keyframe(40.0f, -0.0264f));
                ΔCXgear.AddKey(new Keyframe(45.0f, -0.0243f));
                ΔCXgear.AddKey(new Keyframe(50.0f, -0.0221f));
                ΔCXgear.AddKey(new Keyframe(55.0f, -0.0197f));
                ΔCXgear.AddKey(new Keyframe(60.0f, -0.0172f));
                ΔCXgear.AddKey(new Keyframe(70.0f, -0.0118f));
                ΔCXgear.AddKey(new Keyframe(80.0f, -0.0060f));
                ΔCXgear.AddKey(new Keyframe(90.0f, 0.0000f));

                MathBase.LinearizeCurve(ΔCZgear);
                MathBase.LinearizeCurve(ΔCXgear);

                #endregion

                initialized = true;
            }
            /// <summary>
            /// 
            /// </summary>
            public void Compute()
            {
                if (initialized)
                {
                    double α = _aircraft.m_core.α;
                    m_CXsb = ΔCXsb.Evaluate((float)MathBase.Clamp(α, -20, 90));
                    m_CZsb = ΔCZsb.Evaluate((float)MathBase.Clamp(α, -20, 90));
                    m_Cmsb = ΔCmsb.Evaluate((float)MathBase.Clamp(α, -20, 90));
                    if (_aircraft.m_gearMode == GearMode.Retractable)
                    {
                        m_CZgear = ΔCZgear.Evaluate((float)MathBase.Clamp(α, -20, 90));
                        m_CXgear = ΔCXgear.Evaluate((float)MathBase.Clamp(α, -20, 90));
                    }
                    else { m_gearNorm = 1; }


                    // Coefficient Sum
                    βRad = _aircraft.m_core.βRad;
                    Cl = (Clβ * βRad);
                    if (_aircraft.m_core.z < 10) { CY = (CYβ * βRad); }
                    Cn = (Cnβ * βRad);
                    CX = (m_CXsb * m_speedbrakeNorm) + (m_CXgear * m_gearNorm);
                    CZ = (m_CZsb * m_speedbrakeNorm * 0.1) + (m_CZgear * m_gearNorm);
                    Cm = (m_Cmsb * m_speedbrakeNorm);

                    // Force Sum
                    double Fx = _aircraft.m_core.Qdyn * m_wingArea * CX;
                    double Fy = _aircraft.m_core.Qdyn * m_wingArea * CY;
                    double Fz = _aircraft.m_core.Qdyn * m_wingArea * CZ;
                    if (double.IsNaN(Fx) || double.IsInfinity(Fx)) { Fx = 0.0; }
                    if (double.IsNaN(Fy) || double.IsInfinity(Fy)) { Fy = 0.0; }
                    if (double.IsNaN(Fz) || double.IsInfinity(Fz)) { Fz = 0.0; }
                    m_componentForce = new Vector(Fx, Fy, Fz);

                    double L = Cl * _aircraft.m_core.Qdyn * m_wingArea * m_wingSpan;
                    double M = Cm * _aircraft.m_core.Qdyn * m_wingArea * meanChord;
                    double N = Cn * _aircraft.m_core.Qdyn * m_wingArea * m_wingSpan;
                    if (double.IsNaN(L) || double.IsInfinity(L)) { L = 0.0f; }
                    if (double.IsNaN(M) || double.IsInfinity(M)) { M = 0.0f; }
                    if (double.IsNaN(N) || double.IsInfinity(N)) { N = 0.0f; }
                    m_componentMoment = new Vector(L, M, N);

                    // Collect Data
                    double ub = _aircraft.m_core.u;
                    double vb = _aircraft.m_core.v;
                    double wb = _aircraft.m_core.w;

                    double m_Sref = 4.75;
                    double m_CLαf = 1.2;
                    double m_lref = 3.62;
                    double m_CMαf = 7.5e-3;

                    double ρ = 1.225;
                    double m_fe1 = 1.54;
                    double m_fe2 = 6.16;
                    double m_fe3 = 5.82;

                    double phx = 0.5 * ρ;
                    double D1 = phx * m_fe1;
                    double D2 = phx * m_fe2;
                    double D3 = phx * m_fe3;
                    double L1 = phx * m_Sref * m_CLαf;
                    double M1 = phx * m_Sref * m_lref * m_CMαf;
                    double N1 = M1;

                    FX = ub * ((-D1 * Math.Abs(ub)) + (L1 * (Math.Pow(wb, 2) / Math.Abs(ub))));
                    FY = vb * ((-D2 * Math.Abs(vb)) - (L1 * Math.Abs(ub)));
                    FZ = wb * ((-D3 * Math.Abs(wb)) - (L1 * Math.Abs(ub)));
                    if (double.IsNaN(FX) || double.IsInfinity(FX)) { FX = 0.0; }
                    if (double.IsNaN(FY) || double.IsInfinity(FY)) { FY = 0.0; }
                    if (double.IsNaN(FZ) || double.IsInfinity(FZ)) { FZ = 0.0; }
                    //m_force = new Vector(FX, FY, FZ);

                    double MX = 0f;
                    double MY = M1 * wb * Math.Abs(ub);
                    double MZ = -N1 * vb * ub;
                    if (double.IsNaN(MX) || double.IsInfinity(MX)) { MX = 0.0; }
                    if (double.IsNaN(MY) || double.IsInfinity(MY)) { MY = 0.0; }
                    if (double.IsNaN(MZ) || double.IsInfinity(MZ)) { MZ = 0.0; }
                    m_moment = new Vector(MX, MY, MZ);

                    // Apply Forces
                    Vector3 mcxf = _aircraft.m_rigidbody.velocity.normalized * (float)Fx;
                    Vector3 mczf = _aircraft.transform.up * (float)-Fz;
                    _aircraft.m_rigidbody.AddForceAtPosition(mcxf + mczf, _aircraft.m_rigidbody.worldCenterOfMass, ForceMode.Force);
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public void Draw(Transform aircraft)
            {
#if UNITY_EDITOR
                float m_fl = m_referenceLength * (m_forwardLength * 0.01f);
                float m_rl = m_referenceLength * (m_rearLength * 0.01f);
                Vector3 m_fp = aircraft.position + (aircraft.forward * m_fl);
                Vector3 m_rp = aircraft.position - (aircraft.forward * m_rl);
                Lf = m_fl + m_rl;
                Gizmos.color = Color.red;
                //Gizmos.DrawLine(m_fp, m_rp);

                d = m_meanDiameter * 0.01f * m_referenceLength;
                Handles.color = Color.red;
                //Handles.DrawWireDisc(aircraft.position, aircraft.forward, d * 0.5f);
#endif
            }
        }

        #endregion

        private void Start() { Initialize(); }
        private void FixedUpdate() { ComputeFixedUpdate(); }
        private void Update() { ComputeUpdate(); }
        private void LateUpdate() { ComputeLateUpdate(); }

        #region Internal Functions

        /// <summary>
        /// Check that all the required components of the aircraft are present
        /// </summary>
        protected override void CheckPrerequisites()
        {
            if (m_wings.Length <= 0) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... aerofoil System not assigned"); allOk = false; return; }
            else if (m_fuselage._leftWing == null || m_fuselage._rightWing == null) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... fuselage components missing"); allOk = false; return; }
            else if (m_flcs == null) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... flight computer not connected to aircraft"); allOk = false; return; }

            // Check Engines
            if (m_engineType == EngineType.Piston && (m_pistons == null || m_pistons.Length < 1)) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... Selected engine is Piston but you don't have any Piston engines connected"); allOk = false; return; }
            if (m_engineType == EngineType.Jet)
            {
                if (jetType == JetType.Turbofan && (m_fans == null || m_fans.Length < 1)) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... Selected engine is Turbofan but you don't have any Turbofan engines connected"); allOk = false; return; }
                if (jetType == JetType.Turbojet && (m_jets == null || m_jets.Length < 1)) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... Selected engine is Turbojet but you don't have any Turbojet engines connected"); allOk = false; return; }
                if (jetType == JetType.Turboprop && (m_props == null || m_props.Length < 1)) { Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... Selected engine is Turboprop but you don't have any Turboprop engines connected"); allOk = false; return; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Collect Components
            m_aircraft = GetComponent<FixedController>();
            m_computer = GetComponentInChildren<FixedComputer>();
            m_wings = GetComponentsInChildren<SilantroAerofoil>();
            m_propellers = GetComponentsInChildren<SilantroPropeller>();
            m_fans = GetComponentsInChildren<SilantroTurbofan>();
            m_jets = GetComponentsInChildren<SilantroTurbojet>();
            m_props = GetComponentsInChildren<SilantroTurboprop>();

            // Confirm needed components
            CheckPrerequisites();

            // Initialize fixed components
            if (allOk)
            {
                // ------------------------- Setup Fuselage
                if (m_core != null)
                {
                    m_fuselage._aircraft = m_controller;
                    m_fuselage.Initialize();
                }

                // ------------------------- Setup Propellers
                if (m_propellers != null)
                {
                    foreach (SilantroPropeller prop in m_propellers)
                    {
                        prop.controller = m_controller;
                        prop.Initialize();
                    }
                }

                if(m_computer != null)
                {
                    m_computer.m_aircraft = this;
                }

                // ------------------------- Setup Aerofoils
                if (m_wings != null)
                {
                    foreach (SilantroAerofoil foil in m_wings)
                    {
                        foil._aircraft = m_controller;
                        foil._core = m_core;
                        foil.Initialize();

                        // --- Filter
                        if (foil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                        {
                            //if (flightComputer != null) { flightComputer.wingFoils.Add(foil); }

                            // ---------- Flap
                            if (foil.flapState == SilantroAerofoil.ControlState.Active) { m_flapWing = foil; m_flapLimit = m_flapWing.m_flapSteps[m_flapWing.m_flapSteps.Count - 1]; }
                            // ---------- Slat
                            if (foil.slatState == SilantroAerofoil.ControlState.Active) { m_slatWing = foil; }
                            // ---------- Spoiler
                            if (foil.spoilerState == SilantroAerofoil.ControlState.Active) { m_spoilerWing = foil; m_spoilerLimit = m_spoilerWing.sp_positiveLimit; }
                        }
                    }
                }

                // ------------------------- Setup Engine
                
                //1. Turbofan
                if (jetType == JetType.Turbofan && m_fans != null && m_fans.Length > 0)
                {
                    foreach (SilantroTurbofan engine in m_fans)
                    {
                        engine.controller = m_aircraft;
                        engine.computer = m_core;
                        engine.Initialize();
                    }
                }
                //2. TurboJet Engine
                if (jetType == JetType.Turbojet && m_jets != null && m_jets.Length > 0)
                {
                    foreach (SilantroTurbojet engine in m_jets)
                    {
                        engine.controller = m_aircraft;
                        engine.computer = m_core;
                        engine.Initialize();
                    }
                }
                //3. TurboProp Engine
                if (jetType == JetType.Turboprop && m_props != null && m_props.Length > 0)
                {
                    foreach (SilantroTurboprop engine in m_props)
                    {
                        engine.controller = m_aircraft;
                        engine.computer = m_core;
                        engine.Initialize();
                    }
                }

                // ------------------------- Hot Start
                if (m_startMode == StartMode.Hot && m_hotMode == HotStartMode.AfterInitialization) { StartCoroutine(m_helper.StartUpAircraft(m_controller)); }
            }

            // Tell base Initialization process is done
            FinishInitialization();
            isInitialized = true;
        }
        /// <summary>
        /// Run fixed update functions for the aircraft and its components
        /// </summary>
        protected override void ComputeFixedUpdate()
        {
            base.ComputeFixedUpdate();

            if (allOk && isInitialized)
            {
                if (isControllable)
                {
                    m_fuselage.Compute();
                    // Propellers
                    if (m_propellers != null) { foreach (SilantroPropeller prop in m_propellers) { prop.Compute(_fixedTimestep); } }
                    // Aerofoils
                    if (m_wings != null) { foreach (SilantroAerofoil wing in m_wings) { wing.Compute(_fixedTimestep); } }
                    // Data
                    ComputeData();
                    // Forces
                    ComputeForces();
                }
                // Turbojets
                if (m_jets != null) { foreach (SilantroTurbojet jet in m_jets) { jet.Compute(); } }
                // Turbofans
                if (m_fans != null) { foreach (SilantroTurbofan fan in m_fans) { fan.Compute(); } }
                // Turboprops
                if (m_props != null) { foreach (SilantroTurboprop prop in m_props) { prop.Compute(); } }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void ComputeUpdate()
        {
            base.ComputeUpdate();

            if (allOk && isInitialized && isControllable)
            {
                UpdateComponentInputs();

                // Flaps Levers
                foreach (SilantroInstrument instrument in m_instruments)
                {
                    if (instrument.m_type == SilantroInstrument.Type.Lever && instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Flaps)
                    {
                        instrument.m_lever.Compute(m_flapDeflection / m_flapLimit);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void ComputeData()
        {
            base.ComputeData();

            // Propellers
            if (m_propellers != null && m_propellers.Length > 0) { foreach (SilantroPropeller prop in m_propellers) { m_wowForce += (float)prop.m_Thrust; } }

            // Turbofan
            if (jetType == JetType.Turbofan && m_fans.Length > 0)
            {
                m_engineCount = m_fans.Length;
                if (m_fans != null && m_fans[0] != null) { m_powerState = m_fans[0].core.active; m_powerLevel = m_fans[0].core.coreFactor; }
                foreach (SilantroTurbofan turbofan in m_fans)
                { if (turbofan.core.active) { m_wowForce += turbofan.engineThrust; fuelFlow += turbofan.mf; } }
            }

            // Turbojet
            if (jetType == JetType.Turbojet && m_jets.Length > 0)
            {
                m_engineCount = m_jets.Length;
                if (m_jets != null && m_jets[0] != null) { m_powerState = m_jets[0].core.active; m_powerLevel = m_jets[0].core.coreFactor; }
                foreach (SilantroTurbojet turbojet in m_jets)
                { if (turbojet.core.active) { m_wowForce += turbojet.engineThrust; fuelFlow += turbojet.mf; } }
            }

            // Turboprop
            if (jetType == JetType.Turboprop && m_props.Length > 0)
            {
                m_engineCount = m_props.Length;
                if (m_props != null && m_props[0] != null) { m_powerState = m_props[0].core.active; m_powerLevel = m_props[0].core.coreFactor; }
                foreach (SilantroTurboprop prop in m_props)
                { if (prop.core.active) { fuelFlow += prop.mf; } }
            }

            //Pistons
            if (m_engineType == EngineType.Piston && m_pistons.Length > 0)
            {
                m_engineCount = m_pistons.Length;
                if (m_pistons != null && m_pistons[0] != null) { m_powerState = m_pistons[0].core.active; m_powerLevel = m_pistons[0].core.coreFactor; }
                foreach (SilantroPiston piston in m_pistons)
                { if (piston.core.active) { fuelFlow += piston.Mf; } }
            }

            // Wing Data
            if (m_flapWing != null) { m_flapDeflection = m_flapWing.m_flapDeflection; }
            if (m_slatWing != null) { m_slatDeflection = m_slatWing.m_slatDeflection; }
            if (m_spoilerWing != null) { m_spoilerDeflection = m_spoilerWing.m_flapDeflection; }

            // Actuator Data
            if (gearActuator != null && m_gearMode == GearMode.Retractable) { m_fuselage.m_gearNorm = 1 - gearActuator.currentActuationLevel; }
            if (speedBrakeActuator != null) { m_fuselage.m_speedbrakeNorm = speedBrakeActuator.currentActuationLevel; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void OnDrawGizmos()
        {
            m_fuselage.Draw(transform);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateComponentInputs()
        {
            //if(allOk && Application.isFocused)


            if (allOk)
            {
                base.UpdateComponentInputs();

                // Wings
                foreach (SilantroAerofoil foil in m_wings)
                {
                    foil.m_pitchInput = _pitchInput;
                    foil.m_rollInput = _rollInput;
                    foil.m_yawInput = _yawInput;

                    foil.m_pitchTrimInput = _pitchTrimInput;
                    foil.m_rollTrimInput = _rollTrimInput;
                    foil.m_yawTrimInput = _yawTrimInput;

                    // Deflect LEF based on Pressure and AOA based on F-16 FLCS design
                    if (m_computer != null && m_flcs.m_mode == Computer.Mode.Augmented)
                    {
                        if (m_computer.m_autoSlat == Oyedoyin.Common.Misc.ControlState.Active) { foil._commandSlatMovement = (float)m_computer.m_commandAugmentation.m_slatCommand; }
                        if (m_computer.m_flapControl == Oyedoyin.Common.Misc.ControlMode.Automatic) { foil._flcsFlapCommand = (float)m_computer.m_commandAugmentation.m_flapCommand; }
                        else { foil._flcsFlapCommand = 0; }
                    }
                }

                //Propellers
                foreach (SilantroPropeller prop in m_propellers) { prop.m_pitchInput = _propPitchInput; }
                // Turbojets
                if (m_jets != null) { foreach (SilantroTurbojet jet in m_jets) { jet.core.controlInput = _throttleInput; } }
                // Turbofans
                if (m_fans != null) { foreach (SilantroTurbofan fan in m_fans) { fan.core.controlInput = _throttleInput; } }
                // Turboprops
                if (m_props != null) { foreach (SilantroTurboprop prop in m_props) { prop.core.controlInput = _throttleInput; } }
            }
        }

        #endregion

        #region Call Functions
        /// <summary>
        /// 
        /// </summary>
        public override void LowerFlaps() { foreach (SilantroAerofoil foil in m_wings) { foil.LowerFlap(); } }
        /// <summary>
        /// 
        /// </summary>
        public override void RaiseFlaps() { foreach (SilantroAerofoil foil in m_wings) { foil.RaiseFlap(); } }
        /// <summary>
        /// 
        /// </summary>
        public override void TurnOnEngines()
        {
            if (isControllable)
            {
                if (m_engineType == EngineType.Piston) { foreach (SilantroPiston engine in m_pistons) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                //if (engineType == Controller.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Running) { engine.StartEngine(); } } }
                if (m_engineType == EngineType.Jet)
                {
                    if (jetType == JetType.Turbojet) { foreach (SilantroTurbojet engine in m_jets) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                    if (jetType == JetType.Turbofan) { foreach (SilantroTurbofan engine in m_fans) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                    if (jetType == JetType.Turboprop) { foreach (SilantroTurboprop engine in m_props) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void TurnOffEngines()
        {
            if (isControllable)
            {
                if (m_engineType == EngineType.Piston) { foreach (SilantroPiston engine in m_pistons) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                //if (engineType == Controller.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Running) { engine.ShutDownEngine(); } } }
                if (m_engineType == EngineType.Jet)
                {
                    if (jetType == JetType.Turbojet) { foreach (SilantroTurbojet engine in m_jets) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (jetType == JetType.Turbofan) { foreach (SilantroTurbofan engine in m_fans) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (jetType == JetType.Turboprop) { foreach (SilantroTurboprop engine in m_props) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                }
            }
        }
        /// <summary>
        /// Activate boost or afterburner
        /// </summary>
        public override void EngageBoost()
        {
            if (isControllable && m_engineType == EngineType.Jet)
            {
                if (jetType == JetType.Turbojet)
                {
                    foreach (SilantroTurbojet engine in m_jets)
                    {
                        if (!engine.core.afterburnerOperative && engine.reheatSystem == SilantroTurbojet.ReheatSystem.Afterburning && engine.core.active)
                        {
                            engine.core.afterburnerOperative = true;
                            boostRunning = true;
                            m_boostState = BoostState.Active;
                        }
                    }
                }
                if (jetType == JetType.Turbofan)
                {
                    foreach (SilantroTurbofan engine in m_fans)
                    {
                        if (!engine.core.afterburnerOperative && engine.reheatSystem == SilantroTurbofan.ReheatSystem.Afterburning && engine.core.active)
                        {
                            engine.core.afterburnerOperative = true;
                            boostRunning = true;
                            m_boostState = BoostState.Active;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// deactivate boost or afterburner
        /// </summary>
        public override void DisEngageBoost()
        {
            if (jetType == JetType.Turbojet) { foreach (SilantroTurbojet engine in m_jets) { engine.core.afterburnerOperative = false; boostRunning = false; m_boostState = BoostState.Off; } }
            if (jetType == JetType.Turbofan) { foreach (SilantroTurbofan engine in m_fans) { engine.core.afterburnerOperative = false; boostRunning = false; m_boostState = BoostState.Off; } }
        }
        /// <summary>
        /// Deflect or retract the slat surface(s)
        /// </summary>
        public override void ToggleSlatState() { foreach (SilantroAerofoil foil in m_wings) { foil.ActuateSlat(); } }
        /// <summary>
        /// Deflect or retract the spoiler surface(s)
        /// </summary>
        public override void ToggleSpoilerState() { foreach (SilantroAerofoil foil in m_wings) { foil.ActuateSpoiler(); } }




        #endregion
    }
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CustomEditor(typeof(FixedController))]
    public class FixedControllerEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        FixedController controller;
        SerializedProperty input;
        SerializedProperty fuselage;
        SerializedProperty origin;
        SerializedProperty launcher;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            controller = (FixedController)target;
            controller.m_type = Controller.VehicleType.Aircraft;
            input = serializedObject.FindProperty("m_input");
            fuselage = serializedObject.FindProperty("m_fuselage");
            origin = serializedObject.FindProperty("m_originShift");
            launcher = serializedObject.FindProperty("m_launcher");
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
            EditorGUILayout.HelpBox("Aircraft Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            controller.m_name = controller.transform.gameObject.name;
            EditorGUILayout.LabelField("Label", controller.m_name);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_engineType"), new GUIContent("Engine Type"));

            if (controller.m_engineType == Controller.EngineType.Jet)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("jetType"), new GUIContent(" "));
            }

            // --------------------------------------------------------------------------------------- Input Config
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Control Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_controlType"), new GUIContent("Control Mode"));
            if (controller.m_controlType == Controller.ControlType.Internal)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Cockpit Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Pilot on board", controller.pilotOnBoard.ToString());
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_canvas"), new GUIContent("Display Canvas"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_interiorPilot"), new GUIContent("Interior Pilot"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("getOutPosition"), new GUIContent("Exit Point"));
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox(" ", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
            }
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_inputType"), new GUIContent("Input Mode"));

            if (controller.m_inputType == Controller.InputType.Default)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_inputLogic"), new GUIContent(" "));
            }


            GUILayout.Space(8f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Vehicle Start Mode", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_startMode"), new GUIContent(" "));
            if (controller.m_startMode == Controller.StartMode.Hot)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hotMode"), new GUIContent("Activation"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_startSpeed"), new GUIContent("Start Speed (m/s)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_startAltitude"), new GUIContent("Start Altitude (m)"));
            }

            if (controller.m_inputType == Controller.InputType.VR)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("VR Levers", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("m_joystickLever"), new GUIContent("Control Stick"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("m_throttleLever"), new GUIContent("Throttle Lever"));
                if (controller.m_engineType == Controller.EngineType.Piston)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(input.FindPropertyRelative("m_mixtureLever"), new GUIContent("Mixture Lever"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(input.FindPropertyRelative("m_propPitchLever"), new GUIContent("Propeller Pitch Lever"));
                }
                GUILayout.Space(3f);
            }

            if (controller.m_inputType == Controller.InputType.Mobile)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Touch Controls", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("m_joystickTouch"), new GUIContent("Joystick"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("m_yawTouch"), new GUIContent("Rudder Pedals"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(input.FindPropertyRelative("m_throttleTouch"), new GUIContent("Throttle"));
                if (controller.m_engineType == Controller.EngineType.Piston)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(input.FindPropertyRelative("m_mixtureTouch"), new GUIContent("Mixture"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(input.FindPropertyRelative("m_propPitchTouch"), new GUIContent("Propeller Pitch"));
                }
                GUILayout.Space(3f);
            }

            // --------------------------------------------------------------------------------------- Fuselage Config
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Fuselage Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("_rightWing"), new GUIContent("Right Wing"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("_leftWing"), new GUIContent("Left Wing"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Wing Area", controller.m_fuselage.m_wingArea.ToString("0.00") + " m2");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("MAC", controller.m_fuselage.meanChord.ToString("0.00") + " m");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Aspect Ratio", controller.m_fuselage.m_aspectRatio.ToString("0.00"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Coefficients", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("Clβ"), new GUIContent("Clβ"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("CYβ"), new GUIContent("CYβ"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("Cnβ"), new GUIContent("Cnβ"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Gear", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gearMode"), new GUIContent("Gear Mode"));
            if (controller.m_gearMode == Controller.GearMode.Retractable)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Cx gear", (controller.m_fuselage.m_CXgear * controller.m_fuselage.m_gearNorm).ToString("0.00000"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Cz gear", (controller.m_fuselage.m_CZgear * controller.m_fuselage.m_gearNorm).ToString("0.00000"));
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("m_CXgear"), new GUIContent("Cx gear"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(fuselage.FindPropertyRelative("m_CZgear"), new GUIContent("Cz gear"));
            }

            if (controller.m_fuselage.m_speedbrakeNorm > 0)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Cx Speedbrake", (controller.m_fuselage.m_CXsb * controller.m_fuselage.m_speedbrakeNorm).ToString("0.00000"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Cz Speedbrake", (controller.m_fuselage.m_CZsb * controller.m_fuselage.m_speedbrakeNorm).ToString("0.00000"));
            }


            // --------------------------------------------------------------------------------------- Fuel Config
            if (controller.m_engineType != Controller.EngineType.Electric && controller.m_engineType != Controller.EngineType.Unpowered)
            {

                if (controller.m_engineType == Controller.EngineType.Piston)
                {
                    if (controller.gasFuelType == Controller.GasFuelType.AVGas100) { controller.combustionEnergy = 42.8f; }
                    if (controller.gasFuelType == Controller.GasFuelType.AVGas100LL) { controller.combustionEnergy = 43.5f; }
                    if (controller.gasFuelType == Controller.GasFuelType.AVGas82UL) { controller.combustionEnergy = 43.6f; }
                }
                else
                {
                    if (controller.jetFuelType == Controller.JetFuelType.JetB) { controller.combustionEnergy = 42.8f; }
                    if (controller.jetFuelType == Controller.JetFuelType.JetA1) { controller.combustionEnergy = 43.5f; }
                    if (controller.jetFuelType == Controller.JetFuelType.JP6) { controller.combustionEnergy = 43.02f; }
                    if (controller.jetFuelType == Controller.JetFuelType.JP8) { controller.combustionEnergy = 43.28f; }
                }
                controller.combustionEnergy *= 1000f;


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Fuel Configuration", MessageType.None);
                GUI.color = backgroundColor;
                if (controller.m_engineType == Controller.EngineType.Piston)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gasFuelType"), new GUIContent("Fuel Type"));
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("jetFuelType"), new GUIContent("Fuel Type"));

                }
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Q ", controller.combustionEnergy.ToString("0.00") + " KJ");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Capacity", controller.fuelCapacity.ToString("0.00") + " kg");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Fuel Level", controller.fuelLevel.ToString("0.00") + " kg");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Flow Rate", controller.fuelFlow.ToString("0.00000") + " kg/s");

                float secs = controller.fuelLevel / controller.fuelFlow;
                if (double.IsNaN(secs) || double.IsInfinity(secs)) { secs = 0.0f; }
                System.TimeSpan time = System.TimeSpan.FromSeconds(secs);
                float hr = time.Hours;
                float mn = time.Minutes;
                float ss = time.Seconds;
                if (double.IsNaN(hr) || double.IsInfinity(hr)) { hr = 0.0f; }
                if (double.IsNaN(mn) || double.IsInfinity(mn)) { mn = 0.0f; }
                if (double.IsNaN(ss) || double.IsInfinity(ss)) { ss = 0.0f; }
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Endurance", hr.ToString("0") + " hrs | " + mn.ToString("0") + " mins | " + ss.ToString("0") + " sec");
            }

            // --------------------------------------------------------------------------------------- Weight Config
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Weight Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("emptyWeight"), new GUIContent("Empty Weight"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumWeight"), new GUIContent("Maximum Weight"));
            float fuel = 0;
            float component = 0;
            float munition = 0;
            if (controller.m_core != null)
            {
                component = controller.m_core.componentLoad;
                munition = controller.m_core.gunLoad + controller.m_core.munitionLoad;
                fuel = controller.m_core.fuelLoad;
            }
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Component Weight", component.ToString("0.0") + " kg");
            //GUILayout.Space(3f);
            //EditorGUILayout.LabelField("Fuel Weight", fuel.ToString("0.0") + " kg");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Munition Weight", munition.ToString("0.0") + " kg");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Total Weight", controller.currentWeight.ToString("0.0") + " kg");


            // --------------------------------------------------------------------------------------- Weapons Config
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Weapon Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hardpoints"), new GUIContent("State"));

            if (controller.m_hardpoints == Controller.StoreState.Connected)
            {
                //GUI.enabled = false;
                //GUILayout.Space(3f);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_gunState"), new GUIContent("Gun State"));
                //GUILayout.Space(3f);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_rocketState"), new GUIContent("Rocket State"));
                //GUILayout.Space(3f);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_missileState"), new GUIContent("Missile State"));
                //GUILayout.Space(3f);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_bombState"), new GUIContent("Bomb State"));
                //GUILayout.Space(3f);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hardpointSelection"), new GUIContent("Selection"));
                //GUI.enabled = true;

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Launcher Config", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(launcher.FindPropertyRelative("fireSound"), new GUIContent("Fire Sound"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(launcher.FindPropertyRelative("fireVolume"), new GUIContent("Fire Volume"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(launcher.FindPropertyRelative("rateOfFire"), new GUIContent("Rocket Salvo Rate"));
            }

            // --------------------------------------------------------------------------------------- Origin Shift
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Origin Shift", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_state"), new GUIContent(" "));
            if (controller.m_originShift.m_state == Oyedoyin.Common.Misc.ControlState.Active)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_threshold"), new GUIContent("Threshold (m)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_sceneState"), new GUIContent("Scene State"));

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Particles", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_particleShift"), new GUIContent("State"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_particleCollection"), new GUIContent("Collection Mode"));

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Line Renderer", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_lineRendererShift"), new GUIContent("State"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_lineRendererCollection"), new GUIContent("Collection Mode"));

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Trail Renderer", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_trailRendererShift"), new GUIContent("State"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(origin.FindPropertyRelative("m_trailRendererCollection"), new GUIContent("Collection Mode"));


                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Debug", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Shift Count", controller.m_originShift.m_shiftCount.ToString());
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Distance Traveled", (controller.m_originShift.m_totalDistance / 1000).ToString("0.000") + " km");
            }


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}

