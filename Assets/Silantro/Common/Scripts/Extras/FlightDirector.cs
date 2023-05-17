#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
#if SILANTRO_FIXED
using Oyedoyin.FixedWing;
#endif
#if SILANTRO_ROTARY
using Oyedoyin.RotaryWing;
#endif
using Oyedoyin.Mathematics;

namespace Oyedoyin.Common
{
    public class FlightDirector : MonoBehaviour
    {
        public enum FlightState { Grounded, Taxi, Takeoff, Cruise, Loiter, Decent, Landing }
        public enum TaxiState { Stationary, Moving, Holding }
        public enum LandingState { BaseA, BaseB, BaseC, BaseD }
        public enum LoiterState { Hold, Cruise, Final }
        public enum FinalLoiterState { ApproachBreak, LevelBreak, Downwind, BaseLeg }

        [Header("Connections")]
        public Controller controller;
        public GameObject takeoffButton;

        [Header("State Variables")]
        public FlightState flightState = FlightState.Grounded;
        public TaxiState taxiState = TaxiState.Stationary;
        //public LandingState landingState = LandingState.BaseA;
        //public LoiterState loiterState = LoiterState.Cruise;
        //public FinalLoiterState finalLoiter = FinalLoiterState.ApproachBreak;

#if SILANTRO_FIXED
        // -------------------------------- Taxi
        [Header("Taxi Variables (You can implement this with a simple car steering logic)")]
        public AnimationCurve steerCurve;
        public float maximumTaxiSpeed = 10f;
        public float recommendedTaxiSpeed = 8f;
        public float maximumTurnSpeed = 5f;
        public float maximumSteeringAngle = 30f;
        public float minimumSteeringAngle = 15f;
        public float steeringAngle;
        public float targetTaxiSpeed;

        [Range(0, 1)] public float steerSensitivity = 0.05f;
        [Range(0, 2)] public float brakeSensitivity = 0.85f;
        public float brakeInput;
#endif

        // ----------------------------------------- Variables
        [Header("Variables")]
        public float currentSpeed;
        public float checkListTime = 2f;
        public float transitionTime = 5f;
        private float evaluateTime = 12f;
        private float inputCheckFactor;
        private float currentTestTime;


        [Header("Control Markers")]
        public bool flightInitiated;
        public bool checkedSurfaces;
        public bool groundChecklistComplete, taxiStarted;
        public bool isTaxing;
        public bool checkingSurfaces;
        public bool clearedForTakeoff;
        bool flapSet;

        // -------------------------------- Cruise
        [Header("Commands")]
#if SILANTRO_FIXED
        public float takeoffSpeed = 80f;
#endif
        public float maximumGearSpeed = 100;
        public float cruiseAltitude = 500f;
        public float cruiseSpeed = 90f;
        public float cruiseHeading = 0f;
        public float cruiseClimbRate = 500f;

#if SILANTRO_FIXED
        FixedComputer f_computer => (FixedComputer)controller.m_flcs;


