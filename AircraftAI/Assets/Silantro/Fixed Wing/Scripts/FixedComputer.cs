
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using Oyedoyin.Common;
using Oyedoyin.Analysis;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Handles the collection and organization of all the connected aircraft components
/// </summary>
/// <remarks>
/// This component will collect the components connected to the aircraft root and set them up with the variables and components they
/// need to function properly. It also runs the core control functions in the dependent child components
/// </remarks>
/// 
namespace Oyedoyin.FixedWing
{
    #region Component
    public class FixedComputer : Computer
    {
        #region Enums
        public enum AugmentationType { StabilityAugmentation, CommandAugmentation }
        public enum Speedbrake { Open, Closed }
        public enum BankLimitState { Off, Left, Right }
        public enum PitchLimitState { Off, Up, Down }

        #endregion

        #region Enum Properties

        public AugmentationType m_augmentation = AugmentationType.StabilityAugmentation;
        public Speedbrake m_speedbrake = Speedbrake.Closed;


        #endregion

        public FixedController m_aircraft;
        public StabilityAugmentation m_stabilityAugmentation;
        public CommandAugmentation m_commandAugmentation;
        public Autopilot m_autopilot;

        public AutoThrottle m_throttleAugmentation;
        public ControlState m_lateralAutopilot;
        public ControlState m_longitudinalAutopilot;
        public ControlState m_autoThrottle;

        public ControlState m_autoSlat = ControlState.Off;
        public ControlMode m_flapControl = ControlMode.Manual;
        public ControlState bankLimiter = ControlState.Off;
        public ControlState gLimiter = ControlState.Off;
        public ControlState gWarner = ControlState.Off;
        public ControlState stallWarner = ControlState.Off;

        #region Alerts
        public AudioSource stallWarnerSource, gWarnerSource;
        public AudioClip m_stallClip, m_gClip;
        public bool m_stalling, m_overging;
        public double m_stallSpeed = 100f;
        [Range(0, 1f)] public float m_alarmVolume = 0.75f, m_gAlarmVolume = 0.75f;
        public double gThreshold = 8.5f;

        public List<double> wingStallAngles;
        public List<double> wingLiftCoefficient;
        public double maximumWingAlpha;
        public double baseStallAngle;
        public double alphaProt = 10f;
        public double alphaFloor = 10f;
        [Range(0, 5)] public double alphaThreshold = 2f;

        public double minimumStallSpeed = 30  ;
        public double minimumStallAltitude = 100 ;


        public double wingArea;
        public double baseliftCoefficient;
        public double wingLift;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            FixedComputer _fcs = GetComponent<FixedComputer>();
            m_commandAugmentation._flcs = _fcs;
            m_stabilityAugmentation._flcs = _fcs;
            m_autopilot._flcs = _fcs;
            m_gainSystem.m_controller = _fcs;
            m_throttleAugmentation.m_controller = _fcs;

            m_autopilot.Initialize();
            m_gainSystem.Initialize();
            if (m_augmentation == AugmentationType.CommandAugmentation) { m_commandAugmentation.Initialize(); }
            if (m_augmentation == AugmentationType.StabilityAugmentation) { m_stabilityAugmentation.Initialize(); }

            GameObject soundPoint = new GameObject("Sources");
            soundPoint.transform.parent = this.transform;
            soundPoint.transform.localPosition = Vector3.zero;
            if (stallWarner == ControlState.Active) { if (m_stallClip) { Handler.SetupSoundSource(soundPoint.transform, m_stallClip, "Stall Sound Point", 100f, true, false, out stallWarnerSource); stallWarnerSource.volume = m_alarmVolume; } }
            if (gWarner == ControlState.Active) { if (m_gClip) { Handler.SetupSoundSource(soundPoint.transform, m_gClip, "G Sound Point", 100f, true, false, out gWarnerSource); gWarnerSource.volume = m_gAlarmVolume; } }

