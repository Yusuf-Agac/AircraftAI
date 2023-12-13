using System;
using UnityEngine;
using Oyedoyin.Common;
using Oyedoyin.Analysis;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles the calculation/analysis of aerodynamic force and moments on the aircraft
/// </summary>

namespace Oyedoyin.FixedWing
{
    #region Component
    [RequireComponent(typeof(BoxCollider))]
    public class SilantroAerofoil : MonoBehaviour
    {

        #region Enums
        public enum AerofoilType { Wing, Stabilizer, Canard, Balance, Stabilator }
        public enum WingType { Conventional, Delta }
        public enum WingAlignment { Monoplane }
        public enum SurfaceFinish { SmoothPaint, PolishedMetal, ProductionSheetMetal, MoldedComposite, PaintedAluminium }
        public enum SweepDirection { Unswept, Forward, Backward }
        public enum TwistDirection { Untwisted, Upwards, Downwards }
        public enum IncidenceDirection { None, Upwards, Downwards }
        public enum Position { Right, Left }
        public enum ControlState { Absent, Active }
        public enum ControlType { Stationary, Controllable }
        public enum AvailableControls { PrimaryOnly, PrimaryPlusSecondary, SecondaryOnly }
        public enum SurfaceType { Inactive, Elevator, Rudder, Aileron, Ruddervator, Elevon }
        public enum FlapType { Plain, Split, Flaperon, Flapevon }
        public enum SpoilerType { Plain, Spoileron }
        public enum StabilatorType { Plain, Coupled }
        public enum SlatMovement { Deflection, Extension }
        public enum DeflectionType { Symmetric, Asymmetric }
        public enum TrimState { Absent, Available }
        public enum AnalysisMethod { GeometricOnly, NumericOnly, Combined }
        public enum ModelType { Internal, Actuator, None }
        public enum VortexLift { Consider, Neglect }
        public enum GroundEffectState { Neglect, Consider }
        public enum NumericCorrection { DATCOM, KhanNahon }
        public enum SweepCorrection { YoungLE, DATCOM, None }
        public enum FoilType { Conventional, Advanced }

        #endregion

        #region Enum Properties
        /// <summary>
        /// 
        /// </summary>
        public AerofoilType m_foilType = AerofoilType.Wing;
        /// <summary>
        /// 
        /// </summary>
        public WingType m_wingType = WingType.Conventional;
        /// <summary>
        /// 
        /// </summary>
        public Position m_position = Position.Right;
        /// <summary>
        /// 
        /// </summary>
        public TwistDirection twistDirection = TwistDirection.Untwisted;
        /// <summary>
        /// 
        /// </summary>
        public SweepDirection sweepDirection = SweepDirection.Unswept;
        /// <summary>
        /// 
        /// </summary>
        public WingAlignment wingAlignment = WingAlignment.Monoplane;
        /// <summary>
        /// Determines if the wing has a dihedral (upwards wing inclination ) or anhedral (downwards wing inclination) deflection of lateral stability
        /// </summary>
        public ControlType controlState = ControlType.Stationary;
        /// <summary>
        /// 
        /// </summary>
        public AvailableControls availableControls = AvailableControls.PrimaryOnly;
        /// <summary>
        /// 
        /// </summary>
        public SurfaceType surfaceType = SurfaceType.Aileron;
        /// <summary>
        /// 
        /// </summary>
        public FlapType flapType = FlapType.Plain;
        /// <summary>
        /// 
        /// </summary>
        public ControlState flapState = ControlState.Absent;
        /// <summary>
        /// 
        /// </summary>
        public ControlState slatState = ControlState.Absent;
        /// <summary>
        /// 
        /// </summary>
        public ControlState spoilerState = ControlState.Absent;
        /// <summary>
        /// 
        /// </summary>
        public SpoilerType spoilerType = SpoilerType.Plain;
        /// <summary>
        /// 
        /// </summary>
        public SlatMovement slatMovement = SlatMovement.Deflection;
        /// <summary>
        /// 
        /// </summary>
        public StabilatorType stabilatorType = StabilatorType.Plain;
        /// <summary>
        /// 
        /// </summary>
        public DeflectionType deflectionType = DeflectionType.Symmetric;
        /// <summary>
        /// 
        /// </summary>
        public TrimState trimState = TrimState.Absent;
        /// <summary>
        /// 
        /// </summary>
        public AnalysisMethod flapAnalysis = AnalysisMethod.GeometricOnly;
        /// <summary>
        /// 
        /// </summary>
        public AnalysisMethod controlAnalysis = AnalysisMethod.GeometricOnly;
        /// <summary>
        /// 
        /// </summary>
        public NumericCorrection controlCorrectionMethod = NumericCorrection.DATCOM;
        /// <summary>
        /// 
        /// </summary>
        public NumericCorrection flapCorrectionMethod = NumericCorrection.DATCOM;
        /// <summary>
        /// 
        /// </summary>
        public ModelType flapModelType = ModelType.Internal;
        /// <summary>
        /// 
        /// </summary>
        public ModelType slatModelType = ModelType.Internal;
        /// <summary>
        /// 
        /// </summary>
        public ModelType spoilerModelType = ModelType.Internal;
        /// <summary>
        /// 
        /// </summary>
        public VortexLift vortexLift = VortexLift.Neglect;
        /// <summary>
        /// 
        /// </summary>
        public GroundEffectState groundEffect = GroundEffectState.Neglect;
        /// <summary>
        /// 
        /// </summary>
        public SweepCorrection sweepCorrectionMethod = SweepCorrection.YoungLE;
        /// <summary>
        /// 
        /// </summary>
        public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
        /// <summary>
        /// 
        /// </summary>
        public FoilType foilType = FoilType.Conventional;

        #endregion

        #region Base Properties
        [HideInInspector] public Vector3 leadingRoot;
        /// <summary>
        /// Size of the axis indication Gizmo
        /// </summary>
        [Tooltip("Size of the axis indication Gizmo")] [Range(0.10f, 0.5f)] public float m_axisScale = 0.25f;
        /// <summary>
        /// Number of cells to subdivide the aerofoil into. Higher subdivision counts improve the simulation accuracy but also affect performance
        /// </summary>
        [Tooltip("Number of cells to subdivide the aerofoil into. Higher subdivision counts improve the simulation accuracy but also affect performance")] [Range(3, 30)] public int subdivision = 3;
        [HideInInspector] public float m_section = 90f;
        public float m_span = 1;
        public float m_rootChord = 1;
        public float m_tipChord;

        [Range(0, 60)] public float m_twist;
        [Range(0, 99)] public float m_taper;
        [Range(0, 60)] public float m_sweep;
        [HideInInspector] public float _twist;
        [HideInInspector] public float _sweep;

        // ------------------------------ Switches
        public bool drawFoil;
        public bool drawSplits;
        public bool drawAxes;
        private bool allOk;
        public bool debug;
        public List<string> toolbarStrings;
        public int toolbarTab;
        public string currentTab;

        public int toolbarCellTab;
        public string currentCellTab;

        #endregion

        #region Connections

        public Controller _aircraft;
        public SilantroCore _core;
        public SilantroAerofoil _foil;
        // Base airfoils
        public SilantroAirfoil rootAirfoil, tipAirfoil;
        // Airfoils with Mach corrections
        public Superfoil m_rootSuperfoil,m_tipSuperfoil;
        public List<Cell> m_cells;
        BoxCollider m_collider;
        public SilantroActuator m_controlActuator, m_flapActuator, m_slatActuator, m_spoilerActuator;

        #endregion

        #region Cell
        [System.Serializable]
        public class Cell
        {
            public bool m_reset;
            [HideInInspector] public Quaternion n_rotation;
            [HideInInspector] public Vector3 m_eulerAngles;

            public SilantroAerofoil m_foil;
            [Range(0, 80)] public float m_spanFill;
            [Range(0, 90)] public float m_taper;
            [Range(-30, 30)] public float m_sweep;
            [Range(-15, 15)] public float m_twist;
            [Range(-15, 15)] public float m_dihedral;
            [HideInInspector] public float m_rθ, m_tθ;
            [HideInInspector] public float _dihedral;
            public float m_span;
            public float m_rootChord;
            public float m_tipChord;
            [HideInInspector] public Vector3 m_forward;
            [HideInInspector] public Vector3 m_right;
            [HideInInspector] public Vector3 m_up;