        /// <summary>
        /// 
        /// </summary>
        public void AircraftPlotSteerCurve()
        {
            // ------------------------- Plot Steer Curve
            steerCurve = new AnimationCurve();

            steerCurve.AddKey(new Keyframe(0.0f * maximumSteeringAngle, maximumTaxiSpeed));
            steerCurve.AddKey(new Keyframe(0.5f * maximumSteeringAngle, recommendedTaxiSpeed));
            steerCurve.AddKey(new Keyframe(0.8f * maximumSteeringAngle, maximumTurnSpeed));

#if UNITY_EDITOR
            for (int i = 0; i < steerCurve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(steerCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(steerCurve, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }
#endif

#if SILANTRO_ROTARY
        RotaryComputer r_computer => (RotaryComputer)controller.m_flcs;
#endif




        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
#if SILANTRO_FIXED
            // ------------------------ Plot
            AircraftPlotSteerCurve();
#endif

            if (takeoffButton != null && takeoffButton.activeSelf) { takeoffButton.SetActive(false); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            //---------------------------------------------OPERATION MODE
            switch (flightState)
            {
                case FlightState.Grounded: GroundMode(); break;
                case FlightState.Taxi: TaxiMode(); break;
                case FlightState.Takeoff: TakeoffMode(); break;
                case FlightState.Cruise: CruiseMode(); break;
            }

            // ---------------------------------------------- COLLECT DATA
            currentSpeed = (float)controller.m_core.Vkts;

            // ---------------------------------------------- SURFACE CHECK
            if (checkingSurfaces) { CheckControlSurfaces(inputCheckFactor); }
        }



        #region Ground Functionality

        /// <summary>
        /// 
        /// </summary>
        public void InitializeFlight()
        {
            if (!flightInitiated) { flightInitiated = true; }
        }
        /// <summary>
        /// 
        /// </summary>
        void GroundMode()
        {
            //------------------------- Start Flight Process
            if (flightInitiated && !controller.m_powerState) { controller.TurnOnEngines(); flightInitiated = false; }
            if (flightInitiated && controller.m_powerState) { flightInitiated = false; }


            if (!controller.m_powerState)
            {
                // Check States
                if (controller.m_lightState == Controller.LightState.On) { controller.m_input.TurnOffLights(); }
                if (controller.m_wheels && controller.m_wheels.brakeState == SilantroWheels.BrakeState.Disengaged) { controller.m_wheels.EngageBrakes(); }
            }
            else
            {
                // -------------------------------------------------------------------------------------- Check List
                if (!groundChecklistComplete)
                {
                    //Debug.Log("Engine Start Complete, commencing ground checklist");
                    StartCoroutine(GroundCheckList());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GroundCheckList()
        {
            // --------------------------- Lights
            controller.m_input.TurnOnLights();


            // --------------------------- Actuators
            if (controller.canopyActuator && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator(); }
            if (controller.speedBrakeActuator && controller.speedBrakeActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.speedBrakeActuator.DisengageActuator(); }
            if (controller.wingActuator && controller.wingActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.wingActuator.DisengageActuator(); }

            if (controller.m_type == Controller.VehicleType.Aircraft)
            {
#if SILANTRO_FIXED
                // --------------------------- Flaps
                FixedController aircraft = (FixedController)controller;
                yield return new WaitForSeconds(checkListTime);
                foreach (SilantroAerofoil foil in aircraft.m_wings)
                {
                    if (foil.m_currentFlapStep != 1 && !flapSet) { foil.LowerFlap(); }
                }
                flapSet = true;


                // --------------------------- Slats
                yield return new WaitForSeconds(checkListTime);
                // Put slat code here
#endif
            }
            // --------------------------- Control Surfaces
            yield return new WaitForSeconds(checkListTime);
            if (!checkingSurfaces && currentTestTime < 1f) { currentTestTime = evaluateTime; checkingSurfaces = true; }
            if (!checkedSurfaces)
            {
                float startRange = -1.0f; float endRange = 1.0f; float cycleRange = (endRange - startRange) / 2f;
                float offset = cycleRange + startRange;
                inputCheckFactor = offset + Mathf.Sin(Time.time * 5f) * cycleRange;
            }

            yield return new WaitForSeconds(evaluateTime);
            checkedSurfaces = true; checkingSurfaces = false;
            if (!clearedForTakeoff) { controller.SendCustomAircraftInputs(0, 0, 0, 0.02f, 0, 0); }
            groundChecklistComplete = true;


            // ---------------------------- Transition
            yield return new WaitForSeconds(transitionTime);
            if (!taxiStarted) { flightState = FlightState.Taxi; Debug.Log("Taxi Started"); }
            if (controller.m_wheels != null) { controller.m_wheels.ReleaseBrakes(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlInput"></param>
        void CheckControlSurfaces(float controlInput)
        {
            if (checkingSurfaces)
            {
                currentTestTime -= Time.deltaTime;
                if (currentTestTime < 0) { currentTestTime = 0f; }

                //--------------------- Pitch
                if (currentTestTime < evaluateTime && currentTestTime > (0.75f * evaluateTime))
                {
                    controller.SendCustomAircraftInputs(controlInput, 0, 0, 0.03f, 0, 0);
                }
                //--------------------- Roll
                if (currentTestTime < (0.75f * evaluateTime) && currentTestTime > (0.50f * evaluateTime))
                {
                    controller.SendCustomAircraftInputs(0, controlInput, 0, 0.04f, 0, 0);
                }
                //--------------------- Yaw
                if (currentTestTime < (0.50f * evaluateTime) && currentTestTime > (0.25f * evaluateTime))
                {
                    controller.SendCustomAircraftInputs(0, 0, controlInput, 0.05f, 0, 0);
                }
                //--------------------- Trim
                if (currentTestTime < (0.25f * evaluateTime) && currentTestTime > (0.00f * evaluateTime))
                {

                }
            }
        }

        #endregion

        #region Taxi Functionality
        /// <summary>
        /// 
        /// </summary>
        void TaxiMode()
        {
            // ------------------------------------- Clamp
            //float thresholdSpeed = maximumTaxiSpeed * 0.1f;

            // ------------------------------------- States
            if (taxiState == TaxiState.Stationary)
            {
                // -------------------------- Check the waypoint state
                taxiState = TaxiState.Moving;
            }
            if (taxiState == TaxiState.Moving)
            {
                taxiStarted = true;

                // -------------------------- Perform function while moving
                taxiState = TaxiState.Holding;
            }
            if (taxiState == TaxiState.Holding)
            {
                // -------------------------- Perform function while on hold

                if (takeoffButton != null && !takeoffButton.activeSelf) { takeoffButton.SetActive(true); }
                // -------------------------- Receive clearance
                if (clearedForTakeoff) { flightState = FlightState.Takeoff; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void TakeoffClearance()
        {
            if (flightState == FlightState.Taxi && taxiState == TaxiState.Holding)
            {
                if (!clearedForTakeoff) { clearedForTakeoff = true; }
                else { Debug.Log(controller.transform.name + " has been cleared for takeoff"); }
            }
            else { Debug.Log(controller.transform.name + " clearance invalid! Aircraft not in holding pattern"); }
        }

        #endregion

        #region Takeoff Functionality

        /// <summary>
        /// 
        /// </summary>
        void TakeoffMode()
        {
            if (controller.m_powerState)
            {

#if SILANTRO_FIXED
                if (controller.m_type == Controller.VehicleType.Aircraft)
                {
                    // ------------------------------------- Accelerate
                    if (controller.m_wheels) { controller.m_wheels.ReleaseBrakes(); }
                    controller.SendCustomAircraftInputs(0, 0, 0, 1, 1, 1);
                    if (controller.m_boostState == Controller.BoostState.Off) { controller.EngageBoost(); }

                    // ------------------------------------- Send
                    if (controller.m_wheels != null)
                    {
                        controller.m_wheels.brakeInput = brakeInput = 0f;
                        controller.m_wheels.currentSteerAngle = steeringAngle = 0f;
                    }

                    // ------------------------------------- Switch to Autonomous
                    if (controller.m_core.Vkts >= takeoffSpeed)
                    {
                        if (controller.m_flcs.m_mode != Computer.Mode.Autonomous && f_computer != null)
                        {
                            controller.m_flcs.m_mode = Computer.Mode.Autonomous;
                            f_computer.m_lateralAutopilot = Misc.ControlState.Active;
                            f_computer.m_longitudinalAutopilot = Misc.ControlState.Active;
                            f_computer.m_autopilot.m_longitudinalMode = FixedComputer.Autopilot.LongitudinalMode.AltitudeHold;
                            f_computer.m_autopilot.m_lateralMode = FixedComputer.Autopilot.LateralMode.HeadingHold;
                            f_computer.m_autoThrottle = Misc.ControlState.Active;

                            // Set Takeoff Commands
                            f_computer.m_commands.m_commandAltitude = cruiseAltitude / Constants.m2ft;
                            f_computer.m_commands.m_commandSpeed = cruiseSpeed + 10;
                            f_computer.m_commands.m_commandClimbRate = cruiseClimbRate;
                        }
                    }

                    // ------------------------------------- Checklist before cruise
                    if (controller.m_core.Vkts > maximumGearSpeed - 10f) { StartCoroutine(PostTakeoffCheckList()); }
                }
#endif

#if SILANTRO_ROTARY
                if (controller.m_type == Controller.VehicleType.Helicopter)
                {
                    if (controller.m_flcs.m_mode != Computer.Mode.Autonomous && r_computer != null) { controller.m_flcs.m_mode = Computer.Mode.Autonomous; }
                    double AGL = controller.m_core.m_height * Constants.m2ft;

                    // Accelerate to cruise
                    if (AGL > 98)
                    {
                        StartCoroutine(PostTakeoffCheckList());
                    }
                    // Takeoff
                    else
                    {
                        r_computer.m_autopilot.m_lateralState = Autopilot.LateralState.DriftControl;
                        r_computer.m_autopilot.m_longitudinalMode = Autopilot.LongitudinalState.SpeedHold;
                        r_computer.m_autopilot.m_powerState = Autopilot.PowerState.AltitudeHold;

                        // Set Takeoff Commands
                        r_computer.m_commands.m_commandAltitude = 100 / Constants.m2ft;
                        r_computer.m_commands.m_commandSpeed = 0;
                        r_computer.m_commands.m_commandClimbRate = cruiseClimbRate;

                    }
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator PostTakeoffCheckList()
        {
#if SILANTRO_FIXED
            // --------------------------- Flaps
            yield return new WaitForSeconds(checkListTime);
            FixedController aircraft = (FixedController)controller;
            foreach (SilantroAerofoil foil in aircraft.m_wings) { if (foil.m_currentFlapStep != 0) { foil.RaiseFlap(); } }
#endif

            // --------------------------- Gear
            if (controller.gearActuator)
            {
                yield return new WaitUntil(() => controller.m_core.Vkts > maximumGearSpeed && controller.m_core.m_height > 50f);
                controller.gearActuator.DisengageActuator();
            }

            // ---------------------------- Transition
            yield return new WaitForSeconds(transitionTime);
            // Put Slat stuff 
            flightState = FlightState.Cruise;
        }

        #endregion

        #region Cruise Functionality

        /// <summary>
        /// 
        /// </summary>
        void CruiseMode()
        {
#if SILANTRO_FIXED
            if (controller.m_type == Controller.VehicleType.Aircraft)
            {
                if (controller.m_flcs.m_mode == Computer.Mode.Autonomous && f_computer != null) { controller.m_flcs.m_mode = Computer.Mode.Autonomous; }
                f_computer.m_autopilot.m_longitudinalMode = FixedComputer.Autopilot.LongitudinalMode.AltitudeHold;
                f_computer.m_autopilot.m_lateralMode = FixedComputer.Autopilot.LateralMode.HeadingHold;
                f_computer.m_autoThrottle = Misc.ControlState.Active;

                // Set Cruise Commands
                f_computer.m_commands.m_commandAltitude = cruiseAltitude / Constants.m2ft;
                f_computer.m_commands.m_commandSpeed = cruiseSpeed;
                f_computer.m_commands.m_commandClimbRate = cruiseClimbRate;
                f_computer.m_commands.m_commandHeading = cruiseHeading;

            }
#endif

#if SILANTRO_ROTARY
            if (controller.m_type == Controller.VehicleType.Helicopter)
            {
                r_computer.m_autopilot.m_lateralState = Autopilot.LateralState.HeadingHold;
                r_computer.m_autopilot.m_longitudinalMode = Autopilot.LongitudinalState.SpeedHold;
                r_computer.m_autopilot.m_powerState = Autopilot.PowerState.AltitudeHold;

                // Set Takeoff Commands
                r_computer.m_commands.m_commandAltitude = cruiseAltitude / Constants.m2ft;
                r_computer.m_commands.m_commandSpeed = cruiseSpeed / Constants.ms2knots;
                r_computer.m_commands.m_commandClimbRate = cruiseClimbRate;
            }
#endif
        }

        #endregion
    }
}