            if (m_mode == Mode.Augmented)
            {
                Debug.Log("Flight computer on " + controller.transform.name + " is starting in " + m_augmentation.ToString() + " mode");
            }
            else { Debug.Log("Flight computer on " + controller.transform.name + " is starting in " + m_mode.ToString() + " mode"); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_timestep"></param>
        public override void Compute(double _timestep)
        {
            base.Compute(_timestep);

            if (controller.m_core.V > 2)
            {
                maximumTurnRate = ((controller.m_core.m_atmosphere.g * Math.Tan(maximumTurnBank * Mathf.Deg2Rad)) / controller.m_core.V) * Mathf.Rad2Deg;
            }
            m_gainSystem.Compute();

            if (m_mode == Mode.Augmented)
            {
                if (m_augmentation == AugmentationType.CommandAugmentation) { m_commandAugmentation.Compute(_timestep); }
                if (m_augmentation == AugmentationType.StabilityAugmentation) { m_stabilityAugmentation.Compute(_timestep); }
            }
            if (m_mode == Mode.Autonomous) { m_autopilot.Compute(_timestep); }
            if (m_mode == Mode.Augmented && m_augmentation == AugmentationType.CommandAugmentation) { m_autopilot.Compute(_timestep); }
            if (m_autoThrottle == ControlState.Active) { m_throttleAugmentation.Compute(_timestep); }

            #region Alarms

            FilterWingData();

            // G Warning System
            if (gWarner == ControlState.Active && m_mode != Mode.Manual)
            {
                if (maximumLoadFactor < gThreshold) { gThreshold = maximumLoadFactor; }
                if (controller.m_core.n < -3f || controller.m_core.n > gThreshold) { m_overging = true; } else { m_overging = false; }
                if (gWarnerSource)
                {
                    if (m_overging && !gWarnerSource.isPlaying) { gWarnerSource.Play(); }
                    if (!m_overging && gWarnerSource.isPlaying) { gWarnerSource.Stop(); }
                    gWarnerSource.volume = m_gAlarmVolume;
                }
            }

            // -------------- Stall Warning System
            //1. Speed
            wingLift = Math.Abs(controller.m_core.n) * controller.currentWeight * 9.81f; ;
            double w = controller.m_core.ρ * wingArea * baseliftCoefficient;
            m_stallSpeed = Math.Sqrt(wingLift / w);

            //2. AOA
            double stallThreshold = m_stallSpeed;
            if (controller.m_core.m_height > (minimumStallAltitude / Constants.m2ft)
                && controller.m_core.V > (minimumStallSpeed / Constants.ms2knots))
            {
                if (Math.Abs(maximumWingAlpha) >= Math.Abs(alphaFloor) ||
                    Mathf.Approximately((float)controller.m_core.V, (float)stallThreshold))
                { m_stalling = true; }
                else { m_stalling = false; }
            }
            else { m_stalling = false; }
            if (stallWarnerSource != null && stallWarner == ControlState.Active)
            {
                if (m_stalling && !stallWarnerSource.isPlaying) { stallWarnerSource.Play(); } //TODO Add Stall Data To Reward Function
                if (!m_stalling && stallWarnerSource.isPlaying) { stallWarnerSource.Stop(); }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private void FilterWingData()
        {
            wingStallAngles.Clear();
            wingLiftCoefficient.Clear();
            wingArea = 0f;
            maximumWingAlpha = -90f;

            foreach (SilantroAerofoil foil in m_aircraft.m_wings)
            {
                if (foil.m_maximumAOA > maximumWingAlpha) { maximumWingAlpha = foil.m_maximumAOA; }
                if (foil.rootAirfoil != null &&
                    foil.tipAirfoil != null &&
                    foil.m_foilType == SilantroAerofoil.AerofoilType.Wing)
                {
                    wingStallAngles.Add(foil.rootAirfoil.stallAngle);
                    wingStallAngles.Add(foil.tipAirfoil.stallAngle);

                    wingLiftCoefficient.Add(foil.rootAirfoil.upperLiftLimit);
                    wingLiftCoefficient.Add(foil.tipAirfoil.upperLiftLimit);
                    wingArea += foil.m_area;
                }
            }

            if (wingStallAngles.Count > 0)
            {
                baseStallAngle = wingStallAngles.Min();
                baseliftCoefficient = wingLiftCoefficient.Min();
            }
            if (baseStallAngle == 0 || baseStallAngle > 90f)
            {
                baseStallAngle = 15f;
                baseliftCoefficient = 1.5f;
            }
            alphaProt = baseStallAngle * 0.60f;
            alphaFloor = baseStallAngle - Math.Abs(alphaThreshold);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EnableSceneAutopilot()
        {
            base.EnableSceneAutopilot();
            m_modeStorage = m_mode;
            m_mode = Mode.Autonomous;

            m_autopilot.m_longitudinalMode = Autopilot.LongitudinalMode.AltitudeHold;
            m_autopilot.m_lateralMode = Autopilot.LateralMode.HeadingHold;
            m_autoThrottle = ControlState.Active;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void DisableSceneAutopilot()
        {
            base.DisableSceneAutopilot();
            m_mode = m_modeStorage;
            m_autoThrottle = ControlState.Off;
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class CommandAugmentation
        {
            public enum CommandPreset { Airliner, Fighter }
            public CommandPreset m_commandPreset = CommandPreset.Fighter;
            public BankLimitState m_bankState = BankLimitState.Off;
            public PitchLimitState m_pitchState = PitchLimitState.Off;
            [HideInInspector] public FixedComputer _flcs;

            private Lag m_lateralStick;
            private Lag m_pedalStick;
            private Lag m_alphaFilter;
            private Lag m_longStickFilter;
            private Lag m_gFilter;
            private Lag m_gCommandFilter;
            private LeadLagCompensator m_pitchCompensator;

            private AnimationCurve m_rollChannel;
            private AnimationCurve m_pitchChannel;
            private AnimationCurve m_yawChannel;
            private AnimationCurve m_F3;
            private AnimationCurve m_F9;
            private AnimationCurve m_F10;
            public float m_maximumRollRate = 220;

            [Range(0, 0.5f)] public float rollBreakPoint = 0.4f;
            [Range(0, 0.5f)] public float pitchBreakPoint = 0.4f;

            public bool rollLimitActive, gLimitActive;
            public double presetRollAngle = 0;
            public double commandG;

            public double m_rollRateCommand;
            public double m_rudderCommand;

            #region Leading Edge Flap && Trailing Edge Flap

            public LeadingEdgeFlap m_slatLogic;
            public double m_slatCommand;
            private double m_flapIntegral;
            public double m_flapLimit = 20;
            public double m_flapCommand;

            #endregion

            /// <summary>
            /// 
            /// </summary>
            public void Initialize()
            {
                // Reset Autopilot
                _flcs.m_longitudinalAutopilot = ControlState.Off;
                _flcs.m_lateralAutopilot = ControlState.Off;

                // Set Airliner Values
                if (m_commandPreset == CommandPreset.Airliner)
                {
                    m_maximumRollRate = 15;
                    _flcs.gLimiter = ControlState.Active;
                    _flcs.maximumTurnBank = 33;
                    _flcs.maximumLoadFactor = 2.5;
                    _flcs.minimumLoadFactor = 1;
                }

                m_rollChannel = new AnimationCurve();
                m_rollChannel.AddKey(new Keyframe(-1.0f, m_maximumRollRate));
                m_rollChannel.AddKey(new Keyframe(-0.647f, 0.2429f * m_maximumRollRate));
                m_rollChannel.AddKey(new Keyframe(-0.353f, 0.0610f * m_maximumRollRate));
                m_rollChannel.AddKey(new Keyframe(-0.059f, 0.0f));
                m_rollChannel.AddKey(new Keyframe(0.0f, 0.0f));
                m_rollChannel.AddKey(new Keyframe(0.059f, 0.0f));
                m_rollChannel.AddKey(new Keyframe(0.353f, -0.0610f * m_maximumRollRate));
                m_rollChannel.AddKey(new Keyframe(0.647f, -0.2429f * m_maximumRollRate));
                m_rollChannel.AddKey(new Keyframe(1.0f, -m_maximumRollRate));

                m_pitchChannel = new AnimationCurve();
                m_pitchChannel.AddKey(new Keyframe(-1.0f, 10.86f));
                m_pitchChannel.AddKey(new Keyframe(-0.181f, 0.44f));
                m_pitchChannel.AddKey(new Keyframe(-0.044f, 0.0f));
                m_pitchChannel.AddKey(new Keyframe(0.0f, 0.0f));
                m_pitchChannel.AddKey(new Keyframe(0.044f, 0.0f));
                m_pitchChannel.AddKey(new Keyframe(0.181f, -0.44f));
                m_pitchChannel.AddKey(new Keyframe(1.0f, -4.0f));

                m_yawChannel = new AnimationCurve();
                m_yawChannel.AddKey(new Keyframe(-1.0f, -30.0f));
                m_yawChannel.AddKey(new Keyframe(-0.125f, 0.0f));
                m_yawChannel.AddKey(new Keyframe(0.0f, 0.0f));
                m_yawChannel.AddKey(new Keyframe(0.125f, 0.0f));
                m_yawChannel.AddKey(new Keyframe(1.0f, 30.0f));

                MathBase.LinearizeCurve(m_pitchChannel);
                MathBase.LinearizeCurve(m_rollChannel);
                MathBase.LinearizeCurve(m_yawChannel);

                // -------------------- Roll
                m_lateralStick = new Lag { m_timeConstant = 0.01666666667 };

                // -------------------- Yaw
                m_pedalStick = new Lag { m_timeConstant = 0.01666666667 };

                // -------------------- Pitch
                m_longStickFilter = new Lag { m_timeConstant = 0.01666666667 };
                m_alphaFilter = new Lag { m_timeConstant = 0.10 };
                m_gCommandFilter = new Lag { m_timeConstant = 0.12048 };
                m_pitchCompensator = new LeadLagCompensator();
                m_pitchCompensator.Initialize(1.0, 0.0, 1.0, 1.0);
                m_gFilter = new Lag { m_timeConstant = 0.02 };

                // ------------------------------ Setup LEF
                m_slatLogic.Initialize(30, 25, 0.136, 1.38, 9.05, 1.45);

                #region Gains

                m_F3 = new AnimationCurve();
                m_F3.AddKey(new Keyframe(0, 1));
                m_F3.AddKey(new Keyframe(300, 1));
                m_F3.AddKey(new Keyframe(800, 0.533f));
                m_F3.AddKey(new Keyframe(3000, 0.083f));
                m_F3.AddKey(new Keyframe(3500, 0.083f));
                MathBase.LinearizeCurve(m_F3);

                m_F9 = new AnimationCurve();
                m_F9.AddKey(new Keyframe(0, 0));
                m_F9.AddKey(new Keyframe(0.787f, 0));
                m_F9.AddKey(new Keyframe(1.008f, -2));
                MathBase.LinearizeCurve(m_F9);

                m_F10 = new AnimationCurve();
                m_F10.AddKey(new Keyframe(0, 0.25f));
                m_F10.AddKey(new Keyframe(0.694f, 0.25f));
                m_F10.AddKey(new Keyframe(1.132f, 0.50f));
                m_F10.AddKey(new Keyframe(2.000f, 0.50f));

                #endregion


            }
            /// <summary>
            /// 
            /// </summary>
            public void Compute(double timestep)
            {
                #region Lateral Control

                double m_rollGain;
                if (m_commandPreset == CommandPreset.Airliner) { m_rollGain = 1.4333f; }
                else { m_rollGain = 0.12; }
                double m_rollControl = _flcs.m_autopilot.m_rollRate - m_rollRateCommand;
                double m_aileronCommand = MathBase.Clamp(m_rollControl * m_rollGain, -21.5, 21.5);

                if (_flcs.m_lateralAutopilot == ControlState.Off)
                {
                    #region Bank Limit Check

                    if (_flcs.bankLimiter == ControlState.Active)
                    {
                        if (_flcs.b_rollInput > _flcs.controller.m_input._rollDeadZone && _flcs.m_autopilot.m_bankAngle < -(_flcs.maximumTurnBank)) { presetRollAngle = _flcs.maximumTurnBank; rollLimitActive = true; m_bankState = BankLimitState.Right; }
                        else if (_flcs.b_rollInput < -_flcs.controller.m_input._rollDeadZone && _flcs.m_autopilot.m_bankAngle > (_flcs.maximumTurnBank)) { presetRollAngle = -_flcs.maximumTurnBank; rollLimitActive = true; m_bankState = BankLimitState.Left; }

                        if (rollLimitActive)
                        {
                            if (m_bankState == BankLimitState.Left) { if (_flcs.b_rollInput > rollBreakPoint) { rollLimitActive = false; m_bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                            if (m_bankState == BankLimitState.Right) { if (_flcs.b_rollInput < -rollBreakPoint) { rollLimitActive = false; m_bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                        }
                    }
                    else { if (rollLimitActive) { rollLimitActive = false; } }

                    #endregion

                    if (!rollLimitActive)
                    {
                        // ------------------------------ Lateral Stick force to Roll Rate
                        double m_lateralStickForce = m_lateralStick.Compute((_flcs.b_rollInput * 17f), timestep); //lbs
                        double m_stickNormal = m_lateralStickForce / 17;
                        m_rollRateCommand = m_rollChannel.Evaluate((float)m_stickNormal);
                        if (_flcs.m_gainState == GainState.TakeoffLanding) { m_rollRateCommand *= 0.542; }

                        // ------------------------------ Proportional Rate Control
                        if (_flcs.m_gainState == GainState.TakeoffLanding && _flcs.m_VKTS < 200) { _flcs.m_roll = m_aileronCommand / 21.5; }

                        // ------------------------------ Inner Loop Rate Control
                        else
                        {
                            _flcs.m_autopilot.m_rollRateSolver.m_multiplier = _flcs.m_gainSystem.m_rr;
                            double m_rollRateError = m_rollRateCommand - _flcs.m_autopilot.m_rollRate;
                            _flcs.m_roll = -_flcs.m_autopilot.m_rollRateSolver.Compute(m_rollRateError, _flcs.m_autopilot.m_rollAcceleration, timestep);
                        }
                    }
                    else
                    {
                        // ------------------------------ Outer Loop Bank Control
                        double m_rollAngleError = (-presetRollAngle) - _flcs.m_autopilot.m_bankAngle;
                        double m_rollDelta = _flcs.controller.m_core.δф;
                        _flcs.m_autopilot.m_bankSolver.m_maximum = _flcs.maximumRollRate;
                        _flcs.m_autopilot.m_bankSolver.m_minimum = -_flcs.maximumRollRate;
                        _flcs.m_commands.m_commandRollRate = _flcs.m_autopilot.m_bankSolver.Compute(m_rollAngleError, m_rollDelta, timestep);

                        // ------------------------------ Inner Loop Rate Control
                        _flcs.m_autopilot.m_rollRateSolver.m_multiplier = _flcs.m_gainSystem.m_rr;
                        _flcs.m_commands.m_commandRollRate = MathBase.Clamp(_flcs.m_commands.m_commandRollRate, -_flcs.maximumRollRate, _flcs.maximumRollRate);
                        double m_rollRateError = (_flcs.m_commands.m_commandRollRate) - _flcs.m_autopilot.m_rollRate;
                        _flcs.m_roll = -_flcs.m_autopilot.m_rollRateSolver.Compute(m_rollRateError, _flcs.m_autopilot.m_rollAcceleration, timestep);
                    }
                }

                #endregion

                #region Longitudinal Control

                double gMax = _flcs.maximumLoadFactor;
                double gMin = -_flcs.minimumLoadFactor;
                if (_flcs.m_gainState == GainState.TakeoffLanding) { gMax *= 0.5; gMin *= 0.5; }

                if (_flcs.m_longitudinalAutopilot == ControlState.Off)
                {
                    #region Load Factor Limiter

                    // -------------------------------------------------- G Limiter
                    if (_flcs.gLimiter == ControlState.Active)
                    {
                        if (_flcs.b_pitchInput > _flcs.controller.m_input._pitchDeadZone && _flcs.controller.m_core.n < gMin) { commandG = gMin; gLimitActive = true; m_pitchState = PitchLimitState.Down; }
                        else if (_flcs.b_pitchInput < -_flcs.controller.m_input._pitchDeadZone && _flcs.controller.m_core.n > gMax) { commandG = gMax; gLimitActive = true; m_pitchState = PitchLimitState.Up; }

                        if (gLimitActive)
                        {
                            if (m_pitchState == PitchLimitState.Up) { if (_flcs.b_pitchInput > -_flcs.controller.m_input._pitchDeadZone) { gLimitActive = false; m_pitchState = PitchLimitState.Off; commandG = 0f; } }
                            if (m_pitchState == PitchLimitState.Down) { if (_flcs.b_pitchInput < _flcs.controller.m_input._pitchDeadZone) { gLimitActive = false; m_pitchState = PitchLimitState.Off; commandG = 0f; } }
                        }
                    }
                    else { if (gLimitActive) { gLimitActive = false; } }

                    #endregion

                    double alpha = _flcs.controller.m_core.α;
                    if (_flcs.m_Vm < 5) { alpha = 0; }
                    double α_lag = m_alphaFilter.Compute(alpha, timestep);
                    α_lag = MathBase.Clamp(α_lag, -5.0, 30.0);
                    m_pitchCompensator.Compute(_flcs.m_autopilot.m_pitchRate, timestep);
                    double gFiltered = m_gFilter.Compute(_flcs.controller.m_core.n, timestep);
                    double pitchInput = _flcs.b_pitchInput;
                    double m_pitchBase = pitchInput < 0f ? pitchInput * 40 : pitchInput * 17.65;
                    double m_longStickForce = m_longStickFilter.Compute(-m_pitchBase, timestep);

                    double m_stickNormal = 0;
                    if (_flcs.m_gainState == GainState.Cruise)
                    {
                        if (_flcs.m_dynamicPressue < 1628) { gMin = -1.0; } // 34 psf
                        else if (_flcs.m_dynamicPressue < 8810.0) // 184 psf
                        {
                            gMin = -1.0 - (3.0 / (8810.0 - 1628.0)) * (_flcs.m_dynamicPressue - 1628.0);
                        }
                    }
                    if (m_longStickForce < 0) { m_stickNormal = m_longStickForce / 17.65; }
                    if (m_longStickForce > 0) { m_stickNormal = m_longStickForce / 40; }
                    double m_commandG = m_pitchChannel.Evaluate((float)-m_stickNormal);
                    double m_gbaseCommand = m_gCommandFilter.Compute(m_commandG, timestep);
                    m_gbaseCommand = MathBase.Clamp(m_gbaseCommand, gMin, gMax) * (_flcs.controller.m_grounded ? 0.5 : 1.0);


                    if (_flcs.controller.m_core.Vkts > 40)
                    {
                        // ------------------------------ Inner Loop Rate Control
                        if (!gLimitActive)
                        {
                            _flcs.m_commands.m_commandPitchRate = ((1845f * m_gbaseCommand) / (_flcs.m_Vm * Constants.m2ft));
                            _flcs.m_autopilot.m_pitchRateSolver.m_multiplier = _flcs.m_gainSystem.m_pr;
                            _flcs.m_commands.m_commandPitchRate = MathBase.Clamp(_flcs.m_commands.m_commandPitchRate, -_flcs.maximumPitchRate, _flcs.maximumPitchRate);
                            double m_pitchRateError = (_flcs.m_commands.m_commandPitchRate) - _flcs.m_autopilot.m_pitchRate;
                            _flcs.m_pitch = -_flcs.m_autopilot.m_pitchRateSolver.Compute(m_pitchRateError, _flcs.m_autopilot.m_pitchAcceleration, timestep);
                        }
                        // ------------------------------ GExtreme Control
                        else
                        {
                            _flcs.m_commands.m_commandPitchRate = ((1845f * commandG - 1) / (_flcs.m_Vm * Constants.m2ft));
                            _flcs.m_autopilot.m_pitchRateSolver.m_multiplier = _flcs.m_gainSystem.m_pr;
                            _flcs.m_commands.m_commandPitchRate = MathBase.Clamp(_flcs.m_commands.m_commandPitchRate, -_flcs.maximumPitchRate, _flcs.maximumPitchRate);
                            double m_pitchRateError = (_flcs.m_commands.m_commandPitchRate) - _flcs.m_autopilot.m_pitchRate;
                            _flcs.m_pitch = -_flcs.m_autopilot.m_pitchRateSolver.Compute(m_pitchRateError, _flcs.m_autopilot.m_pitchAcceleration, timestep);
                        }
                    }
                    else { _flcs.m_pitch = _flcs.b_pitchInput; }
                }

                #endregion

                #region Directional Control

                // ------------------------- Pedal force to Rudder Deflection
                double m_pedalForce = m_pedalStick.Compute((-_flcs.b_yawInput * 120f), timestep); //Fp pounds
                double m_pedalNormal = m_pedalForce / 120;
                m_rudderCommand = m_yawChannel.Evaluate((float)m_pedalNormal);

                if (_flcs.m_VKTS > 600) { m_rudderCommand = 0; }
                _flcs.m_yaw = -m_rudderCommand / 30;

                #endregion

                // Slat Control
                if (_flcs.m_Vm > 2) { m_slatCommand = m_slatLogic.Evaluate(_flcs.m_Qbar, _flcs.m_Ps, _flcs.controller.m_core.α, timestep); }
                // Flap Control
                ComputeFlaps(timestep);
            }
            /// <summary>
            /// 
            /// </summary>
            private void ComputeFlaps(double dt)
            {
                double m_command = (_flcs.m_flapState == Flaps.Down || _flcs.controller.m_gearState == Controller.GearState.Down) ? m_flapLimit : 0.0;
                double m_transition = MathBase.Clamp(m_F9.Evaluate((float)_flcs.m_pressureFactor), -2.0, 2.0);
                double m_limit = MathBase.Clamp((m_command + m_transition - m_flapIntegral), -0.625, 0.625);
                m_flapIntegral = m_flapIntegral + 8.0 * dt * m_limit;
                m_flapCommand = m_flapIntegral + 1.5;
                if (_flcs.m_KIAS > 370) { m_flapCommand = 0; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class StabilityAugmentation
        {
            public FixedComputer _flcs;

            #region Stability Augmentation Properties

            [Header("Low Pass Filters")]
            public Lag m_pitchAttitudeFilter;
            public Lag m_pitchAttitudeRateFilter;
            public Lag m_pitchRateFilter;
            public Lag m_rollAttitudeFilter;
            public Lag m_rollAttitudeRateFilter;
            public Lag m_rollRateFilter;
            public Lag m_yawRateFilter;

            [Header("SAS Data")]
            private readonly double ρ0 = 1.225;
            public double ρH;
            public float ρhρ0;
            public float m_rollBreakout = 0.6f;
            public float m_pitchBreakout = 0.5f;

            [Serializable]
            public class SASAuthorityLimit
            {
                [Range(0, 1)] public double m_leveler = 0.4;
                [Range(0, 1)] public double m_rateLimiter = 0.3;
                [Range(0, 1)] public double m_attitudeRateLimiter = 0.3;
            }
            public ControlState m_pitchSAS;
            public ControlState m_pitchLeveler;
            public ControlState m_pitchRateLimiter;
            public ControlState m_pitchAttitudeRateLimiter;
            [Range(0, 1)] public double m_pitchSASLimit = 0.4;
            public SASAuthorityLimit m_pitchAuthorityLimits;
            private readonly double τθ = 0.08;
            private readonly double τq = 0.08;
            private readonly double τδθ = 0.08;
            public double Kθ = 0.15;
            public double Kq = 0.015;
            public double Kδθ = 0.015;

            public ControlState m_rollSAS;
            public ControlState m_rollLeveler;
            public ControlState m_rollRateLimiter;
            public ControlState m_rollAttitudeRateLimiter;
            [Range(0, 1)] public double m_rollSASLimit = 0.4;
            public SASAuthorityLimit m_rollAuthorityLimits;
            private readonly double τф = 0.08;
            private readonly double τp = 0.08;
            private readonly double τδф = 0.08;
            public double Kф = 0.15;
            public double Kp = 0.015;
            public double Kδф = 0.015;
            public double m_groundYawGain = 0.28;

            [Header("SAS Output")]
            public double δeθ;
            public double δeq;
            public double δeδθ;
            public double δe;
            public double δaф;
            public double δap;
            public double δaδф;
            public double δa;

            #endregion

            #region Stability Augmentation Functions

            /// <summary>
            /// 
            /// </summary>
            internal void Initialize()
            {

            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="_timestep"></param>
            internal void Compute(double _timestep)
            {
                // ------------------------------ Air density effects
                ρH = _flcs.controller.m_core.m_atmosphere.ρ;
                ρhρ0 = (float)(ρH / ρ0);

                // ------------------------------ Collect data from core processor
                double m_bankAngle = _flcs.controller.m_core.ф;
                double m_rollRate = _flcs.controller.m_core.p * Mathf.Rad2Deg;
                //double m_rollAcceleration = _flcs.controller.m_core.δp * Mathf.Rad2Deg;
                double m_rollDelta = _flcs.controller.m_core.δф;
                //double m_pitchAcceleration = _flcs.controller.m_core.δq * Mathf.Rad2Deg;
                double m_pitchRate = _flcs.controller.m_core.q * Mathf.Rad2Deg;
                double m_pitchAngle = _flcs.controller.m_core.θ;
                double m_pitchDelta = _flcs.controller.m_core.δθ;
                double m_yawAcceleration = _flcs.controller.m_core.δr * Mathf.Rad2Deg;
                double m_yawRate = _flcs.controller.m_core.r * Mathf.Rad2Deg;

                #region Pitch Axis


                if (m_pitchSAS == ControlState.Active)
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_pitchRateLimiter == ControlState.Active)
                    {
                        m_pitchRateFilter.m_timeConstant = τq;
                        double m_kq = Kq * m_pitchRate * _flcs.m_gainSystem.m_pr;
                        δeq = m_pitchRateFilter.Compute(m_kq, _timestep);
                    }
                    else { δeq = 0; }
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_pitchAttitudeRateLimiter == ControlState.Active)
                    {
                        m_pitchAttitudeRateFilter.m_timeConstant = τδθ;
                        double m_theta_dot = Kδθ * m_pitchDelta * _flcs.m_gainSystem.m_pr;
                        δeδθ = m_pitchAttitudeRateFilter.Compute(m_theta_dot, _timestep);
                    }
                    else { δeδθ = 0; }
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_pitchLeveler == ControlState.Active)
                    {
                        m_pitchAttitudeFilter.m_timeConstant = τθ;
                        double m_ktheta = Kθ * m_pitchAngle * _flcs.m_gainSystem.m_pr;
                        δeθ = m_pitchAttitudeFilter.Compute(m_ktheta, _timestep);
                    }
                    else { δeθ = 0; }
                }
                else
                {
                    δeθ = 0;
                    δeq = 0;
                    δeδθ = 0;
                    δe = 0;
                }


                δeθ = MathBase.Clamp(δeθ, -m_pitchAuthorityLimits.m_leveler, m_pitchAuthorityLimits.m_leveler);
                δeq = MathBase.Clamp(δeq, -m_pitchAuthorityLimits.m_rateLimiter, m_pitchAuthorityLimits.m_rateLimiter);
                δeδθ = MathBase.Clamp(δeδθ, -m_pitchAuthorityLimits.m_attitudeRateLimiter, m_pitchAuthorityLimits.m_attitudeRateLimiter);
                if (double.IsNaN(δeθ) || double.IsInfinity(δeθ)) { δeθ = 0.0; }
                if (double.IsNaN(δeq) || double.IsInfinity(δeq)) { δeq = 0.0; }
                if (double.IsNaN(δeδθ) || double.IsInfinity(δeδθ)) { δeδθ = 0.0; }
                δe = δeθ + δeq + δeδθ;
                if (double.IsNaN(δe) || double.IsInfinity(δe)) { δe = 0.0; }
                δe = MathBase.Clamp(δe, -m_pitchSASLimit, m_pitchSASLimit);
                if (Math.Abs(_flcs.b_pitchInput) > m_pitchBreakout) { δeθ = 0; }
                _flcs.m_pitch = _flcs.b_pitchInput + δe;
                _flcs.m_pitch = MathBase.Clamp(_flcs.m_pitch, -1, 1);
                if (double.IsNaN(_flcs.m_pitch) || double.IsInfinity(_flcs.m_pitch)) { _flcs.m_pitch = 0.0; }

                #endregion

                #region Roll Axis

                if (m_rollSAS == ControlState.Active)
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_rollLeveler == ControlState.Active)
                    {
                        m_rollAttitudeFilter.m_timeConstant = τф;
                        double m_kphi = Kф * m_bankAngle * _flcs.m_gainSystem.m_rr;
                        δaф = m_rollAttitudeFilter.Compute(m_kphi, _timestep);
                    }
                    else { δaф = 0; }
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_rollAttitudeRateLimiter == ControlState.Active)
                    {
                        m_rollAttitudeRateFilter.m_timeConstant = τδф;
                        double m_kphi_dot = Kδф * m_rollDelta * _flcs.m_gainSystem.m_rr;
                        δaδф = m_rollAttitudeRateFilter.Compute(m_kphi_dot, _timestep);
                    }
                    else { δaδф = 0; }
                    /// <summary>
                    /// 
                    /// </summary>
                    if (m_rollRateLimiter == ControlState.Active)
                    {
                        m_rollRateFilter.m_timeConstant = τp;
                        double m_kp = Kp * m_rollRate * _flcs.m_gainSystem.m_rr;
                        δap = m_rollRateFilter.Compute(m_kp, _timestep);
                    }
                    else { δap = 0; }
                }
                else { δaф = δap = δaδф = δa = 0; }

                δaф = MathBase.Clamp(δaф, -m_rollAuthorityLimits.m_leveler, m_rollAuthorityLimits.m_leveler);
                δap = MathBase.Clamp(δap, -m_rollAuthorityLimits.m_rateLimiter, m_rollAuthorityLimits.m_rateLimiter);
                δaδф = MathBase.Clamp(δaδф, -m_rollAuthorityLimits.m_attitudeRateLimiter, m_rollAuthorityLimits.m_attitudeRateLimiter);
                if (double.IsNaN(δaф) || double.IsInfinity(δaф)) { δaф = 0.0; }
                if (double.IsNaN(δap) || double.IsInfinity(δap)) { δap = 0.0; }
                if (double.IsNaN(δaδф) || double.IsInfinity(δaδф)) { δaδф = 0.0; }
                δa = δaф + δap + δaδф;
                if (double.IsNaN(δa) || double.IsInfinity(δa)) { δa = 0.0; }
                δa = MathBase.Clamp(δa, -m_rollSASLimit, m_rollSASLimit);
                if (Math.Abs(_flcs.b_rollInput) > m_rollBreakout) { δaф = 0; }
                _flcs.m_roll = _flcs.b_rollInput + δa;
                _flcs.m_roll = MathBase.Clamp(_flcs.m_roll, -1, 1);
                if (double.IsNaN(_flcs.m_roll) || double.IsInfinity(_flcs.m_roll)) { _flcs.m_roll = 0.0; }

                #endregion

                #region Yaw Axis

                if (_flcs.controller.m_grounded)
                {
                    double m_yawAngle = _flcs.controller.m_core.ψ;
                    double m_yawAngleError = _flcs.m_commands.m_commandYawAngle - m_yawAngle;
                    double m_yawDelta = _flcs.controller.m_core.δψ;
                    _flcs.m_autopilot.m_groundAngleSolver.m_maximum = 3;
                    _flcs.m_autopilot.m_groundAngleSolver.m_minimum = -3;
                    double m_commandYawRate = _flcs.m_autopilot.m_groundAngleSolver.Compute(m_yawAngleError, m_yawDelta, _timestep);

                    double m_yawRateError = m_commandYawRate - m_yawRate;
                    _flcs.m_yaw = _flcs.m_autopilot.m_yawRateSolver.Compute(m_yawRateError, m_yawAcceleration, _timestep);
                }
                else { _flcs.m_yaw = _flcs.b_yawInput; }

                #endregion

                _flcs.m_carbHeatInput = _flcs.b_carbHeatInput;
                _flcs.m_mixtureInput = _flcs.b_mixtureInput;
                _flcs.m_propPitchInput = _flcs.b_propPitchInput;
                _flcs.m_throttleInput = _flcs.b_throttleInput;
                _flcs.m_collectiveInput = _flcs.b_collectiveInput;
            }
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Autopilot
        {
            [HideInInspector] public FixedComputer _flcs;
            public enum LateralMode { HeadingHold, AttitudeHold, TurnHold, RadiusHold }
            public enum LongitudinalMode { AttitudeHold, AltitudeHold }

            public LateralMode m_lateralMode = LateralMode.AttitudeHold;

            [Header("Lateral Components")]
            public Lag m_rollRateSensor;
            public Lag m_rollCommandFilter;
            public SecondOrderFilter m_rollStructuralFilter;
            public FPID m_rollRateSolver;
            [HideInInspector] public FPID m_bankSolver;
            [HideInInspector] public FPID m_headingSolver;

            public double m_bankGain = 1.05;
            public double m_headingGain = 0.15;
            public double m_turnGain = 1.06;

            [Header("Lateral Data")]
            public double m_rollRate;
            public double m_rollAcceleration;
            public double m_bankAngle;
            public double m_heading;
            public double m_turnRate;
            public double m_turnRadius;


            public LongitudinalMode m_longitudinalMode = LongitudinalMode.AttitudeHold;
            public Lag m_pitchRateSensor;
            public Lag m_pitchCommandFilter;
            public SecondOrderFilter m_pitchStructuralFilter;
            private FPID m_pitchAngleSolver;
            public FPID m_pitchRateSolver;
            public FPID m_climbSolver;
            public FPID m_altitudeSolver;


            public double m_pitchGain = 0.95;
            public double m_climbGain = 0.8;
            public double m_climbGainWashout = 0;
            public double m_altitudeGain = 0.15;
            public double m_altitudeIntegral = 0;
            public double m_altitudeWindup = 5;
            public double m_altitudeGainWashout = 0.005;
            public double m_takeoffSpeed = 100;

            [Header("Longitudinal Data")]
            public double m_pitchRate;
            public double m_pitchAngle;
            public double m_α;
            public double m_flightPath;
            public double m_altitude;
            public double m_climbRate;
            public double m_pitchAcceleration;
            double m_speedThreshold;
            double m_baseAltitude;


            [Header("Directional Components")]
            public Lag m_yawRateSensor;
            public Lag m_yawCommandFilter;
            public SecondOrderFilter m_yawStructuralFilter;
            public FPID m_yawRateSolver;
            public FPID m_groundAngleSolver;

            [Header("Directional Data")]
            public double m_yawRate;
            public double m_yawAngle;
            public double m_yawAcceleration;

            /// <summary>
            /// 
            /// </summary>
            internal void Initialize()
            {
                // ------------------------------ Setup Gains
                m_pitchAngleSolver = new FPID { m_Kp = m_pitchGain, m_Ki = 0, m_Kd = 0 };
                m_climbSolver = new FPID { m_Kp = m_climbGain, m_Ki = 0, m_Kd = m_climbGainWashout };
                m_bankSolver = new FPID { m_Kp = m_bankGain, m_Ki = 0, m_Kd = 0 };
                m_headingSolver = new FPID { m_Kp = m_headingGain, m_Ki = 0, m_Kd = 0 };
                m_speedThreshold = m_takeoffSpeed / 1.94384;
                m_baseAltitude = _flcs.controller.transform.position.y;
                m_altitudeSolver = new FPID { m_Kp = m_altitudeGain, m_Ki = m_altitudeIntegral, m_Kd = 0 };
                m_altitudeSolver.m_antiWindup = FPID.AntiWindupState.Active;

                // ------------------------------ Lateral
                m_rollCommandFilter.m_timeConstant = 1.0 / 10.0;
                m_rollRateSensor.m_timeConstant = 1.0 / 50.0; //50 Hz
                m_rollStructuralFilter.Initialize(4.0, 64.0, 6400.0, 1.0, 80.0, 6400.0);

                // ------------------------------ Longitudinal
                m_pitchCommandFilter.m_timeConstant = 0.20;
                m_pitchRateSensor.m_timeConstant = 0.02;
                m_pitchStructuralFilter.Initialize(4.0, 64.0, 6400.0, 1.0, 80.0, 6400.0);

                // ------------------------------ Directional
                m_yawCommandFilter.m_timeConstant = 0.20;
                m_yawRateSensor.m_timeConstant = 0.02;
                m_yawStructuralFilter.Initialize(4.0, 64.0, 6400.0, 1.0, 80.0, 6400.0);
            }
            /// <summary>
            /// 
            /// </summary>
            internal void Compute(double timestep)
            {
                // Common Components
                m_rollAcceleration = _flcs.controller.m_core.δp * Mathf.Rad2Deg;
                double rollSense = m_rollRateSensor.Compute(_flcs.controller.m_core.p, timestep);
                m_rollRate = m_rollStructuralFilter.Compute(rollSense, timestep) * Mathf.Rad2Deg;
                m_turnRate = _flcs.controller.m_core.ωф;
                m_turnRadius = _flcs.controller.m_core.Rф;
                m_bankAngle = _flcs.controller.m_core.ф;
                m_heading = _flcs.controller.transform.eulerAngles.y;
                if (m_heading > 180) { m_heading -= 360f; }

                m_pitchAcceleration = _flcs.controller.m_core.δq * Mathf.Rad2Deg;
                double pitchSense = m_pitchRateSensor.Compute(_flcs.controller.m_core.q, timestep);
                m_pitchRate = m_pitchStructuralFilter.Compute(pitchSense, timestep) * Mathf.Rad2Deg;
                m_pitchAngle = _flcs.controller.m_core.θ;
                m_α = _flcs.controller.m_core.α;
                m_climbRate = _flcs.controller.m_core.δz;
                m_altitude = _flcs.controller.m_core.z;
                m_flightPath = Math.Asin(m_climbRate / _flcs.m_Vm) * Mathf.Rad2Deg;

                m_yawAcceleration = _flcs.controller.m_core.δr * Mathf.Rad2Deg;
                double yawSense = m_yawRateSensor.Compute(_flcs.controller.m_core.r, timestep);
                m_yawRate = m_yawStructuralFilter.Compute(yawSense, timestep) * Mathf.Rad2Deg;
                m_yawAngle = _flcs.controller.m_core.ψ;

                // Set Solver Gains
                m_bankSolver.m_Kp = m_bankGain;
                m_headingSolver.m_Kp = m_headingGain;
                m_climbSolver.m_Kp = m_climbGain;
                m_climbSolver.m_Kd = m_climbGainWashout;
                m_altitudeSolver.m_Kp = m_altitudeGain;
                m_altitudeSolver.m_Ki = m_altitudeIntegral;
                m_altitudeSolver.m_integralLimit = m_altitudeWindup;

                /// <summary>
                /// 
                /// </summary>
                if (_flcs.m_lateralAutopilot == ControlState.Active)
                {
                    /// <summary>
                    /// Outer Loop Heading Control
                    /// </summary>
                    if (m_lateralMode == LateralMode.HeadingHold)
                    {
                        double m_chdg = _flcs.m_commands.m_commandHeading;
                        if (m_chdg > 180) { m_chdg -= 360f; }
                        double m_headingError = m_chdg - m_heading;
                        double m_headingAcceleration = _flcs.controller.m_core.δψ;
                        m_headingSolver.m_maximum = _flcs.maximumTurnRate;
                        m_headingSolver.m_minimum = -_flcs.maximumTurnRate;
                        _flcs.m_commands.m_commandTurnRate = m_headingSolver.Compute(m_headingError, m_headingAcceleration, timestep);

                        _flcs.m_commands.m_commandBankAngle = Math.Atan((_flcs.m_commands.m_commandTurnRate * (_flcs.m_Vm * 1.94384)) / 1091) * Mathf.Rad2Deg;
                        double m_bankLim = Math.Atan((_flcs.maximumTurnRate * (_flcs.m_Vm * 1.94384)) / 1091) * Mathf.Rad2Deg;
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -m_bankLim, m_bankLim);
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -_flcs.maximumTurnBank, _flcs.maximumTurnBank);
                    }
                    /// <summary>
                    /// Outer Loop Turn Control
                    /// </summary>
                    if (m_lateralMode == LateralMode.TurnHold)
                    {
                        _flcs.m_commands.m_commandTurnRate = MathBase.Clamp(_flcs.m_commands.m_commandTurnRate, -_flcs.maximumTurnRate, _flcs.maximumTurnRate);
                        _flcs.m_commands.m_commandBankAngle = Math.Atan((_flcs.m_commands.m_commandTurnRate * _flcs.m_VKTS) / 1091) * Mathf.Rad2Deg;
                        double m_bankLim = Math.Atan((_flcs.maximumTurnRate * _flcs.m_VKTS) / 1091) * Mathf.Rad2Deg;
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -m_bankLim, m_bankLim);
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -_flcs.maximumTurnBank, _flcs.maximumTurnBank);
                    }
                    /// <summary>
                    /// Outer Loop Loiter Control
                    /// </summary>
                    if (m_lateralMode == LateralMode.RadiusHold)
                    {
                        double maxRate = (Math.Tan(_flcs.maximumTurnBank * Mathf.Deg2Rad) * 1091) / _flcs.m_KIAS;
                        double minRadius = (29.5325755 * _flcs.m_KIAS) / maxRate;
                        if (_flcs.m_commands.m_commandRadius < minRadius) { _flcs.m_commands.m_commandRadius = minRadius; }

                        if (Math.Abs(_flcs.m_commands.m_commandRadius) > 1) { _flcs.m_commands.m_commandTurnRate = (29.5325755 * _flcs.m_KIAS) / (_flcs.m_commands.m_commandRadius); }
                        _flcs.m_commands.m_commandBankAngle = Math.Atan((_flcs.m_commands.m_commandTurnRate * _flcs.m_KIAS) / 1091) * Mathf.Rad2Deg;
                        double m_bankLim = Math.Atan((_flcs.maximumTurnRate * _flcs.m_KIAS) / 1091) * Mathf.Rad2Deg;
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -m_bankLim, m_bankLim);
                        _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -_flcs.maximumTurnBank, _flcs.maximumTurnBank);
                    }

                    // ------------------------------ Outer Loop Attitude Control
                    _flcs.m_commands.m_commandBankAngle = MathBase.Clamp(_flcs.m_commands.m_commandBankAngle, -_flcs.maximumTurnBank, _flcs.maximumTurnBank);
                    double m_rollAngleError = _flcs.m_commands.m_commandBankAngle - m_bankAngle;
                    double m_rollDelta = _flcs.controller.m_core.δф;
                    m_bankSolver.m_maximum = _flcs.maximumRollRate;
                    m_bankSolver.m_minimum = -_flcs.maximumRollRate;
                    _flcs.m_commands.m_commandRollRate = m_bankSolver.Compute(m_rollAngleError, m_rollDelta, timestep);


                    // ------------------------------ Inner Loop Rate Control
                    m_rollRateSolver.m_multiplier = _flcs.m_gainSystem.m_rr;
                    _flcs.m_commands.m_commandRollRate = MathBase.Clamp(_flcs.m_commands.m_commandRollRate, -_flcs.maximumRollRate, _flcs.maximumRollRate);
                    double m_rollRateError = (_flcs.m_commands.m_commandRollRate) - m_rollRate;
                    _flcs.m_roll = -m_rollRateSolver.Compute(m_rollRateError, m_rollAcceleration, timestep);
                }
                /// <summary>
                /// 
                /// </summary>
                if (_flcs.m_longitudinalAutopilot == ControlState.Active)
                {
                    if (m_longitudinalMode == LongitudinalMode.AttitudeHold)
                    {
                        // ------------------------------ Outer Loop Attitude Control
                        _flcs.m_commands.m_commandPitchAngle = MathBase.Clamp(_flcs.m_commands.m_commandPitchAngle, -_flcs.minimumPitchAngle, _flcs.maximumPitchAngle);
                        double m_pitchAngleError = _flcs.m_commands.m_commandPitchAngle - m_pitchAngle;
                        double m_pitchDelta = _flcs.controller.m_core.δθ;
                        m_pitchAngleSolver.m_maximum = _flcs.maximumPitchRate;
                        m_pitchAngleSolver.m_minimum = -_flcs.maximumPitchRate;
                        _flcs.m_commands.m_commandPitchRate = m_pitchAngleSolver.Compute(m_pitchAngleError, m_pitchDelta, timestep);
                    }

                    if (m_longitudinalMode == LongitudinalMode.AltitudeHold)
                    {
                        double _commandAlt = _flcs.m_commands.m_commandAltitude;
                        if (_flcs.m_Vm < m_speedThreshold) { _commandAlt = m_baseAltitude; }

                        //// ------------------------------ Outer Loop Altitude Control
                        double m_heightError = _commandAlt - m_altitude;
                        double m_positiveClimb = _flcs.maximumClimbRate / Constants.toFtMin;
                        double m_negativeClimb = _flcs.maximumDecentRate / Constants.toFtMin;
                        m_altitudeSolver.m_maximum = m_positiveClimb;
                        m_altitudeSolver.m_minimum = -m_negativeClimb;
                        _flcs.m_commands.m_commandClimbRate = m_altitudeSolver.Compute(m_heightError, m_climbRate, timestep);
                        if (_flcs.m_Vm < m_speedThreshold) { _flcs.m_commands.m_commandClimbRate = 0; }

                        //// ------------------------------ Outer Loop Climb Control
                        double m_climbError = _flcs.m_commands.m_commandClimbRate - m_climbRate;
                        m_climbSolver.m_maximum = _flcs.maximumPitchRate;
                        m_climbSolver.m_minimum = -_flcs.maximumPitchRate;
                        _flcs.m_commands.m_commandPitchRate = m_climbSolver.Compute(m_climbError, timestep);
                    }

                    // ------------------------------ Inner Loop Rate Control
                    m_pitchRateSolver.m_multiplier = _flcs.m_gainSystem.m_pr;
                    _flcs.m_commands.m_commandPitchRate = MathBase.Clamp(_flcs.m_commands.m_commandPitchRate, -_flcs.maximumPitchRate, _flcs.maximumPitchRate);
                    double m_pitchRateError = (_flcs.m_commands.m_commandPitchRate) - m_pitchRate;
                    _flcs.m_pitch = -m_pitchRateSolver.Compute(m_pitchRateError, m_pitchAcceleration, timestep);
                }
                /// <summary>
                /// 
                /// </summary>
                if (Math.Abs(_flcs.m_commandAugmentation.m_rudderCommand) < 1)
                {
                    if (_flcs.controller.m_grounded)
                    {
                        double m_yawAngleError = _flcs.m_commands.m_commandYawAngle - m_yawAngle;
                        double m_yawDelta = _flcs.controller.m_core.δψ;
                        m_groundAngleSolver.m_maximum = 3;
                        m_groundAngleSolver.m_minimum = -3;
                        double m_commandYawRate = m_groundAngleSolver.Compute(m_yawAngleError, m_yawDelta, timestep);

                        double m_yawRateError = m_commandYawRate - m_yawRate;
                        _flcs.m_yaw = m_yawRateSolver.Compute(m_yawRateError, m_yawAcceleration, timestep);
                    }
                    else
                    {
                        double m_yawRateError = _flcs.controller.m_core.β - m_yawRate;
                        _flcs.m_yaw = m_yawRateSolver.Compute(m_yawRateError, m_yawAcceleration, timestep);
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                if (_flcs.m_autoThrottle == ControlState.Active)
                {
                    _flcs.m_mixtureInput = _flcs.m_throttleAugmentation.m_mixture;
                    _flcs.m_propPitchInput = _flcs.m_throttleAugmentation.m_propellerPitch;
                    _flcs.m_throttleInput = _flcs.m_throttleAugmentation.m_throttle;
                }
                else
                {
                    _flcs.m_mixtureInput = _flcs.b_mixtureInput;
                    _flcs.m_propPitchInput = _flcs.b_propPitchInput;
                    _flcs.m_throttleInput = _flcs.b_throttleInput;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class AutoThrottle
        {
            public SilantroPiston m_piston;
            public FixedComputer m_controller;
            public FPID m_throttleSolver;
            public FPID m_pitchSolver;
            public FPID m_mixtureSolver;

            public double m_throttle;
            public double m_mixture;
            public double m_propellerPitch;
            private double speedError;

            public double m_currentSpeed;
            public double m_ratedRPM = 2500;
            public double m_ratedAF = 12.05;
            public double m_coreRPM = 0;
            public double m_AF;

            /// <summary>
            /// 
            /// </summary>
            public void Compute(double timestep)
            {
                if (m_controller.controller.m_engineType == Controller.EngineType.Piston
                    && m_controller.controller.m_pistons != null && m_controller.controller.m_pistons.Length > 0)
                {
                    // ------------------------------------------ Throttle Control
                    m_currentSpeed = m_controller.controller.m_core.Vkts;
                    speedError = m_controller.m_commands.m_commandSpeed - m_currentSpeed;
                    m_throttleSolver.m_maximum = 1.02;
                    m_throttleSolver.m_minimum = 0.05;
                    m_throttle = m_throttleSolver.Compute(speedError, timestep);

                    // ------------------------------------------ Mixture Control
                    //m_AF = m_piston.AF;
                    //double ratioError = m_ratedAF - m_AF;
                    //m_mixtureSolver.m_maximum = 1;
                    //m_mixtureSolver.m_minimum = 0.05;
                    //m_mixture = 1 - m_mixtureSolver.Compute(ratioError, timestep);
                    m_mixture = 1;

                    // ------------------------------------------ Propeller Pitch Control
                    m_coreRPM = m_piston.core.coreRPM;
                    double m_baseRPM = m_ratedRPM * 0.5;
                    m_pitchSolver.m_maximum = 1;
                    m_pitchSolver.m_minimum = 0.05;
                    if (m_coreRPM > m_baseRPM)
                    {
                        double rpmError = m_ratedRPM - m_coreRPM;
                        m_propellerPitch = 1 - m_pitchSolver.Compute(rpmError, timestep);
                    }
                    else { m_propellerPitch = 0; }
                }
                else
                {
                    // ------------------------------------------ Throttle Control
                    m_currentSpeed = m_controller.controller.m_core.Vkts;
                    speedError = m_controller.m_commands.m_commandSpeed - m_currentSpeed;
                    m_throttleSolver.m_maximum = 1.02;
                    m_throttleSolver.m_minimum = 0.05;
                    m_throttle = m_throttleSolver.Compute(speedError, timestep);
                }


                // ----------------- Boost e.g Piston Turbo or Turbine Reheat
                if (m_throttle > 1f && speedError > 5f && m_controller.controller.m_boostState == Controller.BoostState.Off) { m_controller.controller.EngageBoost(); }
                if (m_throttle < 1f && speedError < 3f && m_controller.controller.m_boostState == Controller.BoostState.Active) { m_controller.controller.DisEngageBoost(); }
            }
        }
    }
    #endregion
}