            [HideInInspector] public Vector3 m_rootCenter;
            [HideInInspector] public Vector3 m_tipCenter, m_quaterCenter;

            [HideInInspector] public Vector3 m_leading_tipLocal;
            [HideInInspector] public Vector3 m_leading_rootLocal;
            [HideInInspector] public Vector3 m_trailing_tipLocal;
            [HideInInspector] public Vector3 m_trailing_rootLocal;

            [HideInInspector] public Vector3 m_quater_root;
            [HideInInspector] public Vector3 m_quater_tip;
            [HideInInspector] public Vector3 groundAxis;
            [HideInInspector] public Vector3[] rects = new Vector3[4];
            public float _controlRootChord, _controlTipChord;
            public float _flapRootChord, _flapTipChord;


            [Header("Controls")]
            [HideInInspector] public float _frc;
            [HideInInspector] public float _ftc, _mfx;
            public float m_controlChord;
            public float m_flapChord;
            public float m_slatChord;
            public float m_spoilerChord;
            public bool m_controlActive;
            public bool m_flapActive;
            public bool m_spoilerActive;
            public bool m_slatActive;

            [Range(-65, 40)] public float m_controlRootCorrection = 0;
            [Range(-65, 40)] public float m_controlTipCorrection = 0;
            [Range(-65, 40)] public float m_flapRootCorrection = 0;
            [Range(-65, 40)] public float m_flapTipCorrection = 0;
            [Range(-65, 40)] public float m_slatRootCorrection = 0;
            [Range(-65, 40)] public float m_slatTipCorrection = 0;
            [Range(-65, 40)] public float m_spoilerRootCorrection = 0;
            [Range(-65, 40)] public float m_spoilerTipCorrection = 0;

            [Header("Dimensions")]
            public float m_meanChord;
            [HideInInspector] public float m_rootArea, m_tipArea;
            public float λ;
            public float Г;
            public float θ;
            public float m_Kθ, ᴧLE, ᴧCT, ᴧQT;
            public float m_area;
            public float m_wettedArea;
            public float m_foilArea;
            public float m_effectiveThickness;
            public float m_edgeRadius;
            public float m_liftSlope;
            public float m_Mcritᴧ;
            public float m_e, m_ARf;

            [Header("Simulation Data")]
            public float m_u;
            public float m_v;
            public float m_w;
            [HideInInspector] public double ucg, vcg, wcg;
            public float α, αf, CLα;
            public float βRad, β;
            public float m_V;
            public float m_Mach;
            public float m_Qdyn, m_Re;
            public float m_groundCorrection;

            [Header("Force")]
            public float ΔCLδc;
            public float ΔCLδf, ΔCLδlf, ΔCLδs, ΔCLvort;
            public float ΔCDδc, ΔCDδf, ΔCDδlf, ΔCDδs, ΔCDvort, ΔCDw, ΔCDi, ΔCDsfr;
            public float CYβ_CL, ΔCYβ, Cnβ_CL;
            public float CL, CD, Cm, CYβ, Cnβ;
            public float m_lift;
            public float m_drag;
            public float m_moment;
        }

        #endregion

        #region Simulation Properties

        public double m_area;
        public double m_wettedArea;
        public double m_aspectRatio;
        public double m_maximumAOA;
        public double m_stallAngle;
        public float k;

        private Vector3 m_world;
        private Vector3 m_wcog;
        private Vector3 m_omega;

        #endregion

        #region Controls Structure


        [Range(0, 100)] public float m_controlRootChord = 10;
        [Range(0, 100)] public float m_controlTipChord = 10;
        public bool[] m_controlSections;
        public float m_baseDeflection;
        public float m_controlDeflection;
        public float _trimDeflection, _trimTabDeflection;
        public float m_controlArea;
        public float c_positiveLimit = 30;
        public float c_negativeLimit = 20;
        public Color m_controlColor = Color.white;
        public float SWc;
        [Range(10, 95f)] public float stabilatorCouplingPercentage = 30f;

        public float _baseInput;
        public float _trimInput;

        public Transform _primaryControlModel, _supportControlModel;
        public Vector3 _primaryControlAxis, _supportControlAxis;
        public Quaternion _primaryControlRotation, _supportControlRotation;
        public RotationAxis _primaryDeflectionAxis, _supportDeflectionAxis;
        public RotationDirection _primaryDeflectionDirection, _supportDeflectionDirection;

        public float _controlActuationSpeed = 60f;
        public float _maximumControlTorque = 5000;
        public float _controlActuatorDeflection;
        public float _positiveTrimLimit = 5;
        public float _positiveTrimTabLimit = 3;
        public float _currentControlEffectiveness;
        public float _controlLockPoint, _controlNullPoint, _controlFullPoint;
        public AnimationCurve _controlEfficiencyCurve;


        [Range(0, 100)] public float m_flapRootChord = 10;
        [Range(0, 100)] public float m_flapTipChord = 10;
        public bool[] m_flapSections;
        public float m_flapDeflection;
        public float m_flapArea;
        public float f_positiveLimit = 30;
        public float f_negativeLimit = 20;
        public Color m_flapColor = Color.yellow;
        public List<float> m_flapSteps = new List<float>() { 0, 30 };
        public int m_currentFlapStep;
        public float SWf;

        public Transform _primaryFlapModel, _supportFlapModel;
        public Vector3 _primaryFlapAxis, _supportFlapAxis;
        public Quaternion _primaryFlapRotation, _supportFlapRotation;
        public RotationAxis _primaryFlapDeflectionAxis, _supportFlapDeflectionAxis;
        public RotationDirection _primaryFlapDeflectionDirection, _supportFlapDeflectionDirection;

        public float _flapActuationSpeed = 30f;
        public float _flapActuatorDeflection;
        public float _maximumFlapTorque = 7000f;
        public float _flapLockPoint, _flapNullPoint, _flapFullPoint;
        public float _currentFlapEffectiveness;
        public float _baseFlapDeflection, _flcsFlapCommand;
        public float _baseFlaperonDeflection;
        public float _baseFlapevonDeflection;
        public AnimationCurve _flapEfficiencyCurve;

        private AudioSource _flapClampSource, _flapLoopSource;
        public AudioClip _flapLoop, _flapClamp;
        public bool _flapDeflectionEngaged;
        public int _flapSoundMode;



        [Range(0, 100)] public float m_slatRootChord = 10;
        [Range(0, 100)] public float m_slatTipChord = 10;
        public bool[] m_slatSections;
        public float m_slatDeflection;
        public float m_slatArea;
        public AnimationCurve clBaseCurve;
        public AnimationCurve k1Curve, k2Curve, k3Curve;
        public AnimationCurve liftDeltaCurve, nMaxCurve, nDeltaCurve;
        public AnimationCurve effectivenessPlot;

        private AudioSource _slatClampSource, _slatLoopSource;
        public AudioClip _slatLoop, _slatClamp;
        public bool _slatDeflectionEngaged;

        public Transform _primarySlatModel, _supportSlatModel;
        public Vector3 _primarySlatAxis, _supportSlatAxis;
        public Quaternion _primarySlatRotation, _supportSlatRotation;
        public Vector3 _primarySlatPosition, _supportSlatPosition;
        public RotationAxis _primarySlatDeflectionAxis, _supportSlatDeflectionAxis;
        public RotationDirection _primarySlatDeflectionDirection, _supportSlatDeflectionDirection;

        public float _slatActuationSpeed = 30f;
        public float _commandSlatMovement;
        public float _slatMovementLimit = 25f;



        [Range(0, 100)] public float m_spoilerHinge = 10;
        [Range(0, 100)] public float m_spoilerChord = 10;
        public bool[] m_spoilerSections;
        public float m_spoilerDeflection;
        public float m_spoilerArea;
        public float sp_positiveLimit = 60;
        public float sp_factor;

        public float _spoilerActuationSpeed = 50f;
        public float _spoilerActuatorDeflection;
        public float _maximumSpoilerTorque = 7000f;
        public float _spoilerLockPoint, _spoilerNullPoint, _spoilerFullPoint;
        public float _currentSpoilerEffectiveness;
        public float _baseSpoilerDeflection;
        public AnimationCurve _spoilerEfficiencyCurve;
        [Range(0, 90f)] public float spoilerRollCoupling = 60f;

