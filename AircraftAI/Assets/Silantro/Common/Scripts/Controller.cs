using System;
using UnityEngine;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;
using Oyedoyin.Common.Components;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if (ENABLE_INPUT_SYSTEM)
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class Controller : MonoBehaviour
    {
        #region Enums
        public enum VehicleType { Aircraft, Helicopter }
        public enum ControlType { External, Internal }
        public enum StartMode { Cold, Hot }
        public enum HotStartMode { AfterInitialization, CustomCall }
        public enum InputSystem { Legacy, New }
        public enum InputType { Default, VR, Mobile, Custom }
        public enum InputLogic { Legacy, InputSystem }
        public enum GearMode { Retractable, Static }
        public enum GearState { Up, Down }
        public enum LightState { On, Off }
        public enum BoostState { Active, Off }
        public enum EngineType { Unpowered, Electric, Jet, Piston }
        public enum JetFuelType { JetB, JetA1, JP6, JP8 }
        public enum GasFuelType { AVGas100, AVGas100LL, AVGas82UL }
        public enum StoreState { Connected, Disconnected }
        public enum Selection { Gun, Missile, Rockets, Bombs }
        public enum SceneAutopilotState { Active, Off }

        #endregion

        #region Connections and Properties

        public VehicleType m_type = VehicleType.Aircraft;
        /// <summary>
        /// Determines if the aircraft uses the Enter-Exit system or not.
        /// </summary>
        public ControlType m_controlType = ControlType.External;
        /// <summary>
        /// 
        /// </summary>
        public InputType m_inputType = InputType.Default;
        /// <summary>
        /// 
        /// </summary>
        public InputLogic m_inputLogic = InputLogic.Legacy;
        /// <summary>
        /// The aircraft start mode. Cold and dark or Hot and running
        /// </summary>
        public StartMode m_startMode = StartMode.Cold;
        /// <summary>
        /// The current camera state
        /// </summary>
        public SilantroCamera.CameraState m_cameraState = SilantroCamera.CameraState.Exterior;
        /// <summary>
        /// 
        /// </summary>
        public EngineType m_engineType = EngineType.Jet;
        /// <summary>
        /// 
        /// </summary>
        public LightState m_lightState = LightState.Off;
        /// <summary>
        /// 
        /// </summary>
        public GearMode m_gearMode = GearMode.Retractable;
        /// <summary>
        /// 
        /// </summary>
        public GearState m_gearState = GearState.Down;
        /// <summary>
        /// 
        /// </summary>
        public HotStartMode m_hotMode = HotStartMode.CustomCall;
        /// <summary>
        /// 
        /// </summary>
        public BoostState m_boostState = BoostState.Off;
        /// <summary>
        /// 
        /// </summary>
        public SceneAutopilotState m_sceneAutopilotState = SceneAutopilotState.Off;


        // -------------------------------- Components
        public Rigidbody m_rigidbody;
        public Controller m_controller;
        public ControllerFunctions m_helper;
        public Computer m_flcs;
        public SilantroInput m_input;
        public OriginShift m_originShift;
        public SilantroCore m_core;
        public SilantroCamera m_view;
        public SilantroWheels m_wheels;
        public SilantroRadar m_radar;
        public SceneAutopilot m_sceneAutopilot;


        public Collider[] m_colliders;
        public SilantroTank[] m_fuelTanks;
        public SilantroPiston[] m_pistons;
        public SilantroInstrument[] m_instruments;
        public SilantroBulb[] m_lights;
        public SilantroPayload[] m_payload;

        // -------------------------------- Actuators
        public SilantroActuator[] m_actuators;
        public SilantroActuator gearActuator;
        public SilantroActuator canopyActuator;
        public SilantroActuator speedBrakeActuator;
        public SilantroActuator wingActuator;
        public SilantroActuator liftSystem;
        public SilantroActuator gunActuator;

        // -------------------------------- Weapons
        public SilantroGun[] m_guns;
        public SilantroMunition[] m_munitions;
        public List<SilantroMunition> rockets;
        public List<SilantroMunition> missiles;
        public Launcher m_launcher;
        public SilantroPylon[] m_pylons;
        public List<string> inputList;

        // -------------------------------- Weight
        public float emptyWeight = 1000f;
        public float currentWeight;
        public float maximumWeight = 5000f;

        // -------------------------------- Fuel
        public GasFuelType gasFuelType = GasFuelType.AVGas100;
        public JetFuelType jetFuelType = JetFuelType.JetB;
        public SilantroFuelSystem m_fuelSystem;
        public float combustionEnergy;
        public float fuelLevel;
        public float fuelCapacity;
        public float fuelFlow;
        public bool fuelLow = false;
        public bool fuelExhausted = false;
        public bool boostRunning;

        // -------------------------------- Control
        public bool allOk = false;
        public bool isControllable = true;
        public bool pilotOnBoard;
        public bool touchPressed;
        public bool m_grounded;
        public bool isInitialized;
        public Vector3 basePosition;
        public Quaternion baseRotation;
        public bool brakeLeverHeld;

        // -------------------------------- Data
        public string m_name;
        public int m_engineCount = 1;
        public float m_powerLevel;
        public bool m_powerState;
        public float _timestep, _fixedTimestep;
        public float m_wowForce;

        // -------------------------------- Hot Start
        public float m_startSpeed = 100;
        public float m_startAltitude = 500;

        // -------------------------------- Weapons
        public StoreState m_hardpoints = StoreState.Disconnected;
        public Selection m_hardpointSelection = Selection.Gun;
        public bool m_triggerHeld;
        public int m_weaponState;
        public ControlState m_gunState;
        public ControlState m_rocketState;
        public ControlState m_bombState;
        public ControlState m_missileState;

        public GameObject m_interiorPilot;
        public GameObject m_canvas;
        public GameObject m_player;
        public Transform getOutPosition;
        public float m_entryTimer = 3;
        public float m_exitTimer = 4;

        [Header("Control Inputs")]
        public float _pitchInput;
        public float _rollInput;
        public float _yawInput;
        public float _pitchTrimInput;
        public float _rollTrimInput;
        public float _yawTrimInput;
        public float _tillerInput;

        [Header("Power Inputs")]
        public float _throttleInput;
        public float _collectiveInput;
        public float _propPitchInput;
        public float _mixtureInput;
        public float _carbHeatInput;

        [Header("Output")]
        public Vector3 force;
        public Vector3 moment;

        #endregion

        #region Base Functions

        /// <summary>
        /// 
        /// </summary>
        protected virtual void CheckPrerequisites() { }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void Initialize()
        {
            // Collect base components
            m_rigidbody = GetComponent<Rigidbody>();
            m_controller = GetComponent<Controller>();
            m_flcs = GetComponentInChildren<Computer>();
            m_core = GetComponentInChildren<SilantroCore>();
            m_wheels = GetComponentInChildren<SilantroWheels>();
            m_view = GetComponentInChildren<SilantroCamera>();
            m_colliders = GetComponentsInChildren<Collider>();
            m_pistons = GetComponentsInChildren<SilantroPiston>();
            m_fuelTanks = GetComponentsInChildren<SilantroTank>();
            m_instruments = GetComponentsInChildren<SilantroInstrument>();
            m_lights = GetComponentsInChildren<SilantroBulb>();
            m_actuators = GetComponentsInChildren<SilantroActuator>();
            m_payload = GetComponentsInChildren<SilantroPayload>();
            m_guns = GetComponentsInChildren<SilantroGun>();
            m_munitions = GetComponentsInChildren<SilantroMunition>();
            m_radar = GetComponentInChildren<SilantroRadar>();
            m_pylons = GetComponentsInChildren<SilantroPylon>();
            basePosition = transform.position;
            baseRotation = transform.rotation;


            // Confirm needed components
            if (m_core != null && m_rigidbody) { allOk = true; }
            if (m_rigidbody == null)
            {
                Debug.LogError("Prerequisites not met on " + transform.name + ".... rigidbody not assigned");
                allOk = false; return;
            }
            else if (m_core == null)
            {
                Debug.LogError("Prerequisites not met on " + transform.name + ".... control module not assigned");
                allOk = false; return;
            }

            // Initialize base components
            if (allOk)
            {
                // ------------------------- Setup Camera
                if (m_view != null)
                {
                    m_view.aircraft = m_rigidbody;
                    m_view.controller = m_controller;
                    m_view.Initialize();
                }

                // ------------------------- Setup Input
                float hx = transform.eulerAngles.y;
                if (hx > 180) { hx -= 360; }
                if (hx < -180) { hx += 360; }
                m_flcs.m_commands.m_commandYawAngle = hx;

                m_input._controller = m_controller;
                m_input.Initialize();
#if (ENABLE_INPUT_SYSTEM)
                if (m_inputLogic == InputLogic.InputSystem) { InitializeNewInputs(); }
#endif
                m_sceneAutopilot.m_vehicle = m_controller;
                m_sceneAutopilot.Initialize();

                // ------------------------- Setup Core
                if (m_core != null)
                {
                    m_core.controller = m_controller;
                    m_core.Initialize();
                    m_originShift.m_vehicle = m_controller;
                    m_originShift.Initialize();
                }

                // ------------------------- Setup Flight Computer
                if (m_flcs != null)
                {
                    m_flcs.controller = m_controller;
                    m_flcs.Initialize();
                }

                // ------------------------- Setup Gear
                if (m_wheels != null)
                {
                    m_wheels.aircraft = m_rigidbody;
                    m_wheels.controller = m_controller;
                    m_wheels.Initialize();
                }

                // ------------------------- Setup Fuel System
                if (m_engineType != EngineType.Electric && m_engineType != EngineType.Unpowered)
                {
                    if (m_fuelTanks.Length > 0)
                    {
                        string tankFuelType = m_fuelTanks[0].fuelType.ToString();
                        if (m_engineType == EngineType.Piston)
                        {
                            if (tankFuelType != gasFuelType.ToString())
                            {
                                Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... fuel selected on controller (" + gasFuelType.ToString() + ") not a match with tank fuel (" + tankFuelType + ")");
                                allOk = false; return;
                            }
                        }
                        if (m_engineType == EngineType.Jet)
                        {
                            if (tankFuelType != jetFuelType.ToString())
                            {
                                Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... fuel selected on controller (" + jetFuelType.ToString() + ") not a match with loaded tank fuel (" + tankFuelType + ")");
                                allOk = false; return;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... no fuel tank attached to the aircraft");
                        allOk = false; return;
                    }

                    if (m_engineType == EngineType.Piston)
                    {
                        if (gasFuelType == GasFuelType.AVGas100) { combustionEnergy = 42.8f; }
                        if (gasFuelType == GasFuelType.AVGas100LL) { combustionEnergy = 43.5f; }
                        if (gasFuelType == GasFuelType.AVGas82UL) { combustionEnergy = 43.6f; }
                    }
                    else
                    {
                        if (jetFuelType == JetFuelType.JetB) { combustionEnergy = 42.8f; }
                        if (jetFuelType == JetFuelType.JetA1) { combustionEnergy = 43.5f; }
                        if (jetFuelType == JetFuelType.JP6) { combustionEnergy = 43.02f; }
                        if (jetFuelType == JetFuelType.JP8) { combustionEnergy = 43.28f; }
                    }
                    combustionEnergy *= 1000f;

                    foreach (SilantroTank tank in m_fuelTanks) { tank.Initialize(); }
                    m_fuelSystem.fuelTanks = m_fuelTanks;
                    m_fuelSystem.controller = m_controller;
                    m_fuelSystem.Initialize();
                }

                // ------------------------- Setup Instruments
                if (m_controller != null)
                {
                    foreach (SilantroInstrument instrument in m_instruments)
                    {
                        instrument.m_controller = m_controller;
                        instrument.Initialize();
                    }
                }

                // ------------------------- Setup Actuators
                if (m_actuators != null)
                {
                    foreach (SilantroActuator actuator in m_actuators)
                    {
                        if (actuator.initialized) { Debug.LogWarning("Actuator for " + actuator.transform.name + " is still in evaluation mode."); }
                        else { actuator.Initialize(); }

                        // ------------- Filter
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.Canopy) { canopyActuator = actuator; }
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.LandingGear) { gearActuator = actuator; }
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.SpeedBrake) { speedBrakeActuator = actuator; }
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.SwingWings) { wingActuator = actuator; }
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.LiftSystem) { liftSystem = actuator; }
                        if (actuator.actuatorType == SilantroActuator.ActuatorType.GunCover) { gunActuator = actuator; }
                    }
                }

                // ------------------------- Setup Bulbs
                if (m_lights != null && m_lights.Length > 0)
                {
                    foreach (SilantroBulb bulb in m_lights)
                    {
                        bulb.Initialize();
                        if (bulb.lightType == SilantroBulb.LightType.Landing && gearActuator != null)
                        {
                            gearActuator.landingBulbs.Add(bulb);
                        }
                    }
                }

                // ------------------------- Setup Pistons
                if (m_engineType == EngineType.Piston && m_pistons != null && m_pistons.Length > 0)
                {
                    foreach (SilantroPiston engine in m_pistons)
                    {
                        engine.controller = m_controller;
                        engine.computer = m_core;
                        engine.Initialize();
                    }
                }

                // ------------------------- Setup Radar
                if (m_radar != null)
                {
                    m_radar.m_controller = m_controller;
                    m_radar.Initialize();
                }

                #region Armament

                // Reset
                m_gunState = ControlState.Off;
                m_rocketState = ControlState.Off;
                m_bombState = ControlState.Off;
                m_missileState = ControlState.Off;
                m_weaponState = 1;

                // ------------------------- Setup Guns
                if (m_guns != null && m_guns.Length > 0)
                {
                    foreach (SilantroGun gun in m_guns)
                    {
                        gun.m_rigidbody = m_rigidbody;
                        gun.Initialize();
                    }
                    // State
                    m_gunState = ControlState.Active;
                }

                // ------------------------- Setup Munitions
                if (m_munitions != null && m_munitions.Length > 0)
                {
                    // Base
                    foreach (SilantroMunition munition in m_munitions) { munition.m_parent = transform; munition.Initialize(); }
                    // Launcher
                    m_launcher.m_controller = m_controller;
                    m_launcher.Initialize(m_core.transform);
                }

                // ------------------------- Setup Pylons
                foreach (SilantroPylon pylon in m_pylons)
                {
                    pylon.m_controller = m_controller;
                    pylon.Initialize();
                }

                SwitchWeapon();

                #endregion

                #region VR Levers

                if (m_input != null)
                {
                    if (m_input.m_collectiveLever != null) { m_input.m_collectiveLever.Initialize(); }
                    if (m_input.m_throttleLever != null) { m_input.m_throttleLever.Initialize(); }
                    if (m_input.m_propPitchLever != null) { m_input.m_propPitchLever.Initialize(); }
                    if (m_input.m_mixtureLever != null) { m_input.m_mixtureLever.Initialize(); }
                    if (m_input.m_joystickLever != null) { m_input.m_joystickLever.Initialize(); }
                }

                #endregion

