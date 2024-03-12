using System;
using UnityEngine;
using Oyedoyin.Mathematics;
using Unity.MLAgents.Actuators;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Common
{
    #region Component

    /// <summary>
    /// Handles the input variable and state collection plus processing	 
    /// </summary>
    /// /// <remarks>
    /// This component will collect the inputs from various sources e.g Keyboard, Joystick, VR or custom
    /// and process them into control variables for the flight computer. It also contains all the control and
    /// command functions for the aircraft operation
    /// </remarks>
    [Serializable]
    public class SilantroInput
    {

        public Controller _controller;

        // Mobile Touch Controllers
        public SilantroTouch m_throttleTouch;
        public SilantroTouch m_collectiveTouch;
        public SilantroTouch m_joystickTouch;
        public SilantroTouch m_yawTouch;
        public SilantroTouch m_mixtureTouch;
        public SilantroTouch m_propPitchTouch;

        // VR Levers
        public SilantroLever m_joystickLever;
        public SilantroLever m_collectiveLever;
        public SilantroLever m_throttleLever;
        public SilantroLever m_mixtureLever;
        public SilantroLever m_propPitchLever;

        public AnimationCurve _pitchInputCurve;
        public AnimationCurve _rollInputCurve;
        public AnimationCurve _yawInputCurve;
        [Range(1, 3)] public float _pitchScale = 2, _rollScale = 2, _yawScale = 2;
        public bool showCurves, inputConfigured;
        public float _pitchDeadZone = 0.01f;
        public float _rollDeadZone = 0.01f;
        public float _yawDeadZone = 0.01f;

        public float _pitchTrimDelta = 0.015f;
        public float _rollTrimDelta = 0.015f;
        public float _yawTrimDelta = 0.01f;

        // Base Control Inputs
        public float _throttleInput;
        public float _collectiveInput;
        public float _propPitchInput;
        public float _mixtureInput;
        public float _carbHeatInput;

        private float _pitchInput;
        public float _pitchTrimInput;
        private float _rollInput;
        public float _rollTrimInput;
        private float _yawInput;
        public float _yawTrimInput;
        public Vector2 _hatViewInput;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static AnimationCurve PlotControlInputCurve(float power)
        {
            if (power <= 0) { power = 1; }
            AnimationCurve inputCurve = new AnimationCurve();

            inputCurve.AddKey(new Keyframe(-1.0f, -Mathf.Pow(1.0f, power)));
            inputCurve.AddKey(new Keyframe(-0.9f, -Mathf.Pow(0.9f, power)));
            inputCurve.AddKey(new Keyframe(-0.8f, -Mathf.Pow(0.8f, power)));
            inputCurve.AddKey(new Keyframe(-0.7f, -Mathf.Pow(0.7f, power)));
            inputCurve.AddKey(new Keyframe(-0.6f, -Mathf.Pow(0.6f, power)));
            inputCurve.AddKey(new Keyframe(-0.5f, -Mathf.Pow(0.5f, power)));
            inputCurve.AddKey(new Keyframe(-0.4f, -Mathf.Pow(0.4f, power)));
            inputCurve.AddKey(new Keyframe(-0.3f, -Mathf.Pow(0.3f, power)));
            inputCurve.AddKey(new Keyframe(-0.2f, -Mathf.Pow(0.2f, power)));
            inputCurve.AddKey(new Keyframe(-0.1f, -Mathf.Pow(0.1f, power)));
            inputCurve.AddKey(new Keyframe(0.000f, Mathf.Pow(0.0f, power)));
            inputCurve.AddKey(new Keyframe(0.100f, Mathf.Pow(0.1f, power)));
            inputCurve.AddKey(new Keyframe(0.200f, Mathf.Pow(0.2f, power)));
            inputCurve.AddKey(new Keyframe(0.300f, Mathf.Pow(0.3f, power)));
            inputCurve.AddKey(new Keyframe(0.400f, Mathf.Pow(0.4f, power)));
            inputCurve.AddKey(new Keyframe(0.500f, Mathf.Pow(0.5f, power)));
            inputCurve.AddKey(new Keyframe(0.600f, Mathf.Pow(0.6f, power)));
            inputCurve.AddKey(new Keyframe(0.700f, Mathf.Pow(0.7f, power)));
            inputCurve.AddKey(new Keyframe(0.800f, Mathf.Pow(0.8f, power)));
            inputCurve.AddKey(new Keyframe(0.900f, Mathf.Pow(0.9f, power)));
            inputCurve.AddKey(new Keyframe(1.000f, Mathf.Pow(1.0f, power)));
#if UNITY_EDITOR
            for (int i = 0; i < inputCurve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(inputCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(inputCurve, i, AnimationUtility.TangentMode.Linear);
            }
#endif
            return inputCurve;
        }

        /// <summary>
        /// Setup and configure the input variables
        /// </summary>
        public void Initialize()
        {
            PlotInputCurves();
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlotInputCurves()
        {
            _pitchInputCurve = PlotControlInputCurve(_pitchScale);
            _rollInputCurve = PlotControlInputCurve(_rollScale);
            _yawInputCurve = PlotControlInputCurve(_yawScale);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            // Legacy Default Inputs
            if (_controller.m_inputType == Controller.InputType.Default && _controller.m_inputLogic == Controller.InputLogic.Legacy && inputConfigured)
            {
#if (ENABLE_LEGACY_INPUT_MANAGER)
                    if (Application.isFocused)
                    {
                        // ----------------------------------------- Base
                        _rawPitchInput = Input.GetAxis("Pitch");
                        _rawRollInput = Input.GetAxis("Roll");
                        _rawYawInput = Input.GetAxis("Rudder");
                        float baseThrottleInput = Input.GetAxis("Throttle");
                        _throttleInput = (baseThrottleInput + 1) / 2;
                        float baseCollectiveInput = Input.GetAxis("Collective");
                        _collectiveInput = (baseCollectiveInput + 1) / 2;

                        float basePropPitch = Input.GetAxis("Propeller");
                        _propPitchInput = (basePropPitch + 1) / 2;
                        float baseMixture = Input.GetAxis("Mixture");
                        _mixtureInput = (baseMixture + 1) / 2;

                        // ----------------------------------------- Commands
                        if (Input.GetButtonDown("Start Engine")) { _controller.TurnOnEngines(); }
                        if (Input.GetButtonDown("Stop Engine")) { _controller.TurnOffEngines(); }
                        if (Input.GetButtonDown("Fire")) { FireWeapon(); }
                        if (Input.GetButtonDown("Start Engine BandL")) { }; // TurnOnLeftEngines(); }
                        if (Input.GetButtonDown("Start Engine BandR")) { }; //TurnOnRightEngines(); }
                        if (Input.GetButtonDown("Stop Engine BandL")) { }; //TurnOffLeftEngines(); }
                        if (Input.GetButtonDown("Stop Engine BandR")) { }; //TurnOffRightEngines(); }

                        // ----------------------------------------- Toggles
                        if (Input.GetButtonDown("Parking Brake")) { ToggleBrakeState(); }
                        if (Input.GetKeyDown(KeyCode.C)) { ToggleCameraState(); }
                        if (Input.GetButtonDown("Actuate Gear")) { ToggleGearState(); }
                        if (Input.GetButtonDown("Speed Brake")) { ToggleSpeedBrakeState(); }
                        if (Input.GetButtonDown("LightSwitch")) { ToggleLightState(); }
                        if (Input.GetButtonDown("Afterburner"))
                        {
                            if (!_controller.boostRunning) { _controller.EngageBoost(); }
                            else if (_controller.boostRunning) { _controller.DisEngageBoost(); }
                        }
                        if (Input.GetKeyDown(KeyCode.R)) { _controller.ResetScene(); }
                        if (Input.GetButtonDown("Actuate Slat")) { _controller.ToggleSlatState(); }
                        if (Input.GetButtonDown("Spoiler")) { _controller.ToggleSpoilerState(); }
                        if (Input.GetButtonDown("Extend Flap")) { _controller.LowerFlaps(); }
                        if (Input.GetButtonDown("Retract Flap")) { _controller.RaiseFlaps(); }

                        // ----------------------------------------- Keys
                        if (Input.GetButton("Brake Lever")) { if (!_controller.brakeLeverHeld) { _controller.brakeLeverHeld = true; } } else { if (_controller.brakeLeverHeld) { _controller.brakeLeverHeld = false; } }
                        if (Input.GetButton("Fire")) { if (!_controller.m_triggerHeld) { _controller.m_triggerHeld = true; } } else { if (_controller.m_triggerHeld) { _controller.m_triggerHeld = false; } }

                        // ----------------------------------------- Radar
                        if (Input.GetButtonDown("Target Up")) { _controller.CycleTargetUpwards(); }
                        if (Input.GetButtonDown("Target Down")) { _controller.CycleTargetDownwards(); }
                        if (Input.GetButtonDown("Target Lock")) { _controller.LockTarget(); }
                        if (Input.GetKeyDown(KeyCode.Backspace)) { _controller.ReleaseTarget(); }
                        if (Input.GetKeyDown(KeyCode.Q)) { _controller.SwitchWeapon(); }
                        if (Input.GetKeyDown(KeyCode.F)) { _controller.ExitAircraft(); }
                    }
#endif
            }

            // Mobile Inputs
            if (_controller.m_inputType == Controller.InputType.Mobile)
            {
                // Check is any of the connected touch controllers are pressed
                bool isPressed = false;
                if (m_throttleTouch != null && m_throttleTouch.isPressed == true) { isPressed = true; }
                if (m_collectiveTouch != null && m_collectiveTouch.isPressed == true) { isPressed = true; }
                if (m_joystickTouch != null && m_joystickTouch.isPressed == true) { isPressed = true; }
                if (m_yawTouch != null && m_yawTouch.isPressed == true) { isPressed = true; }
                if (m_mixtureTouch != null && m_mixtureTouch.isPressed == true) { isPressed = true; }
                if (m_propPitchTouch != null && m_propPitchTouch.isPressed == true) { isPressed = true; }
                _controller.touchPressed = isPressed;

                // Collect and Send Inputs
                if (m_collectiveTouch != null) { _collectiveInput = (1 - m_collectiveTouch.m_yOutput) / 2; }
                if (m_throttleTouch != null) { _throttleInput = (1 - m_throttleTouch.m_yOutput) / 2; }
                if (m_propPitchTouch != null) { _propPitchInput = (1 - m_propPitchTouch.m_yOutput) / 2; }
               
                if (m_mixtureTouch != null) { _mixtureInput = (1 - m_mixtureTouch.m_yOutput) / 2; }
                else { _mixtureInput = _throttleInput; }

                if (m_joystickTouch != null) { _rawPitchInput = m_joystickTouch.m_yOutput; }
                if (m_joystickTouch != null) { _rawRollInput = m_joystickTouch.m_xOutput; }
                if (m_yawTouch != null) { _rawYawInput = m_yawTouch.m_xOutput; }
            }

            // VR Inputs
            if (_controller.m_inputType == Controller.InputType.VR)
            {
                // Collect and Send Inputs
                if (m_collectiveLever != null) { _collectiveInput = m_collectiveLever.leverOutput; }
                if (m_throttleLever != null) { _throttleInput = m_throttleLever.leverOutput; } else { _throttleInput = 1; }
                if (m_propPitchLever != null) { _propPitchInput = m_propPitchLever.leverOutput; }
                if (m_mixtureLever != null) { _mixtureInput = m_mixtureLever.leverOutput; }

                if (m_joystickLever != null) { _rawPitchInput = m_joystickLever.pitchOutput; }
                if (m_joystickLever != null) { _rawRollInput = m_joystickLever.rollOutput; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float _rawPitchInput { set { _pitchInput = value; } }
        /// <summary>
        /// 
        /// </summary>
        public float _rawRollInput { set { _rollInput = value; } }
        /// <summary>
        /// 
        /// </summary>
        public float _rawYawInput { set { _yawInput = value; } }

        /// <summary>
        /// 
        /// </summary>
        public float m_pitchInput
        {
            get
            {
                float _basePitchInput = _pitchInputCurve.Evaluate(_pitchInput);
                float _presetPitchInput;
                if (Mathf.Abs(_basePitchInput) > _pitchDeadZone) { _presetPitchInput = _basePitchInput; } else { _presetPitchInput = 0f; }
                return ((Mathf.Abs(_presetPitchInput) - _pitchDeadZone) / (1 - _pitchDeadZone)) * _presetPitchInput;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public float m_rollInput
        {
            get
            {
                float _baseRollInput = _rollInputCurve.Evaluate(_rollInput);
                float _presetRollInput;
                if (Mathf.Abs(_baseRollInput) > _rollDeadZone) { _presetRollInput = _baseRollInput; } else { _presetRollInput = 0f; }
                return ((Mathf.Abs(_presetRollInput) - _rollDeadZone) / (1 - _rollDeadZone)) * _presetRollInput;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public float m_yawInput
        {
            get
            {
                float _baseYawInput = _yawInputCurve.Evaluate(_yawInput);
                float _presetYawInput;
                if (Mathf.Abs(_baseYawInput) > _yawDeadZone) { _presetYawInput = _baseYawInput; } else { _presetYawInput = 0f; }
                return ((Mathf.Abs(_presetYawInput) - _yawDeadZone) / (1 - _yawDeadZone)) * _presetYawInput;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void SetAgentInputs(ActionBuffers actionBuffer)
        {
            _throttleInput = 1;//(actionBuffer.ContinuousActions[0] + 1) / 2f;
            _rawPitchInput = actionBuffer.ContinuousActions[0];//1];
            _rawRollInput = actionBuffer.ContinuousActions[1];//2];
            _rawYawInput = actionBuffer.ContinuousActions[2];//3];
        }
        
        #region Call Functions

        /// <summary>
        /// Engage or disengage the aircraft parking brakes
        /// </summary>
        public void ToggleBrakeState() { if (_controller.isControllable) { if (_controller != null && _controller.m_wheels != null) { _controller.m_wheels.ToggleBrakes(); } } }
        /// <summary>
        /// Switch the connected aircraft lights on or off
        /// </summary>
        public void ToggleLightState()
        {
            if (_controller != null && _controller.isControllable)
            {
                foreach (SilantroBulb light in _controller.m_lights)
                {
                    if (light.state == SilantroBulb.CurrentState.On)
                    {
                        light.SwitchOff();
                        if (_controller.m_lightState == Controller.LightState.On)
                        {
                            _controller.m_lightState = Controller.LightState.Off;
                        }
                    }
                    else
                    {
                        light.SwitchOn(); if (_controller.m_lightState == Controller.LightState.Off)
                        {
                            _controller.m_lightState = Controller.LightState.On;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Switch off the connected aircraft lights
        /// </summary>
        public void TurnOffLights()
        {
            if (_controller != null && _controller.isControllable)
            {
                foreach (SilantroBulb light in _controller.m_lights)
                {
                    if (light.state == SilantroBulb.CurrentState.On)
                    {
                        light.SwitchOff(); if (_controller.m_lightState == Controller.LightState.On)
                        {
                            _controller.m_lightState = Controller.LightState.Off;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Switch on the connected aircraft lights
        /// </summary>
        public void TurnOnLights()
        {
            if (_controller != null && _controller.isControllable)
            {
                foreach (SilantroBulb light in _controller.m_lights)
                {
                    if (light.state == SilantroBulb.CurrentState.Off)
                    {
                        light.SwitchOn();
                        if (_controller.m_lightState == Controller.LightState.Off)
                        {
                            _controller.m_lightState = Controller.LightState.On;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Cycle through the available camera modes
        /// </summary>
        public void ToggleCameraState() { if (_controller != null && _controller.isControllable && _controller.m_view != null) { _controller.m_view.ToggleCamera(); } }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleGearState()
        {
            if (_controller.isControllable)
            {
                if (_controller != null && _controller.gearActuator != null)
                {
                    if (_controller.gearActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { _controller.gearActuator.EngageActuator(); }
                    else { _controller.gearActuator.DisengageActuator(); }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleSpeedBrakeState()
        {
            if (_controller != null && _controller.isControllable)
            {
                if (_controller.speedBrakeActuator != null)
                {
                    if (_controller.speedBrakeActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { _controller.speedBrakeActuator.EngageActuator(); }
                    else { _controller.speedBrakeActuator.DisengageActuator(); }
                }
                //if (_controller.flightComputer.airbrakeType != SilantroFlightComputer.AirbrakeType.ActuatorOnly) { _controller.flightComputer.airbrakeActive = !_controller.flightComputer.airbrakeActive; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void FireGuns()
        {
            if (_controller != null && _controller.isControllable)
            {
                if (_controller.m_guns != null && _controller.m_guns.Length > 0)
                {
                    foreach (SilantroGun gun in _controller.m_guns) { gun.FireGun(_controller.m_rigidbody.velocity); }
                }
                else { Debug.Log("Gun System Offline"); }
            }
            else { Debug.Log("Weapon System Offline"); }
        }
        /// <summary>
        /// 
        /// </summary>
        public void FireWeapon()
        {
            if (_controller != null && _controller.isControllable)
            {
                if (_controller.m_hardpoints == Controller.StoreState.Connected)
                {
                    // Rocket
                    if (_controller.m_hardpointSelection == Controller.Selection.Rockets) { _controller.m_launcher.FireRocket(); }
                    // Missile
                    if (_controller.m_hardpointSelection == Controller.Selection.Missile) { _controller.m_launcher.FireMissile(); }
                }
                else { Debug.Log("Weapon System Offline"); }
            }
        }

        #endregion
    }
    #endregion
}