        public Transform _primarySpoilerModel, _supportSpoilerModel;
        public Vector3 _primarySpoilerAxis, _supportSpoilerAxis;
        public Quaternion _primarySpoilerRotation, _supportSpoilerRotation;
        public Vector3 _primarySpoilerPosition, _supportSpoilerPosition;
        public RotationAxis _primarySpoilerDeflectionAxis, _supportSpoilerDeflectionAxis;
        public RotationDirection _primarySpoilerDeflectionDirection, _supportSpoilerDeflectionDirection;

        public float _commandSpoilerDeflection;

        #endregion

        public float m_pitchInput;
        public float m_rollInput;
        public float m_yawInput;
        public float m_pitchTrimInput;
        public float m_rollTrimInput;
        public float m_yawTrimInput;

        public Vector m_force;
        public double Lift, Drag, Moment;
         
        public bool slatExtended;
        public bool spoilerExtended;
        public bool airbrakeActive;

        #region Left Copy Management

        public SilantroAerofoil m_left;
        public bool m_updateData = true;

        #endregion

        #region Internal Functions

        /// <summary>
        /// Check that all the required components of the aerofoil are present
        /// </summary>
        protected void _checkPrerequisites()
        {
            if (_aircraft != null && _core != null && rootAirfoil != null && tipAirfoil != null) { allOk = true; }
            else if (_aircraft == null) { Debug.LogError("Prerequisites not met on " + m_foilType.ToString() + " " + transform.name + "....Aircraft controller not assigned"); allOk = false; }
            else if (_core == null) { Debug.LogError("Prerequisites not met on " + m_foilType.ToString() + " " + transform.name + "....Core system not assigned"); allOk = false; }
            else if (rootAirfoil == null) { Debug.LogError("Prerequisites not met on " + m_foilType.ToString() + " " + transform.name + "....Root airfoil has not been assigned"); allOk = false; }
            else if (tipAirfoil == null) { Debug.LogError("Prerequisites not met on " + m_foilType.ToString() + " " + transform.name + "....Tip airfoil has not been assigned"); allOk = false; }
        }
        /// <summary>
        /// Setup and configure the aerofoil and its components
        /// </summary>
        public void Initialize()
        {
            _checkPrerequisites();

            if (allOk)
            {
                // ------------------------------------------------------------------ Set wing stall angle from the assigned airfoils
                m_stallAngle = 90;
                if (rootAirfoil != null && tipAirfoil != null)
                {
                    if (rootAirfoil.stallAngle < m_stallAngle) { m_stallAngle = rootAirfoil.stallAngle; }
                    if (tipAirfoil.stallAngle < m_stallAngle) { m_stallAngle = tipAirfoil.stallAngle; }
                    if (m_stallAngle == 0 || m_stallAngle > 90) { m_stallAngle = 15f; }
                }

                // ------------------ ------------------------------------------------ Configure control models and rotation properties
                if (controlState == ControlType.Controllable)
                {
                    // Configure Control Surface Models
                    if (surfaceType != SurfaceType.Inactive)
                    {
                        if (_primaryControlModel != null) { _primaryControlRotation = _primaryControlModel.transform.localRotation; }
                        if (_supportControlModel != null) { _supportControlRotation = _supportControlModel.transform.localRotation; }
                        _primaryControlAxis = Handler.EstimateModelProperties(_primaryDeflectionDirection.ToString(), _primaryDeflectionAxis.ToString());
                        _supportControlAxis = Handler.EstimateModelProperties(_supportDeflectionDirection.ToString(), _supportDeflectionAxis.ToString());
                    }

                    // Configure Flap Surface Models
                    if (flapState == ControlState.Active)
                    {
                        if (flapType != FlapType.Flaperon && flapType != FlapType.Flapevon)
                        {
                            if (m_flapSteps.Count > 1) { f_positiveLimit = m_flapSteps[m_flapSteps.Count - 1]; f_negativeLimit = m_flapSteps[0]; }
                            else { m_flapSteps = new List<float>() { 0, 30 }; f_negativeLimit = 0; f_positiveLimit = 30; }
                        }

                        if (_primaryFlapModel != null) { _primaryFlapRotation = _primaryFlapModel.transform.localRotation; }
                        if (_supportFlapModel != null) { _supportFlapRotation = _supportFlapModel.transform.localRotation; }
                        _primaryFlapAxis = Handler.EstimateModelProperties(_primaryFlapDeflectionDirection.ToString(), _primaryFlapDeflectionAxis.ToString());
                        _supportFlapAxis = Handler.EstimateModelProperties(_supportFlapDeflectionDirection.ToString(), _supportFlapDeflectionAxis.ToString());
                    }

                    // Configure Slat Surface Models
                    if (slatState == ControlState.Active)
                    {
                        if (_primarySlatModel != null) { _primarySlatRotation = _primarySlatModel.transform.localRotation; _primarySlatPosition = _primarySlatModel.transform.localPosition; }
                        if (_supportSlatModel != null) { _supportSlatRotation = _supportSlatModel.transform.localRotation; _supportSlatPosition = _primarySlatModel.transform.localPosition; }
                        _primarySlatAxis = Handler.EstimateModelProperties(_primarySlatDeflectionDirection.ToString(), _primarySlatDeflectionAxis.ToString());
                        _supportSlatAxis = Handler.EstimateModelProperties(_supportSlatDeflectionDirection.ToString(), _supportSlatDeflectionAxis.ToString());
                    }

                    // Configure Spoiler Surface Models
                    if (spoilerState == ControlState.Active)
                    {
                        if (_primarySpoilerModel != null) { _primarySpoilerRotation = _primarySpoilerModel.transform.localRotation; _primarySpoilerPosition = _primarySpoilerModel.transform.localPosition; }
                        if (_supportSpoilerModel != null) { _supportSpoilerRotation = _supportSpoilerModel.transform.localRotation; _supportSpoilerPosition = _primarySpoilerModel.transform.localPosition; }
                        _primarySpoilerAxis = Handler.EstimateModelProperties(_primarySpoilerDeflectionDirection.ToString(), _primarySpoilerDeflectionAxis.ToString());
                        _supportSpoilerAxis = Handler.EstimateModelProperties(_supportSpoilerDeflectionDirection.ToString(), _supportSpoilerDeflectionAxis.ToString());
                    }
                }
                if (m_foilType != AerofoilType.Balance && controlState == ControlType.Controllable) { AerofoilDesign.PlotControlCurves(_foil, m_foilType == AerofoilType.Wing && slatState == ControlState.Active); }

                // ------------------------------------------------------------------ Configure wing shape and DATCOM controls
                AnalyseStructure(false);
                effectivenessPlot = AerofoilDesign.PlotControlEffectiveness();

                // ------------------------------------------------------------------ Configure Surface Factor
                if (surfaceFinish == SurfaceFinish.MoldedComposite) { k = 0.17f; }
                if (surfaceFinish == SurfaceFinish.PaintedAluminium) { k = 3.33f; }
                if (surfaceFinish == SurfaceFinish.PolishedMetal) { k = 0.50f; }
                if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) { k = 1.33f; }
                if (surfaceFinish == SurfaceFinish.SmoothPaint) { k = 2.08f; }

                // ------------------------------------------------------------------ Configure Sounds
                if (controlState == ControlType.Controllable && m_foilType == AerofoilType.Wing)
                {
                    if (flapState == ControlState.Active)
                    {
                        if (_flapClamp) { Handler.SetupSoundSource(this.transform, _flapClamp, "_flap_clamp", 80f, false, false, out _flapClampSource); _flapClampSource.volume = 1f; }
                        if (_flapLoop) { Handler.SetupSoundSource(this.transform, _flapLoop, "_flap_loop", 80f, true, false, out _flapLoopSource); _flapLoopSource.volume = 1f; }
                    }
                    if (slatState == ControlState.Active)
                    {
                        if (_slatClamp) { Handler.SetupSoundSource(this.transform, _slatClamp, "_slat_clamp", 80f, false, false, out _slatClampSource); _slatClampSource.volume = 1f; }
                        if (_slatLoop) { Handler.SetupSoundSource(this.transform, _slatLoop, "_slat_loop", 80f, true, false, out _slatLoopSource); _slatLoopSource.volume = 1f; }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { AnalyseStructure(true); }
            if(m_updateData && m_left != null) { UpdateLeftWing(); }
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (controlState == ControlType.Controllable) { AerofoilDesign.AnalyseCellControl(_foil, true); }
            }
            if (!Application.isPlaying) { AerofoilDesign.DrawCells(_foil); }
#endif
        }
        /// <summary>
        /// Process control inputs into surface deflection
        /// </summary>
        private void AnalyseControls(float _timestep)
        {
            float m_speed = (float)_core.V;

            if (controlState == ControlType.Controllable)
            {
                #region Base Control

                if (surfaceType != SurfaceType.Inactive)
                {
                    // --------------------------------- Collect Inputs
                    if (surfaceType == SurfaceType.Rudder) { _baseInput = m_yawInput * m_section; _trimInput = -m_yawTrimInput; }
                    if (surfaceType == SurfaceType.Aileron) { _baseInput = m_rollInput * m_section; _trimInput = m_rollTrimInput; }
                    if (surfaceType == SurfaceType.Elevator)
                    {
                        float corePitch = m_pitchInput;
                        if (m_foilType == AerofoilType.Canard) { _baseInput = -corePitch; _trimInput = -m_pitchTrimInput; }
                        else { _baseInput = corePitch; _trimInput = m_pitchTrimInput; }

                        // -------------------------------------- Roll Coupled Stabilator
                        float pitchFactor = Mathf.Abs(m_pitchInput + m_pitchTrimInput);
                        if (stabilatorType == StabilatorType.Coupled && pitchFactor < 0.4f)
                        {
                            if (m_foilType == AerofoilType.Stabilator)
                            { _baseInput = ((corePitch * 2f) + (m_rollInput * m_section * (stabilatorCouplingPercentage / 100f) * 2f)) / 2f; }
                            if (m_foilType == AerofoilType.Canard)
                            { _baseInput = ((-corePitch * 2f) + (m_rollInput * m_section * (stabilatorCouplingPercentage / 100f) * 2f)) / 2f; }
                        }
                    }
                    if (surfaceType == SurfaceType.Elevon)
                    {
                        float basePitch = m_pitchInput;
                        float baseRoll = m_rollInput * m_section;
                        if (transform.localScale.x < 0) { basePitch *= -1; }
                        float baseTrimPitch = m_pitchTrimInput * m_section;
                        float baseTrimRoll = m_rollTrimInput;
                        _baseInput = ((basePitch * 2f) + (baseRoll * 2f)) / 2f;
                        _trimInput = ((baseTrimRoll * 2f) + (baseTrimPitch * 2f)) / 2f;
                    }
                    if (surfaceType == SurfaceType.Ruddervator)
                    {
                        float basePitch = m_pitchInput;
                        float baseYaw = m_yawInput * m_section;
                        if (transform.localScale.x < 0) { basePitch *= -1; }
                        float baseTrimPitch = m_pitchTrimInput * m_section;
                        float baseTrimYaw = -m_yawTrimInput;
                        _baseInput = ((baseYaw * 2f) + (basePitch * 2f)) / 2f;
                        _trimInput = ((baseTrimYaw * 2f) + (baseTrimPitch * 2f)) / 2f;
                    }

                    // --------------------------------- Clamp
                    _baseInput = Mathf.Clamp(_baseInput, -1, 1);
                    _trimInput = Mathf.Clamp(_trimInput, -1, 1);



                    // Symmetric Control Deflection
                    if (deflectionType == DeflectionType.Symmetric)
                    {
                        m_baseDeflection = _baseInput > 0f ? _baseInput * c_positiveLimit : _baseInput * c_positiveLimit;
                        if (trimState == TrimState.Available)
                        {
                            _trimDeflection = _trimInput > 0f ? _trimInput * _positiveTrimLimit : _trimInput * _positiveTrimLimit;
                            _trimTabDeflection = _trimInput > 0f ? _trimInput * _positiveTrimTabLimit : _trimInput * _positiveTrimTabLimit;
                        }
                        else { _trimDeflection = 0f; _trimTabDeflection = 0f; }
                        _controlActuatorDeflection = m_baseDeflection + _trimDeflection;
                        if (_controlActuatorDeflection > c_positiveLimit) { _controlActuatorDeflection = c_positiveLimit; }
                        if (_controlActuatorDeflection < -c_positiveLimit) { _controlActuatorDeflection = -c_positiveLimit; }
                    }
                    // Asymmetric Control Deflection
                    if (deflectionType == DeflectionType.Asymmetric)
                    {
                        m_baseDeflection = _baseInput > 0f ? _baseInput * c_positiveLimit : _baseInput * c_negativeLimit;
                        if (trimState == TrimState.Available)
                        {
                            _trimDeflection = _trimInput > 0f ? _trimInput * _positiveTrimLimit : _trimInput * _positiveTrimLimit;
                            _trimTabDeflection = _trimInput > 0f ? _trimInput * _positiveTrimTabLimit : _trimInput * _positiveTrimTabLimit;
                        }
                        else { _trimDeflection = 0f; _trimTabDeflection = 0f; }
                        _controlActuatorDeflection = m_baseDeflection + _trimDeflection;
                        if (_controlActuatorDeflection > c_positiveLimit) { _controlActuatorDeflection = c_positiveLimit; }
                        if (_controlActuatorDeflection < -c_negativeLimit) { _controlActuatorDeflection = -c_negativeLimit; }
                    }

                    // Control Effectiveness as a function of speed
                    _currentControlEffectiveness = _controlEfficiencyCurve.Evaluate(m_speed) * 0.01f;
                    if (!float.IsNaN(_currentControlEffectiveness)) { _controlActuatorDeflection *= Mathf.Clamp01(_currentControlEffectiveness); }

                    // Deflect the actual surface
                    m_controlDeflection = Mathf.MoveTowards(m_controlDeflection, _controlActuatorDeflection, _controlActuationSpeed * _timestep);
                    if (_primaryControlModel != null)
                    {
                        _primaryControlModel.transform.localRotation = _primaryControlRotation;
                        _primaryControlModel.transform.Rotate(_primaryControlAxis, m_controlDeflection);
                    }
                    if (_supportControlModel != null)
                    {
                        _supportControlModel.transform.localRotation = _supportControlRotation;
                        _supportControlModel.transform.Rotate(_supportControlAxis, m_controlDeflection);
                    }
                }

                #endregion

                #region Flap Control

                if (m_foilType == AerofoilType.Wing && flapState == ControlState.Active)
                {
                    if (m_currentFlapStep > m_flapSteps.Count) { m_currentFlapStep = m_flapSteps.Count - 1; }
                    if (m_currentFlapStep < 0) { m_currentFlapStep = 0; }
                    _baseFlapDeflection = Mathf.MoveTowards(_baseFlapDeflection, m_flapSteps[m_currentFlapStep], _flapActuationSpeed * 0.2f * _timestep);
                    float f_rollInput = m_rollInput * m_section;
                    float f_pitchInput = _aircraft._pitchInput;
                    _baseFlaperonDeflection = f_rollInput > 0f ? f_rollInput * f_negativeLimit : f_rollInput * f_positiveLimit;
                    float _flapCommandDeflection = _baseFlapDeflection + _flcsFlapCommand;

                    // ------------------------------------- Flapevon Control
                    float _flapevonInput = ((f_pitchInput * 2f) + (f_rollInput * 2f)) / 2f;
                    _baseFlapevonDeflection = _flapevonInput > 0f ? _flapevonInput * f_negativeLimit : _flapevonInput * f_positiveLimit;

                    // ------------------------------------- Flap Actuation
                    if (flapType == FlapType.Flaperon) { _flapActuatorDeflection = ((_flapCommandDeflection * 2f) + (_baseFlaperonDeflection * 2f)) / 2f; }
                    else if (flapType == FlapType.Flapevon) { _flapActuatorDeflection = ((_flapCommandDeflection * 2f) + (_baseFlapevonDeflection * 2f)) / 2f; }
                    else { _flapActuatorDeflection = _flapCommandDeflection; }

                    // ------------------------------------- Limits
                    if (_flapActuatorDeflection > m_flapSteps[m_flapSteps.Count - 1]) { _flapActuatorDeflection = m_flapSteps[m_flapSteps.Count - 1]; }

                    // ------------------------------------- Deflection and Control Actuation
                    _currentFlapEffectiveness = _flapEfficiencyCurve.Evaluate(m_speed) * 0.01f;
                    if (!float.IsNaN(_currentFlapEffectiveness)) { _flapActuatorDeflection *= Mathf.Clamp01(_currentFlapEffectiveness); }
                    m_flapDeflection = Mathf.MoveTowards(m_flapDeflection, _flapActuatorDeflection, _flapActuationSpeed * _timestep);

                    // ------------------------------------- Deflect the flap surface
                    if (flapModelType == ModelType.Internal)
                    {
                        if (_primaryFlapModel != null)
                        {
                            _primaryFlapModel.transform.localRotation = _primaryFlapRotation;
                            _primaryFlapModel.transform.Rotate(_primaryFlapAxis, m_flapDeflection);
                        }
                        if (_supportFlapModel != null)
                        {
                            _supportFlapModel.transform.localRotation = _supportFlapRotation;
                            _supportFlapModel.transform.Rotate(_supportFlapAxis, m_flapDeflection);
                        }
                    }
                    else if (flapModelType == ModelType.Actuator)
                    {
                        float _flapLevel = Mathf.Abs(m_flapDeflection) / f_positiveLimit;
                        if (m_flapActuator != null) { m_flapActuator.targetActuationLevel = _flapLevel; }
                    }

                    // ------------------------------------- Sound
                    if (_flapLoopSource != null && _flapClampSource != null) { AnalyseSound(_flapSoundMode, m_flapSteps[m_currentFlapStep]); }
                }

                #endregion

                #region Slat Control

                if (m_foilType == AerofoilType.Wing && slatState == ControlState.Active)
                {
                    if (_commandSlatMovement > _slatMovementLimit) { _commandSlatMovement = _slatMovementLimit; }
                    if (slatMovement == SlatMovement.Deflection) { m_slatDeflection = Mathf.MoveTowards(m_slatDeflection, _commandSlatMovement, _timestep * _slatActuationSpeed); }
                    if (slatMovement == SlatMovement.Extension) { m_slatDeflection = Mathf.Lerp(m_slatDeflection, _commandSlatMovement, _timestep * _slatActuationSpeed); }

                    if (slatModelType == ModelType.Internal)
                    {
                        //DEFLECTION
                        if (slatMovement == SlatMovement.Deflection)
                        {
                            if (_primarySlatModel != null)
                            {
                                _primarySlatModel.transform.localRotation = _primarySlatRotation;
                                _primarySlatModel.transform.Rotate(_primarySlatAxis, m_slatDeflection);
                            }
                            if (_supportSlatModel != null)
                            {
                                _supportSlatModel.transform.localRotation = _supportSlatRotation;
                                _supportSlatModel.transform.Rotate(_supportSlatAxis, m_slatDeflection);
                            }
                        }
                        //SLIDING
                        if (slatMovement == SlatMovement.Extension)
                        {
                            if (_primarySlatModel != null)
                            {
                                _primarySlatModel.transform.localPosition = _primarySlatPosition;
                                _primarySlatModel.transform.localPosition += _primarySlatAxis * (m_slatDeflection / 100);
                            }
                            if (_supportSlatModel != null)
                            {
                                _supportSlatModel.transform.localPosition = _supportSlatPosition;
                                _supportSlatModel.transform.localPosition += _supportSlatAxis * (m_slatDeflection / 100);
                            }
                        }
                    }
                    else if (slatModelType == ModelType.Actuator)
                    {
                        float slatLevel = Mathf.Abs(m_slatDeflection) / _slatMovementLimit;
                        if (m_slatActuator != null) { m_slatActuator.targetActuationLevel = slatLevel; }
                    }
                }

                #endregion

                #region Spoiler Control

                if (m_foilType == AerofoilType.Wing && spoilerState == ControlState.Active)
                {
                    _baseSpoilerDeflection = Mathf.MoveTowards(_baseSpoilerDeflection, _commandSpoilerDeflection, _spoilerActuationSpeed * _timestep);

                    // ------------------------------------- Spoilereron
                    if (spoilerType == SpoilerType.Spoileron)
                    {
                        float rollableSpoiler = sp_positiveLimit * (spoilerRollCoupling / 100f);
                        float f_rollInput = -m_rollInput * m_section;
                        float baseSpoileronDeflection = f_rollInput > 0f ? f_rollInput * rollableSpoiler : f_rollInput * 0;
                        _spoilerActuatorDeflection = ((_baseSpoilerDeflection * 2f) + (baseSpoileronDeflection * 2f)) / 2f;
                    }
                    else { _spoilerActuatorDeflection = _baseSpoilerDeflection; }

                    // ------------------------------------- Limit
                    if (_spoilerActuatorDeflection > sp_positiveLimit) { _spoilerActuatorDeflection = sp_positiveLimit; }
                    if (_spoilerActuatorDeflection < 0) { _spoilerActuatorDeflection = 0f; }

                    // ------------------------------------- Deflection and Control Actuation
                    _currentSpoilerEffectiveness = 1;//_spoilerEfficiencyCurve.Evaluate(m_speed) * 0.01f;
                    if (!float.IsNaN(_currentSpoilerEffectiveness)) { _spoilerActuatorDeflection *= Mathf.Clamp01(_currentSpoilerEffectiveness); }
                    m_spoilerDeflection = Mathf.MoveTowards(m_spoilerDeflection, _spoilerActuatorDeflection, _spoilerActuationSpeed * _timestep);

                    if (_primarySpoilerModel != null)
                    {
                        _primarySpoilerModel.transform.localRotation = _primarySpoilerRotation;
                        _primarySpoilerModel.transform.Rotate(_primarySpoilerAxis, m_spoilerDeflection);
                    }
                    if (_supportSpoilerModel != null)
                    {
                        _supportSpoilerModel.transform.localRotation = _supportSpoilerRotation;
                        _supportSpoilerModel.transform.Rotate(_supportSpoilerAxis, m_spoilerDeflection);
                    }
                }

                #endregion
            }
        }
        /// <summary>
        /// Recalculate vector points for the aerofoil shape based on transform position and set variables
        /// </summary>
        /// <param name="_draw"></param>
        private void AnalyseStructure(bool _draw)
        {
            if (m_position == Position.Left) { m_section = -1; } else { m_section = 1; }
            if (_foil == null) { _foil = transform.GetComponent<SilantroAerofoil>(); }

            // ------------------------------------------------------------------ Collider
            m_collider = transform.GetComponent<BoxCollider>();
            if (m_collider == null) { gameObject.AddComponent<BoxCollider>(); }
            float thickness;
            if (rootAirfoil != null && tipAirfoil != null) { thickness = MathBase.EstimateSection(rootAirfoil.maximumThickness, tipAirfoil.maximumThickness, 0.5f); }
            else { thickness = 0.15f; }
            transform.localScale = new Vector3(transform.localScale.x, thickness * m_rootChord * 0.5f, transform.localScale.z);
            m_collider.center = new Vector3(m_section * 0.5f, 0, 0);

            // ------------------------------------------------------------------ Design Variables
            if (twistDirection == TwistDirection.Downwards) { _twist = m_twist; }
            else if (twistDirection == TwistDirection.Upwards) { _twist = -m_twist; }
            else { _twist = 0; }
            if (sweepDirection == SweepDirection.Forward) { _sweep = m_sweep; }
            else if (sweepDirection == SweepDirection.Backward) { _sweep = -m_sweep; }
            else { _sweep = 0; }

            // ------------------------------------------------------------------ Collect Dimensions
            m_rootChord = transform.localScale.z;
            m_span = transform.localScale.x;
            m_tipChord = m_rootChord * ((100 - m_taper) / 100);

            // ------------------------------------------------------------------ Cell Functions
            if (!Application.isPlaying)
            {
                AerofoilDesign.AnalyseCell(_foil);
                AerofoilDesign.AnalyseDimension(_foil);
                if (controlState == ControlType.Controllable) { AerofoilDesign.AnalyseCellControl(_foil, _draw); }
                if (_foil != null && controlState == ControlType.Controllable) { AerofoilDesign.PlotControlEffectiveness(_foil); }
            }


        }
        /// <summary>
        /// Process aerodynamic forces based on set variables
        /// </summary>
        protected void AnalyseForces()
        {
            // ----------------------------------------- Collect Vectors
            Vector m_wind = _aircraft.m_core.m_atmosphere.m_wind;
            m_world = _aircraft.m_rigidbody.velocity + new Vector3((float)m_wind.y, 0, (float)m_wind.x);
            m_wcog = _aircraft.m_rigidbody.worldCenterOfMass;
            m_omega = _aircraft.m_rigidbody.angularVelocity;

            // ----------------------------------------- Reset Values
            Lift = Drag = Moment = 0;
            m_force = Vector.zero;
            m_maximumAOA = -90f;

            for (int p = 0; p < subdivision; p++)
            {
                // ----------------------------------------- Convert Stored Local Positions to World
                Cell cell = m_cells[p];
                AerofoilDesign._cellPoints(_foil, cell, out Vector3 m_leading_root, out Vector3 m_trailing_root, out Vector3 m_leading_tip, out Vector3 m_trailing_tip);

                #region Base Controls
                if (cell.m_controlActive)
                {
                    float _deflection = -m_section * m_controlDeflection;
                    Vector3 _extension = AerofoilDesign._controlCell(_foil, cell, _deflection, m_controlRootChord, m_controlTipChord);
                    //Vector3 mtp = m_trailing_tip;
                    m_trailing_root = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, (m_controlRootChord * 0.01f)) + _extension;
                    m_trailing_tip = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, (m_controlTipChord * 0.01f)) + _extension;
                    //Debug.DrawLine(m_leading_root, m_trailing_root, Color.blue);
                    //Debug.DrawLine(m_leading_tip, m_trailing_tip, Color.blue);
                    //Debug.DrawLine(m_trailing_tip, mtp, Color.blue);
                }
                #endregion

                #region Flap Controls

                if (cell.m_flapActive)
                {
                    float _deflection = -m_section * m_flapDeflection;
                    Vector3 _extension = AerofoilDesign._controlCell(_foil, cell, _deflection, m_flapRootChord, m_flapTipChord);
                    m_trailing_root = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, (m_flapRootChord * 0.01f)) + _extension;
                    m_trailing_tip = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, (m_flapTipChord * 0.01f)) + _extension;
                }