#if UNITY_EDITOR
                if (m_input != null) { CheckInputCofig(false); }
#endif
                if (!Application.isEditor && m_inputLogic == InputLogic.Legacy) { m_input.inputConfigured = true; }
            }
        }
        /// <summary>
        /// Run fixed update functions for the aircraft and its components
        /// </summary>
        protected virtual void ComputeFixedUpdate()
        {
            if (allOk && isInitialized)
            {
                // Accelerations 
                double W = m_rigidbody.mass * m_core.m_atmosphere.g;
                Vector m_A = (Vector)force / W;
                m_core.m_Ax = m_A.x;
                m_core.m_Ay = m_A.y;
                m_core.m_Az = m_A.z;

                // Reset
                force = Vector3.zero;
                moment = Vector3.zero;
                // Keep timestep constant across components
                _fixedTimestep = Time.fixedDeltaTime;

                // Wheels
                if (m_wheels) { m_wheels.ComputeFixed(); }
                // Fuel System
                if (m_engineType != EngineType.Electric && m_engineType != EngineType.Unpowered) { m_fuelSystem.Compute(); }
                // Piston Engines
                if (m_pistons != null) { foreach (SilantroPiston piston in m_pistons) { piston.Compute(_fixedTimestep); } }

                if (isControllable)
                {
                    // Core
                    m_core.Compute(_fixedTimestep);
                    // Radar
                    if (m_radar != null) { m_radar.Compute(); }
                    // Flight Computer
                    m_flcs.Compute(_fixedTimestep);
                    // Scene Autopilot
                    if (m_flcs.m_mode == Computer.Mode.Autonomous)
                    {
                        if (m_sceneAutopilotState == SceneAutopilotState.Active)
                        {
                            if (m_type == VehicleType.Helicopter)
                            {
                                m_flcs.m_commands.m_commandAltitude = m_sceneAutopilot.m_presetAltitude / Constants.m2ft;
                                m_flcs.m_commands.m_commandSpeed = m_sceneAutopilot.m_presetSpeed / Constants.ms2knots;
                                m_flcs.m_commands.m_commandHeading = m_sceneAutopilot.m_presetHeading;
                                m_flcs.maximumClimbRate = m_sceneAutopilot.m_presetClimb;
                            }
                            else
                            {
                                m_flcs.m_commands.m_commandAltitude = m_sceneAutopilot.m_presetAltitude / Constants.m2ft;
                                m_flcs.m_commands.m_commandSpeed = m_sceneAutopilot.m_presetSpeed;
                                m_flcs.m_commands.m_commandHeading = m_sceneAutopilot.m_presetHeading;
                                m_flcs.maximumClimbRate = m_sceneAutopilot.m_presetClimb;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void ComputeUpdate()
        {
            if (allOk && isInitialized)
            {
                // Keep timestep constant across components
                _timestep = Time.deltaTime;

                // Wheels
                if (m_wheels) { m_wheels.ComputeUpdate(); }
                // Lights
                foreach (SilantroBulb bulb in m_lights) { bulb.Compute(_timestep); }
                // Actuators
                foreach (SilantroActuator actuator in m_actuators) { actuator.Compute(_timestep); }

                if (isControllable)
                {
                    // Inputs
                    m_input.Compute();
                    // Instruments
                    foreach (SilantroInstrument instrument in m_instruments) { instrument.Compute(); }

                    #region VR Levers

                    if (m_input != null)
                    {
                        if (m_input.m_collectiveLever != null) { m_input.m_collectiveLever.Compute(); }
                        if (m_input.m_throttleLever != null) { m_input.m_throttleLever.Compute(); }
                        if (m_input.m_propPitchLever != null) { m_input.m_propPitchLever.Compute(); }
                        if (m_input.m_mixtureLever != null) { m_input.m_mixtureLever.Compute(); }
                        if (m_input.m_joystickLever != null) { m_input.m_joystickLever.Compute(); }
                    }

                    #endregion

                    // Guns
                    if (m_guns != null && m_guns.Length > 0)
                    {
                        if (m_hardpoints == StoreState.Connected && m_hardpointSelection == Selection.Gun)
                        {
                            foreach (SilantroGun gun in m_guns) { gun.Compute(); }
                            if (m_triggerHeld)
                            {
                                m_input.FireGuns();
                                foreach (SilantroGun gun in m_guns) { if (gun.currentAmmo > 0 && gun.canFire) { gun.running = true; } }
                            }
                            else { foreach (SilantroGun gun in m_guns) { if (gun.running) { gun.running = false; } } }
                        }
                    }
                    // Launcher
                    if (m_launcher != null) { m_launcher.Compute(_timestep); }
                }

                // Check Gear Actuator
                if (gearActuator != null)
                {
                    if (gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { m_gearState = GearState.Down; }
                    if (gearActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { m_gearState = GearState.Up; }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void ComputeLateUpdate()
        {
            if (allOk && isInitialized && isControllable)
            {
                // Guns
                foreach (SilantroGun gun in m_guns) { gun.ComputeLate(_timestep); }
                // Origin Shift
                m_originShift.Compute();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void ComputeData()
        {
            // Reset
            m_wowForce = 0; fuelFlow = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void ComputeForces()
        {

        }
        /// <summary>
        /// Send the processed inputs to the components
        /// </summary>
        protected virtual void UpdateComponentInputs()
        {
            if (_mixtureInput < 0.01f) { _mixtureInput = 0.01f; }
            if (_throttleInput < 0.01f) { _throttleInput = 0.01f; }
            if (_propPitchInput < 0.01f) { _propPitchInput = 0.01f; }
            // Cockpit Instruments
            foreach (SilantroInstrument instrument in m_instruments)
            {
                instrument.m_pitch = _pitchInput;
                instrument.m_roll = _rollInput;
                instrument.m_yaw = _yawInput;

                instrument.m_throttle = _throttleInput;
                instrument.m_prop = _propPitchInput;
                instrument.m_mixture = _mixtureInput;
            }
            // Piston Engines
            foreach (SilantroPiston piston in m_pistons)
            {
                piston.core.controlInput = _throttleInput;
                piston.m_throttle = _throttleInput;
                piston.m_mixture = _mixtureInput;
            }
            // Camera
            if (m_view != null)
            {
                m_view.xHatInput = m_input._hatViewInput.x;
                m_view.yHatInput = m_input._hatViewInput.y;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truncate"></param>
#if UNITY_EDITOR
        private void CheckInputCofig(bool truncate)
        {
            if (Application.isEditor && m_inputLogic == InputLogic.Legacy)
            {
                inputList = new List<string>();

                // ------------------------------------- Check Input Config
                var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
                SerializedObject obj = new SerializedObject(inputManager);
                SerializedProperty axisArray = obj.FindProperty("m_Axes");
                if (axisArray.arraySize == 0) Debug.Log("No Axes");

                for (int i = 0; i < axisArray.arraySize; ++i) { inputList.Add(axisArray.GetArrayElementAtIndex(i).displayName); }

                if (inputList.Contains("Start Engine") && inputList.Contains("Stop Engine") && inputList.Contains("Pitch") && inputList.Contains("Roll"))
                {
                    if (inputList.Contains("Start Engine BandL") && inputList.Contains("Stop Engine BandL"))
                    {
                        m_input.inputConfigured = true;
                    }
                    else
                    {
                        m_input.inputConfigured = false;
                        Debug.LogError("Input needs to be reconfigured. Go to Oyedoyin/Common/Setup Input");
                        allOk = false;
                        if (truncate) { return; }
                    }
                }
                else
                {
                    m_input.inputConfigured = false;
                    Debug.LogError("Input needs to be configured. Go to Oyedoyin/Common/Setup Input");
                    allOk = false;
                    if (truncate) { return; }
                }
            }
        }
#endif

        #endregion

        #region Call Functions
        /// <summary>
        /// Sets the state of the aircraft control.
        /// </summary>
        /// <param name="state">If set to <c>true</c> aircraft is controllable.</param>
        public void SetControlState(bool state) { isControllable = state; }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleCameraState() { m_input.ToggleCameraState(); }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleGearState() { m_input.ToggleGearState(); }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleBrakeState() { m_input.ToggleBrakeState(); }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleLightState() { m_input.ToggleLightState(); }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleSpeedBrake() { m_input.ToggleSpeedBrakeState(); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trash"></param>
        public void CleanupGameobject(GameObject trash) { Destroy(trash); }
        /// <summary>
        /// 
        /// </summary>
        public void RestoreAircraft() { m_helper.RestoreFunction(m_rigidbody, m_controller); }
        /// <summary>
        /// 
        /// </summary>
        public void ResetScene() { UnityEngine.SceneManagement.SceneManager.LoadScene(this.gameObject.scene.name); }
        /// <summary>
        /// 
        /// </summary>
        public void HotResetAircraft() { m_helper.HotResetAircraftFunction(m_rigidbody, m_controller); }
        /// <summary>
        /// 
        /// </summary>
        public void StartHotAircraft() { m_helper.StartAircraftFunction(m_rigidbody, m_controller); }
        /// <summary>
        /// 
        /// </summary>
        public void EnterAircraft() { m_helper.EnterAircraftFunction(m_controller); }
        /// <summary>
        /// 
        /// </summary>
        public void ExitAircraft() { m_helper.ExitAircraftFunction(m_controller); }
        /// <summary>
        /// 
        /// </summary>
        public virtual void TurnOnEngines() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void TurnOffEngines() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void EngageBoost() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void DisEngageBoost() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void RaiseFlaps() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void LowerFlaps() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void ToggleSlatState() { }
        /// <summary>
        /// 
        /// </summary>
        public virtual void ToggleSpoilerState() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_pitch"></param>
        /// <param name="input_roll"></param>
        /// <param name="input_yaw"></param>
        /// <param name="input_throttle"></param>
        /// <param name="input_propellerPitch"></param>
        /// <param name="input_mixture"></param>
        public void SendCustomAircraftInputs(float input_pitch, float input_roll, float input_yaw, float input_throttle, float input_propellerPitch, float input_mixture)
        {
            if (m_inputType != InputType.Custom) { m_inputType = InputType.Custom; }

            m_input._propPitchInput = input_propellerPitch;
            m_input._mixtureInput = input_mixture;
            m_input._throttleInput = input_throttle;
            m_input._rawPitchInput = input_pitch;
            m_input._rawRollInput = input_roll;
            m_input._rawYawInput = input_yaw;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_pitch"></param>
        /// <param name="input_roll"></param>
        /// <param name="input_yaw"></param>
        /// <param name="input_throttle"></param>
        /// <param name="input_collective"></param>
        public void SendCustomHelicopterInputs(float input_pitch, float input_roll, float input_yaw, float input_throttle, float input_collective)
        {
            if (m_inputType != InputType.Custom) { m_inputType = InputType.Custom; }

            m_input._collectiveInput = input_collective;
            m_input._throttleInput = input_throttle;
            m_input._rawPitchInput = input_pitch;
            m_input._rawRollInput = input_roll;
            m_input._rawYawInput = input_yaw;
        }
        /// <summary>
        /// 
        /// </summary>
        public void FinishInitialization()
        {
            // ------------------------- Control State
            if (m_controlType == ControlType.Internal)
            {
                SetControlState(false);
                m_helper.InternalControlSetup(m_controller);

                // ------------------------ Disable Canvas
                if (m_canvas != null) { m_canvas.gameObject.SetActive(false); }
            }
            else
            {
                SetControlState(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CountOrdnance()
        {
            m_munitions = m_controller.gameObject.GetComponentsInChildren<SilantroMunition>();
            rockets.Clear();
            missiles.Clear();

            //1. Rockets
            foreach (SilantroMunition munition in m_munitions) { if (munition.munitionType == SilantroMunition.MunitionType.Rocket) { rockets.Add(munition); } }
            //1. State
            if (rockets != null && rockets.Count > 0) { if (m_rocketState != ControlState.Active) { m_rocketState = ControlState.Active; } }

            //2. Missiles
            foreach (SilantroMunition munition in m_munitions) { if (munition.munitionType == SilantroMunition.MunitionType.Missile) { missiles.Add(munition); } }
            //2. State
            if (missiles != null && missiles.Count > 0) { if (m_missileState != ControlState.Active) { m_missileState = ControlState.Active; } }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SwitchWeapon()
        {
            m_weaponState += 1;

            if (m_hardpoints != StoreState.Disconnected)
            {
                if (m_weaponState == 1) { if (m_gunState == ControlState.Active) { m_hardpointSelection = Selection.Gun; } else { m_weaponState += 1; } }
                if (m_weaponState == 2) { if (m_rocketState == ControlState.Active) { m_hardpointSelection = Selection.Rockets; } else { m_weaponState += 1; } }
                if (m_weaponState == 3) { if (m_missileState == ControlState.Active) { m_hardpointSelection = Selection.Missile; } else { m_weaponState += 1; } }
                if (m_weaponState == 4) { if (m_bombState == ControlState.Active) { m_hardpointSelection = Selection.Bombs; } else { m_weaponState += 1; } }
                if (m_weaponState > 4) { m_weaponState = 0; SwitchWeapon(); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ParticleSystem[] CollectParticles() { return FindObjectsOfType<ParticleSystem>(); }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public LineRenderer[] CollectLineRenderer() { return FindObjectsOfType<LineRenderer>(); }
        /// <summary>
        /// Find all the lightmap static objects
        /// </summary>
        /// <returns></returns>
        public TrailRenderer[] CollectTrailRenderer() { return FindObjectsOfType<TrailRenderer>(); }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<GameObject> CollectStaticObjects()
        {
            UnityEngine.Object[] m_objects = FindObjectsOfType(typeof(GameObject));
            List<GameObject> m_array = new List<GameObject>();

            foreach (GameObject _object in m_objects)
            {
#if UNITY_EDITOR
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(_object);
                if ((flags & StaticEditorFlags.ContributeGI) != 0)
                {
                    if (_object.GetComponent<MeshFilter>() != null)
                    {
                        m_array.Add(_object);
                    }
                }
#endif
            }
            return m_array;
        }
        /// <summary>
        /// Scroll up through the discovered/tracked object list
        /// </summary>
        public void CycleTargetUpwards() { if (m_radar != null) { m_radar.SelectedUpperTarget(); } }
        /// <summary>
        /// Scroll down through the discovered/tracked object list
        /// </summary>
        public void CycleTargetDownwards() { if (m_radar != null) { m_radar.SelectLowerTarget(); } }
        /// <summary>
        /// Lock the selected target object
        /// </summary>
        public void LockTarget() { if (m_radar != null) { m_radar.LockSelectedTarget(); } }
        /// <summary>
        /// Unlock the selected/locked target object
        /// </summary>
        public void ReleaseTarget() { if (m_radar != null) { m_radar.ReleaseLockedTarget(); } }


        #endregion

        #region New Inputs
#if (ENABLE_INPUT_SYSTEM)

        PlayerInput _inputManager;
        public string _controlScheme;

        /// <summary>
        /// 
        /// </summary>
        public void InitializeNewInputs()
        {
            _inputManager = transform.GetComponent<PlayerInput>();
            if (_inputManager != null)
            {
                _inputManager.neverAutoSwitchControlSchemes = true;
                _controlScheme = _inputManager.currentControlScheme;
            }
            else
            {
                Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... Player Input component not attached");
                allOk = false;
                return;
            }
        }

        // Base
        public void OnCollectiveLever(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._collectiveInput = (1 - value.Get<float>()) / 2; } }
        public void OnThrottleLever(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._throttleInput = (1 - value.Get<float>()) / 2; } }
        public void OnPropPitchLever(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._propPitchInput = (1 - value.Get<float>()) / 2; } }
        public void OnMixtureLever(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._mixtureInput = 1 - ((1 - value.Get<float>()) / 2); } }
        public void OnCarbHeatLever(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._carbHeatInput = (1 - value.Get<float>()) / 2; } }
        public void OnPitchInput(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._rawPitchInput = value.Get<float>(); } }
        public void OnRollInput(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._rawRollInput = value.Get<float>(); } }
        public void OnYawInput(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._rawYawInput = value.Get<float>(); } }
        public void OnViewHatswitch(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input._hatViewInput = value.Get<Vector2>(); } }

        // Trim
        public void OnPitchTrimDown()
        {
            m_input._pitchTrimInput += m_input._pitchTrimDelta;
            m_input._pitchTrimInput = Mathf.Clamp(m_input._pitchTrimInput, -1, 1);
        }
        public void OnPitchTrimUp()
        {
            m_input._pitchTrimInput -= m_input._pitchTrimDelta;
            m_input._pitchTrimInput = Mathf.Clamp(m_input._pitchTrimInput, -1, 1);
        }
        public void OnRollTrimRight()
        {
            m_input._rollTrimInput += m_input._rollTrimDelta;
            m_input._rollTrimInput = Mathf.Clamp(m_input._rollTrimInput, -1, 1);
        }
        public void OnRollTrimLeft()
        {
            m_input._rollTrimInput -= m_input._rollTrimDelta;
            m_input._rollTrimInput = Mathf.Clamp(m_input._rollTrimInput, -1, 1);
        }
        public void OnYawTrimRight()
        {
            m_input._yawTrimInput += m_input._yawTrimDelta;
            m_input._yawTrimInput = Mathf.Clamp(m_input._yawTrimInput, -1, 1);
        }
        public void OnYawTrimLeft()
        {
            m_input._yawTrimInput -= m_input._yawTrimDelta;
            m_input._yawTrimInput = Mathf.Clamp(m_input._yawTrimInput, -1, 1);
        }

        // Common
        public void OnStartEngineGlobal() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { TurnOnEngines(); } }
        public void OnStopEngineGlobal() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { TurnOffEngines(); } }
        public void OnFlapUpSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { RaiseFlaps(); } }
        public void OnFlapDownSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { LowerFlaps(); } }
        public void OnCameraSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.ToggleCameraState(); } }
        public void OnParkingBrake() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.ToggleBrakeState(); } }
        public void OnLightSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.ToggleLightState(); } }
        public void OnGearActuation() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.ToggleGearState(); } }
        public void OnRestoreAircraft() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_helper.RestoreFunction(m_rigidbody, m_controller); } }
        public void OnResetScene() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { ResetScene(); } }
        public void OnFireHold(InputValue value) { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { if (value.isPressed) { m_triggerHeld = true; } else { m_triggerHeld = false; } } }
        public void OnFirePress() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.FireWeapon(); } }
        public void OnSpeedBrake() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { m_input.ToggleSpeedBrakeState(); } }
        public void OnBoostSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { if (!boostRunning) { EngageBoost(); } else if (boostRunning) { DisEngageBoost(); } } }
        public void OnWeaponSwitch() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { SwitchWeapon(); } }
        public void OnTargetUp() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { CycleTargetUpwards(); } }
        public void OnTargetDown() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { CycleTargetDownwards(); } }
        public void OnTargetLock() { if (m_inputType == InputType.Default && m_inputLogic == InputLogic.InputSystem) { LockTarget(); } }
