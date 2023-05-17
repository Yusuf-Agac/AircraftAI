using System;
using UnityEngine;
using Oyedoyin.Mathematics;
using System.Collections.Generic;

namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Computer : MonoBehaviour
    {
        #region Enums
        public enum Mode { Manual, Augmented, Autonomous }
        public enum GainState { TakeoffLanding, Cruise }
        public enum LateralState { AttitudeHold, HeadingHold, TurnHold, RadiusHold, Off }
        public enum LongitudinalState { AttitudeHold, AltitudeHold, AlphaHold, Off }
        public enum WaypointState { PathHold, TrackTurn }
        public enum RefuelDoor { Open, Closed }
        public enum Flaps { Down, Up }

        #endregion

        #region Enum Properties

        public Mode m_mode = Mode.Augmented;
        protected Mode m_modeStorage;
        public RefuelDoor m_refuel = RefuelDoor.Closed;
        public Flaps m_flapState = Flaps.Up;
        public GainState m_gainState = GainState.TakeoffLanding;

        #endregion

        #region Common

        public Controller controller;
        public GainSystem m_gainSystem;
        public Commands m_commands;

        [Header("Base Inputs")]
        public double b_pitchInput;
        public double b_rollInput;
        public double b_yawInput;
        public double b_pitchTrimInput;
        public double b_rollTrimInput;
        public double b_yawTrimInput;
        public double b_throttleInput;
        // Fixed
        public double b_mixtureInput;
        public double b_propPitchInput;
        public double b_carbHeatInput;
        // Rotary
        public double b_collectiveInput;


        [Header("Command Outputs")]
        public double m_roll;
        public double m_pitch;
        public double m_yaw;
        public double m_pitchTrim;
        public double m_rollTrim;
        public double m_yawTrim;
        public double m_throttleInput;
        // Fixed
        public double m_mixtureInput;
        public double m_propPitchInput;
        public double m_carbHeatInput;
        // Rotary
        public double m_collectiveInput;

        public double maximumRollRate = 15;
        public double maximumPitchRate = 20;
        public double maximumYawRate = 10;
        public double maximumPitchAngle = 30f; //Nose Up
        public double minimumPitchAngle = 15f; //Nose Down
        public double maximumClimbRate = 1500;
        public double maximumDecentRate = 1000;
        public double maximumTurnRate = 4f;
        public double maximumTurnBank = 30f;
        public double maximumLoadFactor = 8;
        public double minimumLoadFactor = 3;

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Commands
        {
            public double m_commandRollRate;
            public double m_commandBankAngle;
            public double m_commandTurnRate;
            public double m_commandHeading;
            public double m_commandRadius;

            public double m_commandAltitude;
            public double m_commandClimbRate;

            public double m_commandPitchRate;
            public double m_commandPitchAngle;
            public double m_commandSpeed;

            public double m_commandYawRate;
            public double m_commandYawAngle;
        }

        [Header("Data")]
        public double m_Vm;
        public double m_VKTS, m_KIAS, m_Mach;
        public double m_dynamicPressue;
        public double m_pressureFactor;
        public double m_Qbar, m_Ps;

        [Header("Display")]
        public int toolbarTab;
        public string currentTab;



        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Compute(double _timestep)
        {
            // ------------------------------ Collect Base Data
            m_Vm = controller.m_core.V;
            m_VKTS = m_Vm * 1.94384;
            m_Mach = controller.m_core.m_atmosphere.M;
            double m_altitude = controller.m_core.z;
            double m_sx = Math.Pow(1.0 - (6.8755856E-6) * m_altitude * 3.28, 5.2558797);
            double m_s1 = 1.0 + m_Mach * m_Mach * 0.2;
            double m_s2 = Math.Pow(m_s1, 3.5) - 1.0;
            double m_s3 = 1.0 + m_sx * m_s2;
            double m_s4 = Math.Pow(m_s3, 0.28571428571) - 1.0;
            m_KIAS = 661.4786 * Math.Pow(5.0 * m_s4, 0.5);

            // ------------------------------- Gain Scheduling
            m_dynamicPressue = controller.m_core.m_atmosphere.qc * 0.02;
            m_Qbar = 0.5 * m_Vm * m_Vm * controller.m_core.m_atmosphere.ρ * 0.00194032033;
            m_Ps = controller.m_core.m_atmosphere.Ps * 0.020885434273;
            m_pressureFactor = m_Qbar / m_Ps;
            m_pressureFactor = MathBase.Clamp(m_pressureFactor, 0, 4);
            if (double.IsNaN(m_pressureFactor) || double.IsInfinity(m_pressureFactor)) { m_pressureFactor = 0f; }

            // ------------------------------- Gain Scheduling
            if ((m_dynamicPressue < 30930.6473 && m_refuel == RefuelDoor.Open) || controller.m_gearState == Controller.GearState.Down || m_flapState == Flaps.Down)
            { m_gainState = GainState.TakeoffLanding; }
            else { m_gainState = GainState.Cruise; }


            // ------------------------------ Collect Base Inputs
            b_pitchTrimInput = controller.m_input._pitchTrimInput;
            b_rollTrimInput = controller.m_input._rollTrimInput;
            b_yawTrimInput = controller.m_input._yawTrimInput;
            b_pitchInput = -controller.m_input.m_pitchInput;
            b_rollInput = -controller.m_input.m_rollInput;
            b_yawInput = controller.m_input.m_yawInput;

            b_throttleInput = controller.m_input._throttleInput;
            b_propPitchInput = controller.m_input._propPitchInput;
            b_carbHeatInput = controller.m_input._carbHeatInput;
            b_mixtureInput = controller.m_input._mixtureInput;
            b_collectiveInput = controller.m_input._collectiveInput;

            if (m_mode == Mode.Manual) { ComputeManual(); }

            // ------------------------------ Send Base Inputs
            controller._pitchInput = (float)m_pitch;
            controller._rollInput = (float)m_roll;
            controller._yawInput = (float)m_yaw;
            controller._pitchTrimInput = (float)m_pitchTrim;
            controller._rollTrimInput = (float)m_rollTrim;
            controller._yawTrimInput = (float)m_yawTrim;

            // ------------------------------ Send Power Inputs
            controller._throttleInput = (float)m_throttleInput;
            controller._propPitchInput = (float)m_propPitchInput;
            controller._carbHeatInput = (float)m_carbHeatInput;
            controller._mixtureInput = (float)m_mixtureInput;
            controller._collectiveInput = (float)m_collectiveInput;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void EnableSceneAutopilot() { controller.m_sceneAutopilotState = Controller.SceneAutopilotState.Active; }
        /// <summary>
        /// 
        /// </summary>
        public virtual void DisableSceneAutopilot() { controller.m_sceneAutopilotState = Controller.SceneAutopilotState.Off; }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class GainSystem
        {
            public Computer m_controller;

            public enum GainState { Static, Dynamic }
            public GainState m_state = GainState.Static;

            public AnimationCurve m_pitchRate;
            public AnimationCurve m_rollRate;

            public List<Gain> m_pitchGains = new List<Gain>() { new Gain(0, 1), new Gain(500, .8f) };
            public List<Gain> m_rollGains = new List<Gain>() { new Gain(0, 1), new Gain(500, .15f) };

            [Header("Output")]
            public float m_pr;
            public float m_rr;

            /// <summary>
            /// 
            /// </summary>
            public void Initialize()
            {
                m_pitchRate = new AnimationCurve();

                if (m_state == GainState.Static)
                {
                    m_pitchRate.AddKey(new Keyframe(0, 1)); m_pitchRate.AddKey(new Keyframe(1000, 1));
                    m_pitchRate.AddKey(new Keyframe(0, 1)); m_pitchRate.AddKey(new Keyframe(1000, 1));

                    m_rollRate.AddKey(new Keyframe(0, 1)); m_rollRate.AddKey(new Keyframe(1000, 1));
                    m_rollRate.AddKey(new Keyframe(0, 1)); m_rollRate.AddKey(new Keyframe(1000, 1));
                }
                if (m_state == GainState.Dynamic)
                {
                    foreach (Gain _vector in m_pitchGains) { float m_s = _vector.speed; float m_p = _vector.factor; m_pitchRate.AddKey(new Keyframe(m_s, m_p)); }
                    foreach (Gain _vector in m_rollGains) { float m_s = _vector.speed; float m_p = _vector.factor; m_rollRate.AddKey(new Keyframe(m_s, m_p)); }
                }

                MathBase.LinearizeCurve(m_pitchRate);
                MathBase.LinearizeCurve(m_rollRate);
            }
            /// <summary>
            /// 
            /// </summary>
            public void Compute()
            {
                float _speed = (float)m_controller.controller.m_core.Vkts;

                // Rates
                m_pr = m_pitchRate.Evaluate(_speed);
                m_rr = m_rollRate.Evaluate(_speed);
            }
        }

        #endregion

        #region Manual Controls

        /// <summary>
        /// 
        /// </summary>
        private void ComputeManual()
        {
            m_pitch = b_pitchInput;
            m_roll = b_rollInput;
            m_yaw = b_yawInput;
            m_pitchTrim = b_pitchTrimInput;
            m_rollTrim = b_rollTrimInput;
            m_yawTrim = b_yawTrimInput;

            m_carbHeatInput = b_carbHeatInput;
            m_mixtureInput = b_mixtureInput;
            m_propPitchInput = b_propPitchInput;
            m_throttleInput = b_throttleInput;
            m_collectiveInput = b_collectiveInput;
        }

        #endregion
    }
}