                #endregion

                // ----------------------------------------- Calculate center markers
                Vector3 _aeroCenter = MathBase.EstimateGeometricCenter(m_leading_tip, m_trailing_tip, m_leading_root, m_trailing_root);
                Vector3 _lec = MathBase.EstimateSectionPosition(m_leading_root, m_leading_tip, 0.5f);
                Vector3 _tec = MathBase.EstimateSectionPosition(m_trailing_root, m_trailing_tip, 0.5f);
                Vector3 _panelCenter = _lec - _tec; _panelCenter.Normalize();
                float _mpf = ((cell.m_span / 6) * ((1 + (2 * cell.λ)) / (1 + cell.λ))) / cell.m_span;
                Vector3 _ymt = MathBase.EstimateSectionPosition(m_trailing_root, m_leading_root, (1 - _mpf));
                Vector3 _ymb = MathBase.EstimateSectionPosition(m_trailing_tip, m_leading_tip, (1 - _mpf));

                // ----------------------------------------- Resolve Velocity to Cell
                if (_core != null && _core.m_handler != null)
                {
                    //_core.m_handler.position = _aeroCenter;
                    //_core.m_handler.parent = transform;
                    //_core.m_handler.localEulerAngles = new Vector3(cell.θ, cell.ᴧQT, cell.Г);
                    //Vector3 m_velocity = _core.m_handler.InverseTransformDirection(m_world);
                    //cell.m_u = m_velocity.z;
                    //cell.m_v = m_velocity.x;
                    //cell.m_w = m_velocity.y;

                    cell.αf = Mathf.Atan(cell.m_w / cell.m_u) * Mathf.Rad2Deg * -1;
                }
                Vector3 _airflow = -m_world;
                Vector3 ϐf = Vector3.Cross(m_omega.normalized, (_aeroCenter - m_wcog).normalized);
                ϐf *= -((m_omega.magnitude) * (_aeroCenter - m_wcog).magnitude);
                _airflow += ϐf;
                Vector3 _parallelFlow = (transform.right * (Vector3.Dot(transform.right, _airflow)));
                _airflow -= _parallelFlow; cell.m_V = _airflow.magnitude;
                Vector3 _normalFlow = _airflow.normalized;