#endif
        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class OriginShift
    {
        // Based on the Unity Wiki FloatingOrigin script by Peter Stirling
        // URL: http://wiki.unity3d.com/index.php/Floating_Origin

        public enum SceneState { ActiveScene, AllScenes }
        public enum Collection { OnInitialize, OnShift }

        public SceneState m_sceneState = SceneState.ActiveScene;

        public Collection m_particleCollection = Collection.OnShift;
        public Collection m_trailRendererCollection = Collection.OnShift;
        public Collection m_lineRendererCollection = Collection.OnShift;

        public ControlState m_state = ControlState.Off;
        public ControlState m_particleShift = ControlState.Active;
        public ControlState m_trailRendererShift = ControlState.Active;
        public ControlState m_lineRendererShift = ControlState.Active;

        public Controller m_vehicle;
        public float m_threshold = 2000;
        public int m_shiftCount;
        public double m_totalDistance;

        // Components
        public List<GameObject> m_staticObjects;
        private ParticleSystem[] m_particles = null;
        private LineRenderer[] m_lineRenderers;
        private TrailRenderer[] m_trailRenderers;
        private ParticleSystem.Particle[] parts = null;



        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (m_state == ControlState.Active)
            {
                Physics.autoSyncTransforms = true;
                m_staticObjects = m_vehicle.CollectStaticObjects();
                if (m_particleShift == ControlState.Active && m_particleCollection == Collection.OnInitialize) { m_particles = m_vehicle.CollectParticles(); }
                if (m_trailRendererShift == ControlState.Active && m_trailRendererCollection == Collection.OnInitialize) { m_trailRenderers = m_vehicle.CollectTrailRenderer(); }
                if (m_lineRendererShift == ControlState.Active && m_lineRendererCollection == Collection.OnInitialize) { m_lineRenderers = m_vehicle.CollectLineRenderer(); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (m_state == ControlState.Active)
            {
                Vector3 m_position = m_vehicle.transform.position;
                m_position.y = 0f;
                if (m_position.magnitude > m_threshold) { Shift(m_position); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Shift(Vector3 _position)
        {
            // Transforms
            if (m_sceneState == SceneState.AllScenes)
            {
                for (int z = 0; z < SceneManager.sceneCount; z++)
                {
                    foreach (GameObject g in SceneManager.GetSceneAt(z).GetRootGameObjects()) { g.transform.position -= _position; }
                }
            }
            else
            {
                foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects()) { g.transform.position -= _position; }
            }
            // Particles
            if (m_particleShift == ControlState.Active)
            {
                if (m_particleCollection == Collection.OnShift) { m_particles = m_vehicle.CollectParticles(); }
                // Shift
                foreach (ParticleSystem system in m_particles)
                {
                    if (system.main.simulationSpace != ParticleSystemSimulationSpace.World)
                        continue;

                    int particlesNeeded = system.main.maxParticles;

                    if (particlesNeeded <= 0)
                        continue;

                    bool wasPaused = system.isPaused;
                    bool wasPlaying = system.isPlaying;

                    if (!wasPaused)
                        system.Pause();

                    // ensure a sufficiently large array in which to store the particles
                    if (parts == null || parts.Length < particlesNeeded)
                    {
                        parts = new ParticleSystem.Particle[particlesNeeded];
                    }

                    // now get the particles
                    int num = system.GetParticles(parts);

                    for (int i = 0; i < num; i++)
                    {
                        parts[i].position -= _position;
                    }

                    system.SetParticles(parts, num);

                    if (wasPlaying)
                        system.Play();
                }
            }
            // Lines
            if (m_lineRendererShift == ControlState.Active)
            {
                if (m_lineRendererCollection == Collection.OnShift) { m_lineRenderers = m_vehicle.CollectLineRenderer(); }
                // Shift
                foreach (var line in m_lineRenderers)
                {
                    Vector3[] positions = new Vector3[line.positionCount];

                    int positionCount = line.GetPositions(positions);
                    for (int i = 0; i < positionCount; ++i)
                        positions[i] -= _position;

                    line.SetPositions(positions);
                }
            }
            // Trails
            if (m_trailRendererShift == ControlState.Active)
            {
                if (m_trailRendererCollection == Collection.OnShift) { m_trailRenderers = m_vehicle.CollectTrailRenderer(); }
                // Shift
                foreach (var trail in m_trailRenderers)
                {
                    Vector3[] positions = new Vector3[trail.positionCount];

                    int positionCount = trail.GetPositions(positions);
                    for (int i = 0; i < positionCount; ++i)
                        positions[i] -= _position;

                    trail.SetPositions(positions);
                }
            }

            m_shiftCount++;
            m_totalDistance += _position.magnitude;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SceneAutopilot
    {
        public Controller m_vehicle;
        public float m_presetSpeed;
        public float m_presetAltitude;
        public float m_presetHeading;
        public float m_presetClimb;

        public enum State { Active, Inactive }
        public State m_state = State.Inactive;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (m_vehicle.m_type == Controller.VehicleType.Aircraft) { m_presetSpeed = 100; m_presetHeading = 0f; m_presetAltitude = 30f; m_presetClimb = 1000f; }
            else { m_presetSpeed = 0; m_presetHeading = 0f; m_presetAltitude = 0f; m_presetClimb = 500f; }

            m_state = State.Inactive;
        }
        /// <summary>
        /// 
        /// </summary>
        public void EnableAutopilot()
        {
            if (m_state == State.Inactive)
            {
                if (m_vehicle != null && m_vehicle.gameObject.activeSelf)
                {
                    m_vehicle.m_flcs.EnableSceneAutopilot();
                    m_state = State.Active;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void DisableAutopilot()
        {
            if (m_state == State.Active)
            {
                if (m_vehicle != null && m_vehicle.gameObject.activeSelf)
                {
                    m_vehicle.m_flcs.DisableSceneAutopilot();
                    m_state = State.Inactive;
                }
            }
        }
    }
}