                // ----------------------------------------- Resolve AOA and Lift Direction
                float ρ = (float)_core.ρ;
                cell.α = Mathf.Acos(Vector3.Dot(_panelCenter, -_normalFlow)) * Mathf.Rad2Deg;
                Vector3 _lft = Vector3.Cross(_panelCenter, (_ymb - _ymt).normalized) * m_section;
                _lft.Normalize();
                if (Vector3.Dot(_lft, _normalFlow) < 0.0f) { cell.α = -cell.α; }
                cell.α = Mathf.Clamp(cell.α, -89.999f, 89.99999f);
                if (float.IsNaN(cell.α) || float.IsInfinity(cell.α)) { cell.α = 0.0f; }
                if (cell.α > m_maximumAOA) { m_maximumAOA = cell.α; }


                cell.m_Qdyn = 0.5f * ρ * Mathf.Pow(cell.m_V, 2);
                cell.m_Re = (ρ * cell.m_V * cell.m_meanChord) / (float)_core.m_atmosphere.μ;
                cell.βRad = (float)Math.Asin((cell.m_v / cell.m_V));
                if (float.IsNaN(cell.βRad) || float.IsInfinity(cell.βRad)) { cell.βRad = 0.0f; }
                cell.β = cell.βRad * Mathf.Rad2Deg;
                cell.m_Mach = cell.m_V / (float)_core.m_atmosphere.a;

                // ----------------------------------------- Extract Coefficients from Airfoils
                float CL, CD;
                if (foilType == FoilType.Conventional)
                {
                    float _rootCL = rootAirfoil.liftCurve.Evaluate(cell.α);
                    float _tipCL = tipAirfoil.liftCurve.Evaluate(cell.α);
                    float _rootCD = rootAirfoil.dragCurve.Evaluate(cell.α);
                    float _tipCD = tipAirfoil.dragCurve.Evaluate(cell.α);
                    CL = MathBase.EstimateSection(_rootCL, _tipCL, cell._mfx);
                    CD = MathBase.EstimateSection(_rootCD, _tipCD, cell._mfx);
                }
                else
                {
                    double rootCL = m_rootSuperfoil.GetCL(cell.m_Mach, cell.α);
                    double tipCL = m_tipSuperfoil.GetCL(cell.m_Mach, cell.α);
                    double rootCD = m_rootSuperfoil.GetCD(cell.m_Mach, cell.α);
                    double tipCD = m_tipSuperfoil.GetCD(cell.m_Mach, cell.α);
                    CL = (float)MathBase.EstimateSection(rootCL, tipCL, cell._mfx);
                    CD = (float)MathBase.EstimateSection(rootCD, tipCD, cell._mfx);
                }
                float _rootCm = rootAirfoil.momentCurve.Evaluate(cell.α);
                float _tipCm = tipAirfoil.momentCurve.Evaluate(cell.α);
              
                float Cm = MathBase.EstimateSection(_rootCm, _tipCm, cell._mfx);
                cell.m_groundCorrection = 1f;
                cell.CLα = cell.CL / (cell.α * Mathf.Deg2Rad);
                if (groundEffect == GroundEffectState.Consider && (_core.z < m_span * 2)) { cell.m_groundCorrection = MathBase.EstimateGroundEffectFactor(transform, cell.groundAxis, _aeroCenter, m_span, _core.groundLayer); }

                #region Vortex Lift

                if (m_foilType == AerofoilType.Wing && vortexLift == VortexLift.Consider)
                {
                    float αr = cell.α * Mathf.Deg2Rad;
                    float potentialLift = CL * Mathf.Sin(αr) * Mathf.Cos(αr) * Mathf.Cos(αr);
                    double ratioFactor = (cell.m_e * CL * CL) / (3.142f * m_aspectRatio);
                    float vortexBase = (CL - (float)ratioFactor) * (1 / (Mathf.Cos(cell.ᴧLE * Mathf.Deg2Rad))) * Mathf.Sin(αr) * Mathf.Sin(αr) * Mathf.Cos(αr);
                    cell.ΔCLvort = potentialLift + vortexBase;
                    cell.ΔCDvort = cell.ΔCLvort * Mathf.Tan(αr);
                }
                #endregion

                #region Slat Controls
                if (cell.m_slatActive && m_slatSections[p] == true)
                {
                    float dCldδ = liftDeltaCurve.Evaluate(cell.m_slatChord);
                    float ƞmax = nMaxCurve.Evaluate(cell.m_edgeRadius / (100 * cell.m_effectiveThickness));
                    float ƞδ = nDeltaCurve.Evaluate(m_slatDeflection);
                    cell.ΔCLδlf = dCldδ * ƞmax * ƞδ * m_slatDeflection;
                }
                #endregion

                #region Numeric Base Control

                if (m_foilType != AerofoilType.Balance)
                {
                    if (controlState == ControlType.Controllable && surfaceType != SurfaceType.Inactive && availableControls != AvailableControls.SecondaryOnly && m_controlSections[p] == true && controlAnalysis != AnalysisMethod.GeometricOnly)
                    {
                        cell.ΔCDδc = MathBase.CDf(cell.m_controlChord / cell.m_meanChord, SWc / m_area, m_controlDeflection, 1);
                        float Δclmax = clBaseCurve.Evaluate(cell.m_effectiveThickness * 100f);
                        float k1 = k1Curve.Evaluate(cell.m_controlChord * 100);
                        float numericDeflection = Mathf.Abs(m_controlDeflection);
                        float k2 = k2Curve.Evaluate(numericDeflection);
                        float k3 = k3Curve.Evaluate(numericDeflection / 60f);

                        if (controlCorrectionMethod == NumericCorrection.DATCOM) { cell.ΔCLδc = k1 * k2 * k3 * Δclmax * Mathf.Sign(m_controlDeflection); }
                        if (controlCorrectionMethod == NumericCorrection.KhanNahon)
                        {
                            float θf = Mathf.Acos(2 * (cell.m_controlChord) - 1);
                            float τ = 1 - (θf - Mathf.Sin(θf)) / Mathf.PI;
                            float ƞ = effectivenessPlot.Evaluate(numericDeflection);
                            cell.ΔCLδc = Mathf.Abs(cell.m_liftSlope) * τ * ƞ * numericDeflection * Mathf.Deg2Rad * Mathf.Sign(m_controlDeflection);
                        }
                    }
                }
                #endregion

                #region Numeric Flap Control

                if (m_foilType == AerofoilType.Wing && flapState == ControlState.Active && m_flapSections[p] == true && flapAnalysis != AnalysisMethod.GeometricOnly)
                {
                    cell.ΔCDδf = MathBase.CDf(cell.m_flapChord / cell.m_meanChord, SWf / m_area, m_flapDeflection, 1);
                    float Δclmax = clBaseCurve.Evaluate(cell.m_effectiveThickness * 100f);
                    float k1 = k1Curve.Evaluate(cell.m_flapChord * 100);
                    float numericDeflection = Mathf.Abs(m_flapDeflection);
                    float k2 = k2Curve.Evaluate(numericDeflection);
                    float k3 = k3Curve.Evaluate(numericDeflection / 60f);

                    if (flapCorrectionMethod == NumericCorrection.KhanNahon)
                    {
                        float θf = Mathf.Acos(2 * (cell.m_flapChord) - 1);
                        float τ = 1 - (θf - Mathf.Sin(θf)) / Mathf.PI;
                        float ƞ = effectivenessPlot.Evaluate(numericDeflection);
                        cell.ΔCLδf = Mathf.Abs(cell.m_liftSlope) * τ * ƞ * numericDeflection * Mathf.Deg2Rad * Mathf.Sign(m_flapDeflection);
                    }
                    if (flapCorrectionMethod == NumericCorrection.DATCOM) { cell.ΔCLδf = k1 * k2 * k3 * Δclmax * Mathf.Sign(m_flapDeflection); }

                }
                #endregion

                #region Spoiler Control
                cell.ΔCLδs = 1;
                if (m_foilType == AerofoilType.Wing && controlState == ControlType.Controllable && spoilerState == ControlState.Active && m_spoilerSections[p] == true)
                {
                    float spl = Mathf.Abs(m_spoilerDeflection) * Mathf.Deg2Rad;
                    cell.ΔCLδs = Mathf.Cos(spl);
                    cell.ΔCDδs = Mathf.Sin(spl) * 0.92f;
                    cell.ΔCLδs = Mathf.Clamp(cell.ΔCLδs, 0, 1);
                    cell.ΔCDδs = Mathf.Clamp(cell.ΔCDδs, 0, 1);
                }
                #endregion

                #region Drag Analysis

                // Induced Drag
                float CLi = CL * cell.m_ARf;
                cell.ΔCDi = (CLi * CLi) / (Mathf.PI * ((float)m_aspectRatio) * cell.m_e);

                // Wave Drag
                float _rootMcrit = rootAirfoil.Mcr;
                float _tipMcrit = tipAirfoil.Mcr;
                float _cellMcr = MathBase.EstimateSection(_rootMcrit, _tipMcrit, cell._mfx);
                if (_cellMcr < 0.5f) { _cellMcr = ((_rootMcrit * 2) + (_tipMcrit * 2)) / 2f; }
                float Mcrit = _cellMcr - (0.1f * CL);
                float M_Mcr = 1 - Mcrit;
                float cdw0 = 0.0264f;//20f * Mathf.Pow(M_Mcr, 4f);

                float δm = 1 - Mcrit;
                float xf = (-8f * (cell.m_Mach - (1 - (0.5f * δm)))) / δm;
                float fmx = Mathf.Exp(xf);
                float fm = 1 / (1 + fmx);
                float kdw = 0.5f;
                float kdwm = 0.05f;
                float dx = Mathf.Pow((Mathf.Pow((cell.m_Mach - kdwm), 2f) - 1), 2) + Mathf.Pow(kdw, 4);
                float correction = Mathf.Pow(Mathf.Cos(cell.ᴧQT * Mathf.Deg2Rad), 2.5f);
                cell.ΔCDw = (fm * cdw0 * kdw) / (Mathf.Pow(dx, 0.25f)) * correction;
                if (cell.ΔCDw < 0) { cell.ΔCDw = 0f; }

                // Skin Friction
                cell.ΔCDsfr = MathBase.EstimateSkinDragCoefficient(cell.m_rootChord, cell.m_tipChord, cell.m_V * 1.945, _core.m_atmosphere.ρ, _core.m_atmosphere.μ, cell.m_Mach, k);
                float cellFrictionDrag = cell.m_Qdyn * cell.m_wettedArea * cell.ΔCDsfr;

                // Induced Drag
                float correctedLift = CL * cell.m_ARf;
                cell.ΔCDi = (correctedLift * correctedLift) / (3.142f * ((float)m_aspectRatio) * cell.m_e);
                #endregion


                // ------------------------------------------------------------------ $$LIFT
                cell.CL =
                    (CL / Mathf.Sqrt(cell.m_groundCorrection)) +
                    cell.ΔCLvort +
                    cell.ΔCLδc +
                    cell.ΔCLδf +
                    cell.ΔCLδlf;
                cell.m_lift = cell.m_Kθ * cell.ΔCLδs * cell.m_area * cell.CL * cell.m_Qdyn;
                Vector3 cellLift = _lft * cell.m_lift;
                float CLsq = cell.CL * cell.CL;
                cell.CYβ = CLsq * cell.CYβ_CL;
                cell.Cnβ = CLsq * cell.Cnβ_CL;

                // ------------------------------------------------------------------ $$DRAG
                cell.CD =
                    (CD * Mathf.Sqrt(cell.m_groundCorrection)) +
                    cell.ΔCDvort +
                    cell.ΔCDδc +
                    cell.ΔCDδf +
                    cell.ΔCDδlf +
                    cell.ΔCDδs +
                    cell.ΔCDi +
                    cell.ΔCDw;
                cell.m_drag = (cell.m_area * cell.CD * cell.m_Qdyn * Mathf.Cos(cell.ᴧLE * Mathf.Deg2Rad)) + cellFrictionDrag;
                Vector3 cellDrag = _airflow;
                cellDrag.Normalize();
                cellDrag *= cell.m_drag;

                //3. ------------------------------------------------------------------ $$MOMENT
                cell.m_moment = cell.m_Qdyn * cell.m_area * cell.m_meanChord * cell.m_meanChord * (Cm * (float)(m_aspectRatio / (m_aspectRatio + 4f)));
                Vector3 cellMoment = Vector3.Cross(_panelCenter, cellLift.normalized);
                cellMoment.Normalize(); cellMoment *= cell.m_moment; cell.Cm = Cm;


                // ----------------------------------------- Apply
                Lift += cell.m_lift;
                Drag += cell.m_drag;
                Moment += cell.m_moment;
                if (!float.IsNaN(cell.m_lift) && !float.IsInfinity(cell.m_lift)) { _aircraft.m_rigidbody.AddForceAtPosition(cellLift, _aeroCenter, ForceMode.Force); }
                if (!float.IsNaN(cell.m_drag) && !float.IsInfinity(cell.m_drag)) { _aircraft.m_rigidbody.AddForceAtPosition(cellDrag, _aeroCenter, ForceMode.Force); }
                if (!float.IsNaN(cell.m_moment) && !float.IsInfinity(cell.m_moment)) { _aircraft.m_rigidbody.AddTorque(cellMoment, ForceMode.Force); }
                Vector _force = transform.InverseTransformDirection(cellLift + cellDrag);
                m_force += Transformation.UnityToVector(_force);
            }
        }
        /// <summary>
        /// Process sound emission for the flap and slat surfaces
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="target"></param>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        void AnalyseSound(int mode, float target)
        {
            if (m_foilType == AerofoilType.Wing)
            {
                if (_aircraft != null && _aircraft.m_view != null)
                {
                    if (_aircraft.m_view.cameraState == SilantroCamera.CameraState.Exterior)
                    {
                        if (_flapClampSource != null && _flapLoopSource != null) { _flapLoopSource.volume = 1f; _flapClampSource.volume = 1f; }
                        if (_slatClampSource != null && _slatLoopSource != null) { _slatLoopSource.volume = 1f; _slatClampSource.volume = 1f; }
                    }
                    if (_aircraft.m_view.cameraState == SilantroCamera.CameraState.Interior)
                    {
                        if (_flapClampSource != null && _flapLoopSource != null) { _flapLoopSource.volume = 0.3f; _flapClampSource.volume = 0.3f; }
                        if (_slatClampSource != null && _slatLoopSource != null) { _slatLoopSource.volume = 0.3f; _slatClampSource.volume = 0.3f; }
                    }
                }
                else
                {
                    if (_flapClampSource != null && _flapLoopSource != null) { _flapLoopSource.volume = 1f; _flapClampSource.volume = 1f; }
                    if (_slatClampSource != null && _slatLoopSource != null) { _slatLoopSource.volume = 1f; _slatClampSource.volume = 1f; }
                }
            }

            if (_flapDeflectionEngaged)
            {
                if (mode == 1) { if (_baseFlapDeflection > target) { if (!_flapLoopSource.isPlaying) { _flapLoopSource.Play(); } } else { _flapLoopSource.Stop(); _flapClampSource.PlayOneShot(_flapClamp); _flapDeflectionEngaged = false; } }
                if (mode == 2) { if (_baseFlapDeflection < target) { if (!_flapLoopSource.isPlaying) { _flapLoopSource.Play(); } } else { _flapLoopSource.Stop(); _flapClampSource.PlayOneShot(_flapClamp); _flapDeflectionEngaged = false; } }
            }

        }
        /// <summary>
        /// Call functions
        /// </summary>
        public void Compute(float _timestep)
        {
            if (allOk)
            {
                AnalyseControls(_timestep);
                AnalyseForces();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateLeftWing()
        {
            if (m_left != null)
            {
                // Switch Alignment
                if (m_left.m_position != Position.Left)
                {
                    m_left.m_position = Position.Left;
                }
                // Update Position
                m_left.transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                // Update Rotation
                m_left.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, -transform.localEulerAngles.y, -transform.localEulerAngles.z );

                m_left.sweepDirection = sweepDirection;
                m_left.m_sweep = m_sweep;
                m_left.sweepCorrectionMethod = sweepCorrectionMethod;
                m_left.twistDirection = twistDirection;
                m_left.m_twist = m_twist;
                m_left.m_taper = m_taper;
                m_left.subdivision = subdivision;
                m_left.m_axisScale = m_axisScale;

                m_left.controlState = controlState;
                m_left.availableControls = availableControls;
                m_left.flapState = flapState;
                m_left.controlState = controlState;
                m_left.slatState = slatState;
                m_left.spoilerState = spoilerState;

                m_left.m_flapRootChord = m_flapRootChord;
                m_left.m_flapTipChord = m_flapTipChord;
                m_left.m_controlRootChord = m_controlRootChord;
                m_left.m_controlTipChord = m_controlTipChord;
                m_left.m_slatRootChord = m_slatRootChord;
                m_left.m_slatTipChord = m_slatTipChord;

            }
        }

        #endregion

        #region Call Functions

        /// <summary>
        /// Decrease the current flap step and set the flap angle to a lower value
        /// </summary>
        public void RaiseFlap()
        {
            if (controlState == ControlType.Controllable && flapState == ControlState.Active && m_foilType == AerofoilType.Wing)
            {
                m_currentFlapStep--;
                if (m_currentFlapStep < 0) { m_currentFlapStep = 0; }
                if (_flapLoopSource != null && _flapLoopSource.isPlaying) { _flapLoopSource.Stop(); }
                if (_flapClampSource != null && _flapClampSource.isPlaying) { _flapClampSource.Stop(); }
                _flapDeflectionEngaged = true; _flapSoundMode = 1;
            }
        }
        /// <summary>
        /// Increase the current flap step and set the flap angle to a higher value
        /// </summary>
        public void LowerFlap()
        {
            if (controlState == ControlType.Controllable && flapState == ControlState.Active && m_foilType == AerofoilType.Wing)
            {
                m_currentFlapStep++;
                if (m_currentFlapStep > (m_flapSteps.Count - 1)) { m_currentFlapStep = m_flapSteps.Count - 1; }
                if (_flapLoopSource != null && _flapLoopSource.isPlaying) { _flapLoopSource.Stop(); }
                if (_flapClampSource != null && _flapClampSource.isPlaying) { _flapClampSource.Stop(); }
                _flapDeflectionEngaged = true; _flapSoundMode = 2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ActuateSlat() { if (!slatExtended) { _commandSlatMovement = _slatMovementLimit; StartCoroutine(ExtendSlat()); } else { _commandSlatMovement = 0f; StartCoroutine(RetractSlat()); } }
        public IEnumerator ExtendSlat() { yield return new WaitUntil(() => m_slatDeflection >= _slatMovementLimit - 1f); slatExtended = true; }
        public IEnumerator RetractSlat() { yield return new WaitUntil(() => m_slatDeflection <= 1f); slatExtended = false; }
        /// <summary>
        /// 
        /// </summary>
        public void ActuateSpoiler() { if (!spoilerExtended) { _commandSpoilerDeflection = sp_positiveLimit; StartCoroutine(ExtendSpoiler()); } else { _commandSpoilerDeflection = 0f; StartCoroutine(RetractSpoiler()); } }
        public IEnumerator ExtendSpoiler() { yield return new WaitUntil(() => Mathf.Abs(m_spoilerDeflection) >= sp_positiveLimit - 1f); spoilerExtended = true; }
        public IEnumerator RetractSpoiler() { yield return new WaitUntil(() => Mathf.Abs(m_spoilerDeflection) <= 1f); spoilerExtended = false; }

        #endregion
    }
    #endregion
}